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
        S_EnterRoom s_EnterRoomPacket = packet as S_EnterRoom;
        PlayerInfo playerInfo = s_EnterRoomPacket.PlayerInfo;

        Debug.Log($"S_EnterRoom : {s_EnterRoomPacket}");

        GameObject myPlayer = null;

        if (playerInfo.PlayerGender == PlayerGender.PlayerMale)
            myPlayer = Managers.Resource.Instantiate("Creature/MyPlayerMale");
        else if (playerInfo.PlayerGender == PlayerGender.PlayerFemale)
            myPlayer = Managers.Resource.Instantiate("Creature/MyPlayerFemale");

        myPlayer.name = $"{playerInfo.PlayerName}_{playerInfo.ObjectInfo.ObjectId}";

        Managers.Object.Add(myPlayer, playerInfo.ObjectInfo);
        Managers.Object.MyPlayer = myPlayer.GetComponent<MyPlayerController>();

        PlayerController pc = myPlayer.GetComponent<PlayerController>();
        pc.PlayerName = playerInfo.PlayerName;
        pc.PlayerGender = playerInfo.PlayerGender;
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
                anotherPlayer = Managers.Resource.Instantiate("Creature/MyPlayerMale");
            else if (player.PlayerGender == PlayerGender.PlayerFemale)
                anotherPlayer = Managers.Resource.Instantiate("Creature/MyPlayerFemale");

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

        Vector3 curPos = new Vector3(bc.PosInfo.PosX, bc.PosInfo.PosY, 0);
        Vector3 nextPos = new Vector3(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY, 0);

        if (curPos != nextPos)
        {
            ((PlayerController)bc).MoveTimer = 0;
        }

        bc.PosInfo = movePacket.PosInfo;
    }

    public static void S_AddPokemonHandler(PacketSession session, IMessage packet)
    {
        S_AddPokemon s_ServerPokemonPacket = packet as S_AddPokemon;
        PokemonSummary pokemonSum = s_ServerPokemonPacket.PokemonSum;

        Debug.Log($"S_AddPokemon : {s_ServerPokemonPacket}");

        Pokemon pokemon = new Pokemon(pokemonSum);
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

    public static void S_UsePokemonMoveHandler(PacketSession session, IMessage packet)
    {
        S_UsePokemonMove s_UseMovePacket = packet as S_UsePokemonMove;
        int remainedPP = s_UseMovePacket.RemainedPP;

        Debug.Log($"S_UsePokemonMove : {s_UseMovePacket.RemainedPP}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_UseMovePacket);
    }

    public static void S_ChangePokemonHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangePokemonHp s_changeHpPacket = packet as S_ChangePokemonHp;
        int remainedHp = s_changeHpPacket.RemainedHp;

        Debug.Log($"S_ChangePokemonHp : {remainedHp}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_changeHpPacket);
    }

    public static void S_GetEnemyPokemonExpHandler(PacketSession session, IMessage packet)
    {
        S_GetEnemyPokemonExp s_GetExpPacket = packet as S_GetEnemyPokemonExp;

        Debug.Log($"S_GetEnemyPokemonExp : {s_GetExpPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_GetExpPacket);
    }

    public static void S_ChangePokemonExpHandler(PacketSession session, IMessage packet)
    {
        S_ChangePokemonExp s_ChangeExpPacket = packet as S_ChangePokemonExp;

        Debug.Log($"s_ChangeExpPacket : {s_ChangeExpPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_ChangeExpPacket);
    }

    public static void S_ChangePokemonLevelHandler(PacketSession session, IMessage packet)
    {
        S_ChangePokemonLevel s_ChangeLevelPacket = packet as S_ChangePokemonLevel;

        Debug.Log($"s_ChangeLevelPacket : {s_ChangeLevelPacket}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_ChangeLevelPacket);
    }
}
