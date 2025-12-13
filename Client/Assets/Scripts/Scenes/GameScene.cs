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
}

public class GameScene : BaseScene
{
    GameSceneState _sceneState = GameSceneState.NONE;

    protected override void Init()
    {
        base.Init();

        Managers.Scene.CurrentScene = this;

        Screen.SetResolution(1280, 720, false);
    }

    protected override void Start()
    {
        base.Start();

        // 테스트 시 사용.
        if (Managers.Network.Packet == null)
        {
            //C_EnterRoom enterRoomPacket = new C_EnterRoom();
            //enterRoomPacket.PlayerId = -1;

            //SceneType = Define.Scene.Game;

            //Managers.Network.Send(enterRoomPacket);
        }
        else
        {
            // Managers.Network.SendSavedPacket();
        }
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;

        if (_contentStack.Count == 0)
        {
            if (packet is S_EnterRoom)
            {
                ContentManager.Instance.PlayScreenEffecter("FadeIn_NonBroading");

                S_EnterRoom s_enterRoomPacket = packet as S_EnterRoom;
                PlayerInfo playerInfo = s_enterRoomPacket.PlayerInfo;
                int roomId = s_enterRoomPacket.RoomId;
                RoomType roomType = s_enterRoomPacket.RoomType;
                IList npcInfos = s_enterRoomPacket.NpcInfos;

                Managers.Map.LoadMap(roomId, roomType);

                // 내 플레이어 생성
                GameObject myPlayer = null;

                if (playerInfo.PlayerGender == PlayerGender.PlayerMale)
                    myPlayer = Managers.Resource.Instantiate("Creature/MyPlayerMale");
                else if (playerInfo.PlayerGender == PlayerGender.PlayerFemale)
                    myPlayer = Managers.Resource.Instantiate("Creature/MyPlayerFemale");

                Managers.Object.Add(myPlayer, playerInfo.ObjectInfo);
                Managers.Object.MyPlayerController.SetMyPlayerInfo(playerInfo);

                // npc 정보 적용
                List<CreatureController> npcs = Util.FindChilds<CreatureController>(Managers.Map.GameMap);

                for (int i = 0; i < npcs.Count; i++)
                {
                    NPCInfo npcInfo = npcInfos[i] as NPCInfo;

                    npcs[i].name = $"{npcInfo.NpcName}_{npcInfo.ObjectInfo.ObjectId}";

                    Managers.Object.Add(npcs[i].gameObject, npcInfo.ObjectInfo);
                }

                ContentManager.Instance.LoadUIPrefabs();
                ContentManager.Instance.SetGameMenu();

                // 대화중인 npc가 있는 지 확인
                if (Managers.Object.MyPlayerController.NPC != null)
                {
                    CreatureController npc = Managers.Object.MyPlayerController.NPC;

                    _contentStack.Push(Managers.Object.FindById(npc.Id).GetComponent<ObjectContents>());
                    _contentStack.Peek().UpdateData(_packet);
                }

                _sceneState = GameSceneState.MOVING_PLAYER;
            }
            else if (packet is S_ReturnGame)
            {
                ContentManager.Instance.PlayScreenEffecter("FadeIn_NonBroading");

                Managers.Object.MyPlayerController.State = CreatureState.Idle;

                IList<ObjectInfo> players = ((S_ReturnGame)packet).OtherPlayers;

                foreach (ObjectInfo player in players)
                {
                    GameObject obj = Managers.Object.FindById(player.ObjectId);

                    BaseController bc = obj.GetComponent<BaseController>();

                    bc.CellPos = new Vector3Int(player.PosInfo.PosX, player.PosInfo.PosY);
                    bc.Dir = player.PosInfo.MoveDir;
                    bc.State = CreatureState.Idle;
                }

                _sceneState = GameSceneState.MOVING_PLAYER;
            }
            else if (packet is S_MeetWildPokemon)
            {
                _packet = packet as S_MeetWildPokemon;
            }
            else if (packet is S_GetDoorDestDir)
            {
                Managers.Object.MyPlayerController.Packet = packet;
            }
            else if (packet is S_SendTalk)
            {
                S_SendTalk sendTalkPacket = packet as S_SendTalk;
                OtherPlayerInfo otherPlayer = sendTalkPacket.OtherPlayerInfo;

                if (otherPlayer != null)
                {
                    _contentStack.Push(Managers.Object.FindById(otherPlayer.ObjectInfo.ObjectId).GetComponent<PlayerContents>());
                    _contentStack.Peek().UpdateData(packet);
                }
            }
            else if (packet is S_ReceiveTalk)
            {
                S_ReceiveTalk receiveTalkPacket = packet as S_ReceiveTalk;
                OtherPlayerInfo otherPlayer = receiveTalkPacket.OtherPlayerInfo;

                if (otherPlayer != null)
                {
                    _contentStack.Push(Managers.Object.FindById(otherPlayer.ObjectInfo.ObjectId).GetComponent<PlayerContents>());
                    _contentStack.Peek().UpdateData(packet);
                }
            }
        }
        else
        {
            _contentStack.Peek().UpdateData(packet);
            return;
        }
    }

