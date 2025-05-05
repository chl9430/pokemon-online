using Google.Protobuf.Protocol;
using System;
using UnityEngine;

public class Pokemon : BaseController, IComparable<Pokemon>
{
    int _order;
    PokemonSummary _pokemonSummary;

    public PokemonSummary PokemonSummary {  get { return _pokemonSummary; } }

    public int CompareTo(Pokemon other)
    {
        return _order.CompareTo(other._order);
    }

    public Pokemon(PokemonSummary summary)
    {
        _pokemonSummary = summary;
    }
}
