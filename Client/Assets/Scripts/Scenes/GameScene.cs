using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    IMessage _packet;
    GameMenuUI _gameMenu;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        Managers.Map.LoadMap(1);

        _gameMenu = Managers.Resource.Instantiate("UI/GameScene/GameMenuUI").GetComponent<GameMenuUI>();

        _gameMenu.transform.SetParent(_ui.transform);

        Screen.SetResolution(1280, 720, false);

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

    public override void RegisterPacket(IMessage packet)
    {
        _packet = packet;
    }

    public override void DoNextActionWithTimeline()
    {
        Managers.Network.SavePacket(_packet);

        // 씬 변경
        Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.Battle);
    }

    public override void DoNextAction(object value = null)
    {
    }

    public override void Clear()
    {
    }

    public void ToggleGameMenu(bool toggle)
    {
        _gameMenu.gameObject.SetActive(toggle);
    }
}
