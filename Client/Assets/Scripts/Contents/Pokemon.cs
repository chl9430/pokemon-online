using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    PokemonSummary _pokemonSummary;

    PokemonInfo _pokemonInfo;
    PokemonSkill _pokemonSkill;
    PokemonStat _pokemonStat;
    List<PokemonMove> _moves;

    PokemonMove _selectedMove;

    Texture2D pokemonImg;
    Texture2D pokemonBackImg;

    public List<PokemonMove> Moves
    {
        get
        {
            return _moves;
        }

        set
        {
            _moves = value;
        }
    }

    public PokemonMove SelectedMove { set { _selectedMove = value; } get { return _selectedMove; } }

    public PokemonSummary PokemonSummary 
    {  
        get 
        {
            return _pokemonSummary;
        }
    }
    public PokemonInfo PokemonInfo { get { return _pokemonInfo; } }
    public PokemonSkill PokemonSkill { get { return _pokemonSkill; } }
    public PokemonStat PokemonStat { get { return _pokemonStat; } set { _pokemonStat = value; } }

    public Texture2D PokemonImage { get { return pokemonImg; } }
    public Texture2D PokemonBackImage { get { return pokemonBackImg; } }

    public Pokemon(PokemonSummary summary)
    {
        _pokemonSummary = summary;
        _pokemonInfo = summary.Info;
        _pokemonSkill = summary.Skill;
        _pokemonStat = summary.Skill.Stat;
        _moves = new List<PokemonMove>();

        IList moves = summary.BattleMoves;
        for (int i = 0; i < moves.Count; i++)
        {
            PokemonBattleMove battleMove = (PokemonBattleMove)moves[i];

            PokemonMove move = new PokemonMove(battleMove.MaxPP, battleMove.MovePower, battleMove.MoveAccuracy, battleMove.MoveName, battleMove.MoveType, battleMove.MoveCategory);
            _moves.Add(move);
        }

        pokemonImg = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{summary.Info.PokemonName}");
        pokemonBackImg = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{summary.Info.PokemonName}_Back");
    }

    public bool IsHitByAcc()
    {
        int ran = Random.Range(0, 100);

        if (ran > _selectedMove.Accuracy)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}