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

        // Console.WriteLine($"MoveDir : {movePacket.PosInfo.MoveDir}, ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

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

        Player player = null;

        if (c_CreatePlayerPacket.Name == "TEST")
        {
            player = MakeTestPlayer(clientSession);
        }
        else
        {
            player = ObjectManager.Instance.Add<Player>();
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
        }

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
            $"{switchPokemonPacket}\n" +
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
        C_ReturnGame c_returnGamePacket = packet as C_ReturnGame;
        int playerId = c_returnGamePacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ReturnGame\n" +
            $"{c_returnGamePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        player.PosInfo.State = CreatureState.Idle;
        var players = player.Room.Players;

        S_EnterRoom enterPacket = new S_EnterRoom();
        enterPacket.PlayerInfo = player.MakePlayerInfo();

        player.Session.Send(enterPacket);

        S_Spawn spawnPacket = new S_Spawn();
        foreach (Player p in players.Values)
        {
            if (player != p)
            {
                spawnPacket.Players.Add(p.MakePlayerInfo());
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

    public static void C_EnterPokemonListSceneHandler(PacketSession session, IMessage packet)
    {
        C_EnterPokemonListScene c_enterListScenePacket = packet as C_EnterPokemonListScene;
        int playerId = c_enterListScenePacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_EnterPokemonListScene\n" +
            $"{c_enterListScenePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        if (player == null)
            player = MakeTestPlayer(clientSession);

        S_EnterPokemonListScene s_enterListScenePacket = new S_EnterPokemonListScene();
        s_enterListScenePacket.PlayerInfo = player.MakePlayerInfo();

        foreach (Pokemon pokemon in player.Pokemons)
        {
            s_enterListScenePacket.PokemonSums.Add(pokemon.MakePokemonSummary());
        }

        player.Session.Send(s_enterListScenePacket);
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

        player.Info.PosInfo.State = CreatureState.Fight;
        player.BattleRoom = new PrivateBattleRoom(player);
        player.BattleRoom.MakeWildPokemon(locationNum);
        player.BattleRoom.MyPokemon = player.Pokemons[0];

        S_EnterPokemonBattleScene s_EnterBattleScenePacket = new S_EnterPokemonBattleScene();
        s_EnterBattleScenePacket.PlayerInfo = player.MakePlayerInfo();
        s_EnterBattleScenePacket.EnemyPokemon = player.BattleRoom.WildPokemon.MakePokemonSummary();

        foreach (Pokemon pokemon in player.Pokemons)
        {
            s_EnterBattleScenePacket.PlayerPokemons.Add(pokemon.MakePokemonSummary());
        }

        player.Session.Send(s_EnterBattleScenePacket);
    }

    public static void C_UseItemHandler(PacketSession session, IMessage packet)
    {
        C_UseItem c_useItemPacket = packet as C_UseItem;
        int playerId = c_useItemPacket.PlayerId;
        ItemCategory itemCategory = c_useItemPacket.ItemCategory;
        int itemOrder = c_useItemPacket.ItemOrder;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_UseItem\n" +
            $"{c_useItemPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PrivateBattleRoom battleRoom = player.BattleRoom;

        S_UseItem s_useItemPacket = new S_UseItem();
        s_useItemPacket.PlayerInfo = player.MakePlayerInfo();
        s_useItemPacket.EnemyPokemon = battleRoom.WildPokemon.MakePokemonSummary();

        foreach (Pokemon pokemon in player.Pokemons)
        {
            s_useItemPacket.PlayerPokemons.Add(pokemon.MakePokemonSummary());
        }

        s_useItemPacket.UsedItem = player.UseItem(itemCategory, itemOrder).MakeItemSummary();

        player.Session.Send(s_useItemPacket);
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
        player.Gender = PlayerGender.PlayerFemale;

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
        PrivateBattleRoom battleRoom = player.BattleRoom;

        S_UsePokemonMove s_UseMovePacket = new S_UsePokemonMove();

        if (pokemonOrder == -1)
            s_UseMovePacket.RemainedPP = battleRoom.UseWildPokemonMove(moveOrder, usedPP);
        else
            s_UseMovePacket.RemainedPP = battleRoom.UseMyPokemonMove(moveOrder, usedPP);

        player.Session.Send(s_UseMovePacket);
    }

    public static void C_ChangePokemonHpHandler(PacketSession session, IMessage packet)
    {
        C_ChangePokemonHp c_ChangeHPPacket = packet as C_ChangePokemonHp;
        ClientSession clientSession = session as ClientSession;

        MoveCategory moveCategory = c_ChangeHPPacket.MoveCategory;
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
        PrivateBattleRoom battleRoom = player.BattleRoom;

        S_ChangePokemonHp s_ChangeHPPacket = new S_ChangePokemonHp();

        if (pokemonOrder == -1)
            s_ChangeHPPacket.RemainedHp = battleRoom.ChangeWildPokemonHp(moveCategory, movePower);
        else
            s_ChangeHPPacket.RemainedHp = battleRoom.ChangeMyPokemonHp(moveCategory, movePower);

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

        Player player = ObjectManager.Instance.Find(playerId);
        
        S_GetEnemyPokemonExp s_GetExpPacket = new S_GetEnemyPokemonExp();
        s_GetExpPacket.Exp = player.BattleRoom.GetExp();

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
}