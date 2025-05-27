using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 서버 연동 시 삭제
public struct PokemonBaseStat
{
    public int MaxHP;
    public int Attack;
    public int Defense;
    public int SpecialAttack;
    public int SpecialDefense;
    public int Speed;
}

public struct LevelUpStatusRate
{
    public int MaxHP;
    public int Attack;
    public int Defense;
    public int SpecialAttack;
    public int SpecialDefense;
    public int Speed;
}

public class Pokemon
{
    // 서버 연동 시 삭제
    PokemonBaseStat baseStat;
    PokemonSummary _pokemonSummary;
    PokemonInfo _pokemonInfo;
    PokemonSkill _pokemonSkill;
    PokemonStat _pokemonStat;
    List<PokemonMove> _moves;

    Pokemon _enemyPokemon;
    PokemonMove _selectedMove;

    Texture2D pokemonImg;
    Texture2D pokemonBackImg;

    int _curLvEXP;

    public PokemonBaseStat PokemonBaseStat { set { baseStat = value; } }

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

        set
        {
            _pokemonSummary = value;
        }
    }
    public PokemonInfo PokemonInfo { get { return _pokemonInfo; } }
    public PokemonSkill PokemonSkill { get { return _pokemonSkill; } }
    public PokemonStat PokemonStat { get { return _pokemonStat; } }

    public Texture2D PokemonImage { get { return pokemonImg; } }
    public Texture2D PokemonBackImage { get { return pokemonBackImg; } }

    public int CurLevelEXP {  get { return _curLvEXP; } }

    public Pokemon(PokemonSummary summary)
    {
        _pokemonSummary = summary;
        _pokemonInfo = summary.Info;
        _pokemonSkill = summary.Skill;
        _pokemonStat = summary.Skill.Stat;

        _curLvEXP = 0;

        pokemonImg = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{summary.Info.PokemonName}");
        pokemonBackImg = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{summary.Info.PokemonName}_Back");
    }

    public void SetStat()
    {
        PokemonStat stat = _pokemonSummary.Skill.Stat;

        stat.MaxHp = (int)((((float)baseStat.MaxHP * 2f) * _pokemonInfo.Level / 100f) + 10f + _pokemonInfo.Level);
        stat.Attack = (int)((((float)baseStat.Attack * 2) * (_pokemonInfo.Level) / 100f) + 5f);
        stat.Defense = (int)((((float)baseStat.Defense * 2) * (_pokemonInfo.Level) / 100f) + 5f);
        stat.SpecialAttack = (int)((((float)baseStat.SpecialAttack * 2) * (_pokemonInfo.Level) / 100f) + 5f);
        stat.SpecialDefense = (int)((((float)baseStat.SpecialDefense * 2) * (_pokemonInfo.Level) / 100f) + 5f);
        stat.Speed = (int)((((float)baseStat.Speed * 2) * (_pokemonInfo.Level) / 100f) + 5f);

        PokemonSkill skill = _pokemonSummary.Skill;

        int totalExp = 0;

        for (int i = 1; i <= _pokemonInfo.Level; i++)
        {
            totalExp += (i - 1) * 100;
        }

        skill.TotalExp = totalExp;
        skill.RemainLevelExp = (_pokemonInfo.Level == 100) ? 0 : _pokemonInfo.Level * 100;
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

    public void GetEXP(int exp)
    {
        _curLvEXP += exp;

        _pokemonSkill.TotalExp += exp;

        _pokemonSkill.RemainLevelExp -= exp;   
    }

    public LevelUpStatusRate PokemonLevelUp()
    {
        PokemonStat prevStat = new PokemonStat()
        {
            Hp = _pokemonStat.Hp,
            MaxHp = _pokemonStat.MaxHp,
            Attack = _pokemonStat.Attack,
            Defense = _pokemonStat.Defense,
            SpecialAttack = _pokemonStat.SpecialAttack,
            SpecialDefense = _pokemonStat.SpecialDefense,
            Speed = _pokemonStat.Speed,
        };

        _pokemonInfo.Level += 1;
        _curLvEXP = 0;

        _pokemonSkill.RemainLevelExp = _pokemonInfo.Level * 100;

        SetStat();
        _pokemonStat.Hp += (_pokemonStat.MaxHp - prevStat.MaxHp);

        LevelUpStatusRate rate = new LevelUpStatusRate()
        {
            MaxHP = _pokemonStat.MaxHp - prevStat.MaxHp,
            Attack = _pokemonStat.Attack - prevStat.Attack,
            Defense = _pokemonStat.Defense - prevStat.Defense,
            SpecialAttack = _pokemonStat.SpecialAttack - prevStat.SpecialAttack,
            SpecialDefense = _pokemonStat.SpecialDefense - prevStat.SpecialDefense,
            Speed = _pokemonStat.Speed - prevStat.Speed,
        };

        return rate;
    }

    public void GetDamaged(int damage, MoveCategory moveCategory)
    {
        if (damage <= 0)
            damage = 1;

        int hp = _pokemonSummary.Skill.Stat.Hp;

        hp -= damage;

        _pokemonSummary.Skill.Stat.Hp = hp;
    }
}
