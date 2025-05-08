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
            $"C_AddPokemon\n" +
            $"Player({clientPokemonPacket.OwnerId}) got a pokemon!\n" +
            $"{clientPokemonPacket.NickName}({clientPokemonPacket.PokemonName})\n" +
            $"Owner : {clientPokemonPacket.OwnerName}({clientPokemonPacket.OwnerId})\n" +
            $"Level : {clientPokemonPacket.Level}\n" +
            $"Gender : {clientPokemonPacket.Gender}\n" +
            $"RemainHp : {clientPokemonPacket.Hp}\n" +
            $"Nature : {clientPokemonPacket.Nature}\n" +
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
                    Hp = clientPokemonPacket.Hp > ((int)(summaryDictData.maxHp * rate)) ? ((int)(summaryDictData.maxHp * rate)) : clientPokemonPacket.Hp,
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
        player.AddPokemon(pokemon);

        // 클라이언트에 전송
        S_AddPokemon serverPokemonPacket = new S_AddPokemon();

        serverPokemonPacket.Summary = summary;

        player.Session.Send(serverPokemonPacket);
    }

    public static void C_SwitchPokemonHandler(PacketSession session, IMessage packet)
    {
        C_SwitchPokemon switchPokemonPacket = packet as C_SwitchPokemon;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_SwitchPokemon\n" +
            $"Player({switchPokemonPacket.OwnerId}) wants to switch pokemon order!\n" +
            $"Pokemon({switchPokemonPacket.PokemonFromIdx}) is going to {switchPokemonPacket.PokemonToIdx}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(switchPokemonPacket.OwnerId);

        player.SwitchPokemonOrder(switchPokemonPacket.PokemonFromIdx, switchPokemonPacket.PokemonToIdx);
    }

    public static void C_ExitGameHandler(PacketSession session, IMessage packet)
    {
        C_ExitGame exitPacket = packet as C_ExitGame;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ExitGame\n" +
            $"Player(${exitPacket.ObjectId}) has exited from game!\n" +
            $"=====================\n"
            );

        ObjectManager.Instance.Remove(exitPacket.ObjectId);
        clientSession.Disconnect();
    }

    public static void C_ReturnGameHandler(PacketSession session, IMessage packet)
    {
        C_ReturnGame returnGamePacket = packet as C_ReturnGame;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ReturnGame\n" +
            $"Player(${returnGamePacket.PlayerId}) returns to game!\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(returnGamePacket.PlayerId);
        var _players = player.Room.Players;

        S_EnterGame enterPacket = new S_EnterGame();
        enterPacket.Player = player.Info;
        player.Session.Send(enterPacket);

        S_Spawn spawnPacket = new S_Spawn();
        foreach (Player p in _players.Values)
        {
            if (player != p)
                spawnPacket.Objects.Add(p.Info);
        }
        player.Session.Send(spawnPacket);
    }

    public static void C_AccessPokemonSummaryHandler(PacketSession session, IMessage packet)
    {
        C_AccessPokemonSummary c_AccessPacket = packet as C_AccessPokemonSummary;
        
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_AccessPokemonSummary\n" +
            $"Player(${c_AccessPacket.PlayerId}) requests PokemonSummary!\n" +
            $"Please find (DictNum : {c_AccessPacket.PkmDicNum}) summary!\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(c_AccessPacket.PlayerId);

        List<Pokemon> pokemons = player.Pokemons;

        Pokemon pokemon = null;
        for (int i = 0; i < pokemons.Count; i++)
        {
            PokemonSummary summary = pokemons[i].PokemonSummary;

            if (c_AccessPacket.PkmDicNum == summary.Info.DictionaryNum)
            {
                pokemon = pokemons[i];
                break;
            }
        }

        S_AccessPokemonSummary s_AccessPacket = new S_AccessPokemonSummary();
        s_AccessPacket.PkmSummary = pokemon.PokemonSummary;

        player.Session.Send(s_AccessPacket);
    }
}