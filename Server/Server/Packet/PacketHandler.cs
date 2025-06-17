using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System.Numerics;

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
        C_CreatePlayer c_CreatePlayerPacket = packet as C_CreatePlayer;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine(
            $"=====================\n" +
            $"C_CreatePlayer\n" +
            $"{c_CreatePlayerPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Add<Player>();
        {
            player.Info.PosInfo.State = CreatureState.Idle;
            player.Info.PosInfo.MoveDir = MoveDir.Down;
            player.Info.PosInfo.PosX = 0;
            player.Info.PosInfo.PosY = 0;

            player.Session = clientSession;
        }

        player.Name = c_CreatePlayerPacket.Name;
        player.Gender = c_CreatePlayerPacket.Gender;

        clientSession.MyPlayer = player;

        GameRoom room = RoomManager.Instance.Find(1);
        room.Push(room.EnterRoom, player);
    }

    public static void C_AddPokemonHandler(PacketSession session, IMessage packet)
    {
        C_AddPokemon c_AddPokemonPacket = packet as C_AddPokemon;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine(
            $"=====================\n" +
            $"C_AddPokemon\n" +
            $"{c_AddPokemonPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(c_AddPokemonPacket.PlayerId);

        Pokemon pokemon = new Pokemon(c_AddPokemonPacket.PokemonName, c_AddPokemonPacket.NickName, c_AddPokemonPacket.Level, player.Name, c_AddPokemonPacket.PlayerId, c_AddPokemonPacket.Hp);

        // 서버에 저장
        player.AddPokemon(pokemon);

        // 클라이언트에 전송
        S_AddPokemon s_ServerPokemonPacket = new S_AddPokemon();
        s_ServerPokemonPacket.PokemonSum = pokemon.MakePokemonSummary();

        player.Session.Send(s_ServerPokemonPacket);
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
        PlayerInfo playerInfo = new PlayerInfo()
        {
            ObjectInfo = player.Info,
            PlayerName = player.Name,
            PlayerGender = player.Gender,
        };
        enterPacket.PlayerInfo = playerInfo;

        player.Session.Send(enterPacket);

        S_Spawn spawnPacket = new S_Spawn();
        foreach (Player p in _players.Values)
        {
            if (player != p)
            {
                PlayerInfo info = new PlayerInfo()
                {
                    ObjectInfo = p.Info,
                    PlayerName = p.Name,
                    PlayerGender = p.Gender,
                };

                spawnPacket.Players.Add(info);
            }
        }
        player.Session.Send(spawnPacket);
    }

    public static void C_AccessPokemonSummaryHandler(PacketSession session, IMessage packet)
    {
        C_AccessPokemonSummary c_AccessSummaryPacket = packet as C_AccessPokemonSummary;
        int playerId = c_AccessSummaryPacket.PlayerId;
        int pokemonOrder = c_AccessSummaryPacket.PokemonOrder;
        
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_AccessPokemonSummary\n" +
            $"{c_AccessSummaryPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        Pokemon pokemon = player.Pokemons[pokemonOrder];

        S_AccessPokemonSummary s_AccessPacket = new S_AccessPokemonSummary();
        s_AccessPacket.PokemonSum = pokemon.MakePokemonSummary();

        player.Session.Send(s_AccessPacket);
    }

    public static void C_EnterPokemonBattleSceneHandler(PacketSession session, IMessage packet)
    {
        C_EnterPokemonBattleScene c_EnterBattleScenePacket = packet as C_EnterPokemonBattleScene;
        int playerId = c_EnterBattleScenePacket.PlayerId;
        int locationNum = c_EnterBattleScenePacket.LocationNum;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_EnterPokemonBattleScene\n" +
            $"{c_EnterBattleScenePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        // 테스트용 플레이어
        if (player == null)
        {
            player = MakeTestPlayer(clientSession);
        }

        if (DataManager.WildPKMLocationDict.TryGetValue(locationNum, out WildPokemonAppearData[] wildPokemonDatas))
        {
            WildPokemonAppearData wildPokemonData = wildPokemonDatas[0];

            Random random = new Random();
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

            Pokemon wildPokemon = new Pokemon(wildPokemonData.pokemonName, wildPokemonData.pokemonName, wildPokemonData.pokemonLevel, player.Name, playerId);

            S_EnterPokemonBattleScene s_EnterBattleScenePacket = new S_EnterPokemonBattleScene();
            s_EnterBattleScenePacket.PlayerInfo = player.MakePlayerInfo();
            s_EnterBattleScenePacket.EnemyPokemon = wildPokemon.MakePokemonSummary();

            foreach(Pokemon pokemon in player.Pokemons)
            {
                s_EnterBattleScenePacket.PlayerPokemons.Add(pokemon.MakePokemonSummary());
            }

            player.Session.Send(s_EnterBattleScenePacket);
        }
        else
        {
            Console.WriteLine("Cannot find Location Data!");
        }
    }

    public static Player MakeTestPlayer(ClientSession clientSession)
    {
        Player player = ObjectManager.Instance.Add<Player>();
        {
            player.Info.PosInfo.State = CreatureState.Idle;
            player.Info.PosInfo.MoveDir = MoveDir.Down;
            player.Info.PosInfo.PosX = 0;
            player.Info.PosInfo.PosY = 0;

            player.Session = clientSession;
        }

        // 플레이어 정보
        player.Name = "TEST";
        player.Gender = PlayerGender.PlayerMale;

        // 플레이어 포켓몬
        player.Pokemons = new List<Pokemon>();
        player.Pokemons.Add(new Pokemon("Pikachu", "PIKAO", 5, player.Name, -1));
        player.Pokemons.Add(new Pokemon("Bulbasaur", "BUBAS", 7, player.Name, -1));
        player.Pokemons.Add(new Pokemon("Charmander", "CHAKI", 10, player.Name, -1));
        player.Pokemons.Add(new Pokemon("Squirtle", "SKIRT", 3, player.Name, -1));

        // 플레이어 아이템
        player.AddItem(ItemCategory.PokeBall, "Monster Ball", 99);
        player.AddItem(ItemCategory.PokeBall, "Great Ball", 99);
        player.AddItem(ItemCategory.PokeBall, "Ultra Ball", 99);
        player.AddItem(ItemCategory.PokeBall, "Ultra Ball", 99);
        player.AddItem(ItemCategory.PokeBall, "Ultra Ball", 99);
        player.AddItem(ItemCategory.PokeBall, "Ultra Ball", 99);
        player.AddItem(ItemCategory.PokeBall, "Ultra Ball", 99);
        player.AddItem(ItemCategory.PokeBall, "Monster Ball", 45);
        player.AddItem(ItemCategory.PokeBall, "Great Ball", 78);
        player.AddItem(ItemCategory.PokeBall, "Great Ball", 34);
        player.AddItem(ItemCategory.PokeBall, "Great Ball", 10);
        player.AddItem(ItemCategory.PokeBall, "Great Ball", 10);
        player.AddItem(ItemCategory.PokeBall, "Great Ball", 50);

        clientSession.MyPlayer = player;

        return player;
    }

    public static void C_EnterPlayerBagSceneHandler(PacketSession session, IMessage packet)
    {
        C_EnterPlayerBagScene c_EnterBagScenePacket = packet as C_EnterPlayerBagScene;
        int playerId = c_EnterBagScenePacket.PlayerId;
        ItemCategory itemCategory = c_EnterBagScenePacket.ItemCategory;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_EnterPlayerBagScene\n" +
            $"{c_EnterBagScenePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        // 테스트용 플레이어
        if (player == null)
        {
            player = MakeTestPlayer(clientSession);
        }

        S_EnterPlayerBagScene s_EnterBagScenePacket = new S_EnterPlayerBagScene();
        s_EnterBagScenePacket.PlayerInfo = player.MakePlayerInfo();

        foreach (var pair in player.Items)
        {
            CategoryInventory categoryInventory = new CategoryInventory();

            foreach (Item item in pair.Value)
            {
                categoryInventory.CategoryItemSums.Add(item.MakeItemSummary());
            }

            s_EnterBagScenePacket.Inventory.Add((int)pair.Key, categoryInventory);
        }

        player.Session.Send(s_EnterBagScenePacket);
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
            $"{c_UseMovePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PokemonMove usedMove = player.Pokemons[pokemonOrder].PokemonMoves[moveOrder];

        usedMove.CurPP -= usedPP;

        S_UsePokemonMove s_UseMovePacket = new S_UsePokemonMove();
        s_UseMovePacket.RemainedPP = usedMove.CurPP;

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
        int pokemonOrder = c_ChangeHPPacket.PokemonOrder;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ChangePokemonHp\n" +
            $"{c_ChangeHPPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        Pokemon pokemon = null;

        // 방어 포켓몬이 야생 포켓몬이라면
        if (pokemonOrder != -1)
        {
            pokemon = player.Pokemons[pokemonOrder];
        }

        // 데미지 계산
        int finalDamage = CalFinalDamage(moveCategory, attackPKMInfo.Level, attackPKMStat, defensePKMStat, movePower);

        S_ChangePokemonHp s_ChangeHPPacket = new S_ChangePokemonHp();

        // 방어 포켓몬이 내 포켓몬이라면
        if (pokemon != null)
        {
            pokemon.GetDamage(finalDamage);
            s_ChangeHPPacket.RemainedHp = pokemon.PokemonStat.Hp;
        }
        else // 방어 포켓몬이 야생 포켓몬이라면
        {
            defensePKMStat.Hp -= finalDamage;

            if (defensePKMStat.Hp < 0)
                defensePKMStat.Hp = 0;

            s_ChangeHPPacket.RemainedHp = defensePKMStat.Hp;
        }

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
        s_ChangeExpPacket.PokemonExpInfo = pokemon.ExpInfo;

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
        Pokemon pokemon = player.Pokemons[pokemonOrder];

        LevelUpStatusDiff levelUpDiff = pokemon.LevelUp();

        S_ChangePokemonLevel changeLevelPacket = new S_ChangePokemonLevel();
        changeLevelPacket.PokemonLevel = pokemon.PokemonInfo.Level;
        changeLevelPacket.PokemonStat = pokemon.PokemonStat;
        changeLevelPacket.StatDiff = levelUpDiff;
        changeLevelPacket.PokemonRemainLevelExp = pokemon.ExpInfo.RemainExpToNextLevel;
        changeLevelPacket.PokemonCurExp = pokemon.ExpInfo.CurExp;

        player.Session.Send(changeLevelPacket);
    }



    static int CalFinalDamage(MoveCategory moveCategory, int attackPKMLevel, PokemonStat attackPKMStat, PokemonStat defensePKMStat, int movePower)
    {
        int finalDamage = 0;

        if (moveCategory == MoveCategory.Physical)
            finalDamage = (int)((
            ((((float)attackPKMLevel) * 2f / 5f) + 2f)
            * ((float)movePower)
            * ((float)attackPKMStat.Attack) / ((float)defensePKMStat.Defense)
            ) / 50f + 2f);
        else if (moveCategory == MoveCategory.Special)
            finalDamage = (int)((
            ((((float)attackPKMLevel) * 2f / 5f) + 2f)
            * ((float)movePower)
            * ((float)attackPKMStat.SpecialAttack) / ((float)defensePKMStat.SpecialDefense)
            ) / 50f + 2f);

        if (finalDamage <= 0)
            finalDamage = 1;

        return finalDamage;
    }
}