using Google.Protobuf;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
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
        List<Pokemon> pokemons;
        Dictionary<ItemCategory, List<ItemBase>> _items;
        PlayerTalkRoom _talkRoom;
        PokemonBattleRoom _pokemonBattleRoom;
        PrivateBattleRoom _battleRoom;
        PokemonExchangeRoom _exchangeRoom;
        GameObject _talkingNpc;
        PlayerGender _gender;
        int _money;
        int _npcNumber;

        public ClientSession Session { get; set; }

        public PlayerGender Gender { get { return _gender; } set { _gender = value; } }

        public List<Pokemon> Pokemons
        {
            get { return pokemons; }
            set { pokemons = value; }
        }

        public Dictionary<ItemCategory, List<ItemBase>> Items
        {
            get { return _items; }
        }

        public PlayerTalkRoom TalkRoom { get { return _talkRoom; } set { _talkRoom = value; } }

        public PokemonBattleRoom PokemonBattleRoom { get { return _pokemonBattleRoom; } set { _pokemonBattleRoom = value; } }

        public PrivateBattleRoom BattleRoom { get { return _battleRoom; } set { _battleRoom = value; } }

        public PokemonExchangeRoom ExchangeRoom { get { return _exchangeRoom; } set { _exchangeRoom = value; } }

        public GameObject TalkingNPC { set { _talkingNpc = value; } get { return _talkingNpc; } }

        public int Money { get { return _money; } set { _money = value; } }

        public int NPCNumber { get { return _npcNumber; } set { _npcNumber = value; } }

        public Player()
        {
            Info.ObjectType = GameObjectType.Player;
            pokemons = new List<Pokemon>();

            _items = new Dictionary<ItemCategory, List<ItemBase>>();
            _items.Add(ItemCategory.Item, new List<ItemBase>());
            _items.Add(ItemCategory.PokeBall, new List<ItemBase>());
            _items.Add(ItemCategory.TechnicalMachine, new List<ItemBase>());
            _items.Add(ItemCategory.Berry, new List<ItemBase>());
            _items.Add(ItemCategory.KeyItem, new List<ItemBase>());
        }

        public OtherPlayerInfo MakeOtherPlayerInfo()
        {
            OtherPlayerInfo info = new OtherPlayerInfo();
            info.ObjectInfo = Info;
            info.PlayerGender = Gender;
            info.PlayerName = Name;

            return info;
        }

        public PlayerInfo MakePlayerInfo()
        {
            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.ObjectInfo = Info;
            playerInfo.PlayerName = Name;
            playerInfo.PlayerGender = Gender;
            playerInfo.Money = _money;

            if (_talkingNpc != null)
                playerInfo.NpcInfo = ((NPC)_talkingNpc).MakeNPCInfo();

            // 인벤토리 딕셔너리를 모두 가져옴
            foreach (var pair in _items)
            {
                CategoryInventory categoryInventory = new CategoryInventory();

                foreach (ItemBase item in pair.Value)
                {
                    categoryInventory.CategoryItemSums.Add(item.MakeItemSummary());
                }

                playerInfo.Inventory.Add((int)pair.Key, categoryInventory);
            }

            // 포켓몬 리스트 채우기
            if (Info.PosInfo.State == CreatureState.Fight)
            {
                List<Pokemon> battlePokemons = GetBattlePokemonOrderArray();
                
                // 야생 포켓몬 이펙트 중에 엔터 플레이어시 BattleRoom이 null이라 오류가 난다.
                foreach (Pokemon pokemon in battlePokemons)
                    playerInfo.PokemonSums.Add(pokemon.MakePokemonSummary());
            }
            else
            {
                foreach (Pokemon pokemon in pokemons)
                    playerInfo.PokemonSums.Add(pokemon.MakePokemonSummary());
            }

            return playerInfo;
        }

        public List<Pokemon> GetBattlePokemonOrderArray()
        {
            List<Pokemon> battlePokemons = [.. pokemons];

            // 선두에 기절 포켓몬이 있으면 그렇지 않은 포켓몬과 교체해준다.
            if (battlePokemons[0].PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
            {
                for (int i = 0; i < battlePokemons.Count; i++)
                {
                    if (battlePokemons[i].PokemonInfo.PokemonStatus != PokemonStatusCondition.Fainting)
                    {
                        Pokemon temp = battlePokemons[0];
                        battlePokemons[0] = battlePokemons[i];
                        battlePokemons[i] = temp;
                        break;
                    }
                }
            }

            return battlePokemons;
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

        public void AddItem(ItemCategory itemCategory, string itemName, int itemCnt, IMessage packet = null)
        {
            if (itemCnt > 999)
                Console.WriteLine("Cannot add too many items!");

            if (DataManager.ItemBaseDict.TryGetValue(itemCategory, out List<ItemBase> itemBases))
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                };

                ItemBase itemBase = itemBases.Find(item => item._name == itemName);
                string json = JsonConvert.SerializeObject(itemBase, settings);
                ItemBase newItemBase = JsonConvert.DeserializeObject<ItemBase>(json, settings);

                List<ItemBase> items = _items[itemCategory];

                if (items != null)
                {
                    ItemBase foundItemBase = items.Find(item => item._name == itemName && (item._itemCnt < 999));
                    int foundItemIdx = items.IndexOf(foundItemBase);

                    if (foundItemBase != null)
                    {
                        if (foundItemBase._itemCnt + itemCnt <= 999)
                        {
                            foundItemBase._itemCnt += itemCnt;

                            if (packet != null)
                            {
                                ((S_BuyItem)packet).FoundItemCnt = foundItemBase._itemCnt;
                                ((S_BuyItem)packet).FoundItemIdx = foundItemIdx;
                                ((S_BuyItem)packet).CreateNewIdx = false;
                            }
                        }
                        else
                        {
                            int remainedCnt = itemCnt - (999 - foundItemBase._itemCnt);
                            foundItemBase._itemCnt = 999;

                            newItemBase._itemCnt = remainedCnt;
                            items.Add(newItemBase);

                            if (packet != null)
                            {
                                ((S_BuyItem)packet).FoundItemCnt = foundItemBase._itemCnt;
                                ((S_BuyItem)packet).NewItemCnt = newItemBase._itemCnt;
                                ((S_BuyItem)packet).FoundItemIdx = foundItemIdx;
                                ((S_BuyItem)packet).CreateNewIdx = true;
                            }
                        }
                    }
                    else
                    {
                        newItemBase._itemCnt = itemCnt;
                        items.Add(newItemBase);

                        if (packet != null)
                        {
                            ((S_BuyItem)packet).NewItemCnt = newItemBase._itemCnt;
                            ((S_BuyItem)packet).FoundItemIdx = foundItemIdx;
                            ((S_BuyItem)packet).CreateNewIdx = true;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Cannot ItemBase!");
            }
        }

        public S_UseItemInListScene UseItem(ItemCategory itemCategory, int itemOrder, int pokemonOrder)
        {
            S_UseItemInListScene s_UseItemPacket = new S_UseItemInListScene();

            Pokemon targetPokemon = null;

            if (PosInfo.State == CreatureState.Fight)
                targetPokemon = _battleRoom.Pokemons[pokemonOrder];
            else
                targetPokemon = pokemons[pokemonOrder];

            ItemBase item = _items[itemCategory][itemOrder];
            item.UseItem(s_UseItemPacket, targetPokemon, this);
            item._itemCnt--;

            if (item._itemCnt == 0)
                _items[itemCategory].RemoveAt(itemOrder);

            s_UseItemPacket.ItemSum = item.MakeItemSummary();
            s_UseItemPacket.TargetPokemonSum = targetPokemon.MakePokemonSummary();

            return s_UseItemPacket;
        }

        public S_SellItem SellItem(ItemCategory itemCategory, int itemOrder, int itemQuantity)
        {
            S_SellItem sellItemPacket = new S_SellItem();

            ItemBase selectedItem = _items[itemCategory][itemOrder];

            _money += selectedItem._price / 2 * itemQuantity;

            selectedItem._itemCnt -= itemQuantity;

            if (selectedItem._itemCnt <= 0)
            {
                _items[itemCategory].RemoveAt(itemOrder);
                sellItemPacket.ItemQuantity = 0;
            }
            else
                sellItemPacket.ItemQuantity = selectedItem._itemCnt;

            sellItemPacket.Money = _money;

            return sellItemPacket;
        }
    }
}