    public override void DoNextAction(object value = null)
    {
        // 게임 메뉴도 콘텐츠 방식으로 수정 필요
        if (_contentStack.Count > 0)
        {
            _contentStack.Peek().SetNextAction(value);
            return;
        }

        switch (_sceneState)
        {
            case GameSceneState.NONE:
                {
                    
                }
                break;
            case GameSceneState.MOVING_PLAYER:
                {
                    CreatureState state = (CreatureState)value;

                    if (state == CreatureState.WatchMenu)
                    {
                        ContentManager.Instance.OpenGameMenu();
                    }
                    else if (state == CreatureState.Fight)
                    {
                        _sceneState = GameSceneState.WILD_POKEMON_APPEARING_EFFECT;
                    }
                }
                break;
            case GameSceneState.WATCHING_MENU:
                {
                    
                }
                break;
            case GameSceneState.WILD_POKEMON_APPEARING_EFFECT:
                {
                    if (Managers.Network.Packet is C_EnterPokemonBattleScene)
                        Managers.Scene.AsyncLoadScene(Define.Scene.Battle, () => {
                            ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
                            Managers.Scene.CurrentScene = GameObject.FindFirstObjectByType<BattleScene>();
                        }, LoadSceneMode.Additive);
                    else if (Managers.Network.Packet is C_EnterRoom)
                        Managers.Scene.LoadScene(Define.Scene.Game);

                    _sceneState = GameSceneState.NONE;
                }
                break;
        }
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
            enterPokemonBattleScene.PlayerId = Managers.Object.MyPlayerController.Id;
            enterPokemonBattleScene.LocationNum = roomId;
            enterPokemonBattleScene.BushNum = bushNum;

            Managers.Network.SavePacket(enterPokemonBattleScene);

            Managers.Object.MyPlayerController.State = CreatureState.Fight;

            ContentManager.Instance.PlayScreenEffecter("PokemonAppear");

            _sceneState = GameSceneState.WILD_POKEMON_APPEARING_EFFECT;

            return true;
        }
        else
            return false;
    }

    public void SaveEnterScenePacket()
    {
        C_EnterRoom enterRoomPacket = new C_EnterRoom();
        enterRoomPacket.PlayerId = Managers.Object.MyPlayerController.Id;

        string mapName = Managers.Map.CurrentGrid.transform.parent.name;
        int roomId = int.Parse(mapName.Substring(mapName.Length - 1));
        RoomType roomType = (RoomType)Enum.Parse(typeof(RoomType), mapName.Substring(0, mapName.Length - 2));

        enterRoomPacket.PrevRoomId = roomId;
        enterRoomPacket.PrevRoomType = roomType;

        Managers.Network.SavePacket(enterRoomPacket);

        ContentManager.Instance.PlayScreenEffecter("FadeOut");

        _sceneState = GameSceneState.WILD_POKEMON_APPEARING_EFFECT;
    }

    public override void Clear()
    {
    }
}
