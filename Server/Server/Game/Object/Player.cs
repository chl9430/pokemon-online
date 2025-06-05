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
        PlayerGender _gender;

        public ClientSession Session { get; set; }

        public string Name { get { return _name; } set { _name = value; } }

        public PlayerGender Gender { get { return _gender; } set { _gender = value; } }

        public List<Pokemon> Pokemons
        {
            get { return pokemons; }
        }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            pokemons = new List<Pokemon>();
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
    }
}
