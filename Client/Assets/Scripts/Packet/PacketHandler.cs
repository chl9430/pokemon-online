using Google.Protobuf;
using Google.Protobuf.Protocol;
using NUnit.Framework;
using ServerCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;

        Debug.Log($"S_EnterGame : ");
    }

    public static void S_EnterRoomHandler(PacketSession session, IMessage packet)
    {
        S_EnterRoom s_enterRoomPacket = packet as S_EnterRoom;

        Debug.Log($"S_EnterRoom : {s_enterRoomPacket}");

        Managers.Scene.CurrentScene.UpdateData(s_enterRoomPacket);
    }

    public static void S_LeaveRoomHandler(PacketSession session, IMessage packet)
    {
        S_LeaveRoom leaveRoomPacket = packet as S_LeaveRoom;

        Debug.Log($"S_LeaveRoom : ");
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn s_SpawnPacket = packet as S_Spawn;

        Debug.Log($"S_Spawn : {s_SpawnPacket}");

        foreach (OtherPlayerInfo otherPlayer in s_SpawnPacket.OtherPlayers)
        {
            GameObject anotherPlayer = null;

            if (otherPlayer.PlayerGender == PlayerGender.PlayerMale)
                anotherPlayer = Managers.Resource.Instantiate("Creature/PlayerMale");
            else if (otherPlayer.PlayerGender == PlayerGender.PlayerFemale)
                anotherPlayer = Managers.Resource.Instantiate("Creature/PlayerFemale");

            Managers.Object.Add(anotherPlayer, otherPlayer.ObjectInfo);

            PlayerController pc = anotherPlayer.GetComponent<PlayerController>();
            pc.SetPlayerInfo(otherPlayer);
        }
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        //Debug.Log($"{movePacket.ObjectId} : [{movePacket.PosInfo.State}], [{movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}]");

        Vector3 curPos = new Vector3(bc.PosInfo.PosX, bc.PosInfo.PosY, 0);
        Vector3 nextPos = new Vector3(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY, 0);

        if (curPos != nextPos)
        {
            ((PlayerController)bc).MoveTimer = 0;
        }

        bc.PosInfo = movePacket.PosInfo;
    }

    public static void S_SendTalkHandler(PacketSession session, IMessage packet)
    {
        S_SendTalk s_TalkPacket = packet as S_SendTalk;

        Debug.Log($"S_SendTalk : {s_TalkPacket}");

        Managers.Scene.CurrentScene.UpdateData(s_TalkPacket);
    }

    public static void S_ReceiveTalkHandler(PacketSession session, IMessage packet)
    {
        S_ReceiveTalk s_TalkPacket = packet as S_ReceiveTalk;

        Debug.Log($"S_ReceiveTalkHandler : {s_TalkPacket}");

        Managers.Scene.CurrentScene.UpdateData(s_TalkPacket);
    }

    public static void S_UseItemInListSceneHandler(PacketSession session, IMessage packet)
    {
        S_UseItemInListScene s_UseItemPacket = packet as S_UseItemInListScene;

        Debug.Log($"S_UseItemInListScene : {s_UseItemPacket}");

        Managers.Scene.CurrentScene.UpdateData(s_UseItemPacket);
    }

    public static void S_AccessPokemonSummaryHandler(PacketSession session, IMessage packet)
    {
        S_AccessPokemonSummary s_AccessPacket = packet as S_AccessPokemonSummary;

        Debug.Log($"S_AccessPokemonSummary : {s_AccessPacket}");

        Managers.Scene.CurrentScene.UpdateData(s_AccessPacket);
    }

    public static void S_EnterPokemonBattleSceneHandler(PacketSession session, IMessage packet)
    {
        S_EnterPokemonBattleScene s_EnterBattleScenePacket = packet as S_EnterPokemonBattleScene;

        Debug.Log($"S_EnterPokemonBattleScene : {s_EnterBattleScenePacket}");

        Managers.Scene.CurrentScene.UpdateData(s_EnterBattleScenePacket);
    }

    public static void S_MeetWildPokemonHandler(PacketSession session, IMessage packet)
    {
        S_MeetWildPokemon meetPokemonPacket = packet as S_MeetWildPokemon;

        Debug.Log($"S_MeetWildPokemon : {meetPokemonPacket}");

        Managers.Scene.CurrentScene.UpdateData(meetPokemonPacket);
    }

    public static void S_GetDoorDestDirHandler(PacketSession session, IMessage packet)
    {
        S_GetDoorDestDir doorDestDirPacket = packet as S_GetDoorDestDir;

        Debug.Log($"S_GetDoorDestDir : {doorDestDirPacket}");

        Managers.Scene.CurrentScene.UpdateData(doorDestDirPacket);
    }

    public static void S_GetNpcTalkHandler(PacketSession session, IMessage packet)
    {
        S_GetNpcTalk npcTalkPacket = packet as S_GetNpcTalk;

        Debug.Log($"S_GetNpcTalk : {npcTalkPacket}");

        Managers.Scene.CurrentScene.UpdateData(npcTalkPacket);
    }

    public static void S_GetTrainerTalkHandler(PacketSession session, IMessage packet)
    {
        S_GetTrainerTalk trainerTalkPacket = packet as S_GetTrainerTalk;

        Debug.Log($"S_GetTrainerTalk : {trainerTalkPacket}");

        Managers.Scene.CurrentScene.UpdateData(trainerTalkPacket);
    }

    public static void S_ShopItemListHandler(PacketSession session, IMessage packet)
    {
        S_ShopItemList itemListPacket = packet as S_ShopItemList;

        Debug.Log($"S_ShopItemList : {itemListPacket}");

        Managers.Scene.CurrentScene.UpdateData(itemListPacket);
    }

    public static void S_GetItemCountHandler(PacketSession session, IMessage packet)
    {
        S_GetItemCount getItemCountPacket = packet as S_GetItemCount;

        Debug.Log($"S_GetItemCount : {getItemCountPacket}");

        Managers.Scene.CurrentScene.UpdateData(getItemCountPacket);
    }

    public static void S_BuyItemHandler(PacketSession session, IMessage packet)
    {
        S_BuyItem buyItemPacket = packet as S_BuyItem;

        Debug.Log($"S_BuyItem : {buyItemPacket}");

        Managers.Scene.CurrentScene.UpdateData(buyItemPacket);
    }

    public static void S_SellItemHandler(PacketSession session, IMessage packet)
    {
        S_SellItem sellItemPacket = packet as S_SellItem;

        Debug.Log($"S_SellItem : {sellItemPacket}");

        Managers.Scene.CurrentScene.UpdateData(sellItemPacket);
    }

    public static void S_RestorePokemonHandler(PacketSession session, IMessage packet)
    {
        S_RestorePokemon restorePacket = packet as S_RestorePokemon;

        Debug.Log($"S_RestorePokemon : {restorePacket}");

        Managers.Scene.CurrentScene.UpdateData(restorePacket);
    }

    public static void S_CheckAndApplyRemainedExpHandler(PacketSession session, IMessage packet)
    {
        S_CheckAndApplyRemainedExp s_CheckAndApplyExpPacket = packet as S_CheckAndApplyRemainedExp;

        Debug.Log($"S_CheckAndApplyRemainedExp : {s_CheckAndApplyExpPacket}");

        Managers.Scene.CurrentScene.UpdateData(s_CheckAndApplyExpPacket);
    }

    public static void S_ForgetAndLearnNewMoveHandler(PacketSession session, IMessage packet)
    {
        S_ForgetAndLearnNewMove learnNewMovePacket = packet as S_ForgetAndLearnNewMove;

        Debug.Log($"S_ForgetAndLearnNewMove : {learnNewMovePacket}");

        Managers.Scene.CurrentScene.UpdateData(learnNewMovePacket);
    }

    public static void S_CheckAvailableBattlePokemonHandler(PacketSession session, IMessage packet)
    {
        S_CheckAvailableBattlePokemon checkBattlePokemonPacket = packet as S_CheckAvailableBattlePokemon;

        Debug.Log($"S_CheckAvailableBattlePokemon : {checkBattlePokemonPacket}");

        Managers.Scene.CurrentScene.UpdateData(checkBattlePokemonPacket);
    }

    public static void S_SendOpponentNextPokemonHandler(PacketSession session, IMessage packet)
    {
        S_SendOpponentNextPokemon sendPokemon = packet as S_SendOpponentNextPokemon;

        Debug.Log($"S_SendOpponentNextPokemon : {sendPokemon}");

        Managers.Scene.CurrentScene.UpdateData(sendPokemon);
    }

    public static void S_EscapeFromWildPokemonHandler(PacketSession session, IMessage packet)
    {
        S_EscapeFromWildPokemon s_EscapePacket = packet as S_EscapeFromWildPokemon;

        Debug.Log($"S_EscapeFromWildPokemon : {s_EscapePacket}");

        Managers.Scene.CurrentScene.UpdateData(s_EscapePacket);
    }

    public static void S_EnterTrainerBattleHandler(PacketSession session, IMessage packet)
    {
        S_EnterTrainerBattle enterBattlePacket = packet as S_EnterTrainerBattle;

        Debug.Log($"S_EnterTrainerBattle : {enterBattlePacket}");

        Managers.Scene.CurrentScene.UpdateData(enterBattlePacket);
    }

    public static void S_CheckAvailableMoveHandler(PacketSession session, IMessage packet)
    {
        S_CheckAvailableMove checkMovePacket = packet as S_CheckAvailableMove;

        Debug.Log($"S_CheckAvailableMove : {checkMovePacket}");

        Managers.Scene.CurrentScene.UpdateData(checkMovePacket);
    }

    public static void S_SendActionHandler(PacketSession session, IMessage packet)
    {
        S_SendAction sendActionPacket = packet as S_SendAction;

        Debug.Log($"S_SendAction : {sendActionPacket}");

        Managers.Scene.CurrentScene.UpdateData(sendActionPacket);
    }

    public static void S_CheckAvailablePokemonHandler(PacketSession session, IMessage packet)
    {
        S_CheckAvailablePokemon checkPokemonPacket = packet as S_CheckAvailablePokemon;

        Debug.Log($"S_CheckAvailablePokemon : {checkPokemonPacket}");

        Managers.Scene.CurrentScene.UpdateData(checkPokemonPacket);
    }

    public static void S_SurrenderTrainerBattleHandler(PacketSession session, IMessage packet)
    {
        S_SurrenderTrainerBattle surrenderPacket = packet as S_SurrenderTrainerBattle;

        Debug.Log($"S_SurrenderTrainerBattle : {surrenderPacket}");

        Managers.Scene.CurrentScene.UpdateData(surrenderPacket);
    }

    public static void S_EnterPokemonExchangeSceneHandler(PacketSession session, IMessage packet)
    {
        S_EnterPokemonExchangeScene s_EnterPokemonExchangeScenePacket = packet as S_EnterPokemonExchangeScene;

        Debug.Log($"S_EnterPokemonExchangeScene : {s_EnterPokemonExchangeScenePacket}");

        Managers.Scene.CurrentScene.UpdateData(s_EnterPokemonExchangeScenePacket);
    }

    public static void S_ChooseExchangePokemonHandler(PacketSession session, IMessage packet)
    {
        S_ChooseExchangePokemon chooseExchangePacket = packet as S_ChooseExchangePokemon;

        Debug.Log($"S_ChooseExchangePokemon : {chooseExchangePacket}");

        Managers.Scene.CurrentScene.UpdateData(chooseExchangePacket);
    }

    public static void S_FinalAnswerToExchangeHandler(PacketSession session, IMessage packet)
    {
        S_FinalAnswerToExchange finalAnswerPacket = packet as S_FinalAnswerToExchange;

        Debug.Log($"S_FinalAnswerToExchange : {finalAnswerPacket}");

        Managers.Scene.CurrentScene.UpdateData(finalAnswerPacket);
    }

    public static void S_ExitPokemonExchangeSceneHandler(PacketSession session, IMessage packet)
    {
        S_ExitPokemonExchangeScene exitExchangePacket = packet as S_ExitPokemonExchangeScene;

        Debug.Log($"S_ExitPokemonExchangeScene : {exitExchangePacket}");

        Managers.Scene.CurrentScene.UpdateData(exitExchangePacket);
    }

    public static void S_MoveExchangeCursorHandler(PacketSession session, IMessage packet)
    {
        S_MoveExchangeCursor s_MoveCursorPacket = packet as S_MoveExchangeCursor;

        Debug.Log($"S_MoveExchangeCursor : {s_MoveCursorPacket}");

        Managers.Scene.CurrentScene.UpdateData(s_MoveCursorPacket);
    }

    public static void S_ProcessTurnHandler(PacketSession session, IMessage packet)
    {
        S_ProcessTurn processTrun = packet as S_ProcessTurn;

        Debug.Log($"S_ProcessTurn : {processTrun}");

        Managers.Scene.CurrentScene.UpdateData(processTrun);
    }

    public static void S_GetEnemyPokemonExpHandler(PacketSession session, IMessage packet)
    {
        S_GetEnemyPokemonExp s_GetExpPacket = packet as S_GetEnemyPokemonExp;

        Debug.Log($"S_GetEnemyPokemonExp : {s_GetExpPacket}");

        Managers.Scene.CurrentScene.UpdateData(s_GetExpPacket);
    }

    public static void S_SendTalkRequestHandler(PacketSession session, IMessage packet)
    {
        S_SendTalkRequest s_SendTalkPacket = packet as S_SendTalkRequest;

        Debug.Log($"S_SendTalkRequest : {s_SendTalkPacket}");

        Managers.Scene.CurrentScene.UpdateData(s_SendTalkPacket);
    }

    public static void S_SwitchBattlePokemonHandler(PacketSession session, IMessage packet)
    {
        S_SwitchBattlePokemon s_SwitchPokemonPacket = packet as S_SwitchBattlePokemon;

        Debug.Log($"S_SwitchBattlePokemon : {s_SwitchPokemonPacket}");

        Managers.Scene.CurrentScene.UpdateData(s_SwitchPokemonPacket);
    }

    public static void S_CheckPokemonEvolutionHandler(PacketSession session, IMessage packet)
    {
        S_CheckPokemonEvolution checkEvolution = packet as S_CheckPokemonEvolution;

        Debug.Log($"S_CheckPokemonEvolution : {checkEvolution}");

        Managers.Scene.CurrentScene.UpdateData(checkEvolution);
    }

    public static void S_GetRewardInfoHandler(PacketSession session, IMessage packet)
    {
        S_GetRewardInfo getRewardPacket = packet as S_GetRewardInfo;

        Debug.Log($"S_GetRewardInfo : {getRewardPacket}");

        Managers.Scene.CurrentScene.UpdateData(getRewardPacket);
    }

    public static void S_ReturnGameHandler(PacketSession session, IMessage packet)
    {
        S_ReturnGame returnGamePacket = packet as S_ReturnGame;

        Debug.Log($"S_ReturnGame : {returnGamePacket}");

        Managers.Scene.CurrentScene.UpdateData(returnGamePacket);
    }

    public static void S_PokemonEvolutionHandler(PacketSession session, IMessage packet)
    {
        S_PokemonEvolution evolutionPacket = packet as S_PokemonEvolution;

        Debug.Log($"S_PokemonEvolution : {evolutionPacket}");

        Managers.Scene.CurrentScene.UpdateData(evolutionPacket);
    }
}