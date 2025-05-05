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
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(clientPokemonPacket.OwnerId);
        // PriorityQueue<Pokemon> pokemons = player.Pokemons;

        PokemonSummaryDictData summaryDictData;

        PokemonSummary summary = new PokemonSummary();
        PokemonInfo info;
        PokemonSkill skill;
        PokemonBattleMove battleMove;

        if (DataManager.PokemonSummaryDict.TryGetValue(clientPokemonPacket.PokemonName, out summaryDictData))
        {
            info = new PokemonInfo()
            {
                DictionaryNum = summaryDictData.dictionaryNum,
                NickName = clientPokemonPacket.NickName,
                PokemonName = summaryDictData.pokemonName,
                Level = clientPokemonPacket.Level,
                Gender = clientPokemonPacket.Gender,

                OwnerName = clientPokemonPacket.OwnerName,
                OwnerId = clientPokemonPacket.OwnerId,
                Type1 = (PokemonType)Enum.Parse(typeof(PokemonType), summaryDictData.type1),
                Type2 = (PokemonType)Enum.Parse(typeof(PokemonType), summaryDictData.type2),

                Nature = clientPokemonPacket.Nature,
                MetLevel = clientPokemonPacket.Level,
            };

            float rate = clientPokemonPacket.Level / 10.0f;
            int totalExp = 0;

            for (int i = 1; i <= clientPokemonPacket.Level; i++)
            {
                totalExp += (i - 1) * 100;
            }

            skill = new PokemonSkill()
            {
                Stat = new PokemonStat()
                {
                    Hp = clientPokemonPacket.Hp,
                    MaxHp = (int)(summaryDictData.maxHp * rate),
                    Attack = (int)(summaryDictData.attack * rate),
                    Defense = (int)(summaryDictData.defense * rate),
                    SpecialAttack = (int)(summaryDictData.specialAttack * rate),
                    SpecialDefense = (int)(summaryDictData.specialAttack * rate),
                    Speed = (int)(summaryDictData.speed * rate),
                },
                TotalExp = totalExp,
                RemainLevelExp = clientPokemonPacket.Level * 100,
            };

            battleMove = new PokemonBattleMove()
            {

            };
        }
        else
        {
            Console.WriteLine("Cannot find Pokemon Base Stat!");
            return;
        }

        summary.Info = info;
        summary.Skill = skill;
        summary.BattleMove = battleMove;

        Pokemon pokemon = new Pokemon(info, skill, battleMove);

        // 서버에 저장
        player.PushPokemon(pokemon);

        // 클라이언트에 전송
        S_AddPokemon serverPokemonPacket = new S_AddPokemon();

        serverPokemonPacket.Summary = summary;

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