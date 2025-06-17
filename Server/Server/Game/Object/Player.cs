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
        string _name;
        List<Pokemon> pokemons;
        Dictionary<ItemCategory, List<Item>> _items;
        PlayerGender _gender;

        public ClientSession Session { get; set; }

        public string Name { get { return _name; } set { _name = value; } }

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

            return playerInfo;
        }

        public void AddItem(ItemCategory itemCategory, string itemName, int itemCnt)
        {
            if (itemCnt > 99)
                Console.WriteLine("Cannot add too many items!");

            if (_items.TryGetValue(itemCategory, out List<Item> categoryItems))
            {
                foreach (Item item in categoryItems)
                {
                    if (item.ItemName == itemName)
                    {
                        if (item.ItemCount + itemCnt <= 99)
                        {
                            item.ItemCount += itemCnt;
                            return;
                        }
                    }
                }

                if (itemCategory == ItemCategory.PokeBall)
                    categoryItems.Add(new PokeBall(itemName, itemCnt));
            }
            else
            {
                Console.WriteLine("Cannot find item category!");
            }
        }
    }
}
