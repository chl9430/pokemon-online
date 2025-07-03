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
        s_EnterBattleScenePacket.EnemyPokemonSum = player.BattleRoom.WildPokemon.MakePokemonSummary();
        s_EnterBattleScenePacket.PlayerPokemonSum = player.BattleRoom.MyPokemon.MakePokemonSummary();

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
        s_useItemPacket.EnemyPokemonSum = battleRoom.WildPokemon.MakePokemonSummary();
        s_useItemPacket.PlayerPokemonSum = battleRoom.MyPokemon.MakePokemonSummary();

        s_useItemPacket.UsedItem = player.UseItem(itemCategory, itemOrder).MakeItemSummary();

        player.Session.Send(s_useItemPacket);
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
        int myMoveOrder = c_UseMovePacket.MyMoveOrder;
        int enemyMoveOrder = c_UseMovePacket.EnemyMoveOrder;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_UsePokemonMove\n" +
            $"{c_UseMovePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PrivateBattleRoom battleRoom = player.BattleRoom;
        battleRoom.UseBattlePokemonMove(myMoveOrder, enemyMoveOrder);

        S_UsePokemonMove s_UseMovePacket = new S_UsePokemonMove();
        s_UseMovePacket.MyRemainedPP = battleRoom.MyPokemon.SelectedMove.CurPP;
        s_UseMovePacket.EnemyRemainedPP = battleRoom.WildPokemon.SelectedMove.CurPP;

        player.Session.Send(s_UseMovePacket);
    }

    public static void C_ChangePokemonHpHandler(PacketSession session, IMessage packet)
    {
        C_ChangePokemonHp c_ChangeHPPacket = packet as C_ChangePokemonHp;
        int playerId = c_ChangeHPPacket.PlayerId;
        bool isMyPokemon = c_ChangeHPPacket.IsMyPokemon;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ChangePokemonHp\n" +
            $"{c_ChangeHPPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PrivateBattleRoom battleRoom = player.BattleRoom;
        battleRoom.ChangeBattlePokemonHp(isMyPokemon);

        S_ChangePokemonHp s_ChangeHPPacket = new S_ChangePokemonHp();

        if (isMyPokemon)
            s_ChangeHPPacket.RemainedHp = battleRoom.MyPokemon.PokemonStat.Hp;
        else
            s_ChangeHPPacket.RemainedHp = battleRoom.WildPokemon.PokemonStat.Hp;

        player.Session.Send(s_ChangeHPPacket);
    }

    public static void C_GetEnemyPokemonExpHandler(PacketSession session, IMessage packet)
    {
        C_GetEnemyPokemonExp c_GetExpPacket = packet as C_GetEnemyPokemonExp;
        int playerId = c_GetExpPacket.PlayerId;

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
        int exp = c_ChangeExpPacket.Exp;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ChangePokemonExp\n" +
            $"{c_ChangeExpPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        Pokemon pokemon = player.BattleRoom.MyPokemon;
        pokemon.GetExp(exp);

        S_ChangePokemonExp s_ChangeExpPacket = new S_ChangePokemonExp();
        s_ChangeExpPacket.PokemonExpInfo = pokemon.ExpInfo;

        player.Session.Send(s_ChangeExpPacket);
    }

    public static void C_ChangePokemonLevelHandler(PacketSession session, IMessage packet)
    {
        C_ChangePokemonLevel c_ChangeLevelPacket = packet as C_ChangePokemonLevel;
        int playerId = c_ChangeLevelPacket.PlayerId;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ChangePokemonLevel\n" +
            $"{c_ChangeLevelPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        Pokemon pokemon = player.BattleRoom.MyPokemon;

        LevelUpStatusDiff levelUpInfo = pokemon.LevelUp();

        S_ChangePokemonLevel changeLevelPacket = new S_ChangePokemonLevel();
        changeLevelPacket.PokemonLevel = pokemon.PokemonInfo.Level;
        changeLevelPacket.PokemonStat = pokemon.PokemonStat;
        changeLevelPacket.PokemonExp = pokemon.ExpInfo;
        changeLevelPacket.StatDiff = levelUpInfo;

        player.Session.Send(changeLevelPacket);
    }

    public static void C_CheckNewLearnableMoveHandler(PacketSession session, IMessage packet)
    {
        C_CheckNewLearnableMove c_CheckMovePacket = packet as C_CheckNewLearnableMove;
        int playerId = c_CheckMovePacket.PlayerId;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_CheckNewLearnableMove\n" +
            $"{c_CheckMovePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        Pokemon pokemon = player.BattleRoom.MyPokemon;

        S_CheckNewLearnableMove s_CheckMovePacket = new S_CheckNewLearnableMove();
        s_CheckMovePacket.NewMoveSum = pokemon.CheckNewLearnableMove();

        player.Session.Send(s_CheckMovePacket);
    }

    public static void C_SwitchBattlePokemonHandler(PacketSession session, IMessage packet)
    {
        C_SwitchBattlePokemon c_SwitchPokemonPacket = packet as C_SwitchBattlePokemon;
        int playerId = c_SwitchPokemonPacket.PlayerId;
        int pokemonOrder = c_SwitchPokemonPacket.SelectedPokemonOrder;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_SwitchBattlePokemon\n" +
            $"{c_SwitchPokemonPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PrivateBattleRoom battleRoom = player.BattleRoom;

        battleRoom.MyPokemon = player.Pokemons[pokemonOrder];

        S_SwitchBattlePokemon s_SwitchPokemonPacket = new S_SwitchBattlePokemon();
        s_SwitchPokemonPacket.PlayerInfo = player.MakePlayerInfo();
        s_SwitchPokemonPacket.EnemyPokemonSum = player.BattleRoom.WildPokemon.MakePokemonSummary();
        s_SwitchPokemonPacket.MyPokemonSum = battleRoom.MyPokemon.MakePokemonSummary();

        player.Session.Send(s_SwitchPokemonPacket);
    }

    public static void C_ReturnPokemonBattleSceneHandler(PacketSession session, IMessage packet)
    {
        C_ReturnPokemonBattleScene c_ReturnBattleScenePacket = packet as C_ReturnPokemonBattleScene;
        int playerId = c_ReturnBattleScenePacket.PlayerId;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ReturnPokemonBattleScene\n" +
            $"{c_ReturnBattleScenePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        player.Info.PosInfo.State = CreatureState.Fight;

        S_ReturnPokemonBattleScene s_CheckMovePacket = new S_ReturnPokemonBattleScene();
        s_CheckMovePacket.PlayerInfo = player.MakePlayerInfo();
        s_CheckMovePacket.EnemyPokemonSum = player.BattleRoom.WildPokemon.MakePokemonSummary();
        s_CheckMovePacket.PlayerPokemonSum = player.BattleRoom.MyPokemon.MakePokemonSummary();

        player.Session.Send(s_CheckMovePacket);
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
}