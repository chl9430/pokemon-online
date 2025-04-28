using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        // Console.WriteLine($"MoveDir : {movePacket.PosInfo.MoveDir}, ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}), {movePacket.PosInfo.State}, {movePacket.AnimRate}");

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleMove, player, movePacket);
    }

    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {

    }

    public static void C_AddPokemonHandler(PacketSession session, IMessage packet)
    {
        C_AddPokemon clientPokemonPacket = packet as C_AddPokemon;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine(
            $"=====================\n" +
            $"Please add this pokemon!\n" +
            $"{clientPokemonPacket.NickName}({clientPokemonPacket.PokemonName})\n" +
            $"Owner : {clientPokemonPacket.OwnerId}\n" +
            $"Level : {clientPokemonPacket.Level}\n" +
            $"Hp : {clientPokemonPacket.Hp}\n" +
            $"Exp : {clientPokemonPacket.Exp}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(clientPokemonPacket.OwnerId);
        // PriorityQueue<Pokemon> pokemons = player.Pokemons;

        Pokemon pokemon = new Pokemon(clientPokemonPacket.NickName, clientPokemonPacket.PokemonName, clientPokemonPacket.Level, clientPokemonPacket.Hp, player);

        // 서버에 저장
        player.PushPokemon(pokemon);

        // 클라이언트에 전송
        S_AddPokemon serverPokemonPacket = new S_AddPokemon();

        PokemonInfo info = new PokemonInfo()
        {
            NickName = pokemon.NickName,
            PokemonName = pokemon.FinalStatInfo.PokemonName,
            Level = pokemon.Level,
            Hp = pokemon.Hp,
            Exp = pokemon.Exp,
            MaxExp = pokemon.MaxExp,
            Order = pokemon.Order,
            StatInfo = pokemon.FinalStatInfo,
            ObjInfo = pokemon.Owner.Info,
        };

        serverPokemonPacket.PokemonInfo = info;
        serverPokemonPacket.OwnerId = player.Id;

        player.Session.Send(serverPokemonPacket);
    }

    public static void C_ExitGameHandler(PacketSession session, IMessage packet)
    {
        C_ExitGame exitPacket = packet as C_ExitGame;
        ClientSession clientSession = session as ClientSession;

        ObjectManager.Instance.Remove(exitPacket.ObjectId);
        clientSession.Disconnect();

        Console.WriteLine($"{exitPacket.ObjectId} has gone!");
    }
}