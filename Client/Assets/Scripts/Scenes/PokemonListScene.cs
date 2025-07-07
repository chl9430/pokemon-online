using Google.Protobuf.Collections;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;

public enum PokemonListSceneState
{
    NONE = 0,
    CHOOSING_POKEMON = 1,
    CHOOSING_ACTION = 2,
    CHOOSING_POKEMON_TO_SWITCH = 3,
    SWITCHING_POKEMON = 4,
    MOVING_TO_GAME_SCENE = 5,
    MOVING_TO_SUMMARY_SCENE = 6,
    MOVING_TO_BATTLE_SCENE = 7,
}

public class PokemonListScene : BaseScene
{
    int _selectedPokemonIdx;
    int _selectedSwitchPokemonIdx;
    PlayerInfo _playerInfo;
    List<Pokemon> _myPokemons;
    List<DynamicButton> _pokemonBtns;
    List<DynamicButton> _actionBtns;
    PokemonListSceneState _sceneState;

    [SerializeField] PokemonListSelectArea _pokemonSelectingZone;
    [SerializeField] List<DynamicButton> _pokemonSelectBtns;
    [SerializeField] GridLayoutSelectBox _actionSelectBox;
    List<DynamicButton> _actionSelectBtns;
    [SerializeField] DynamicButton _actionBtn;
    [SerializeField] TextMeshProUGUI _instructorTmp;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.PokemonList;
    }

    protected override void Start()
    {
        base.Start();

        // 테스트 시 사용.
        if (Managers.Network.Packet == null)
        {
            C_EnterPokemonListScene enterListScenePacket = new C_EnterPokemonListScene();
            enterListScenePacket.PlayerId = -1;

            Managers.Network.Send(enterListScenePacket);
        }
        else
            Managers.Network.SendSavedPacket();
    }

    public override void UpdateData(IMessage packet)
    {
        switch (_sceneState)
        {
            case PokemonListSceneState.NONE:
                {
                    _enterEffect.PlayEffect("FadeIn");

                    S_EnterPokemonListScene s_enterPokemonListPacket = packet as S_EnterPokemonListScene;
                    PlayerInfo playerInfo = s_enterPokemonListPacket.PlayerInfo;
                    IList myPokmeonSums = s_enterPokemonListPacket.PokemonSums;

                    // 플레이어, 포켓몬 데이터 채우기
                    Managers.Object.PlayerInfo = playerInfo;
                    _playerInfo = playerInfo;
                    _myPokemons = new List<Pokemon>();

                    foreach (PokemonSummary sum in myPokmeonSums)
                    {
                        _myPokemons.Add(new Pokemon(sum));
                    }

                    // 포켓몬 선택 기능 세팅
                    _pokemonBtns = new List<DynamicButton>();

                    for (int i = 0; i <= _myPokemons.Count; i++)
                    {
                        if (i == _myPokemons.Count)
                        {
                            _pokemonBtns.Add(_pokemonSelectBtns[_pokemonSelectBtns.Count - 1]);
                            _pokemonBtns[i].BtnData = Util.FindChild<TextMeshProUGUI>(_pokemonBtns[i].gameObject, "ContentText", true).text;
                        }
                        else
                        {
                            _pokemonSelectBtns[i].gameObject.SetActive(true);
                            _pokemonSelectBtns[i].GetComponent<PokemonCard>().FillPokemonCard(_myPokemons[i]);
                            _pokemonBtns.Add(_pokemonSelectBtns[i]);
                            _pokemonBtns[i].BtnData = _myPokemons[i];
                        }
                    }

                    _pokemonSelectingZone.FillButtonGrid(_pokemonBtns.Count, 1, _pokemonBtns);

                    // 액션 선택 기능 세팅
                    _actionBtns = new List<DynamicButton>();

                    if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            _actionBtns.Add(GameObject.Instantiate(_actionBtn, _actionSelectBox.transform));

                            if (i == 0)
                                _actionBtns[i].SetButtonName("Send Out");
                            else if (i == 1)
                                _actionBtns[i].SetButtonName("Summary");
                            else if (i == 2)
                                _actionBtns[i].SetButtonName("Cancel");
                        }
                    }
                    else
                    {
                        // 비전머신이 만들어지면 커스텀
                        for (int i = 0; i < 4; i++)
                        {
                            _actionBtns.Add(GameObject.Instantiate(_actionBtn, _actionSelectBox.transform));

                            if (i == 0)
                                _actionBtns[i].SetButtonName("Summary");
                            else if (i == 1)
                                _actionBtns[i].SetButtonName("Switch");
                            else if (i == 2)
                                _actionBtns[i].SetButtonName("Item");
                            else if (i == 3)
                                _actionBtns[i].SetButtonName("Cancel");
                        }
                    }
                    _actionSelectBox.SetSelectBoxContent(_actionBtns, _actionBtns.Count, 1);
                }
                break;
        }
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);

        switch (_sceneState)
        {
            case PokemonListSceneState.NONE:
                {
                    // 씬 상태 변경
                    _sceneState = PokemonListSceneState.CHOOSING_POKEMON;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case PokemonListSceneState.CHOOSING_POKEMON:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                            {
                                C_ReturnPokemonBattleScene returnBattleScene = new C_ReturnPokemonBattleScene();
                                returnBattleScene.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(returnBattleScene);

                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = PokemonListSceneState.MOVING_TO_BATTLE_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = PokemonListSceneState.MOVING_TO_GAME_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (_pokemonBtns[_selectedPokemonIdx].BtnData is Pokemon)
                            {
                                _sceneState = PokemonListSceneState.CHOOSING_ACTION;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else if (_pokemonBtns[_selectedPokemonIdx].BtnData as string == "CANCEL")
                            {
                                if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                                {
                                    C_ReturnPokemonBattleScene returnBattleScene = new C_ReturnPokemonBattleScene();
                                    returnBattleScene.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                    Managers.Network.SavePacket(returnBattleScene);

                                    _enterEffect.PlayEffect("FadeOut");

                                    _sceneState = PokemonListSceneState.MOVING_TO_BATTLE_SCENE;
                                    ActiveUIBySceneState(_sceneState);
                                }
                                else
                                {
                                    _enterEffect.PlayEffect("FadeOut");

                                    _sceneState = PokemonListSceneState.MOVING_TO_GAME_SCENE;
                                    ActiveUIBySceneState(_sceneState);
                                }
                            }
                        }
                    }
                    else
                    {
                        _selectedPokemonIdx = (int)value;
                    }
                }
                break;
            case PokemonListSceneState.CHOOSING_ACTION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            _sceneState = PokemonListSceneState.CHOOSING_POKEMON;
                            ActiveUIBySceneState(_sceneState);
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            string selectedAction = _actionSelectBox.GetSelectedBtnData() as string;

                            if (selectedAction == "Summary")
                            {
                                Managers.Scene.Data = _pokemonBtns[_selectedPokemonIdx].BtnData;

                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = PokemonListSceneState.MOVING_TO_SUMMARY_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else if (selectedAction == "Switch")
                            {
                                _sceneState = PokemonListSceneState.CHOOSING_POKEMON_TO_SWITCH;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else if (selectedAction == "Item")
                            {
                            }
                            else if (selectedAction == "Send Out")
                            {
                                int selectedIdx = _pokemonSelectingZone.GetSelectedIdx();

                                if (selectedIdx == 0 || _myPokemons[selectedIdx].PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                                {
                                    return;
                                }

                                C_SwitchPokemon switchPokemon = new C_SwitchPokemon();
                                switchPokemon.OwnerId = _playerInfo.ObjectInfo.ObjectId;
                                switchPokemon.PokemonFromIdx = 0;
                                switchPokemon.PokemonToIdx = selectedIdx;

                                Managers.Network.SavePacket(switchPokemon);
                                Managers.Scene.Data = _myPokemons[0];

                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = PokemonListSceneState.MOVING_TO_BATTLE_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else if (selectedAction == "Cancel")
                            {
                                _sceneState = PokemonListSceneState.CHOOSING_POKEMON;
                                ActiveUIBySceneState(_sceneState);
                            }
                        }
                    }
                }
                break;
            case PokemonListSceneState.CHOOSING_POKEMON_TO_SWITCH:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            _sceneState = PokemonListSceneState.CHOOSING_POKEMON;
                            ActiveUIBySceneState(_sceneState);
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (_pokemonBtns[_selectedSwitchPokemonIdx].BtnData is Pokemon)
                            {
                                C_SwitchPokemon switchPacket = new C_SwitchPokemon();
                                switchPacket.OwnerId = _playerInfo.ObjectInfo.ObjectId;
                                switchPacket.PokemonFromIdx = _selectedPokemonIdx;
                                switchPacket.PokemonToIdx = _selectedSwitchPokemonIdx;

                                Managers.Network.Send(switchPacket);

                                _sceneState = PokemonListSceneState.SWITCHING_POKEMON;
                                ActiveUIBySceneState(_sceneState);

                                _pokemonSelectingZone.MoveAndSwitchPokemonCard();
                            }
                            else if (_pokemonBtns[_selectedSwitchPokemonIdx].BtnData as string == "CANCEL")
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = PokemonListSceneState.MOVING_TO_GAME_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                        }
                    }
                    else
                    {
                        _selectedSwitchPokemonIdx = (int)value;
                    }
                }
                break;
            case PokemonListSceneState.SWITCHING_POKEMON:
                {
                    _pokemonSelectingZone.ResetSelectedPokemon();

                    _sceneState = PokemonListSceneState.CHOOSING_POKEMON;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case PokemonListSceneState.MOVING_TO_GAME_SCENE:
                {
                    C_ReturnGame returnGamePacket = new C_ReturnGame();
                    returnGamePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                    Managers.Network.SavePacket(returnGamePacket);

                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.Game);
                }
                break;
            case PokemonListSceneState.MOVING_TO_SUMMARY_SCENE:
                {
                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.PokemonSummary);
                }
                break;
            case PokemonListSceneState.MOVING_TO_BATTLE_SCENE:
                {
                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.Battle);
                }
                break;
        }
    }

    public override void Clear()
    {
    }

    void ActiveUIBySceneState(PokemonListSceneState state)
    {
        if (state == PokemonListSceneState.CHOOSING_POKEMON)
        {
            _instructorTmp.text = "Choose a Pokemon.";
        }
        else if (state == PokemonListSceneState.CHOOSING_ACTION)
        {
            _instructorTmp.text = "Do what with this Pokemon?";
        }
        else if (state == PokemonListSceneState.CHOOSING_POKEMON_TO_SWITCH)
        {
            _instructorTmp.text = "Move to where?";
        }

        if (state == PokemonListSceneState.CHOOSING_ACTION)
        {
            _actionSelectBox.gameObject.SetActive(true);
            _actionSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
        }
        else
            _actionSelectBox.gameObject.SetActive(false);

        if (state == PokemonListSceneState.CHOOSING_POKEMON)
        {
            _pokemonSelectingZone.State = PokemonSelectAreaState.SELECTING_POKEMON;
        }
        else if (state == PokemonListSceneState.CHOOSING_POKEMON_TO_SWITCH)
        {
            _pokemonSelectingZone.State = PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON;
        }
        else
        {
            _pokemonSelectingZone.State = PokemonSelectAreaState.NONE;
        }
    }
}
