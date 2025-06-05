using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
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
        S_EnterRoom enterRoomPacket = packet as S_EnterRoom;

        Debug.Log($"S_EnterRoom : {enterRoomPacket.Player}");

        Managers.Object.Add(enterRoomPacket.Player, myPlayer: true);
    }

    public static void S_LeaveRoomHandler(PacketSession session, IMessage packet)
    {
        S_LeaveRoom leaveRoomPacket = packet as S_LeaveRoom;

        Debug.Log($"S_LeaveRoom : ");
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;

        Debug.Log($"S_Spawn : {spawnPacket.Objects.Count}");

        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj, myPlayer: false);
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
        S_AddPokemon serverPokemonPacket = packet as S_AddPokemon;
        PokemonSummary summary = serverPokemonPacket.Summary;

        Debug.Log($"S_AddPokemon : {serverPokemonPacket.Summary}");

        GameObject player = Managers.Object.FindById(summary.Info.OwnerId);
        if (player == null)
            return;

        Pokemon pokemon = new Pokemon(summary);

        Managers.Object._pokemons.Add(pokemon);
    }

    public static void S_AccessPokemonSummaryHandler(PacketSession session, IMessage packet)
    {
        S_AccessPokemonSummary s_AccessPacket = packet as S_AccessPokemonSummary;

        Debug.Log($"S_AccessPokemonSummary : {s_AccessPacket.PkmSummary}");

        BaseScene scene = Managers.Scene.CurrentScene;
        scene.UpdateData(s_AccessPacket);
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
        int remainedHp = s_changeHpPacket.RemainedHP;

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
