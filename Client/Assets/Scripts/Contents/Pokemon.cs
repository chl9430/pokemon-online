using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    PokemonInfo _pokemonInfo;
    PokemonStat _pokemonStat;
    PokemonExpInfo _pokemonExpInfo;
    List<PokemonMove> _pokemonMoves;

    PokemonMove _selectedMove;
    PokemonMove _noPPMove;

    Texture2D _pokemonImg;
    Texture2D _pokemonBackImg;
    Texture2D _pokemonIconImg;
    Texture2D _pokemonGenderImg;
    Texture2D _pokemonStatusImg;

    public List<PokemonMove> PokemonMoves
    {
        get
        {
            return _pokemonMoves;
        }

        set
        {
            _pokemonMoves = value;
        }
    }

    public PokemonMove SelectedMove { set { _selectedMove = value; } get { return _selectedMove; } }
    public PokemonInfo PokemonInfo { get { return _pokemonInfo; } set { _pokemonInfo = value; } }
    public PokemonStat PokemonStat { get { return _pokemonStat; } set { _pokemonStat = value; } }
    public PokemonExpInfo PokemonExpInfo { get { return _pokemonExpInfo; } set { _pokemonExpInfo = value; } }

    public Texture2D PokemonImage { get { return _pokemonImg; } }
    public Texture2D PokemonBackImage { get { return _pokemonBackImg; } }
    public Texture2D PokemonIconImage { get { return _pokemonIconImg; } }
    public Texture2D PokemonGenderImage {  get { return _pokemonGenderImg; } }
    public Texture2D PokemonStatusImage { get  { return _pokemonStatusImg; } }

    public Pokemon(PokemonSummary pokemonSum)
    {
        _pokemonInfo = pokemonSum.PokemonInfo;
        _pokemonStat = pokemonSum.PokemonStat;
        _pokemonExpInfo = pokemonSum.PokemonExpInfo;
        _pokemonMoves = new List<PokemonMove>();

        foreach (PokemonMoveSummary moveSum in pokemonSum.PokemonMoves)
        {
            PokemonMove move = new PokemonMove(moveSum);
            _pokemonMoves.Add(move);
        }

        _noPPMove = new PokemonMove(pokemonSum.NoPPMove);

        _pokemonImg = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{_pokemonInfo.PokemonName}");
        _pokemonBackImg = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{_pokemonInfo.PokemonName}_Back");
        _pokemonIconImg = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{_pokemonInfo.PokemonName}_Icon");
        _pokemonGenderImg = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/PokemonGender_{_pokemonInfo.Gender}");
        _pokemonStatusImg = Managers.Resource.Load<Texture2D>($"Textures/UI/{_pokemonInfo.PokemonStatus}_Icon");
    }

    public int GetSelectedMoveIdx()
    {
        return _pokemonMoves.IndexOf(_selectedMove);
    }

    public void SetNoPPMove()
    {
        _selectedMove = _noPPMove;
    }

    public bool IsHitByAcc()
    {
        int ran = Random.Range(0, 100);

        if (ran > _selectedMove.MoveAccuracy)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void SetPokemonStatus(PokemonStatusCondition status)
    {
        PokemonInfo.PokemonStatus = status;

        _pokemonStatusImg = Managers.Resource.Load<Texture2D>($"Textures/UI/{status}_Icon");
    }
}