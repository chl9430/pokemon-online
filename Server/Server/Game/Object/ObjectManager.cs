using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        object _lock = new object();
        Dictionary<GameObjectType, Dictionary<int, GameObject>> _objs = new Dictionary<GameObjectType, Dictionary<int, GameObject>>();

        int _counter = 0;

        public T Add<T>() where T : GameObject, new()
        {
            T gameObject = new T();

            lock (_lock)
            {
                if (gameObject.ObjectType == GameObjectType.Player)
                {
                    gameObject.Id = GenerateId(gameObject.ObjectType);

                    if (_objs.ContainsKey(GameObjectType.Player) == false)
                    {
                        _objs.Add(GameObjectType.Player, new Dictionary<int, GameObject>());
                    }

                    _objs[GameObjectType.Player].Add(gameObject.Id, gameObject as Player);
                }
                else if (gameObject.ObjectType == GameObjectType.Npc)
                {
                    if (_objs.ContainsKey(GameObjectType.Npc) == false)
                    {
                        _objs.Add(GameObjectType.Npc, new Dictionary<int, GameObject>());
                    }

                    if (gameObject is TrainerNPC)
                    {
                        (gameObject as TrainerNPC).SetNPCId(_objs[GameObjectType.Npc].Count + 1);
                        _objs[GameObjectType.Npc].Add(gameObject.Id, gameObject as NPC);
                    }
                }
            }

            return gameObject;
        }

        public Player AddLoadedPlayer(ClientSession session, PlayerInfo loadedInfo)
        {
            Player player = new Player();
            player.ApplyPlayerInfo(loadedInfo, session);
            session.MyPlayer = player;

            if (_objs.ContainsKey(GameObjectType.Player) == false)
            {
                _objs.Add(GameObjectType.Player, new Dictionary<int, GameObject>());
            }

            _objs[GameObjectType.Player].Add(player.Id, player);

            return player;
        }

        int GenerateId(GameObjectType type)
        {
            lock (_lock)
            {
                return ((int)type << 24) | (_counter++);
            }
        }

        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        public bool Remove(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (objectType == GameObjectType.Player)
                    return _objs[GameObjectType.Player].Remove(objectId);
            }

            return false;
        }

        public Player Find(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (objectType == GameObjectType.Player)
                {
                    GameObject player = null;
                    if (_objs[GameObjectType.Player].TryGetValue(objectId, out player))
                        return player as Player;
                }
            }

            return null;
        }

        public NPC FindNPC(int objectId)
        {
            lock (_lock)
            {
                GameObject npc = null;
                if (_objs[GameObjectType.Npc].TryGetValue(objectId, out npc))
                    return npc as NPC;
            }

            return null;
        }
    }
}
