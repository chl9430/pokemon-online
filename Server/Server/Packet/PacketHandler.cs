using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {

    }

    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {

    }

    public static void C_ExitGameHandler(PacketSession session, IMessage packet)
    {
        C_ExitGame exitPacket = packet as C_ExitGame;
        ClientSession clientSession = session as ClientSession;

        ObjectManager.Instance.Remove(exitPacket.ObjectId);
        clientSession.Disconnect();

        Console.WriteLine($"{exitPacket.ObjectId} has gone!");
    }
}