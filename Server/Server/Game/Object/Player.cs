using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Player : GameObject
    {
        //string _name;
        List<Pokemon> pokemons;
        Dictionary<ItemCategory, List<Item>> _items;
        PlayerTalkRoom _talkRoom;
        PrivateBattleRoom _battleRoom;
        PokemonExchangeRoom _exchangeRoom;
        NPC _talkingNpc;
        PlayerGender _gender;
        int _money;

        public ClientSession Session { get; set; }

        //public string Name { get { return _name; } set { _name = value; } }

        public PlayerGender Gender { get { return _gender; } set { _gender = value; } }

        public List<Pokemon> Pokemons
        {
            get { return pokemons; }
            set { pokemons = value; }
        }

        public Dictionary<ItemCategory, List<Item>> Items
        {
            get { return _items; }
        }

        public PlayerTalkRoom TalkRoom { get { return _talkRoom; } set { _talkRoom = value; } }

        public PrivateBattleRoom BattleRoom { get { return _battleRoom; } set { _battleRoom = value; } }

        public PokemonExchangeRoom ExchangeRoom { get { return _exchangeRoom; } set { _exchangeRoom = value; } }

        public NPC TalkingNPC { set { _talkingNpc = value; } }

        public int Money { get { return _money; } set { _money = value; } }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            pokemons = new List<Pokemon>();

            _items = new Dictionary<ItemCategory, List<Item>>();
            _items.Add(ItemCategory.Item, new List<Item>());
            _items.Add(ItemCategory.PokeBall, new List<Item>());
            _items.Add(ItemCategory.TechnicalMachine, new List<Item>());
            _items.Add(ItemCategory.Berry, new List<Item>());
            _items.Add(ItemCategory.KeyItem, new List<Item>());
        }

        public GameObject FindObject()
        {
            Vector2Int objPos = CellPos;

            if (PosInfo.MoveDir == MoveDir.Up)
                objPos += Vector2Int.up;
            else if (PosInfo.MoveDir == MoveDir.Down)
                objPos += Vector2Int.down;
            else if (PosInfo.MoveDir == MoveDir.Left)
                objPos += Vector2Int.left;
            else if (PosInfo.MoveDir == MoveDir.Right)
                objPos += Vector2Int.right;

            GameObject obj = Room.Map.Find(objPos);

            return obj;
        }

        public void TalkWithPlayer(Player otherPlayer)
        {
            _talkRoom = new PlayerTalkRoom(this, otherPlayer);
            otherPlayer.TalkRoom = _talkRoom;

            Vector2Int playerPos = new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            Vector2Int otherPlayerPos = new Vector2Int(otherPlayer.Info.PosInfo.PosX, otherPlayer.Info.PosInfo.PosY);

            Vector2Int posDiff = playerPos - otherPlayerPos;

            MoveDir otherPlayerDir;

            if (posDiff.x == 1)
                otherPlayerDir = MoveDir.Right;
            else if (posDiff.x == -1)
                otherPlayerDir = MoveDir.Left;
            else if (posDiff.y == 1)
                otherPlayerDir = MoveDir.Up;
            else
                otherPlayerDir = MoveDir.Down;

            otherPlayer.Info.PosInfo.MoveDir = otherPlayerDir;
        }

        public void AddPokemon(Pokemon pokemon)
        {
            pokemons.Add(pokemon);
        }

        public void SwitchPokemonOrder(int from, int to)
        {
            Pokemon pokemon = pokemons[from];

            pokemons[from] = pokemons[to];
            pokemons[to] = pokemon;
        }

        public PlayerInfo MakePlayerInfo()
        {
            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.ObjectInfo = Info;
            playerInfo.PlayerName = Name;
            playerInfo.PlayerGender = Gender;
            playerInfo.Money = _money;

            if (_talkingNpc != null)
                playerInfo.NpcInfo = _talkingNpc.MakeNPCInfo();

            return playerInfo;
        }

        public void AddItem(ItemCategory itemCategory, string itemName, int itemCnt)
        {
            if (itemCnt > 999)
                Console.WriteLine("Cannot add too many items!");

            if (_items.TryGetValue(itemCategory, out List<Item> categoryItems))
            {
                Item item = categoryItems.Find(item => item.ItemName == itemName && (item.ItemCount < 999));

                if (item != null)
                {
                    if (item.ItemCount + itemCnt <= 999)
                    {
                        item.ItemCount += itemCnt;
                        return;
                    }
                    else
                    {
                        int remainedCnt = itemCnt - (999 - item.ItemCount);
                        item.ItemCount = 999;

                        if (itemCategory == ItemCategory.PokeBall)
                            categoryItems.Add(new PokeBall(itemName, remainedCnt));
                    }
                }
                else
                {
                    if (itemCategory == ItemCategory.PokeBall)
                        categoryItems.Add(new PokeBall(itemName, itemCnt));
                }
            }
            else
            {
                Console.WriteLine("Cannot find item category!");
            }
        }

        public Item UseItem(ItemCategory itemCategory, int itemOrder)
        {
            Item usedItem = _items[itemCategory][itemOrder];

            if (_battleRoom != null)
            {
                _battleRoom.UsedItem = usedItem;
                _battleRoom.MyTurn = true;
            }

            usedItem.ItemCount--;

            if (usedItem.ItemCount <= 0)
                _items[itemCategory].RemoveAt(itemOrder);

            return usedItem;
        }

        public S_SellItem SellItem(ItemCategory itemCategory, int itemOrder, int itemQuantity)
        {
            S_SellItem sellItemPacket = new S_SellItem();

            Item selectedItem = _items[itemCategory][itemOrder];

            _money += selectedItem.Price / 2 * itemQuantity;

            selectedItem.ItemCount -= itemQuantity;

            if (selectedItem.ItemCount <= 0)
            {
                _items[itemCategory].RemoveAt(itemOrder);
                sellItemPacket.ItemQuantity = 0;
            }
            else
                sellItemPacket.ItemQuantity = selectedItem.ItemCount;

            sellItemPacket.Money = _money;

            return sellItemPacket;
        }
    }
}
