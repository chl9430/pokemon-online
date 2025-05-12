using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    public ObjectInfo myPlayerObjInfo { get; set; }
    public List<Pokemon> _pokemons = new List<Pokemon>();
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }

    public void Add(ObjectInfo info, bool myPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(info.ObjectId);
        if (objectType == GameObjectType.Player)
        {
            if (myPlayer)
            {
                GameObject go = null;

                if (info.Gender == PlayerGender.PlayerMale)
                    go = Managers.Resource.Instantiate("Creature/MyPlayerMale");
                else if (info.Gender == PlayerGender.PlayerFemale)
                    go = Managers.Resource.Instantiate("Creature/MyPlayerFemale");

                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                MyPlayer = go.GetComponent<MyPlayerController>();
                myPlayerObjInfo = info;
                MyPlayer.Id = info.ObjectId;
                MyPlayer.PosInfo = info.PosInfo;
                MyPlayer.SyncPos();
            }
            else
            {
                GameObject go = null;

                if (info.Gender == PlayerGender.PlayerMale)
                    go = Managers.Resource.Instantiate("Creature/PlayerMale");
                else if (info.Gender == PlayerGender.PlayerFemale)
                    go = Managers.Resource.Instantiate("Creature/PlayerFemale");

                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.PosInfo = info.PosInfo;
                pc.SyncPos();
            }
        }
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
