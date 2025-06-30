using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GameSceneState
{
    NONE = 0,
    MOVING_PLAYER = 1,
    WATCHING_MENU = 2,
    WILD_POKEMON_APPEARING_EFFECT = 3,
    MOVING_TO_POKEMON_SCENE = 4,
    MOVING_TO_BAG_SCENE = 5
}

public class GameScene : BaseScene
{
    int _selectedMenuBtnIdx;
    GameSceneState _sceneState = GameSceneState.NONE;

    [SerializeField] GridLayoutSelectBox _menuSelectBox;
    [SerializeField] List<DynamicButton> _menuBtns;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        Screen.SetResolution(1280, 720, false);
    }

    protected override void Start()
    {
        base.Start();

        Managers.Map.LoadMap(1);

        // 테스트 시 사용.
        if (Managers.Network.Packet == null)
        {
            C_CreatePlayer createPlayerPacket = new C_CreatePlayer();
            createPlayerPacket.Gender = PlayerGender.PlayerFemale;
            createPlayerPacket.Name = "TEST";

            Managers.Network.Send(createPlayerPacket);
        }
        else
            Managers.Network.SendSavedPacket();
    }

    public override void UpdateData(IMessage packet)
    {
        switch (_sceneState)
        {
            case GameSceneState.NONE:
                {
                    _enterEffect.PlayEffect("FadeIn");

                    S_EnterRoom s_enterRoomPacket = packet as S_EnterRoom;
                    PlayerInfo playerInfo = s_enterRoomPacket.PlayerInfo;

                    // 내 플레이어 생성
                    GameObject myPlayer = null;

                    if (playerInfo.PlayerGender == PlayerGender.PlayerMale)
                        myPlayer = Managers.Resource.Instantiate("Creature/MyPlayerMale");
                    else if (playerInfo.PlayerGender == PlayerGender.PlayerFemale)
                        myPlayer = Managers.Resource.Instantiate("Creature/MyPlayerFemale");

                    myPlayer.name = $"{playerInfo.PlayerName}_{playerInfo.ObjectInfo.ObjectId}";

                    Managers.Object.Add(myPlayer, playerInfo.ObjectInfo);
                    Managers.Object.MyPlayer = myPlayer.GetComponent<MyPlayerController>();

                    PlayerController pc = myPlayer.GetComponent<PlayerController>();
                    pc.PlayerName = playerInfo.PlayerName;
                    pc.PlayerGender = playerInfo.PlayerGender;

                    // 메뉴 버튼 데이터 채우기
                    for (int i = 0; i < _menuBtns.Count; i++)
                    {
                        _menuBtns[i].BtnData = Util.FindChild<TextMeshProUGUI>(_menuBtns[i].gameObject, "ContentText", true).text;
                    }

                    _menuSelectBox.SetSelectBoxContent(_menuBtns, 4, 1);
                }
                break;
        }
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);
        switch (_sceneState)
        {
            case GameSceneState.NONE:
                {
                    // 씬 상태 변경
                    _sceneState = GameSceneState.MOVING_PLAYER;
                    ActiveUIBySceneState(_sceneState);
                    Managers.Object.MyPlayer.IsLocked = false;
                }
                break;
            case GameSceneState.MOVING_PLAYER:
                {
                    CreatureState state = (CreatureState)value;

                    if (state == CreatureState.WatchMenu)
                    {
                        _sceneState = GameSceneState.WATCHING_MENU;
                        ActiveUIBySceneState(_sceneState);
                    }
                    else if (state == CreatureState.Fight)
                    {
                        _sceneState = GameSceneState.WILD_POKEMON_APPEARING_EFFECT;
                        ActiveUIBySceneState(_sceneState);
                    }
                }
                break;
            case GameSceneState.WATCHING_MENU:
                {
                    if (value is CreatureState)
                    {
                        CreatureState state = (CreatureState)value;

                        if (state == CreatureState.Idle)
                        {
                            _sceneState = GameSceneState.MOVING_PLAYER;
                            ActiveUIBySceneState(_sceneState);
                        }
                    }
                    else if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            Managers.Object.MyPlayer.State = CreatureState.Idle;

                            _sceneState = GameSceneState.MOVING_PLAYER;
                            ActiveUIBySceneState(_sceneState);
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (_menuBtns[_selectedMenuBtnIdx].BtnData as string == "POKEMON")
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = GameSceneState.MOVING_TO_POKEMON_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else if (_menuBtns[_selectedMenuBtnIdx].BtnData as string == "BAG")
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = GameSceneState.MOVING_TO_BAG_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                        }
                    }
                    else
                    {
                        _selectedMenuBtnIdx = (int)value;
                    }
                }
                break;
            case GameSceneState.WILD_POKEMON_APPEARING_EFFECT:
                {
                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.Battle);
                }
                break;
            case GameSceneState.MOVING_TO_POKEMON_SCENE:
                {
                    C_EnterPokemonListScene enterPokemonListPacket = new C_EnterPokemonListScene();
                    enterPokemonListPacket.PlayerId = Managers.Object.MyPlayer.Id;

                    Managers.Network.SavePacket(enterPokemonListPacket);

                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.PokemonList);
                }
                break;
            case GameSceneState.MOVING_TO_BAG_SCENE:
                {
                    C_EnterPlayerBagScene enterBagPacket = new C_EnterPlayerBagScene();
                    enterBagPacket.PlayerId = Managers.Object.MyPlayer.Id;

                    Managers.Network.SavePacket(enterBagPacket);

                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.Bag);
                }
                break;
        }
    }

    void ActiveUIBySceneState(GameSceneState state)
    {
        if (state == GameSceneState.WATCHING_MENU)
        {
            _menuSelectBox.gameObject.SetActive(true);
            _menuSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
        }
        else if (state == GameSceneState.MOVING_TO_POKEMON_SCENE)
        {
            _menuSelectBox.UIState = GridLayoutSelectBoxState.NONE;
        }
        else if (state == GameSceneState.MOVING_TO_BAG_SCENE)
        {
            _menuSelectBox.UIState = GridLayoutSelectBoxState.NONE;
        }
        else
        {
            _menuSelectBox.gameObject.SetActive(false);
            _menuSelectBox.UIState = GridLayoutSelectBoxState.NONE;
        }
    }

    public override void Clear()
    {
    }
}
