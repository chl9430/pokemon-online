using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    GameMenuUI _gameMenu;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        Managers.Map.LoadMap(1);

        _gameMenu = Managers.Resource.Instantiate("UI/GameScene/GameMenuUI").GetComponent<GameMenuUI>();

        _gameMenu.transform.SetParent(_ui.transform);

        Screen.SetResolution(1280, 720, false);

        if (Managers.Object.MyPlayer == null && Managers.Object.myPlayerObjInfo != null)
        {
            C_ReturnGame returnPacket = new C_ReturnGame();
            returnPacket.PlayerId = Managers.Object.myPlayerObjInfo.ObjectId;
            Managers.Network.Send(returnPacket);
        }

        Managers.Network.SendSavedPacket();

        //Managers.UI.ShowSceneUI<UI_Inven>();
        //Dictionary<int, Data.Stat> dict = Managers.Data.StatDict;
        //gameObject.GetOrAddComponent<CursorController>();

        //GameObject player = Managers.Game.Spawn(Define.WorldObject.Player, "UnityChan");
        //Camera.main.gameObject.GetOrAddComponent<CameraController>().SetPlayer(player);

        ////Managers.Game.Spawn(Define.WorldObject.Monster, "Knight");
        //GameObject go = new GameObject { name = "SpawningPool" };
        //SpawningPool pool = go.GetOrAddComponent<SpawningPool>();
        //pool.SetKeepMonsterCount(2);
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
