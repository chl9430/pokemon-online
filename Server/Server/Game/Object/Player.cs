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
        List<Pokemon> pokemons;
        public ClientSession Session { get; set; }
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
