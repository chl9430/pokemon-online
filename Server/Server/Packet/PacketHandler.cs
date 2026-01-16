using Google.Protobuf;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using Server;
using ServerCore;
using System.Diagnostics;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Schema;

public class PacketHandler
{
    public static void C_LogInHandler(PacketSession session, IMessage packet)
    {
        C_LogIn logInPacket = packet as C_LogIn;
        string id = logInPacket.Id;
        string pw = logInPacket.Password;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine(
            $"=====================\n" +
            $"C_LogIn\n" +
            $"{logInPacket}\n" +
            $"=====================\n"
            );

        LogInManager.Instance.CheckLogInInfo(clientSession, id, pw);
    }

    public static void C_CreateAccountHandler(PacketSession session, IMessage packet)
    {
        C_CreateAccount createAccountPacket = packet as C_CreateAccount;
        string id = createAccountPacket.Id;
        string pw = createAccountPacket.Password;

        ClientSession clientSession = session as ClientSession;

        LogInManager.Instance.CreateNewAccount(clientSession, id, pw);
    }

    public static void C_CheckSaveDataHandler(PacketSession session, IMessage packet)
    {
        C_CheckSaveData checkData = packet as C_CheckSaveData;
        string id = checkData.Id;

        ClientSession clientSession = session as ClientSession;

        SaveManager.Instance.CheckGameSaveData(clientSession, id);
    }

    public static void C_LoadGameDataHandler(PacketSession session, IMessage packet)
    {
        C_LoadGameData loadGamePacket = packet as C_LoadGameData;
        string accountId = loadGamePacket.AccountId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine(
            $"=====================\n" +
            $"C_LoadGameData\n" +
            $"{loadGamePacket}\n" +
            $"=====================\n"
            );

        PlayerInfo info = SaveManager.Instance.GetGameSaveData(accountId);

        Player player = ObjectManager.Instance.AddLoadedPlayer(clientSession, info);
    }

    public static void C_EnterRoomHandler(PacketSession session, IMessage packet)
    {
        C_EnterRoom enterRoom = packet as C_EnterRoom;
        int playerId = enterRoom.PlayerId;
        int prevRoomId = enterRoom.PrevRoomId;
        RoomType prevRoomType = enterRoom.PrevRoomType;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine(
            $"=====================\n" +
            $"C_EnterRoom\n" +
            $"{enterRoom}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        if (player == null)
        {
            player = MakeTestPlayer(clientSession, CreatureState.Idle);
        }

        GameRoom room = player.Room;
        if (room == null)
        {
            GameRoom testRoom = RoomManager.Instance.Find(1, RoomType.Map);
            testRoom.Push(testRoom.EnterRoom, player);
        }
        else
        {
            room.Push(room.MoveAnotherRoom, player);
        }
    }

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

        Random _random = new Random();

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
            player.UserId = c_CreatePlayerPacket.UserId;
            Pokemon pokemon = null;
            
            player.AddPokemon(new Pokemon("Pikachu", "Pikachu", 5, player.Name, -1));
            player.AddPokemon(new Pokemon("Charmander", "Charmander", 5, player.Name, -1));
            player.AddPokemon(new Pokemon("Bulbasaur", "Bulbasaur", 5, player.Name, -1));
            player.AddPokemon(new Pokemon("Squirtle", "Squirtle", 5, player.Name, -1));

            player.AddItem(ItemCategory.PokeBall, "Monster Ball", 10);

            player.Money += 1000000;

            clientSession.MyPlayer = player;
        }

        GameRoom room = RoomManager.Instance.Find(1, RoomType.Map);
        room.Push(room.EnterRoom, player);

        player.Room = room;

        SaveManager.Instance.SaveGameData(clientSession, c_CreatePlayerPacket.UserId, player.Id);
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

