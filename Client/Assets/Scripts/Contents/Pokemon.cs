using Google.Protobuf.Protocol;
using System;
using UnityEngine;

public class Pokemon : MonoBehaviour
{
    PokemonSummary _pokemonSummary;

    public PokemonSummary PokemonSummary {  get { return _pokemonSummary; } }

    public Pokemon(PokemonSummary summary)
    {
        _pokemonSummary = summary;
    }
}
