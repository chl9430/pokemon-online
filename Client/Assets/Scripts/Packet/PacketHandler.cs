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

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_enterRoomPacket);
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

        foreach (PlayerInfo player in s_SpawnPacket.Players)
        {
            GameObject anotherPlayer = null;

            if (player.PlayerGender == PlayerGender.PlayerMale)
                anotherPlayer = Managers.Resource.Instantiate("Creature/PlayerMale");
            else if (player.PlayerGender == PlayerGender.PlayerFemale)
                anotherPlayer = Managers.Resource.Instantiate("Creature/PlayerFemale");

            anotherPlayer.name = player.PlayerName;

            Managers.Object.Add(anotherPlayer, player.ObjectInfo);

            PlayerController pc = anotherPlayer.GetComponent<PlayerController>();
            pc.PlayerName = player.PlayerName;
            pc.PlayerGender = player.PlayerGender;
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

        // Debug.Log($"{movePacket.ObjectId} : [{movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}]");
        // Debug.Log($"{movePacket.ObjectId} : [{movePacket.PosInfo.State}]");

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

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_TalkPacket);
    }

    public static void S_ReceiveTalkHandler(PacketSession session, IMessage packet)
    {
        S_ReceiveTalk s_TalkPacket = packet as S_ReceiveTalk;

        Debug.Log($"S_ReceiveTalkHandler : {s_TalkPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_TalkPacket);
    }

    public static void S_AddPokemonHandler(PacketSession session, IMessage packet)
    {
        S_AddPokemon s_ServerPokemonPacket = packet as S_AddPokemon;
        PokemonSummary pokemonSum = s_ServerPokemonPacket.PokemonSum;

        Debug.Log($"S_AddPokemon : {s_ServerPokemonPacket}");

        Pokemon pokemon = new Pokemon(pokemonSum);
    }

    public static void S_EnterPokemonListSceneHandler(PacketSession session, IMessage packet)
    {
        S_EnterPokemonListScene s_enterPokemonListPacket = packet as S_EnterPokemonListScene;

        Debug.Log($"S_EnterPokemonListScene : {s_enterPokemonListPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_enterPokemonListPacket);
    }

    public static void S_AccessPokemonSummaryHandler(PacketSession session, IMessage packet)
    {
        S_AccessPokemonSummary s_AccessPacket = packet as S_AccessPokemonSummary;

        Debug.Log($"S_AccessPokemonSummary : {s_AccessPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_AccessPacket);
    }

    public static void S_EnterPokemonBattleSceneHandler(PacketSession session, IMessage packet)
    {
        S_EnterPokemonBattleScene s_EnterBattleScenePacket = packet as S_EnterPokemonBattleScene;

        Debug.Log($"S_EnterPokemonBattleScene : {s_EnterBattleScenePacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_EnterBattleScenePacket);
    }

    public static void S_CheckAndApplyRemainedExpHandler(PacketSession session, IMessage packet)
    {
        S_CheckAndApplyRemainedExp s_CheckAndApplyExpPacket = packet as S_CheckAndApplyRemainedExp;

        Debug.Log($"S_CheckAndApplyRemainedExp : {s_CheckAndApplyExpPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_CheckAndApplyExpPacket);
    }

    public static void S_CheckAvailableBattlePokemonHandler(PacketSession session, IMessage packet)
    {
        S_CheckAvailableBattlePokemon checkBattlePokemonPacket = packet as S_CheckAvailableBattlePokemon;

        Debug.Log($"S_CheckAvailableBattlePokemon : {checkBattlePokemonPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(checkBattlePokemonPacket);
    }

    public static void S_EscapeFromWildPokemonHandler(PacketSession session, IMessage packet)
    {
        S_EscapeFromWildPokemon s_EscapePacket = packet as S_EscapeFromWildPokemon;

        Debug.Log($"S_EscapeFromWildPokemon : {s_EscapePacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_EscapePacket);
    }

    public static void S_UseItemHandler(PacketSession session, IMessage packet)
    {
        S_UseItem s_useItemPacket = packet as S_UseItem;

        Debug.Log($"S_UseItem : {s_useItemPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_useItemPacket);
    }

    public static void S_EnterPlayerBagSceneHandler(PacketSession session, IMessage packet)
    {
        S_EnterPlayerBagScene s_EnterPlayerBagScenePacket = packet as S_EnterPlayerBagScene;

        Debug.Log($"S_EnterPlayerBagScene : {s_EnterPlayerBagScenePacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_EnterPlayerBagScenePacket);
    }

    public static void S_EnterPokemonExchangeSceneHandler(PacketSession session, IMessage packet)
    {
        S_EnterPokemonExchangeScene s_EnterPokemonExchangeScenePacket = packet as S_EnterPokemonExchangeScene;

        Debug.Log($"S_EnterPokemonExchangeScene : {s_EnterPokemonExchangeScenePacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_EnterPokemonExchangeScenePacket);
    }

    public static void S_ChooseExchangePokemonHandler(PacketSession session, IMessage packet)
    {
        S_ChooseExchangePokemon chooseExchangePacket = packet as S_ChooseExchangePokemon;

        Debug.Log($"S_ChooseExchangePokemon : {chooseExchangePacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(chooseExchangePacket);
    }

    public static void S_FinalAnswerToExchangeHandler(PacketSession session, IMessage packet)
    {
        S_FinalAnswerToExchange finalAnswerPacket = packet as S_FinalAnswerToExchange;

        Debug.Log($"S_FinalAnswerToExchange : {finalAnswerPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(finalAnswerPacket);
    }

    public static void S_ExitPokemonExchangeSceneHandler(PacketSession session, IMessage packet)
    {
        S_ExitPokemonExchangeScene exitExchangePacket = packet as S_ExitPokemonExchangeScene;

        Debug.Log($"S_ExitPokemonExchangeScene : {exitExchangePacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(exitExchangePacket);
    }

    public static void S_MoveExchangeCursorHandler(PacketSession session, IMessage packet)
    {
        S_MoveExchangeCursor s_MoveCursorPacket = packet as S_MoveExchangeCursor;

        Debug.Log($"S_MoveExchangeCursor : {s_MoveCursorPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_MoveCursorPacket);
    }

    public static void S_ProcessTurnHandler(PacketSession session, IMessage packet)
    {
        S_ProcessTurn processTrun = packet as S_ProcessTurn;

        Debug.Log($"S_ProcessTurn : {processTrun}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(processTrun);
    }

    public static void S_GetEnemyPokemonExpHandler(PacketSession session, IMessage packet)
    {
        S_GetEnemyPokemonExp s_GetExpPacket = packet as S_GetEnemyPokemonExp;

        Debug.Log($"S_GetEnemyPokemonExp : {s_GetExpPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_GetExpPacket);
    }

    public static void S_SendTalkRequestHandler(PacketSession session, IMessage packet)
    {
        S_SendTalkRequest s_SendTalkPacket = packet as S_SendTalkRequest;

        Debug.Log($"S_SendTalkRequest : {s_SendTalkPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_SendTalkPacket);
    }

    public static void S_SwitchBattlePokemonHandler(PacketSession session, IMessage packet)
    {
        S_SwitchBattlePokemon s_SwitchPokemonPacket = packet as S_SwitchBattlePokemon;

        Debug.Log($"S_SwitchBattlePokemon : {s_SwitchPokemonPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_SwitchPokemonPacket);
    }

    public static void S_CheckPokemonEvolutionHandler(PacketSession session, IMessage packet)
    {
        S_CheckPokemonEvolution checkEvolution = packet as S_CheckPokemonEvolution;

        Debug.Log($"S_CheckPokemonEvolution : {checkEvolution}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(checkEvolution);
    }

    public static void S_CheckExpPokemonHandler(PacketSession session, IMessage packet)
    {
        S_CheckExpPokemon checkExpPacket = packet as S_CheckExpPokemon;

        Debug.Log($"S_CheckExpPokemon : {checkExpPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(checkExpPacket);
    }

    public static void S_ReturnPokemonBattleSceneHandler(PacketSession session, IMessage packet)
    {
        S_ReturnPokemonBattleScene s_ReturnBattleScenePacket = packet as S_ReturnPokemonBattleScene;

        Debug.Log($"S_ReturnPokemonBattleScene : {s_ReturnBattleScenePacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_ReturnBattleScenePacket);
    }

    public static void S_EnterPokemonEvolutionSceneHandler(PacketSession session, IMessage packet)
    {
        S_EnterPokemonEvolutionScene enterEvolutionPacket = packet as S_EnterPokemonEvolutionScene;

        Debug.Log($"S_EnterPokemonEvolutionScene : {enterEvolutionPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(enterEvolutionPacket);
    }

    public static void S_PokemonEvolutionHandler(PacketSession session, IMessage packet)
    {
        S_PokemonEvolution evolutionPacket = packet as S_PokemonEvolution;

        Debug.Log($"S_PokemonEvolution : {evolutionPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(evolutionPacket);
    }

    public static void S_EnterMoveSelectionSceneHandler(PacketSession session, IMessage packet)
    {
        S_EnterMoveSelectionScene enterMovePacket = packet as S_EnterMoveSelectionScene;

        Debug.Log($"S_EnterMoveSelectionScene : {enterMovePacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(enterMovePacket);
    }

    public static void S_MoveSceneToBattleSceneHandler(PacketSession session, IMessage packet)
    {
        S_MoveSceneToBattleScene battleScenePacket = packet as S_MoveSceneToBattleScene;

        Debug.Log($"S_MoveSceneToBattleScene : {battleScenePacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(battleScenePacket);
    }

    public static void S_MoveSceneToEvolveSceneHandler(PacketSession session, IMessage packet)
    {
        S_MoveSceneToEvolveScene evolveScenePacket = packet as S_MoveSceneToEvolveScene;

        Debug.Log($"S_MoveSceneToEvolveScene : {evolveScenePacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(evolveScenePacket);
    }
}