        if (player.Info.PosInfo.State == CreatureState.Fight && player.BattleRoom != null)
        {
            PrivateBattleRoom battleRoom = player.BattleRoom;
            S_SwitchBattlePokemon s_SwitchPokemonPacket = battleRoom.SwitchBattlePokemon(fromIdx, toIdx);

            player.Session.Send(s_SwitchPokemonPacket);
        }
        else if (player.Info.PosInfo.State == CreatureState.Fight && player.PokemonBattleRoom != null)
        {
            PokemonBattleRoom battleRoom = player.PokemonBattleRoom;
            List<Pokemon> pokemons = battleRoom.GetPokemonListByPlayer(player);

            if (pokemons[0].PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
            {
                battleRoom.Push(battleRoom.SwitchDiePokemon, player, fromIdx, toIdx);
            }
            else
            {
                C_SendAction actionPacket = new C_SendAction();
                actionPacket.PlayerId = player.Id;
                actionPacket.SwitchBattlePokemon = new SwitchBattlePokemon();
                actionPacket.SwitchBattlePokemon.FromIdx = fromIdx;
                actionPacket.SwitchBattlePokemon.ToIdx = toIdx;

                battleRoom.Push(battleRoom.SetPlayerAction, player, actionPacket);
            }
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

        player.BattleRoom = null;
        player.TalkingNPC = null;
        player.PokemonBattleRoom = null;

        GameRoom room = player.Room;
        room.Push(room.GetAnotherPlayerData, player);
    }

    public static void C_UseItemInListSceneHandler(PacketSession session, IMessage packet)
    {
        C_UseItemInListScene useItemPacket = packet as C_UseItemInListScene;
        int playerId = useItemPacket.PlayerId;
        int targetPokemonOrder = useItemPacket.TargetPokemonOrder;
        ItemCategory usedItemCategory = useItemPacket.UsedItemCategory;
        int usedItemOrder = useItemPacket.UsedItemOrder;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_UseItemInListScene\n" +
            $"{useItemPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        if (player.Info.PosInfo.State == CreatureState.Fight)
        {
            player.BattleRoom.ProcessMyTurn();
        }

        S_UseItemInListScene s_UseItemPacket = player.UseItem(usedItemCategory, usedItemOrder, targetPokemonOrder);

        player.Session.Send(s_UseItemPacket);
    }

    public static void C_EnterPokemonBattleSceneHandler(PacketSession session, IMessage packet)
    {
        C_EnterPokemonBattleScene c_EnterBattleScenePacket = packet as C_EnterPokemonBattleScene;
        int playerId = c_EnterBattleScenePacket.PlayerId;
        int locationNum = c_EnterBattleScenePacket.LocationNum;
        int bushNum = c_EnterBattleScenePacket.BushNum;

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

            player.TalkingNPC = ObjectManager.Instance.FindNPC(1);

            GameRoom room = RoomManager.Instance.Find(1, RoomType.Map);

            room.Push(room.EnterRoom, player);
        }

        player.BattleRoom = new PrivateBattleRoom(player, player.Pokemons);

        S_EnterPokemonBattleScene s_EnterBattleScenePacket = new S_EnterPokemonBattleScene();

        if (locationNum > 0 && bushNum > 0)
        {
            player.BattleRoom.MakeWildPokemon(locationNum, bushNum);
        }
        else
        {
            TrainerNPC npc = player.TalkingNPC as TrainerNPC;
            player.BattleRoom.GetBattleReady(npc);
        }

        if (player.TalkingNPC != null)
        {
            s_EnterBattleScenePacket.NpcInfo = ((NPC)player.TalkingNPC).MakeNPCInfo();
        }
        s_EnterBattleScenePacket.MyFirstBattlePokemonOrder = player.Pokemons.IndexOf(player.BattleRoom.MyPokemon);
        s_EnterBattleScenePacket.EnemyPokemonSum = player.BattleRoom.OpponentPokemon.MakePokemonSummary();

        player.Session.Send(s_EnterBattleScenePacket);
    }

    public static void C_ShopItemListHandler(PacketSession session, IMessage packet)
    {
        C_ShopItemList shopItemPacket = packet as C_ShopItemList;
        int playerId = shopItemPacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ShopItemList\n" +
            $"{shopItemPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        S_ShopItemList s_ShopItemPacket = new S_ShopItemList();
        if (player.Room is FriendlyShop)
        {
            FriendlyShop shopRoom = (FriendlyShop)player.Room;

            List<ItemBase> itemList = shopRoom.ShopItems;

            foreach (ItemBase item in itemList)
            {
                s_ShopItemPacket.ShopItemSums.Add(item.MakeItemSummary());
            }
        }

        player.Session.Send(s_ShopItemPacket);
    }

    public static void C_GetItemCountHandler(PacketSession session, IMessage packet)
    {
        C_GetItemCount getCountPacket = packet as C_GetItemCount;
        int playerId = getCountPacket.PlayerId;
        ItemCategory itemCategory = getCountPacket.ItemCategory;
        string itemName = getCountPacket.ItemName;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_GetItemCount\n" +
            $"{getCountPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        S_GetItemCount s_GetCountPacket = new S_GetItemCount();
        s_GetCountPacket.Money = player.Money;

        int totalCount = 0;
        foreach (ItemBase item in player.Items[itemCategory])
        {
            if (item._name == itemName)
            {
                totalCount += item._itemCnt;
            }
        }

        s_GetCountPacket.ItemCount = totalCount;

        player.Session.Send(s_GetCountPacket);
    }

