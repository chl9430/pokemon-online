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

    public static void C_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        C_CreatePlayer playerPacket = packet as C_CreatePlayer;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine(
            $"=====================\n" +
            $"C_CreatePlayer\n" +
            $"Please make new player!\n" +
            $"Name : {playerPacket.Name}, Gender : {playerPacket.Gender}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Add<Player>();
        {
            player.Info.Name = $"{playerPacket.Name}({player.Info.ObjectId})";
            player.Info.Gender = playerPacket.Gender;
            player.Info.PosInfo.State = CreatureState.Idle;
            player.Info.PosInfo.MoveDir = MoveDir.Down;
            player.Info.PosInfo.PosX = 0;
            player.Info.PosInfo.PosY = 0;

            player.Session = clientSession;
        }

        clientSession.MyPlayer = player;

        GameRoom room = RoomManager.Instance.Find(1);
        room.Push(room.EnterRoom, player);
    }

    public static void C_AddPokemonHandler(PacketSession session, IMessage packet)
    {
        C_AddPokemon clientPokemonPacket = packet as C_AddPokemon;
        ClientSession clientSession = session as ClientSession;

        ObjectInfo playerInfo = clientPokemonPacket.PlayerInfo;

        Console.WriteLine(
            $"=====================\n" +
            $"C_AddPokemon\n" +
            $"Player({clientPokemonPacket.PlayerInfo.Name}, {clientPokemonPacket.PlayerInfo.ObjectId}, {clientPokemonPacket.PlayerInfo.Gender}, {clientPokemonPacket.PlayerInfo.PosInfo}) got a pokemon!\n" +
            $"{clientPokemonPacket.NickName}({clientPokemonPacket.PokemonName})\n" +
            $"Level : {clientPokemonPacket.Level}\n" +
            $"Gender : {clientPokemonPacket.Gender}\n" +
            $"RemainHp : {clientPokemonPacket.Hp}\n" +
            $"Nature : {clientPokemonPacket.Nature}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerInfo.ObjectId);
        
        PokemonSummary summary;

        if (DataManager.PokemonSummaryDict.TryGetValue(clientPokemonPacket.PokemonName, out PokemonSummaryDictData summaryDictData))
        {
            summary = MakePokemonSummary(summaryDictData, clientPokemonPacket.Level, clientPokemonPacket.Gender, playerInfo.Name, playerInfo.ObjectId, clientPokemonPacket.Nature);
        }
        else
        {
            Console.WriteLine("Cannot find Pokemon Base Stat!");
            return;
        }

        LearnMoveData[] learnMoveDatas = summaryDictData.moves;
        string[] moves = new string[4];

        int foundIdx = FindLastLearnableMoveIndex(learnMoveDatas, clientPokemonPacket.Level);
        int moveIdxToFill = 0;
        int fillCnt = 0;

        for (int i = moves.Length - 1; i >= 0; i--)
        {
            int idx = foundIdx - i;

            if (idx < 0)
                idx += (moves.Length - 1 - foundIdx);

            if (fillCnt <= foundIdx)
                moves[moveIdxToFill] = learnMoveDatas[idx].moveName;
            else
                moves[moveIdxToFill] = "";

            fillCnt++;
            moveIdxToFill++;
        }

        for (int i = 0; i < moves.Length; i++)
        {
            if (DataManager.PokemonMoveDict.TryGetValue(moves[i], out PokemonMoveDictData moveDictData))
            {
                PokemonBattleMove battleMove = new PokemonBattleMove()
                {
                    MoveName = moveDictData.moveName,
                    MovePower = moveDictData.movePower,
                    MoveAccuracy = moveDictData.moveAccuracy,
                    CurPP = moveDictData.maxPP,
                    MaxPP = moveDictData.maxPP,
                    MoveType = moveDictData.moveType,
                    MoveCategory = moveDictData.moveCategory,
                };

                summary.BattleMoves.Add(battleMove);
            }
            else
            {
                continue;
            }
        }

        Pokemon pokemon = new Pokemon(summary);

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

        S_EnterRoom enterPacket = new S_EnterRoom();
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

    public static void C_MeetWildPokemonHandler(PacketSession session, IMessage packet)
    {
        C_MeetWildPokemon c_MeetPacket = packet as C_MeetWildPokemon;

        ClientSession clientSession = session as ClientSession;

        ObjectInfo playerInfo = c_MeetPacket.PlayerInfo;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_MeetWildPokemon\n" +
            $"Player({c_MeetPacket.PlayerInfo.Name}, {c_MeetPacket.PlayerInfo.ObjectId}, {c_MeetPacket.PlayerInfo.Gender}, {c_MeetPacket.PlayerInfo.PosInfo}) meet wild pokemon in location(number : {c_MeetPacket.LocationNum})!\n" +
            $"=====================\n"
            );

        WildPokemonAppearData[] wildPokemonDatas = DataManager.WildPKMLocationDict[c_MeetPacket.LocationNum];
        WildPokemonAppearData wildPokemonData = wildPokemonDatas[0];
        Random random = new Random();

        if (wildPokemonDatas != null)
        {
            int ran = random.Next(100);

            int totalRateCnt = 0;

            for (int i = 0; i < wildPokemonDatas.Length; i++)
            {
                int rateCnt = 0;
                int appearRate = wildPokemonDatas[i].appearRate;

                bool found = false;

                while (rateCnt < appearRate)
                {
                    if (totalRateCnt != ran)
                    {
                        totalRateCnt++;
                        rateCnt++;
                    }
                    else
                    {
                        wildPokemonData = wildPokemonDatas[i];
                        found = true;
                        break;
                    }
                }

                if (found)
                    break;
            }
        }
        else
        {
            Console.WriteLine($"Cannot find location(number : {c_MeetPacket.LocationNum})!");
        }

        PokemonSummaryDictData summaryDictData = DataManager.PokemonSummaryDict[wildPokemonData.pokemonName];
        GenderRatioData[] genderRatioDatas = summaryDictData.genderRatio;
        GenderRatioData foundGenderData = genderRatioDatas[0];

        if (genderRatioDatas != null)
        {
            float ran = (float)(random.NextDouble() * 100.0f);

            float totalRateCnt = 0;

            for (int i = 0; i < genderRatioDatas.Length; i++)
            {
                float rateCnt = 0;
                float genderRatio = genderRatioDatas[i].ratio;

                bool found = false;

                while (rateCnt < genderRatio)
                {
                    if (totalRateCnt != ran)
                    {
                        totalRateCnt += 0.1f;
                        rateCnt += 0.1f;
                    }
                    else
                    {
                        foundGenderData = genderRatioDatas[i];
                        found = true;
                        break;
                    }
                }

                if (found)
                    break;
            }
        }
        else
        {
            Console.WriteLine($"Cannot find location(number : {c_MeetPacket.LocationNum})!");
        }

        PokemonGender foundGender = (PokemonGender)Enum.Parse(typeof(PokemonGender), foundGenderData.gender);

        PokemonNature[] allNatures = (PokemonNature[])Enum.GetValues(typeof(PokemonNature));
        int randomNatureIndex = random.Next(0, allNatures.Length);
        PokemonNature randomNature = allNatures[randomNatureIndex];

        PokemonSummary summary = MakePokemonSummary(summaryDictData, wildPokemonData.pokemonLevel, foundGender, playerInfo.Name, playerInfo.ObjectId, randomNature);

        LearnMoveData[] learnMoveDatas = summaryDictData.moves;
        string[] moves = new string[4];

        int foundIdx = FindLastLearnableMoveIndex(learnMoveDatas, wildPokemonData.pokemonLevel);
        int moveIdxToFill = 0;
        int fillCnt = 0;

        for (int i = moves.Length - 1; i >= 0; i--)
        {
            int idx = foundIdx - i; // 5 - 3 = 2

            if (idx < 0)
                idx += (moves.Length - 1 - foundIdx);

            if (fillCnt <= foundIdx)
                moves[moveIdxToFill] = learnMoveDatas[idx].moveName;
            else
                moves[moveIdxToFill] = "";

            fillCnt++;
            moveIdxToFill++;
        }

        for (int i = 0; i < moves.Length; i++)
        {
            if (DataManager.PokemonMoveDict.TryGetValue(moves[i], out PokemonMoveDictData moveDictData))
            {
                PokemonBattleMove battleMove = new PokemonBattleMove()
                {
                    MoveName = moveDictData.moveName,
                    MovePower = moveDictData.movePower,
                    MoveAccuracy = moveDictData.moveAccuracy,
                    CurPP = moveDictData.maxPP,
                    MaxPP = moveDictData.maxPP,
                    MoveType = moveDictData.moveType,
                    MoveCategory = moveDictData.moveCategory,
                };

                summary.BattleMoves.Add(battleMove);
            }
            else
            {
                continue;
            }
        }

        Player player = ObjectManager.Instance.Find(playerInfo.ObjectId);
        player.Info = playerInfo;

        S_AccessPokemonSummary s_SummaryPacket = new S_AccessPokemonSummary();
        s_SummaryPacket.PkmSummary = summary;
        s_SummaryPacket.PlayerInfo = player.Info;

        player.Session.Send(s_SummaryPacket);
    }

    public static void C_UsePokemonMoveHandler(PacketSession session, IMessage packet)
    {
        C_UsePokemonMove c_UseMovePacket = packet as C_UsePokemonMove;
        int playerId = c_UseMovePacket.PlayerId;
        int pokemonOrder = c_UseMovePacket.PokemonOrder;
        int moveOrder = c_UseMovePacket.MoveOrder;
        int usedPP = c_UseMovePacket.UsedPP;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_UsePokemonMove\n" +
            $"Player(ID : {playerId})'s pokemon(Order : {pokemonOrder}) will use a skill(Order : {moveOrder}) with its ({usedPP})PP!\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PokemonBattleMove move = player.Pokemons[pokemonOrder].PokemonSummary.BattleMoves[moveOrder];

        move.CurPP -= usedPP;

        S_UsePokemonMove s_UseMovePacket = new S_UsePokemonMove();
        s_UseMovePacket.RemainedPP = move.CurPP;

        player.Session.Send(s_UseMovePacket);
    }

    public static void C_ChangePokemonHpHandler(PacketSession session, IMessage packet)
    {
        C_ChangePokemonHp c_ChangeHPPacket = packet as C_ChangePokemonHp;
        ClientSession clientSession = session as ClientSession;

        MoveCategory moveCategory = c_ChangeHPPacket.MoveCategory;
        PokemonInfo attackPKMInfo = c_ChangeHPPacket.AttackPKMInfo;
        PokemonInfo defensePKMInfo = c_ChangeHPPacket.DefensePKMInfo;
        PokemonStat attackPKMStat = c_ChangeHPPacket.AttackPKMStat;
        PokemonStat defensePKMStat = c_ChangeHPPacket.DefensePKMStat;
        int movePower = c_ChangeHPPacket.MovePower;
        int playerId = c_ChangeHPPacket.PlayerId;
        int pokemonOrder = c_ChangeHPPacket .PokemonOrder;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ChangePokemonHp\n" +
            $"AttackPKMStat(Level : {attackPKMInfo.Level}, Attack : {attackPKMStat.Attack}, SP.Attack : {attackPKMStat.SpecialAttack}) => DefensePKMStat(Level : {defensePKMInfo.Level}, Defense : {defensePKMStat.Defense}, SP.Defense : {defensePKMStat.SpecialDefense})\n" +
            $"Player(ID : {playerId})'s pokemon(Order : {pokemonOrder}) got hit by move(Category: {moveCategory}, Power: {movePower})\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PokemonStat pokemonStat;

        if (pokemonOrder == -1)
        {
            pokemonStat = defensePKMStat;
        }
        else
        {
            pokemonStat = player.Pokemons[pokemonOrder].PokemonSummary.Skill.Stat;
        }

        // 데미지 계산
        int finalDamage = 0;

        if (moveCategory == MoveCategory.Physical)
            finalDamage = (int)((
            ((((float)attackPKMInfo.Level) * 2f / 5f) + 2f)
            * ((float)movePower)
            * ((float)attackPKMStat.Attack) / ((float)defensePKMStat.Defense)
            ) / 50f + 2f);
        else if (moveCategory == MoveCategory.Special)
            finalDamage = (int)((
            ((((float)attackPKMInfo.Level) * 2f / 5f) + 2f)
            * ((float)movePower)
            * ((float)attackPKMStat.SpecialAttack) / ((float)defensePKMStat.SpecialDefense)
            ) / 50f + 2f);

        if (finalDamage <= 0)
            finalDamage = 1;

        pokemonStat.Hp -= finalDamage;

        if (pokemonStat.Hp < 0) 
            pokemonStat.Hp = 0;

        S_ChangePokemonHp s_ChangeHPPacket = new S_ChangePokemonHp();
        s_ChangeHPPacket.RemainedHP = pokemonStat.Hp;

        player.Session.Send(s_ChangeHPPacket);
    }

    public static void C_GetEnemyPokemonExpHandler(PacketSession session, IMessage packet)
    {
        C_GetEnemyPokemonExp c_GetExpPacket = packet as C_GetEnemyPokemonExp;
        int playerId = c_GetExpPacket.PlayerId;
        PokemonInfo enemyPokemonInfo = c_GetExpPacket.EnemyPokemonInfo;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_GetEnemyPokemonExp\n" +
            $"{c_GetExpPacket}\n" +
            $"=====================\n"
            );

        int exp = (112 * enemyPokemonInfo.Level) / 7;

        Player player = ObjectManager.Instance.Find(playerId);

        S_GetEnemyPokemonExp s_GetExpPacket = new S_GetEnemyPokemonExp();
        s_GetExpPacket.Exp = exp;

        player.Session.Send(s_GetExpPacket);
    }

    public static void C_ChangePokemonExpHandler(PacketSession session, IMessage packet)
    {
        C_ChangePokemonExp c_ChangeExpPacket = packet as C_ChangePokemonExp;
        int playerId = c_ChangeExpPacket.PlayerId;
        int pokemonOrder = c_ChangeExpPacket.PokemonOrder;
        int exp = c_ChangeExpPacket.Exp;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ChangePokemonExp\n" +
            $"{c_ChangeExpPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        Pokemon pokemon = player.Pokemons[pokemonOrder];
        pokemon.GetExp(exp);

        S_ChangePokemonExp s_ChangeExpPacket = new S_ChangePokemonExp();
        s_ChangeExpPacket.PokemonTotalExp = pokemon.PokemonSkill.TotalExp;
        s_ChangeExpPacket.PokemonRemainLevelExp = pokemon.PokemonSkill.RemainLevelExp;
        s_ChangeExpPacket.PokemonCurExp = pokemon.PokemonSkill.CurExp;

        player.Session.Send(s_ChangeExpPacket);
    }

    public static void C_ChangePokemonLevelHandler(PacketSession session, IMessage packet)
    {
        C_ChangePokemonLevel c_ChangeLevelPacket = packet as C_ChangePokemonLevel;
        int playerId = c_ChangeLevelPacket.PlayerId;
        int pokemonOrder = c_ChangeLevelPacket.PokemonOrder;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ChangePokemonLevel\n" +
            $"{c_ChangeLevelPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PokemonInfo pokemonInfo = player.Pokemons[pokemonOrder].PokemonSummary.Info;
        PokemonStat pokemonStat = player.Pokemons[pokemonOrder].PokemonSummary.Skill.Stat;

        PokemonStat prevStat = new PokemonStat()
        {
            Hp = pokemonStat.Hp,
            MaxHp = pokemonStat.MaxHp,
            Attack = pokemonStat.Attack,
            Defense = pokemonStat.Defense,
            SpecialAttack = pokemonStat.SpecialAttack,
            SpecialDefense = pokemonStat.SpecialDefense,
            Speed = pokemonStat.Speed,
        };

        player.Pokemons[pokemonOrder].LevelUp();

        if (DataManager.PokemonSummaryDict.TryGetValue(pokemonInfo.PokemonName, out PokemonSummaryDictData summaryDictData))
        {
            pokemonStat.MaxHp = CalPokemonStat(true, summaryDictData.maxHp, pokemonInfo.Level);
            pokemonStat.Attack = CalPokemonStat(false, summaryDictData.attack, pokemonInfo.Level);
            pokemonStat.Defense = CalPokemonStat(false, summaryDictData.defense, pokemonInfo.Level);
            pokemonStat.SpecialAttack = CalPokemonStat(false, summaryDictData.specialAttack, pokemonInfo.Level);
            pokemonStat.SpecialDefense = CalPokemonStat(false, summaryDictData.specialDefense, pokemonInfo.Level);
            pokemonStat.Speed = CalPokemonStat(false, summaryDictData.speed, pokemonInfo.Level);
        }
        else
        {
            Console.WriteLine($"Cannot find {pokemonInfo.PokemonName}'s base stat!");
        }


        LevelUpStatusDiff diff = new LevelUpStatusDiff()
        {
            MaxHP = pokemonStat.MaxHp - prevStat.MaxHp,
            Attack = pokemonStat.Attack - prevStat.Attack,
            Defense = pokemonStat.Defense - prevStat.Defense,
            SpecialAttack = pokemonStat.SpecialAttack - prevStat.SpecialAttack,
            SpecialDefense = pokemonStat.SpecialDefense - prevStat.SpecialDefense,
            Speed = pokemonStat.Speed - prevStat.Speed,
        };

        pokemonStat.Hp += diff.MaxHP;

        S_ChangePokemonLevel changeLevelPacket = new S_ChangePokemonLevel();
        changeLevelPacket.PokemonStat = pokemonStat;
        changeLevelPacket.PokemonLevel = pokemonInfo.Level;
        changeLevelPacket.StatDiff = diff;
        changeLevelPacket.PokemonRemainLevelExp = player.Pokemons[pokemonOrder].PokemonSkill.RemainLevelExp;
        changeLevelPacket.PokemonCurExp = player.Pokemons[pokemonOrder].PokemonSkill.CurExp;

        player.Session.Send(changeLevelPacket);
    }



    static int FindLastLearnableMoveIndex(LearnMoveData[] moveDatas, int pokemonLevel)
    {
        int low = 0;
        int high = moveDatas.Length - 1;
        int resultIndex = -1;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;

            if (moveDatas[mid].learnLevel <= pokemonLevel)
            {
                resultIndex = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        return resultIndex;
    }

    static PokemonSummary MakePokemonSummary(PokemonSummaryDictData summaryDictData, int level, PokemonGender gender, string ownerName, int ownerId, PokemonNature nature)
    {
        int prevLevelTotalExp = 0;
        int nextLevelTotalExp = 0;

        if (level > 1)
            prevLevelTotalExp = (int)Math.Pow(level - 1, 3);

        if (level < 100)
            nextLevelTotalExp = (int)Math.Pow(level, 3);

        PokemonSummary summary = new PokemonSummary();
        PokemonInfo info = new PokemonInfo()
        {
            DictionaryNum = summaryDictData.dictionaryNum,
            NickName = summaryDictData.pokemonName,
            PokemonName = summaryDictData.pokemonName,
            Level = level,

            Gender = gender,

            OwnerName = ownerName,
            OwnerId = ownerId,
            Type1 = (PokemonType)Enum.Parse(typeof(PokemonType), summaryDictData.type1),
            Type2 = (PokemonType)Enum.Parse(typeof(PokemonType), summaryDictData.type2),

            Nature = nature,
            MetLevel = level,
        };
        PokemonSkill skill = new PokemonSkill()
        {
            Stat = new PokemonStat()
            {
                Hp = CalPokemonStat(true, summaryDictData.maxHp, level),
                MaxHp = CalPokemonStat(true, summaryDictData.maxHp, level),
                Attack = CalPokemonStat(false, summaryDictData.attack, level),
                Defense = CalPokemonStat(false, summaryDictData.defense, level),
                SpecialAttack = CalPokemonStat(false, summaryDictData.specialAttack, level),
                SpecialDefense = CalPokemonStat(false, summaryDictData.specialDefense, level),
                Speed = CalPokemonStat(false, summaryDictData.speed, level),
            },
            TotalExp = prevLevelTotalExp,
            RemainLevelExp = nextLevelTotalExp - prevLevelTotalExp,
        };

        summary.Info = info;
        summary.Skill = skill;

        return summary;
    }

    static int CalPokemonStat(bool isHP, int baseStat, int level)
    {
        int stat = 0;

        if (isHP)
            stat = (int)(((((float)baseStat) * 2f) * ((float)level) / 100f) + 10f + ((float)level));
        else
            stat = (int)(((((float)baseStat) * 2f) * ((float)level) / 100f) + 5f);

        return stat;
    }
}