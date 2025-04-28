using Google.Protobuf.Protocol;
using UnityEngine;

public class PokemonList : MonoBehaviour
{
    PriorityQueue<Pokemon> _pokemons;

    public PriorityQueue<Pokemon> Pokemons { get { return _pokemons; } }

    void Awake()
    {
        _pokemons = new PriorityQueue<Pokemon>();
    }
}
