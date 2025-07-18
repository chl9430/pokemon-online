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
        _onRecv.Add((ushort)MsgId.CExitGame, MakePacket<C_ExitGame>);
        _handler.Add((ushort)MsgId.CExitGame, PacketHandler.C_ExitGameHandler);

        _onRecv.Add((ushort)MsgId.CMove, MakePacket<C_Move>);
        _handler.Add((ushort)MsgId.CMove, PacketHandler.C_MoveHandler);

        _onRecv.Add((ushort)MsgId.CCreatePlayer, MakePacket<C_CreatePlayer>);
        _handler.Add((ushort)MsgId.CCreatePlayer, PacketHandler.C_CreatePlayerHandler);

        _onRecv.Add((ushort)MsgId.CAddPokemon, MakePacket<C_AddPokemon>);
        _handler.Add((ushort)MsgId.CAddPokemon, PacketHandler.C_AddPokemonHandler);

        _onRecv.Add((ushort)MsgId.CSwitchPokemon, MakePacket<C_SwitchPokemon>);
        _handler.Add((ushort)MsgId.CSwitchPokemon, PacketHandler.C_SwitchPokemonHandler);

        _onRecv.Add((ushort)MsgId.CAccessPokemonSummary, MakePacket<C_AccessPokemonSummary>);
        _handler.Add((ushort)MsgId.CAccessPokemonSummary, PacketHandler.C_AccessPokemonSummaryHandler);

        _onRecv.Add((ushort)MsgId.CEnterPokemonListScene, MakePacket<C_EnterPokemonListScene>);
        _handler.Add((ushort)MsgId.CEnterPokemonListScene, PacketHandler.C_EnterPokemonListSceneHandler);

        _onRecv.Add((ushort)MsgId.CEnterPokemonBattleScene, MakePacket<C_EnterPokemonBattleScene>);
        _handler.Add((ushort)MsgId.CEnterPokemonBattleScene, PacketHandler.C_EnterPokemonBattleSceneHandler);

        _onRecv.Add((ushort)MsgId.CEnterPlayerBagScene, MakePacket<C_EnterPlayerBagScene>);
        _handler.Add((ushort)MsgId.CEnterPlayerBagScene, PacketHandler.C_EnterPlayerBagSceneHandler);

        _onRecv.Add((ushort)MsgId.CUseItem, MakePacket<C_UseItem>);
        _handler.Add((ushort)MsgId.CUseItem, PacketHandler.C_UseItemHandler);

        _onRecv.Add((ushort)MsgId.CUsePokemonMove, MakePacket<C_UsePokemonMove>);
        _handler.Add((ushort)MsgId.CUsePokemonMove, PacketHandler.C_UsePokemonMoveHandler);

        _onRecv.Add((ushort)MsgId.CSetBattlePokemonMove, MakePacket<C_SetBattlePokemonMove>);
        _handler.Add((ushort)MsgId.CSetBattlePokemonMove, PacketHandler.C_SetBattlePokemonMoveHandler);

        _onRecv.Add((ushort)MsgId.CReturnGame, MakePacket<C_ReturnGame>);
        _handler.Add((ushort)MsgId.CReturnGame, PacketHandler.C_ReturnGameHandler);

        _onRecv.Add((ushort)MsgId.CReturnPokemonBattleScene, MakePacket<C_ReturnPokemonBattleScene>);
        _handler.Add((ushort)MsgId.CReturnPokemonBattleScene, PacketHandler.C_ReturnPokemonBattleSceneHandler);

        _onRecv.Add((ushort)MsgId.CPlayerTalk, MakePacket<C_PlayerTalk>);
        _handler.Add((ushort)MsgId.CPlayerTalk, PacketHandler.C_PlayerTalkHandler);

        _onRecv.Add((ushort)MsgId.CRequestDataById, MakePacket<C_RequestDataById>);
        _handler.Add((ushort)MsgId.CRequestDataById, PacketHandler.C_RequestDataByIdHandler);
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