    public static void C_BuyItemHandler(PacketSession session, IMessage packet)
    {
        C_BuyItem buyItemPacket = packet as C_BuyItem;
        int playerId = buyItemPacket.PlayerId;
        int quantity = buyItemPacket.ItemQuantity;
        int itemIdx = buyItemPacket.ItemIdx;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_BuyItem\n" +
            $"{buyItemPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        FriendlyShop friendlyShopRoom = player.Room as FriendlyShop;

        if (friendlyShopRoom != null)
        {
            friendlyShopRoom.Push(friendlyShopRoom.BuyItem, player, itemIdx, quantity);
        }
    }

    public static void C_SellItemHandler(PacketSession session, IMessage packet)
    {
        C_SellItem sellItemPacket = packet as C_SellItem;
        int playerId = sellItemPacket.PlayerId;
        ItemCategory itemCategory = sellItemPacket.ItemCategory;
        int quantity = sellItemPacket.ItemQuantity;
        int itemIdx = sellItemPacket.ItemIdx;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_SellItem\n" +
            $"{sellItemPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        S_SellItem s_SellItemPacket = player.SellItem(itemCategory, itemIdx, quantity);

        player.Session.Send(s_SellItemPacket);
    }

    public static void C_RestorePokemonHandler(PacketSession session, IMessage packet)
    {
        C_RestorePokemon restorePacket = packet as C_RestorePokemon;
        int playerId = restorePacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_RestorePokemon\n" +
            $"{restorePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        List<Pokemon> pokemons = player.Pokemons;

        foreach (Pokemon pokemon in pokemons)
            pokemon.RestorePokemon();

        S_RestorePokemon s_RestorePacket = new S_RestorePokemon();
        s_RestorePacket.PokemonCount = pokemons.Count;

        player.Session.Send(s_RestorePacket);
    }

    public static void C_FinishNpcTalkHandler(PacketSession session, IMessage packet)
    {
        C_FinishNpcTalk finishTalkPacket = packet as C_FinishNpcTalk;
        int playerId = finishTalkPacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_FinishNpcTalk\n" +
            $"{finishTalkPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        player.TalkingNPC = null;
    }

    public static void C_SaveGameDataHandler(PacketSession session, IMessage packet)
    {
        C_SaveGameData saveGamePacket = packet as C_SaveGameData;
        string accountId = saveGamePacket.AccountId;
        int playerId = saveGamePacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_SaveGameData\n" +
            $"{saveGamePacket}\n" +
            $"=====================\n"
            );

