using Google.Protobuf.Protocol;
using System.Reflection;

namespace Server
{
    public class NPC : GameObject
    {
        NPCType _npcType;

        public NPCType NPCType { set { _npcType = value; } }

        public NPC() : base()
        {
            Info.ObjectType = GameObjectType.Npc;
        }

        public NPCInfo MakeNPCInfo()
        {
            NPCInfo npcInfo = new NPCInfo();
            npcInfo.ObjectInfo = Info;
            npcInfo.NpcName = Name;
            npcInfo.NpcType = _npcType;

            return npcInfo;
        }
    }
}
