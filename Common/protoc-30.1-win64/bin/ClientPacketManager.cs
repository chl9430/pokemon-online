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
        _onRecv.Add((ushort)MsgId.SLogIn, MakePacket<S_LogIn>);
        _handler.Add((ushort)MsgId.SLogIn, PacketHandler.S_LogInHandler);

        _onRecv.Add((ushort)MsgId.SCreateAccount, MakePacket<S_CreateAccount>);
        _handler.Add((ushort)MsgId.SCreateAccount, PacketHandler.S_CreateAccountHandler);

        _onRecv.Add((ushort)MsgId.SCheckSaveData, MakePacket<S_CheckSaveData>);
        _handler.Add((ushort)MsgId.SCheckSaveData, PacketHandler.S_CheckSaveDataHandler);

        _onRecv.Add((ushort)MsgId.SEnterGame, MakePacket<S_EnterGame>);
        _handler.Add((ushort)MsgId.SEnterGame, PacketHandler.S_EnterGameHandler);

        _onRecv.Add((ushort)MsgId.SEnterRoom, MakePacket<S_EnterRoom>);
        _handler.Add((ushort)MsgId.SEnterRoom, PacketHandler.S_EnterRoomHandler);

        _onRecv.Add((ushort)MsgId.SLeaveRoom, MakePacket<S_LeaveRoom>);
        _handler.Add((ushort)MsgId.SLeaveRoom, PacketHandler.S_LeaveRoomHandler);

        _onRecv.Add((ushort)MsgId.SEnterPokemonBattleScene, MakePacket<S_EnterPokemonBattleScene>);
        _handler.Add((ushort)MsgId.SEnterPokemonBattleScene, PacketHandler.S_EnterPokemonBattleSceneHandler);

        _onRecv.Add((ushort)MsgId.SMeetWildPokemon, MakePacket<S_MeetWildPokemon>);
        _handler.Add((ushort)MsgId.SMeetWildPokemon, PacketHandler.S_MeetWildPokemonHandler);

        _onRecv.Add((ushort)MsgId.SGetDoorDestDir, MakePacket<S_GetDoorDestDir>);
        _handler.Add((ushort)MsgId.SGetDoorDestDir, PacketHandler.S_GetDoorDestDirHandler);

        _onRecv.Add((ushort)MsgId.SGetNpcTalk, MakePacket<S_GetNpcTalk>);
        _handler.Add((ushort)MsgId.SGetNpcTalk, PacketHandler.S_GetNpcTalkHandler);

        _onRecv.Add((ushort)MsgId.SGetTrainerTalk, MakePacket<S_GetTrainerTalk>);
        _handler.Add((ushort)MsgId.SGetTrainerTalk, PacketHandler.S_GetTrainerTalkHandler);

        _onRecv.Add((ushort)MsgId.SShopItemList, MakePacket<S_ShopItemList>);
        _handler.Add((ushort)MsgId.SShopItemList, PacketHandler.S_ShopItemListHandler);

        _onRecv.Add((ushort)MsgId.SGetItemCount, MakePacket<S_GetItemCount>);
        _handler.Add((ushort)MsgId.SGetItemCount, PacketHandler.S_GetItemCountHandler);

        _onRecv.Add((ushort)MsgId.SBuyItem, MakePacket<S_BuyItem>);
        _handler.Add((ushort)MsgId.SBuyItem, PacketHandler.S_BuyItemHandler);

        _onRecv.Add((ushort)MsgId.SSellItem, MakePacket<S_SellItem>);
        _handler.Add((ushort)MsgId.SSellItem, PacketHandler.S_SellItemHandler);

        _onRecv.Add((ushort)MsgId.SRestorePokemon, MakePacket<S_RestorePokemon>);
        _handler.Add((ushort)MsgId.SRestorePokemon, PacketHandler.S_RestorePokemonHandler);

        _onRecv.Add((ushort)MsgId.SSaveGameData, MakePacket<S_SaveGameData>);
        _handler.Add((ushort)MsgId.SSaveGameData, PacketHandler.S_SaveGameDataHandler);

        _onRecv.Add((ushort)MsgId.SUseItemInListScene, MakePacket<S_UseItemInListScene>);
        _handler.Add((ushort)MsgId.SUseItemInListScene, PacketHandler.S_UseItemInListSceneHandler);

        _onRecv.Add((ushort)MsgId.SEnterTrainerBattle, MakePacket<S_EnterTrainerBattle>);
        _handler.Add((ushort)MsgId.SEnterTrainerBattle, PacketHandler.S_EnterTrainerBattleHandler);

        _onRecv.Add((ushort)MsgId.SCheckAvailableMove, MakePacket<S_CheckAvailableMove>);
        _handler.Add((ushort)MsgId.SCheckAvailableMove, PacketHandler.S_CheckAvailableMoveHandler);

        _onRecv.Add((ushort)MsgId.SSendAction, MakePacket<S_SendAction>);
        _handler.Add((ushort)MsgId.SSendAction, PacketHandler.S_SendActionHandler);

        _onRecv.Add((ushort)MsgId.SCheckAvailablePokemon, MakePacket<S_CheckAvailablePokemon>);
        _handler.Add((ushort)MsgId.SCheckAvailablePokemon, PacketHandler.S_CheckAvailablePokemonHandler);

        _onRecv.Add((ushort)MsgId.SSurrenderTrainerBattle, MakePacket<S_SurrenderTrainerBattle>);
        _handler.Add((ushort)MsgId.SSurrenderTrainerBattle, PacketHandler.S_SurrenderTrainerBattleHandler);

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

        _onRecv.Add((ushort)MsgId.SProcessTurn, MakePacket<S_ProcessTurn>);
        _handler.Add((ushort)MsgId.SProcessTurn, PacketHandler.S_ProcessTurnHandler);

        _onRecv.Add((ushort)MsgId.SSwitchBattlePokemon, MakePacket<S_SwitchBattlePokemon>);
        _handler.Add((ushort)MsgId.SSwitchBattlePokemon, PacketHandler.S_SwitchBattlePokemonHandler);

        _onRecv.Add((ushort)MsgId.SGetEnemyPokemonExp, MakePacket<S_GetEnemyPokemonExp>);
        _handler.Add((ushort)MsgId.SGetEnemyPokemonExp, PacketHandler.S_GetEnemyPokemonExpHandler);

        _onRecv.Add((ushort)MsgId.SCheckAndApplyRemainedExp, MakePacket<S_CheckAndApplyRemainedExp>);
        _handler.Add((ushort)MsgId.SCheckAndApplyRemainedExp, PacketHandler.S_CheckAndApplyRemainedExpHandler);

        _onRecv.Add((ushort)MsgId.SForgetAndLearnNewMove, MakePacket<S_ForgetAndLearnNewMove>);
        _handler.Add((ushort)MsgId.SForgetAndLearnNewMove, PacketHandler.S_ForgetAndLearnNewMoveHandler);

        _onRecv.Add((ushort)MsgId.SCheckAvailableBattlePokemon, MakePacket<S_CheckAvailableBattlePokemon>);
        _handler.Add((ushort)MsgId.SCheckAvailableBattlePokemon, PacketHandler.S_CheckAvailableBattlePokemonHandler);

        _onRecv.Add((ushort)MsgId.SSendOpponentNextPokemon, MakePacket<S_SendOpponentNextPokemon>);
        _handler.Add((ushort)MsgId.SSendOpponentNextPokemon, PacketHandler.S_SendOpponentNextPokemonHandler);

        _onRecv.Add((ushort)MsgId.SEscapeFromWildPokemon, MakePacket<S_EscapeFromWildPokemon>);
        _handler.Add((ushort)MsgId.SEscapeFromWildPokemon, PacketHandler.S_EscapeFromWildPokemonHandler);

        _onRecv.Add((ushort)MsgId.SCheckPokemonEvolution, MakePacket<S_CheckPokemonEvolution>);
        _handler.Add((ushort)MsgId.SCheckPokemonEvolution, PacketHandler.S_CheckPokemonEvolutionHandler);

        _onRecv.Add((ushort)MsgId.SGetRewardInfo, MakePacket<S_GetRewardInfo>);
        _handler.Add((ushort)MsgId.SGetRewardInfo, PacketHandler.S_GetRewardInfoHandler);

        _onRecv.Add((ushort)MsgId.SReturnGame, MakePacket<S_ReturnGame>);
        _handler.Add((ushort)MsgId.SReturnGame, PacketHandler.S_ReturnGameHandler);

        _onRecv.Add((ushort)MsgId.SSendTalkRequest, MakePacket<S_SendTalkRequest>);
        _handler.Add((ushort)MsgId.SSendTalkRequest, PacketHandler.S_SendTalkRequestHandler);

        _onRecv.Add((ushort)MsgId.SPokemonEvolution, MakePacket<S_PokemonEvolution>);
        _handler.Add((ushort)MsgId.SPokemonEvolution, PacketHandler.S_PokemonEvolutionHandler);
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