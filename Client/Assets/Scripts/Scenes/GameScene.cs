using DG.Tweening;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    IMessage _packet;
    GameSceneState _sceneState = GameSceneState.NONE;
    ObjectContents _contents;

    [SerializeField] GridLayoutSelectBox _menuSelectBox;
    [SerializeField] List<DynamicButton> _menuBtns;

    public IMessage Packet {  get { return _packet; } }

    protected override void Init()
    {
        base.Init();

        Screen.SetResolution(1280, 720, false);
    }

    protected override void Start()
    {
        base.Start();

        // 테스트 시 사용.
        if (Managers.Network.Packet == null)
        {
            C_EnterRoom enterRoomPacket = new C_EnterRoom();
            enterRoomPacket.PlayerId = -1;

            SceneType = Define.Scene.Game;

            Managers.Network.Send(enterRoomPacket);
        }
        else
        {
            Managers.Network.SendSavedPacket();
        }
    }

    public override void UpdateData(IMessage packet)
    {
        if (_contents == null)
        {
            if (packet is S_EnterRoom)
            {
                _enterEffect.PlayEffect("FadeIn");

                S_EnterRoom s_enterRoomPacket = packet as S_EnterRoom;
                PlayerInfo playerInfo = s_enterRoomPacket.PlayerInfo;
                int roomId = s_enterRoomPacket.RoomId;
                RoomType roomType = s_enterRoomPacket.RoomType;

                Managers.Map.LoadMap(roomId, roomType);

                Managers.Object.PlayerInfo = playerInfo;

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
            else if (packet is S_MeetWildPokemon)
            {
                _packet = packet as S_MeetWildPokemon;
            }
            else if (packet is S_GetDoorDestDir)
            {
                Managers.Object.MyPlayer.Packet = packet;
            }
            else if (packet is S_SendTalk)
            {
                Managers.Object.MyPlayer.IsLoading = false;
                S_SendTalk sendTalkPacket = packet as S_SendTalk;
                PlayerInfo otherPlayer = sendTalkPacket.OtherPlayerInfo;

                if (otherPlayer != null)
                {
                    _contents = Managers.Object.FindById(otherPlayer.ObjectInfo.ObjectId).GetComponent<PlayerContents>();
                    _contents.UpdateData(packet);
                }
            }
            else if (packet is S_ReceiveTalk)
            {
                Managers.Object.MyPlayer.IsLoading = false;
                S_ReceiveTalk receiveTalkPacket = packet as S_ReceiveTalk;
                PlayerInfo otherPlayer = receiveTalkPacket.PlayerInfo;

                if (otherPlayer != null)
                {
                    _contents = Managers.Object.FindById(otherPlayer.ObjectInfo.ObjectId).GetComponent<PlayerContents>();
                    _contents.UpdateData(packet);
                }
            }
        }
        else
        {
            _contents.UpdateData(packet);
            return;
        }
    }

    public override void DoNextAction(object value = null)
    {
        // 게임 메뉴도 콘텐츠 방식으로 수정 필요
        if (_contents != null)
        {
            _contents.SetNextAction(value);
            return;
        }

        Debug.Log(value);
        switch (_sceneState)
        {
            case GameSceneState.NONE:
                {
                    // 씬 상태 변경
                    _sceneState = GameSceneState.MOVING_PLAYER;
                    ActiveUIBySceneState(_sceneState);
                    Managers.Object.MyPlayer.IsLoading = false;
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
                            if (_menuBtns[_selectedMenuBtnIdx].BtnData as string == "Pokemon")
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = GameSceneState.MOVING_TO_POKEMON_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else if (_menuBtns[_selectedMenuBtnIdx].BtnData as string == "Bag")
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
                    if (Managers.Network.Packet is C_EnterPokemonBattleScene)
                        Managers.Scene.LoadScene(Define.Scene.Battle);
                    else if (Managers.Network.Packet is C_EnterRoom)
                        Managers.Scene.LoadScene(Define.Scene.Game);
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

    public override void FinishContents()
    {
        _contents = null;
    }

    public bool DidMeetWildPokemon()
    {
        if (_packet is S_MeetWildPokemon)
        {
            S_MeetWildPokemon meetPokemonPacket = _packet as S_MeetWildPokemon;
            int roomId = meetPokemonPacket.RoomId;
            int bushNum = meetPokemonPacket.BushNum;

            _packet = null;

            C_EnterPokemonBattleScene enterPokemonBattleScene = new C_EnterPokemonBattleScene();
            enterPokemonBattleScene.PlayerId = Managers.Object.PlayerInfo.ObjectInfo.ObjectId;
            enterPokemonBattleScene.LocationNum = roomId;
            enterPokemonBattleScene.BushNum = bushNum;

            Managers.Network.SavePacket(enterPokemonBattleScene);

            Managers.Object.MyPlayer.State = CreatureState.Fight;

            ScreenEffecter = Managers.Resource.Instantiate("UI/GameScene/PokemonAppearEffect", ScreenEffecterZone).GetComponent<ScreenEffecter>();
            ScreenEffecter.PlayEffect("PokemonAppear");

            _sceneState = GameSceneState.WILD_POKEMON_APPEARING_EFFECT;

            return true;
        }
        else
            return false;
    }

    public void SaveEnterScenePacket()
    {
        C_EnterRoom enterRoomPacket = new C_EnterRoom();
        enterRoomPacket.PlayerId = Managers.Object.PlayerInfo.ObjectInfo.ObjectId;

        string mapName = GameObject.FindAnyObjectByType<Grid>().name;
        int roomId = int.Parse(mapName.Substring(mapName.Length - 1));
        RoomType roomType = (RoomType)Enum.Parse(typeof(RoomType), mapName.Substring(0, mapName.Length - 2));

        enterRoomPacket.PrevRoomId = roomId;
        enterRoomPacket.PrevRoomType = roomType;

        Managers.Network.SavePacket(enterRoomPacket);

        _enterEffect.PlayEffect("FadeOut");

        _sceneState = GameSceneState.WILD_POKEMON_APPEARING_EFFECT;
    }

    public override void Clear()
    {
    }
}
