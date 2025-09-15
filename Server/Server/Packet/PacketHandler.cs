using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System.Diagnostics;
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
            S_SwitchBattlePokemon s_SwitchPokemonPacket = battleRoom.SwitchBattlePokemon(fromIdx, toIdx);

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
                    S_GetEnemyPokemonExp s_GetExpPacket = player.BattleRoom.GetExp();

                    player.Session.Send(s_GetExpPacket);
                }
                break;
            case RequestType.CheckAndApplyRemainedExp:
                {
                    S_CheckAndApplyRemainedExp s_CheckAndApplyExpPacket = player.BattleRoom.CheckAndApplyExp();

                    player.Session.Send(s_CheckAndApplyExpPacket);
                }
                break;
            case RequestType.CheckAvailableBattlePokemon:
                {
                    S_CheckAvailableBattlePokemon checkBattlePokemon = new S_CheckAvailableBattlePokemon();
                    checkBattlePokemon.CanFight = player.BattleRoom.FindAvailableBattlePokemon();

                    player.Session.Send(checkBattlePokemon);
                }
                break;
            case RequestType.EscapeFromWildPokemon:
                {
                    S_EscapeFromWildPokemon s_EscapePacket = new S_EscapeFromWildPokemon();

                    s_EscapePacket.CanEscape = player.BattleRoom.CalEscapeRate();

                    player.Session.Send(s_EscapePacket);
                }
                break;
            case RequestType.CheckExpPokemon:
                {
                    S_CheckExpPokemon checkExpPacket = new S_CheckExpPokemon();
                    checkExpPacket.IsThereMoreExpPokemon = player.BattleRoom.CheckExpPokemons();

                    player.Session.Send(checkExpPacket);
                }
                break;
            case RequestType.CheckPokemonEvolution:
                {
                    S_CheckPokemonEvolution checkEvolutionPacket = new S_CheckPokemonEvolution();

                    checkEvolutionPacket.GoToEvolutionScene = player.BattleRoom.CheckEvolutionPokemon();

                    player.Session.Send(checkEvolutionPacket);
                }
                break;
        }
    }

    public static void C_ProcessTurnHandler(PacketSession session, IMessage packet)
    {
        C_ProcessTurn processTurnPacket = packet as C_ProcessTurn;
        int playerId = processTurnPacket.PlayerId;
        int moveOrder = processTurnPacket.MoveOrder;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ProcessTurn\n" +
            $"{processTurnPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        S_ProcessTurn s_ProcessTurnPacket = player.BattleRoom.ProcessTurn(moveOrder, player);

        player.Session.Send(s_ProcessTurnPacket);
    }

    public static void C_IsSuccessPokeBallCatchHandler(PacketSession session, IMessage packet)
    {
        C_IsSuccessPokeBallCatch successCatchPacket = packet as C_IsSuccessPokeBallCatch;
        int playerId = successCatchPacket.PlayerId;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_IsSuccessPokeBallCatch\n" +
            $"{successCatchPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        S_IsSuccessPokeBallCatch s_SuccessCatchPacket = player.BattleRoom.CatchPokemon();

        player.Session.Send(s_SuccessCatchPacket);
    }

    public static void C_EnterPokemonEvolutionSceneHandler(PacketSession session, IMessage packet)
    {
        C_EnterPokemonEvolutionScene c_EnterEvolutionPacket = packet as C_EnterPokemonEvolutionScene;
        int playerId = c_EnterEvolutionPacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_EnterPokemonEvolutionScene\n" +
            $"PlayerId : {playerId}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        if (player == null)
        {
            player = MakeTestPlayer(clientSession, CreatureState.PokemonEvolving);
            player.BattleRoom = new PrivateBattleRoom(player, player.Pokemons);
            player.BattleRoom.MakeWildPokemon(1);
            player.BattleRoom.EvolvePokemons.Add(player.Pokemons[0]);
            player.BattleRoom.EvolvePokemons.Add(player.Pokemons[1]);
            player.BattleRoom.EvolvePokemons.Add(player.Pokemons[2]);
        }

        player.Info.PosInfo.State = CreatureState.PokemonEvolving;

        S_EnterPokemonEvolutionScene s_EnterEvolutionPacket = new S_EnterPokemonEvolutionScene();
        Pokemon myPokemon = player.BattleRoom.GetEvolutionPokemon();

        s_EnterEvolutionPacket.PlayerInfo = player.MakePlayerInfo();
        s_EnterEvolutionPacket.PokemonSum = myPokemon.MakePokemonSummary();
        s_EnterEvolutionPacket.EvolvePokemonName = myPokemon.GetEvolvePokemonName();

        player.Session.Send(s_EnterEvolutionPacket);
    }

    public static void C_PokemonEvolutionHandler(PacketSession session, IMessage packet)
    {
        C_PokemonEvolution evolutionPacket = packet as C_PokemonEvolution;
        int playerId = evolutionPacket.PlayerId;
        bool isEvolution = evolutionPacket.IsEvolution;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_PokemonEvolution\n" +
            $"{evolutionPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        S_PokemonEvolution s_EvolutionPacket = player.BattleRoom.EvolvePokemon(isEvolution);

        if (isEvolution)
            player.Session.Send(s_EvolutionPacket);
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

    public static void C_ItemSceneToBattleSceneHandler(PacketSession session, IMessage packet)
    {
        C_ItemSceneToBattleScene itemToBattlePacket = packet as C_ItemSceneToBattleScene;
        int playerId = itemToBattlePacket.PlayerId;
        ItemCategory itemCategory = itemToBattlePacket.ItemCategory;
        int itemOrder = itemToBattlePacket.ItemOrder;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ItemSceneToBattleScene\n" +
            $"{itemToBattlePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        PrivateBattleRoom battleRoom = player.BattleRoom;

        S_ItemSceneToBattleScene s_ItemToBattlePacket = new S_ItemSceneToBattleScene();
        s_ItemToBattlePacket.PlayerInfo = player.MakePlayerInfo();
        s_ItemToBattlePacket.EnemyPokemonSum = battleRoom.WildPokemon.MakePokemonSummary();
        s_ItemToBattlePacket.PlayerPokemonSum = battleRoom.MyPokemon.MakePokemonSummary();

        s_ItemToBattlePacket.UsedItem = itemOrder != -1 ? player.UseItem(itemCategory, itemOrder).MakeItemSummary() : null;

        player.Session.Send(s_ItemToBattlePacket);
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
            player = MakeTestPlayer(clientSession, CreatureState.Fight);
        }

        S_EnterPlayerBagScene s_EnterBagScenePacket = new S_EnterPlayerBagScene();
        s_EnterBagScenePacket.PlayerInfo = player.MakePlayerInfo();

        // 인벤토리 딕셔너리를 모두 가져옴
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

    public static void C_EnterMoveSelectionSceneHandler(PacketSession session, IMessage packet)
    {
        C_EnterMoveSelectionScene enterMoveScenePacket = packet as C_EnterMoveSelectionScene;
        int playerId = enterMoveScenePacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_EnterMoveSelectionScene\n" +
            $"{enterMoveScenePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        if (player == null)
        {
            player = MakeTestPlayer(clientSession, CreatureState.PokemonEvolving);
            player.BattleRoom = new PrivateBattleRoom(player, player.Pokemons);
            player.BattleRoom.MakeWildPokemon(1);
            player.BattleRoom.EvolvePokemons.Add(player.Pokemons[0]);
            player.BattleRoom.EvolvePokemons.Add(player.Pokemons[1]);
            player.BattleRoom.EvolvePokemons.Add(player.Pokemons[2]);

            var moveData = DataManager.PokemonMoveDict["Seed Bomb"];
            player.BattleRoom.LearnableMove = new PokemonMove(moveData.moveName);
        }

        PrivateBattleRoom battleRoom = player.BattleRoom;

        S_EnterMoveSelectionScene s_EnterMoveScenePacket = battleRoom.EnterMoveSelectionScene();

        player.Session.Send(s_EnterMoveScenePacket);
    }

    public static void C_MoveSceneToBattleSceneHandler(PacketSession session, IMessage packet)
    {
        C_MoveSceneToBattleScene battleScenePacket = packet as C_MoveSceneToBattleScene;
        int playerId = battleScenePacket.PlayerId;
        int prevMoveIdx = battleScenePacket.PrevMoveIdx;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_MoveSceneToBattleScene\n" +
            $"{battleScenePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        Pokemon myPokemon = player.BattleRoom.GetExpPokemon();

        S_MoveSceneToBattleScene s_BattleScenePacket = new S_MoveSceneToBattleScene();
        s_BattleScenePacket.PlayerInfo = player.MakePlayerInfo();
        s_BattleScenePacket.MyPokemonSum = player.BattleRoom.MyPokemon.MakePokemonSummary();
        s_BattleScenePacket.EnemyPokemonSum = player.BattleRoom.WildPokemon.MakePokemonSummary();
        s_BattleScenePacket.MovePokemonName = player.BattleRoom.GetExpPokemon().PokemonInfo.NickName;

        if (prevMoveIdx != -1)
        {
            s_BattleScenePacket.PrevMoveName = myPokemon.PokemonMoves[prevMoveIdx].MoveName;
            myPokemon.ForgetAndLearnNewMove(prevMoveIdx, player.BattleRoom.LearnableMove);
            s_BattleScenePacket.NewMoveName = myPokemon.PokemonMoves[prevMoveIdx].MoveName;
        }
        else
        {
            s_BattleScenePacket.PrevMoveName = "";
            s_BattleScenePacket.NewMoveName = player.BattleRoom.LearnableMove.MoveName;
        }

        player.Session.Send(s_BattleScenePacket);
    }

    public static void C_MoveSceneToEvolveSceneHandler(PacketSession session, IMessage packet)
    {
        C_MoveSceneToEvolveScene evolveScenePacket = packet as C_MoveSceneToEvolveScene;
        int playerId = evolveScenePacket.PlayerId;
        int prevMoveIdx = evolveScenePacket.PrevMoveIdx;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_MoveSceneToEvolveScene\n" +
            $"{evolveScenePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        Pokemon myPokemon = player.BattleRoom.GetEvolutionPokemon();
        player.BattleRoom._curEvolutionIdx++;

        S_MoveSceneToEvolveScene s_EvolveScenePacket = new S_MoveSceneToEvolveScene();
        s_EvolveScenePacket.PlayerInfo = player.MakePlayerInfo();
        s_EvolveScenePacket.PokemonSum = myPokemon.MakePokemonSummary();

        if (prevMoveIdx != -1)
        {
            s_EvolveScenePacket.PrevMoveName = myPokemon.PokemonMoves[prevMoveIdx].MoveName;
            myPokemon.ForgetAndLearnNewMove(prevMoveIdx, player.BattleRoom.LearnableMove);
            s_EvolveScenePacket.NewMoveName = myPokemon.PokemonMoves[prevMoveIdx].MoveName;
        }
        else
        {
            s_EvolveScenePacket.PrevMoveName = "";
            s_EvolveScenePacket.NewMoveName = player.BattleRoom.LearnableMove.MoveName;
        }

        player.Session.Send(s_EvolveScenePacket);
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
            player.Info.PosInfo.State = state;
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
        player.Pokemons.Add(new Pokemon("Bulbasaur", "BUBAS", 15, player.Name, -1));
        player.Pokemons.Add(new Pokemon("Squirtle", "SKIRT", 14, player.Name, -1));
        player.Pokemons.Add(new Pokemon("Charmander", "CHAKI", 14, player.Name, -1));
        player.Pokemons.Add(new Pokemon("Pikachu", "PIKAO", 5, player.Name, -1));

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