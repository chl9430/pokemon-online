using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using UnityEngine;

public class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
        Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {

    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {

    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {

    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {

    }

    public static void S_DieHandler(PacketSession session, IMessage packet)
    {

    }
}
