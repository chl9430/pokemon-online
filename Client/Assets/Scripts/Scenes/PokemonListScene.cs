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
    PokemonListSceneState _sceneState;

    [SerializeField] PokemonListSelectArea _pokemonSelectingZone;
    [SerializeField] GridLayoutSelectBox _actionSelectBox;
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
                    IList myPokemonSums = s_enterPokemonListPacket.PokemonSums;

                    // 플레이어, 포켓몬 데이터 채우기
                    Managers.Object.PlayerInfo = playerInfo;
                    _playerInfo = playerInfo;

                    // 포켓몬 선택 기능 세팅
                    List<object> datas = new List<object>();
                    for (int i = 0; i < myPokemonSums.Count; i++)
                    {
                        PokemonSummary pokemonSum = myPokemonSums[i] as PokemonSummary;

                        datas.Add(pokemonSum);
                    }
                    _pokemonSelectingZone.CreateButton(datas);

                    // 포켓몬카드에 렌더링 하기
                    List<DynamicButton> btns = _pokemonSelectingZone.ChangeBtnGridDataToList();
                    for (int i = 0; i < btns.Count; i++)
                    {
                        btns[i].GetComponent<PokemonCard>().FillPokemonCard(myPokemonSums[i] as PokemonSummary);
                    }

                    // 액션 선택 기능 세팅
                    if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                    {
                        List<string> btnNames = new List<string>()
                        {
                            "Send Out",
                            "Summary",
                            "Cancel"
                        };
                        _actionSelectBox.CreateButtons(btnNames, 1, 400, 100);
                    }
                    else
                    {
                        // 비전머신이 만들어지면 커스텀
                        List<string> btnNames = new List<string>()
                        {
                            "Summary",
                            "Switch",
                            "Item",
                            "Cancel"
                        };
                        _actionSelectBox.CreateButtons(btnNames, 1, 400, 100);
                    }
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
                                List<DynamicButton> pokemonBtns = _pokemonSelectingZone.ChangeBtnGridDataToList();
                                PokemonSummary firstPokemonSum = pokemonBtns[0].BtnData as PokemonSummary;

                                if (firstPokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                                {
                                    return;
                                }
                                else
                                {
                                    C_ReturnPokemonBattleScene returnBattleScene = new C_ReturnPokemonBattleScene();
                                    returnBattleScene.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                    Managers.Network.SavePacket(returnBattleScene);

                                    _enterEffect.PlayEffect("FadeOut");

                                    _sceneState = PokemonListSceneState.MOVING_TO_BATTLE_SCENE;
                                    ActiveUIBySceneState(_sceneState);
                                }
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
                            if (_pokemonSelectingZone.GetSelectedBtnData() as string == "Cancel")
                            {
                                if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                                {
                                    List<DynamicButton> pokemonBtns = _pokemonSelectingZone.ChangeBtnGridDataToList();
                                    PokemonSummary firstPokemonSum = pokemonBtns[0].BtnData as PokemonSummary;

                                    if (firstPokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        C_ReturnPokemonBattleScene returnBattleScene = new C_ReturnPokemonBattleScene();
                                        returnBattleScene.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                        Managers.Network.SavePacket(returnBattleScene);

                                        _enterEffect.PlayEffect("FadeOut");

                                        _sceneState = PokemonListSceneState.MOVING_TO_BATTLE_SCENE;
                                        ActiveUIBySceneState(_sceneState);
                                    }
                                }
                                else
                                {
                                    _enterEffect.PlayEffect("FadeOut");

                                    _sceneState = PokemonListSceneState.MOVING_TO_GAME_SCENE;
                                    ActiveUIBySceneState(_sceneState);
                                }
                            }
                            else
                            {
                                _sceneState = PokemonListSceneState.CHOOSING_ACTION;
                                ActiveUIBySceneState(_sceneState);
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
                                Managers.Scene.Data = _pokemonSelectingZone.GetSelectedBtnData();

                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = PokemonListSceneState.MOVING_TO_SUMMARY_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else if (selectedAction == "Switch")
                            {
                                Debug.Log(_pokemonSelectingZone.GetSelectedSwitchBtnData());
                                _sceneState = PokemonListSceneState.CHOOSING_POKEMON_TO_SWITCH;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else if (selectedAction == "Item")
                            {
                            }
                            else if (selectedAction == "Send Out")
                            {
                                PokemonSummary pokemonSum = _pokemonSelectingZone.GetSelectedBtnData() as PokemonSummary;
                                int selectedIdx = _pokemonSelectingZone.GetSelectedIndex();

                                if (selectedIdx == 0 || pokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                                {
                                    return;
                                }

                                C_SwitchPokemon switchPokemon = new C_SwitchPokemon();
                                switchPokemon.OwnerId = _playerInfo.ObjectInfo.ObjectId;
                                switchPokemon.PokemonFromIdx = 0;
                                switchPokemon.PokemonToIdx = selectedIdx;

                                Managers.Network.SavePacket(switchPokemon);
                                Managers.Scene.Data = _pokemonSelectingZone.ChangeBtnGridDataToList()[0];

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
                            Debug.Log(_pokemonSelectingZone.GetSelectedBtnData());
                            if (_pokemonSelectingZone.GetSelectedBtnData() as string == "Cancel")
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = PokemonListSceneState.MOVING_TO_GAME_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else
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
