using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PokemonCenter : GameRoom
    {
        public PokemonCenter(RoomType roomType, int roomId) : base(roomType, roomId)
        {
            // 프렌들리숍 스태프 생성
            if (roomType == RoomType.PokemonCenter)
            {
                NPC npc = ObjectManager.Instance.Add<NPC>();
                {
                    npc.Info.PosInfo.State = CreatureState.Idle;
                    npc.Info.PosInfo.MoveDir = MoveDir.Down;
                    npc.Name = "Nurse";
                    npc.Room = this;
                }

                if (_objs.ContainsKey(GameObjectType.Npc) == false)
                    _objs.Add(GameObjectType.Npc, new Dictionary<int, GameObject>());

                if (_objs[GameObjectType.Npc].ContainsKey(npc.Id) == false)
                    _objs[GameObjectType.Npc].Add(npc.Id, npc);

                Vector2Int npcPos = Map.GetTilePos(7, 3);
                npc.PosInfo.PosX = npcPos.x;
                npc.PosInfo.PosY = npcPos.y;

                Map.SetObj(3, 7, npc);
            }
        }
    }
}
