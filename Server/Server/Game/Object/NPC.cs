using Google.Protobuf.Protocol;
using System.Reflection;

namespace Server
{
    public class NPC : GameObject
    {
        public NPC() : base()
        {
            ObjectType = GameObjectType.Npc;
        }

        public NPCInfo MakeNPCInfo()
        {
            NPCInfo npcInfo = new NPCInfo();
            npcInfo.ObjectInfo = Info;
            npcInfo.NpcName = Name;

            return npcInfo;
        }
    }
}
