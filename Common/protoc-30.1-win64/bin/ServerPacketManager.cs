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

        _onRecv.Add((ushort)MsgId.CEnterRoom, MakePacket<C_EnterRoom>);
        _handler.Add((ushort)MsgId.CEnterRoom, PacketHandler.C_EnterRoomHandler);

        _onRecv.Add((ushort)MsgId.CEnterPokemonBattleScene, MakePacket<C_EnterPokemonBattleScene>);
        _handler.Add((ushort)MsgId.CEnterPokemonBattleScene, PacketHandler.C_EnterPokemonBattleSceneHandler);

        _onRecv.Add((ushort)MsgId.CShopItemList, MakePacket<C_ShopItemList>);
        _handler.Add((ushort)MsgId.CShopItemList, PacketHandler.C_ShopItemListHandler);

        _onRecv.Add((ushort)MsgId.CGetItemCount, MakePacket<C_GetItemCount>);
        _handler.Add((ushort)MsgId.CGetItemCount, PacketHandler.C_GetItemCountHandler);

        _onRecv.Add((ushort)MsgId.CBuyItem, MakePacket<C_BuyItem>);
        _handler.Add((ushort)MsgId.CBuyItem, PacketHandler.C_BuyItemHandler);

        _onRecv.Add((ushort)MsgId.CSellItem, MakePacket<C_SellItem>);
        _handler.Add((ushort)MsgId.CSellItem, PacketHandler.C_SellItemHandler);

        _onRecv.Add((ushort)MsgId.CRestorePokemon, MakePacket<C_RestorePokemon>);
        _handler.Add((ushort)MsgId.CRestorePokemon, PacketHandler.C_RestorePokemonHandler);

        _onRecv.Add((ushort)MsgId.CFinishNpcTalk, MakePacket<C_FinishNpcTalk>);
        _handler.Add((ushort)MsgId.CFinishNpcTalk, PacketHandler.C_FinishNpcTalkHandler);

        _onRecv.Add((ushort)MsgId.CUseItemInListScene, MakePacket<C_UseItemInListScene>);
        _handler.Add((ushort)MsgId.CUseItemInListScene, PacketHandler.C_UseItemInListSceneHandler);

        _onRecv.Add((ushort)MsgId.CEnterTrainerBattle, MakePacket<C_EnterTrainerBattle>);
        _handler.Add((ushort)MsgId.CEnterTrainerBattle, PacketHandler.C_EnterTrainerBattleHandler);

        _onRecv.Add((ushort)MsgId.CCheckAvailableMove, MakePacket<C_CheckAvailableMove>);
        _handler.Add((ushort)MsgId.CCheckAvailableMove, PacketHandler.C_CheckAvailableMoveHandler);

        _onRecv.Add((ushort)MsgId.CSendAction, MakePacket<C_SendAction>);
        _handler.Add((ushort)MsgId.CSendAction, PacketHandler.C_SendActionHandler);

        _onRecv.Add((ushort)MsgId.CRequestNextBattleAction, MakePacket<C_RequestNextBattleAction>);
        _handler.Add((ushort)MsgId.CRequestNextBattleAction, PacketHandler.C_RequestNextBattleActionHandler);

        _onRecv.Add((ushort)MsgId.CCheckAvailablePokemon, MakePacket<C_CheckAvailablePokemon>);
        _handler.Add((ushort)MsgId.CCheckAvailablePokemon, PacketHandler.C_CheckAvailablePokemonHandler);

        _onRecv.Add((ushort)MsgId.CSurrenderTrainerBattle, MakePacket<C_SurrenderTrainerBattle>);
        _handler.Add((ushort)MsgId.CSurrenderTrainerBattle, PacketHandler.C_SurrenderTrainerBattleHandler);

        _onRecv.Add((ushort)MsgId.CEnterPokemonExchangeScene, MakePacket<C_EnterPokemonExchangeScene>);
        _handler.Add((ushort)MsgId.CEnterPokemonExchangeScene, PacketHandler.C_EnterPokemonExchangeSceneHandler);

        _onRecv.Add((ushort)MsgId.CMoveExchangeCursor, MakePacket<C_MoveExchangeCursor>);
        _handler.Add((ushort)MsgId.CMoveExchangeCursor, PacketHandler.C_MoveExchangeCursorHandler);

        _onRecv.Add((ushort)MsgId.CChooseExchangePokemon, MakePacket<C_ChooseExchangePokemon>);
        _handler.Add((ushort)MsgId.CChooseExchangePokemon, PacketHandler.C_ChooseExchangePokemonHandler);

        _onRecv.Add((ushort)MsgId.CFinalAnswerToExchange, MakePacket<C_FinalAnswerToExchange>);
        _handler.Add((ushort)MsgId.CFinalAnswerToExchange, PacketHandler.C_FinalAnswerToExchangeHandler);

        _onRecv.Add((ushort)MsgId.CExitPokemonExchangeScene, MakePacket<C_ExitPokemonExchangeScene>);
        _handler.Add((ushort)MsgId.CExitPokemonExchangeScene, PacketHandler.C_ExitPokemonExchangeSceneHandler);

        _onRecv.Add((ushort)MsgId.CMove, MakePacket<C_Move>);
        _handler.Add((ushort)MsgId.CMove, PacketHandler.C_MoveHandler);

        _onRecv.Add((ushort)MsgId.CCreatePlayer, MakePacket<C_CreatePlayer>);
        _handler.Add((ushort)MsgId.CCreatePlayer, PacketHandler.C_CreatePlayerHandler);

        _onRecv.Add((ushort)MsgId.CSwitchPokemon, MakePacket<C_SwitchPokemon>);
        _handler.Add((ushort)MsgId.CSwitchPokemon, PacketHandler.C_SwitchPokemonHandler);

        _onRecv.Add((ushort)MsgId.CAccessPokemonSummary, MakePacket<C_AccessPokemonSummary>);
        _handler.Add((ushort)MsgId.CAccessPokemonSummary, PacketHandler.C_AccessPokemonSummaryHandler);

        _onRecv.Add((ushort)MsgId.CRequestDataById, MakePacket<C_RequestDataById>);
        _handler.Add((ushort)MsgId.CRequestDataById, PacketHandler.C_RequestDataByIdHandler);

        _onRecv.Add((ushort)MsgId.CProcessTurn, MakePacket<C_ProcessTurn>);
        _handler.Add((ushort)MsgId.CProcessTurn, PacketHandler.C_ProcessTurnHandler);

        _onRecv.Add((ushort)MsgId.CForgetAndLearnNewMove, MakePacket<C_ForgetAndLearnNewMove>);
        _handler.Add((ushort)MsgId.CForgetAndLearnNewMove, PacketHandler.C_ForgetAndLearnNewMoveHandler);

        _onRecv.Add((ushort)MsgId.CGetRewardInfo, MakePacket<C_GetRewardInfo>);
        _handler.Add((ushort)MsgId.CGetRewardInfo, PacketHandler.C_GetRewardInfoHandler);

        _onRecv.Add((ushort)MsgId.CReturnGame, MakePacket<C_ReturnGame>);
        _handler.Add((ushort)MsgId.CReturnGame, PacketHandler.C_ReturnGameHandler);

        _onRecv.Add((ushort)MsgId.CPlayerTalk, MakePacket<C_PlayerTalk>);
        _handler.Add((ushort)MsgId.CPlayerTalk, PacketHandler.C_PlayerTalkHandler);

        _onRecv.Add((ushort)MsgId.CPokemonEvolution, MakePacket<C_PokemonEvolution>);
        _handler.Add((ushort)MsgId.CPokemonEvolution, PacketHandler.C_PokemonEvolutionHandler);
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