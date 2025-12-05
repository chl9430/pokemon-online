using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    PlayerInfo _playerInfo;
    MyPlayerController _myPlayerController;
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public PlayerInfo PlayerInfo { get {  return _playerInfo; } set { _playerInfo = value; } }
    public MyPlayerController MyPlayerController { get { return _myPlayerController; } set { _myPlayerController = value; } }

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }

    public void Add(GameObject go, ObjectInfo info)
    {
        // GameObjectType objectType = GetObjectTypeById(info.ObjectId);

        BaseController bc = go.GetComponent<BaseController>();

        if (info.ObjectType == GameObjectType.Player)
        {
            if (bc is MyPlayerController)
            {
                _myPlayerController = (MyPlayerController)bc;
            }

            bc.PosInfo = info.PosInfo;
            bc.SyncPos();
            bc.Id = info.ObjectId;
        }
        else if (info.ObjectType == GameObjectType.Npc)
        {
            bc.PosInfo = info.PosInfo;
            bc.SyncPos();
            bc.Id = info.ObjectId;
        }

        _objects.Add(info.ObjectId, go);
    }

    public void Remove(int id)
    {
        GameObject go = FindById(id);
        if (go == null)
            return;

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public GameObject FindCreature(Vector3Int cellPos)
    {
        foreach (GameObject obj in _objects.Values)
        {
            CreatureController cc = obj.GetComponent<CreatureController>();
            if (cc == null)
                continue;

            if (cc.CellPos == cellPos)
                return obj;
        }

        return null;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Managers.Resource.Destroy(obj);
        _objects.Clear();
        // MyPlayer = null;
    }
}
