using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Player : GameObject
    {
        PriorityQueue<Pokemon> pokemons;
        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            pokemons = new PriorityQueue<Pokemon>();
        }

        public void PushPokemon(Pokemon pokemon)
        {
            pokemon.Order = pokemons.Count;
            pokemons.Push(pokemon);
        }
    }
}
