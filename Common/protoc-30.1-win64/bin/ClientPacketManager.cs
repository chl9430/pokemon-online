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

        _onRecv.Add((ushort)MsgId.SEnterPokemonListScene, MakePacket<S_EnterPokemonListScene>);
        _handler.Add((ushort)MsgId.SEnterPokemonListScene, PacketHandler.S_EnterPokemonListSceneHandler);

        _onRecv.Add((ushort)MsgId.SEnterPokemonBattleScene, MakePacket<S_EnterPokemonBattleScene>);
        _handler.Add((ushort)MsgId.SEnterPokemonBattleScene, PacketHandler.S_EnterPokemonBattleSceneHandler);

        _onRecv.Add((ushort)MsgId.SEnterPlayerBagScene, MakePacket<S_EnterPlayerBagScene>);
        _handler.Add((ushort)MsgId.SEnterPlayerBagScene, PacketHandler.S_EnterPlayerBagSceneHandler);

        _onRecv.Add((ushort)MsgId.SEnterPokemonExchangeScene, MakePacket<S_EnterPokemonExchangeScene>);
        _handler.Add((ushort)MsgId.SEnterPokemonExchangeScene, PacketHandler.S_EnterPokemonExchangeSceneHandler);

        _onRecv.Add((ushort)MsgId.SMoveExchangeCursor, MakePacket<S_MoveExchangeCursor>);
        _handler.Add((ushort)MsgId.SMoveExchangeCursor, PacketHandler.S_MoveExchangeCursorHandler);

        _onRecv.Add((ushort)MsgId.SChooseExchangePokemon, MakePacket<S_ChooseExchangePokemon>);
        _handler.Add((ushort)MsgId.SChooseExchangePokemon, PacketHandler.S_ChooseExchangePokemonHandler);

        _onRecv.Add((ushort)MsgId.SFinalAnswerToExchange, MakePacket<S_FinalAnswerToExchange>);
        _handler.Add((ushort)MsgId.SFinalAnswerToExchange, PacketHandler.S_FinalAnswerToExchangeHandler);

        _onRecv.Add((ushort)MsgId.SExitPokemonExchangeScene, MakePacket<S_ExitPokemonExchangeScene>);
        _handler.Add((ushort)MsgId.SExitPokemonExchangeScene, PacketHandler.S_ExitPokemonExchangeSceneHandler);

        _onRecv.Add((ushort)MsgId.SSpawn, MakePacket<S_Spawn>);
        _handler.Add((ushort)MsgId.SSpawn, PacketHandler.S_SpawnHandler);

        _onRecv.Add((ushort)MsgId.SDespawn, MakePacket<S_Despawn>);
        _handler.Add((ushort)MsgId.SDespawn, PacketHandler.S_DespawnHandler);

        _onRecv.Add((ushort)MsgId.SMove, MakePacket<S_Move>);
        _handler.Add((ushort)MsgId.SMove, PacketHandler.S_MoveHandler);

        _onRecv.Add((ushort)MsgId.SSendTalk, MakePacket<S_SendTalk>);
        _handler.Add((ushort)MsgId.SSendTalk, PacketHandler.S_SendTalkHandler);

        _onRecv.Add((ushort)MsgId.SReceiveTalk, MakePacket<S_ReceiveTalk>);
        _handler.Add((ushort)MsgId.SReceiveTalk, PacketHandler.S_ReceiveTalkHandler);

        _onRecv.Add((ushort)MsgId.SAddPokemon, MakePacket<S_AddPokemon>);
        _handler.Add((ushort)MsgId.SAddPokemon, PacketHandler.S_AddPokemonHandler);

        _onRecv.Add((ushort)MsgId.SAccessPokemonSummary, MakePacket<S_AccessPokemonSummary>);
        _handler.Add((ushort)MsgId.SAccessPokemonSummary, PacketHandler.S_AccessPokemonSummaryHandler);

        _onRecv.Add((ushort)MsgId.SUseItem, MakePacket<S_UseItem>);
        _handler.Add((ushort)MsgId.SUseItem, PacketHandler.S_UseItemHandler);

        _onRecv.Add((ushort)MsgId.SUsePokemonMove, MakePacket<S_UsePokemonMove>);
        _handler.Add((ushort)MsgId.SUsePokemonMove, PacketHandler.S_UsePokemonMoveHandler);

        _onRecv.Add((ushort)MsgId.SSetBattlePokemonMove, MakePacket<S_SetBattlePokemonMove>);
        _handler.Add((ushort)MsgId.SSetBattlePokemonMove, PacketHandler.S_SetBattlePokemonMoveHandler);

        _onRecv.Add((ushort)MsgId.SSwitchBattlePokemon, MakePacket<S_SwitchBattlePokemon>);
        _handler.Add((ushort)MsgId.SSwitchBattlePokemon, PacketHandler.S_SwitchBattlePokemonHandler);

        _onRecv.Add((ushort)MsgId.SReturnPokemonBattleScene, MakePacket<S_ReturnPokemonBattleScene>);
        _handler.Add((ushort)MsgId.SReturnPokemonBattleScene, PacketHandler.S_ReturnPokemonBattleSceneHandler);

        _onRecv.Add((ushort)MsgId.SSendTalkRequest, MakePacket<S_SendTalkRequest>);
        _handler.Add((ushort)MsgId.SSendTalkRequest, PacketHandler.S_SendTalkRequestHandler);

        _onRecv.Add((ushort)MsgId.SGetEnemyPokemonExp, MakePacket<S_GetEnemyPokemonExp>);
        _handler.Add((ushort)MsgId.SGetEnemyPokemonExp, PacketHandler.S_GetEnemyPokemonExpHandler);

        _onRecv.Add((ushort)MsgId.SCheckAndApplyRemainedExp, MakePacket<S_CheckAndApplyRemainedExp>);
        _handler.Add((ushort)MsgId.SCheckAndApplyRemainedExp, PacketHandler.S_CheckAndApplyRemainedExpHandler);

        _onRecv.Add((ushort)MsgId.SEscapeFromWildPokemon, MakePacket<S_EscapeFromWildPokemon>);
        _handler.Add((ushort)MsgId.SEscapeFromWildPokemon, PacketHandler.S_EscapeFromWildPokemonHandler);
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