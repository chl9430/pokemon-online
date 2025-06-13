using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance { get { return _instance; } }
    #endregion

    PacketManager()
    {
        Register();
    }

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
    Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();

    public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

    public void Register()
    {
        _onRecv.Add((ushort)MsgId.SEnterGame, MakePacket<S_EnterGame>);
        _handler.Add((ushort)MsgId.SEnterGame, PacketHandler.S_EnterGameHandler);

        _onRecv.Add((ushort)MsgId.SEnterRoom, MakePacket<S_EnterRoom>);
        _handler.Add((ushort)MsgId.SEnterRoom, PacketHandler.S_EnterRoomHandler);

        _onRecv.Add((ushort)MsgId.SLeaveRoom, MakePacket<S_LeaveRoom>);
        _handler.Add((ushort)MsgId.SLeaveRoom, PacketHandler.S_LeaveRoomHandler);

        _onRecv.Add((ushort)MsgId.SSpawn, MakePacket<S_Spawn>);
        _handler.Add((ushort)MsgId.SSpawn, PacketHandler.S_SpawnHandler);

        _onRecv.Add((ushort)MsgId.SDespawn, MakePacket<S_Despawn>);
        _handler.Add((ushort)MsgId.SDespawn, PacketHandler.S_DespawnHandler);

        _onRecv.Add((ushort)MsgId.SMove, MakePacket<S_Move>);
        _handler.Add((ushort)MsgId.SMove, PacketHandler.S_MoveHandler);

        _onRecv.Add((ushort)MsgId.SAddPokemon, MakePacket<S_AddPokemon>);
        _handler.Add((ushort)MsgId.SAddPokemon, PacketHandler.S_AddPokemonHandler);

        _onRecv.Add((ushort)MsgId.SAccessPokemonSummary, MakePacket<S_AccessPokemonSummary>);
        _handler.Add((ushort)MsgId.SAccessPokemonSummary, PacketHandler.S_AccessPokemonSummaryHandler);

        _onRecv.Add((ushort)MsgId.SEnterPokemonBattleScene, MakePacket<S_EnterPokemonBattleScene>);
        _handler.Add((ushort)MsgId.SEnterPokemonBattleScene, PacketHandler.S_EnterPokemonBattleSceneHandler);

        _onRecv.Add((ushort)MsgId.SEnterPlayerBagScene, MakePacket<S_EnterPlayerBagScene>);
        _handler.Add((ushort)MsgId.SEnterPlayerBagScene, PacketHandler.S_EnterPlayerBagSceneHandler);

        _onRecv.Add((ushort)MsgId.SUsePokemonMove, MakePacket<S_UsePokemonMove>);
        _handler.Add((ushort)MsgId.SUsePokemonMove, PacketHandler.S_UsePokemonMoveHandler);

        _onRecv.Add((ushort)MsgId.SChangePokemonHp, MakePacket<S_ChangePokemonHp>);
        _handler.Add((ushort)MsgId.SChangePokemonHp, PacketHandler.S_ChangePokemonHpHandler);

        _onRecv.Add((ushort)MsgId.SGetEnemyPokemonExp, MakePacket<S_GetEnemyPokemonExp>);
        _handler.Add((ushort)MsgId.SGetEnemyPokemonExp, PacketHandler.S_GetEnemyPokemonExpHandler);

        _onRecv.Add((ushort)MsgId.SChangePokemonExp, MakePacket<S_ChangePokemonExp>);
        _handler.Add((ushort)MsgId.SChangePokemonExp, PacketHandler.S_ChangePokemonExpHandler);

        _onRecv.Add((ushort)MsgId.SChangePokemonLevel, MakePacket<S_ChangePokemonLevel>);
        _handler.Add((ushort)MsgId.SChangePokemonLevel, PacketHandler.S_ChangePokemonLevelHandler);
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>, ushort> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer, id);
    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
    {
        T pkt = new T();
        pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

        if (CustomHandler != null)
        {
            CustomHandler.Invoke(session, pkt, id);
        }
        else
        {
            Action<PacketSession, IMessage> action = null;
            if (_handler.TryGetValue(id, out action))
                action.Invoke(session, pkt);
        }
    }

    public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
    {
        Action<PacketSession, IMessage> action = null;
        if (_handler.TryGetValue(id, out action))
            return action;
        return null;
    }
}