        SaveManager.Instance.SaveGameData(clientSession, accountId, playerId);
    }

    public static void C_EnterTrainerBattleHandler(PacketSession session, IMessage packet)
    {
        C_EnterTrainerBattle enterBattle = packet as C_EnterTrainerBattle;
        int playerId = enterBattle.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_EnterTrainerBattle\n" +
            $"{enterBattle}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        GameRoom room = RoomManager.Instance.Find(player.Room.RoomId, player.Room.RoomType);

        if (player.PokemonBattleRoom == null)
            player.TalkRoom.CreatePokemonBattleRoom(player);
    }

    public static void C_CheckAvailableMoveHandler(PacketSession session, IMessage packet)
    {
        C_CheckAvailableMove checkMovePacket = packet as C_CheckAvailableMove;
        int playerId = checkMovePacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_CheckAvailableMoveHandler\n" +
            $"{checkMovePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        PokemonBattleRoom battleRoom = player.PokemonBattleRoom;

        battleRoom.Push(battleRoom.CheckAvailableMove, player);
    }

    public static void C_SendActionHandler(PacketSession session, IMessage packet)
    {
        C_SendAction sendActionPacket = packet as C_SendAction;
        int playerId = sendActionPacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_SendMyAction\n" +
            $"{sendActionPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        PokemonBattleRoom battleRoom = player.PokemonBattleRoom;

        battleRoom.Push(battleRoom.SetPlayerAction, player, sendActionPacket);
    }

    public static void C_RequestNextBattleActionHandler(PacketSession session, IMessage packet)
    {
        C_RequestNextBattleAction requestActionPacket = packet as C_RequestNextBattleAction;
        int playerId = requestActionPacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_RequestNextBattleAction\n" +
            $"{requestActionPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        PokemonBattleRoom battleRoom = player.PokemonBattleRoom;

        battleRoom.Push(battleRoom.RequestNextBattleAction);
    }

    public static void C_CheckAvailablePokemonHandler(PacketSession session, IMessage packet)
    {
        C_CheckAvailablePokemon checkPokemonPacket = packet as C_CheckAvailablePokemon;
        int playerId = checkPokemonPacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_CheckAvailablePokemon\n" +
            $"{checkPokemonPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        PokemonBattleRoom battleRoom = player.PokemonBattleRoom;

        battleRoom.Push(battleRoom.CheckAvailablePokemon, player);
    }

    public static void C_SurrenderTrainerBattleHandler(PacketSession session, IMessage packet)
    {
        C_SurrenderTrainerBattle surrenderPacket = packet as C_SurrenderTrainerBattle;
        int playerId = surrenderPacket.PlayerId;

        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_SurrenderTrainerBattle\n" +
            $"{surrenderPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        PokemonBattleRoom battleRoom = player.PokemonBattleRoom;

        battleRoom.Push(battleRoom.SurrenderBattle, player);
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

        Player player = ObjectManager.Instance.Find(playerId);

        // 테스트용 플레이어
        if (player == null)
        {
            player = MakeTestPlayer(clientSession, CreatureState.Exchanging);

            GameRoom room = RoomManager.Instance.Find(1, RoomType.Map);

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
            GameRoom room = RoomManager.Instance.Find(player.Room.RoomId, player.Room.RoomType);

            if (player.ExchangeRoom == null)
                player.TalkRoom.CreatePokmeonExchangeRoom(player);

            if (player.PosInfo.State == CreatureState.Exchanging)
            {
                PokemonExchangeRoom exchangeRoom = player.ExchangeRoom;
                exchangeRoom.Push(exchangeRoom.ReturnRoom, player);
            }
            else
            {
                // player.PosInfo.State = CreatureState.Exchanging;
            }
        }
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
                        //S_SendTalk s_TalkPacket = new S_SendTalk();

                        //player.Session.Send(s_TalkPacket);
                    }
                    else
                    {
                        if (obj.ObjectType == GameObjectType.Player)
                        {
                            player.TalkingNPC = obj;

                            Player otherPlayer = obj as Player;

                            // 말 건 사람은 상대방의 정보를 받는다.
                            S_SendTalk s_SendTalkPacket = new S_SendTalk();
                            s_SendTalkPacket.OtherPlayerInfo = otherPlayer.MakeOtherPlayerInfo();

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
                            s_ReceiveTalkPacket.OtherPlayerInfo = player.MakeOtherPlayerInfo();

                            otherPlayer.Session.Send(s_ReceiveTalkPacket);
                        }
                        else if (obj.ObjectType == GameObjectType.Npc)
                        {
                            player.TalkingNPC = obj;

                            if (obj is TrainerNPC)
                            {
                                S_GetTrainerTalk trainerTalkPacket = new S_GetTrainerTalk();
                                trainerTalkPacket.NpcId = obj.Id;

                                string[] scripts = (obj as TrainerNPC).GetTalk(player.NPCNumber);

                                foreach (string script in scripts)
                                {
                                    trainerTalkPacket.Scripts.Add(script);
                                }

                                trainerTalkPacket.CanBattle = (obj as TrainerNPC).CanBattle(player.NPCNumber);

                                player.Session.Send(trainerTalkPacket);
                            }
                            else
                            {
                                S_GetNpcTalk npcTalkPacket = new S_GetNpcTalk();
                                npcTalkPacket.NpcId = obj.Id;

                                player.Session.Send(npcTalkPacket);
                            }
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
            case RequestType.SendOpponentNextPokemon:
                {
                    S_SendOpponentNextPokemon sendNextPokemon = new S_SendOpponentNextPokemon();
                    sendNextPokemon.OpponentPokemonSum = player.BattleRoom.SetNextOpponentPokemon();

                    player.Session.Send(sendNextPokemon);
                }
                break;
            case RequestType.CheckPokemonEvolution:
                {
                    S_CheckPokemonEvolution checkEvolutionPacket = new S_CheckPokemonEvolution();

                    Pokemon evolvePokemon = null;
                    if (player.PosInfo.State == CreatureState.Fight)
                        evolvePokemon = player.BattleRoom.SetEvolutionPokemon();

                    if (evolvePokemon != null)
                    {
                        checkEvolutionPacket.EvolvePokemonIdx = player.Pokemons.IndexOf(evolvePokemon);
                        checkEvolutionPacket.EvolutionPokemonName = evolvePokemon.GetEvolvePokemonName();
                    }
                    else
                    {
                        checkEvolutionPacket.EvolvePokemonIdx = -1;
                        checkEvolutionPacket.EvolutionPokemonName = "";
                    }

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

    public static void C_ForgetAndLearnNewMoveHandler(PacketSession session, IMessage packet)
    {
        C_ForgetAndLearnNewMove forgetMovePacket = packet as C_ForgetAndLearnNewMove;
        int playerId = forgetMovePacket.PlayerId;
        int forgetMoveOrder = forgetMovePacket.ForgetMoveOrder;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_ForgetAndLearnNewMove\n" +
            $"{forgetMovePacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);
        Pokemon myPokemon = player.BattleRoom.GetExpPokemon();

        S_ForgetAndLearnNewMove s_LearnNewMovePacket = new S_ForgetAndLearnNewMove();

        // 뭔가 잊어버리기로 했다면
        if (forgetMoveOrder != -1)
        {
            s_LearnNewMovePacket.PrevMoveName = myPokemon.PokemonMoves[forgetMoveOrder].MoveName;

            myPokemon.ForgetAndLearnNewMove(forgetMoveOrder, player.BattleRoom.LearnableMove);

            s_LearnNewMovePacket.NewMoveName = myPokemon.PokemonMoves[forgetMoveOrder].MoveName;
        }
        else // 잊이버리지 않는다면
        {
            s_LearnNewMovePacket.PrevMoveName = "";
            s_LearnNewMovePacket.NewMoveName = player.BattleRoom.LearnableMove.MoveName;
        }

        player.BattleRoom.LearnableMove = null;

        player.Session.Send(s_LearnNewMovePacket);
    }

    public static void C_GetRewardInfoHandler(PacketSession session, IMessage packet)
    {
        C_GetRewardInfo getRewardPacket = packet as C_GetRewardInfo;
        int playerId = getRewardPacket.PlayerId;

        Console.WriteLine($"" +
            $"=====================\n" +
            $"C_GetRewardInfo\n" +
            $"{getRewardPacket}\n" +
            $"=====================\n"
            );

        Player player = ObjectManager.Instance.Find(playerId);

        S_GetRewardInfo s_GetRewardPacket = player.BattleRoom.FillRewardInfoPacket();

        player.Session.Send(s_GetRewardPacket);
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

        if (talkRequestType == TalkRequestType.CancelTalk ||
            talkRequestType == TalkRequestType.Reject)
        {
            player.TalkRoom = null;
            talkPlayer.TalkRoom = null;

            player.TalkingNPC = null;
            talkPlayer.TalkingNPC = null;
        }

        S_SendTalkRequest s_SendTalkPacket = new S_SendTalkRequest();
        s_SendTalkPacket.TalkRequestType = talkRequestType;

        talkPlayer.Session.Send(s_SendTalkPacket);
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
        player.Pokemons.Add(new Pokemon("Charmander", "Chevillar", 5, player.Name, -1));
        //player.Pokemons.Add(new Pokemon("Charmander", "Chevillar", 15, player.Name, -1));
        //player.Pokemons.Add(new Pokemon("Bulbasaur", "Belletti", 15, player.Name, -1));
        //player.Pokemons.Add(new Pokemon("Pikachu", "Pizteck", 1, player.Name, -1));
        //player.Pokemons.Add(new Pokemon("Squirtle", "Salamaker", 15, player.Name, -1));

        // 플레이어 아이템
        player.AddItem(ItemCategory.Item, "Potion", 1);
        player.AddItem(ItemCategory.Item, "Super Potion", 1);
        player.AddItem(ItemCategory.Item, "Hyper Potion", 1);
        player.AddItem(ItemCategory.PokeBall, "Monster Ball", 431);
        player.AddItem(ItemCategory.PokeBall, "Monster Ball", 1);
        player.AddItem(ItemCategory.PokeBall, "Monster Ball", 999);
        player.AddItem(ItemCategory.PokeBall, "Great Ball", 1);
        player.AddItem(ItemCategory.PokeBall, "Great Ball", 10);
        player.AddItem(ItemCategory.PokeBall, "Ultra Ball", 10);

        player.Money += 1000000;

        clientSession.MyPlayer = player;

        return player;
    }
}