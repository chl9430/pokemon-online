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

        Debug.Log($"S_EnterGame : {enterGamePacket.Player}");
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame enterGamePacket = packet as S_LeaveGame;
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

    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer playerPacket = packet as S_CreatePlayer;

        Debug.Log($"S_CreatePlayer : {playerPacket.Player}");

        Managers.Object.Add(playerPacket.Player, myPlayer: true);
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {

    }

    public static void S_DieHandler(PacketSession session, IMessage packet)
    {

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

        PokemonSummaryScene scene = Managers.Scene.CurrentScene as PokemonSummaryScene;
        scene.UpdateData(s_AccessPacket);
    }
}
