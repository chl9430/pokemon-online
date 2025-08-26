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
            player = MakeTestPlayer(clientSession, CreatureState.Idle);
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
        int fromIdx = switchPokemonPacket.PokemonFromIdx;
        int toIdx = switchPokemonPacket.PokemonToIdx;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_SwitchPokemon\n" +
            $"{switchPokemonPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(switchPokemonPacket.OwnerId);

        if (player.Info.PosInfo.State == CreatureState.Fight)
        {
            PrivateBattleRoom battleRoom = player.BattleRoom;
            battleRoom.SwitchBattlePokemon(fromIdx, toIdx);

            S_SwitchBattlePokemon s_SwitchPokemonPacket = new S_SwitchBattlePokemon();
            s_SwitchPokemonPacket.PlayerInfo = player.MakePlayerInfo();
            s_SwitchPokemonPacket.EnemyPokemonSum = battleRoom.WildPokemon.MakePokemonSummary();

            foreach (Pokemon pokemon in battleRoom.Pokemons)
                s_SwitchPokemonPacket.MyPokemonSums.Add(pokemon.MakePokemonSummary());

            player.Session.Send(s_SwitchPokemonPacket);
        }
        else
        {
            player.SwitchPokemonOrder(fromIdx, toIdx);
        }
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
            player = MakeTestPlayer(clientSession, CreatureState.WatchMenu);

        S_EnterPokemonListScene s_enterListScenePacket = new S_EnterPokemonListScene();
        s_enterListScenePacket.PlayerInfo = player.MakePlayerInfo();

        if (player.Info.PosInfo.State == CreatureState.Fight)
        {
            foreach (Pokemon pokemon in player.BattleRoom.Pokemons)
                s_enterListScenePacket.PokemonSums.Add(pokemon.MakePokemonSummary());
        }
        else
        {
            foreach (Pokemon pokemon in player.Pokemons)
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
            player = MakeTestPlayer(clientSession, CreatureState.Fight);
        }

        player.Info.PosInfo.State = CreatureState.Fight;
        player.BattleRoom = new PrivateBattleRoom(player, player.Pokemons);
        player.BattleRoom.MakeWildPokemon(locationNum);

        S_EnterPokemonBattleScene s_EnterBattleScenePacket = new S_EnterPokemonBattleScene();
        s_EnterBattleScenePacket.PlayerInfo = player.MakePlayerInfo();
        s_EnterBattleScenePacket.EnemyPokemonSum = player.BattleRoom.WildPokemon.MakePokemonSummary();

        foreach (Pokemon pokemon in player.BattleRoom.Pokemons)
            s_EnterBattleScenePacket.PlayerPokemonSums.Add(pokemon.MakePokemonSummary());

        player.Session.Send(s_EnterBattleScenePacket);
    }

    public static void C_EnterPokemonExchangeSceneHandler(PacketSession session, IMessage packet)
    {
        C_EnterPokemonExchangeScene c_EnterExchangePacket = packet as C_EnterPokemonExchangeScene;
        int playerId = c_EnterExchangePacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_EnterPokemonExchangeScene\n" +
            $"{c_EnterExchangePacket}\n" +
            $"=====================\n"
            );

        GameRoom room = RoomManager.Instance.Find(1);
        Player player = ObjectManager.Instance.Find(playerId);

        // 테스트용 플레이어
        if (player == null)
        {
            player = MakeTestPlayer(clientSession, CreatureState.Exchanging);

            room.Push(room.EnterRoom, player);

            int roomId = 1;

            while (true)
            {
                PokemonExchangeRoom exchangeRoom = RoomManager.Instance.FindPokemonExchangeRoom(roomId);

                if (exchangeRoom == null)
                {
                    exchangeRoom = RoomManager.Instance.Add();
                    exchangeRoom.TickRoom(50);

                    player.ExchangeRoom = exchangeRoom;
                    exchangeRoom.Push(exchangeRoom.EnterRoom, player);
                    break;
                }
                else
                {
                    if (exchangeRoom.ExchangePlayers.Count == exchangeRoom.MaxPlayerCount)
                    {
                        roomId++;
                    }
                    else
                    {
                        player.ExchangeRoom = exchangeRoom;

                        exchangeRoom.Push(exchangeRoom.EnterRoom, player);
                        break;
                    }
                }
            }
        }
        else
        {
            if (player.ExchangeRoom == null)
                player.TalkRoom.CreatePokmeonExchangeRoom(player);

            if (player.PosInfo.State == CreatureState.Exchanging)
            {
                PokemonExchangeRoom exchangeRoom = player.ExchangeRoom;
                exchangeRoom.Push(exchangeRoom.ReturnRoom, player);
            }
        }

        player.PosInfo.State = CreatureState.Exchanging;

        S_Move movePacket = new S_Move();
        movePacket.ObjectId = player.Info.ObjectId;
        movePacket.PosInfo = player.PosInfo;

        room.Push(room.Broadcast, player, movePacket);
    }

    public static void C_ChooseExchangePokemonHandler(PacketSession session, IMessage packet)
    {
        C_ChooseExchangePokemon choosePacket = packet as C_ChooseExchangePokemon;
        int playerId = choosePacket.PlayerId;
        int selectedOrder = choosePacket.PokemonOrder;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ChooseExchangePokemon\n" +
            $"{choosePacket}\n" +
            $"=====================\n"
            );

        GameRoom room = RoomManager.Instance.Find(1);
        Player player = ObjectManager.Instance.Find(playerId);

        PokemonExchangeRoom exchangeRoom = player.ExchangeRoom;

        exchangeRoom.Push(exchangeRoom.SetSelectedPokemonOrder, player, selectedOrder);
    }

    public static void C_FinalAnswerToExchangeHandler(PacketSession session, IMessage packet)
    {
        C_FinalAnswerToExchange finalAnswerPacket = packet as C_FinalAnswerToExchange;
        int playerId = finalAnswerPacket.PlayerId;
        bool finalAnswer = finalAnswerPacket.FinalAnswer;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_FinalAnswerToExchange\n" +
            $"{finalAnswerPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        PokemonExchangeRoom exchangeRoom = player.ExchangeRoom;

        exchangeRoom.Push(exchangeRoom.SetFinalAnswer, player, finalAnswer);
    }

    public static void C_MoveExchangeCursorHandler(PacketSession session, IMessage packet)
    {
        C_MoveExchangeCursor c_moveCursorePacket = packet as C_MoveExchangeCursor;
        int playerId = c_moveCursorePacket.PlayerId;
        int x = c_moveCursorePacket.X;
        int y = c_moveCursorePacket.Y;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_MoveExchangeCursor\n" +
            $"{c_moveCursorePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PokemonExchangeRoom exchangeRoom = player.ExchangeRoom;

        exchangeRoom.Push(exchangeRoom.HandlerCursorMove, player, x, y);
    }

    public static void C_ExitPokemonExchangeSceneHandler(PacketSession session, IMessage packet)
    {
        C_ExitPokemonExchangeScene exitExchangePacket = packet as C_ExitPokemonExchangeScene;
        int playerId = exitExchangePacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ExitPokemonExchangeScene\n" +
            $"{exitExchangePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PokemonExchangeRoom exchangeRoom = player.ExchangeRoom;

        exchangeRoom.Push(exchangeRoom.ExitExchangeRoom, player);
    }

    public static void C_RequestDataByIdHandler(PacketSession session, IMessage packet)
    {
        C_RequestDataById c_RequestDataPacket = packet as C_RequestDataById;
        int playerId = c_RequestDataPacket.PlayerId;
        RequestType requestType = c_RequestDataPacket.RequestType;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_RequestDataById\n" +
            $"PlayerId : {playerId}, RequestType : {requestType}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        switch (requestType)
        {
            case RequestType.CheckObjectInMap:
                {
                    GameRoom room = player.Room;
                    GameObject obj = player.FindObject();

                    if (obj == null)
                    {
                        // 수정 필요
                        S_SendTalk s_TalkPacket = new S_SendTalk();

                        player.Session.Send(s_TalkPacket);
                    }
                    else
                    {
                        if (obj.ObjectType == GameObjectType.Player)
                        {
                            Player otherPlayer = obj as Player;

                            // 말 건 사람은 상대방의 정보를 받는다.
                            S_SendTalk s_SendTalkPacket = new S_SendTalk();
                            s_SendTalkPacket.OtherPlayerInfo = otherPlayer.MakePlayerInfo();

                            player.Session.Send(s_SendTalkPacket);

                            if (otherPlayer.Info.PosInfo.State != CreatureState.Idle)
                                return;

                            // 상대방은 말 건 사람을 향해 바라본다.
                            player.TalkWithPlayer(otherPlayer);

                            // 타인한테 정보 전송
                            S_Move movePacket = new S_Move();
                            movePacket.ObjectId = otherPlayer.Info.ObjectId;
                            movePacket.PosInfo = otherPlayer.PosInfo;

                            foreach (Player p in room.Players.Values)
                            {
                                p.Session.Send(movePacket);
                            }

                            // 상대방은 말 건 사람의 정보를 받는다.
                            S_ReceiveTalk s_ReceiveTalkPacket = new S_ReceiveTalk();
                            s_ReceiveTalkPacket.PlayerInfo = player.MakePlayerInfo();

                            otherPlayer.Session.Send(s_ReceiveTalkPacket);
                        }
                    }
                }
                break;
            case RequestType.GetEnemyPokemonExp:
                {
                    S_GetEnemyPokemonExp s_GetExpPacket = new S_GetEnemyPokemonExp();

                    s_GetExpPacket.Exp = player.BattleRoom.GetExp();

                    player.Session.Send(s_GetExpPacket);
                }
                break;
            case RequestType.CheckAndApplyRemainedExp:
                {
                    S_CheckAndApplyRemainedExp s_CheckAndApplyExpPacket = player.BattleRoom.CheckAndApplyExp();

                    player.Session.Send(s_CheckAndApplyExpPacket);
                }
                break;
            case RequestType.EscapeFromWildPokemon:
                {
                    S_EscapeFromWildPokemon s_EscapePacket = new S_EscapeFromWildPokemon();

                    s_EscapePacket.CanEscape = player.BattleRoom.CalEscapeRate();

                    player.Session.Send(s_EscapePacket);
                }
                break;
        }
    }

    public static void C_PlayerTalkHandler(PacketSession session, IMessage packet)
    {
        C_PlayerTalk c_TalkPacket = packet as C_PlayerTalk;
        int playerId = c_TalkPacket.PlayerId;
        TalkRequestType talkRequestType = c_TalkPacket.TalkRequestType;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_PlayerTalk\n" +
            $"PlayerId : {playerId}, RequestType : {talkRequestType}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        Player talkPlayer = player.TalkRoom.GetAnotherPlayer(player);

        S_SendTalkRequest s_SendTalkPacket = new S_SendTalkRequest();
        s_SendTalkPacket.TalkRequestType = talkRequestType;

        talkPlayer.Session.Send(s_SendTalkPacket);
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
            player = MakeTestPlayer(clientSession, CreatureState.WatchMenu);
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

    public static void C_SetBattlePokemonMoveHandler(PacketSession session, IMessage packet)
    {
        C_SetBattlePokemonMove c_SetMovePacket = packet as C_SetBattlePokemonMove;
        int playerId = c_SetMovePacket.PlayerId;
        int moveOrder = c_SetMovePacket.MoveOrder;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_SetBattlePokemonMove\n" +
            $"{c_SetMovePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PrivateBattleRoom battleRoom = player.BattleRoom;

        battleRoom.SetBattlePokemonMove(moveOrder);

        S_SetBattlePokemonMove s_SetMovePacket = new S_SetBattlePokemonMove();
        s_SetMovePacket.MyMoveOrder = battleRoom.MyPokemon.GetSelectedMoveIdx();
        s_SetMovePacket.EnemyMoveOrder = battleRoom.WildPokemon.GetSelectedMoveIdx();

        player.Session.Send(s_SetMovePacket);
    }

    public static void C_UsePokemonMoveHandler(PacketSession session, IMessage packet)
    {
        C_UsePokemonMove c_UseMovePacket = packet as C_UsePokemonMove;
        int playerId = c_UseMovePacket.PlayerId;
        int myMoveOrder = c_UseMovePacket.MoveOrder;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_UsePokemonMove\n" +
            $"{c_UseMovePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        S_UsePokemonMove s_UseMovePacket = player.BattleRoom.UseBattlePokemonMove();

        player.Session.Send(s_UseMovePacket);
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

        foreach (Pokemon pokemon in player.BattleRoom.Pokemons)
            s_CheckMovePacket.MyPokemonSums.Add(pokemon.MakePokemonSummary());

        player.Session.Send(s_CheckMovePacket);
    }

    public static Player MakeTestPlayer(ClientSession clientSession, CreatureState state)
    {
        Random _random = new Random();
        int ran = _random.Next(11);
        string randomName = "";

        if (ran == 0) randomName = "MESSI";
        else if (ran == 1) randomName = "VILLA";
        else if (ran == 2) randomName = "PEDRO";
        else if (ran == 3) randomName = "XAVI";
        else if (ran == 4) randomName = "INIES";
        else if (ran == 5) randomName = "SERGI";
        else if (ran == 6) randomName = "ALVES";
        else if (ran == 7) randomName = "PUYOL";
        else if (ran == 8) randomName = "PIQUE";
        else if (ran == 9) randomName = "ERIC";
        else if (ran == 10) randomName = "VALDES";


        Player player = ObjectManager.Instance.Add<Player>();
        {
            player.Info.PosInfo.State = CreatureState.Idle;
            player.Info.PosInfo.MoveDir = MoveDir.Down;
            player.Info.PosInfo.PosX = 0;
            player.Info.PosInfo.PosY = 0;

            player.Session = clientSession;
        }

        // 플레이어 정보
        player.Name = randomName;
        player.Gender = PlayerGender.PlayerFemale;

        // 플레이어 포켓몬
        player.Pokemons = new List<Pokemon>();
        player.Pokemons.Add(new Pokemon("Pikachu", "PIKAO", 7, player.Name, -1));
        player.Pokemons.Add(new Pokemon("Bulbasaur", "BUBAS", 6, player.Name, -1));
        player.Pokemons.Add(new Pokemon("Charmander", "CHAKI", 5, player.Name, -1));
        player.Pokemons.Add(new Pokemon("Squirtle", "SKIRT", 6, player.Name, -1));

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