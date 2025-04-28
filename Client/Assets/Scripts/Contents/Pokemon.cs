using Google.Protobuf.Protocol;
using System;
using UnityEngine;

public class Pokemon : BaseController, IComparable<Pokemon>
{
    string _nickName;
    int _level;
    int _hp;
    int _exp;
    int _maxExp;
    int _order;
    GameObject _owner;
    PokemonFinalStatInfo _statInfo;

    public string NickName { get { return _nickName; } }
    public int Level { get { return _level; } }
    public int Hp { get { return _hp; } }
    public int Exp { get { return _exp; } }
    public int MaxExp { get { return _maxExp; } }
    public int Order { set { _order = value; } get { return _order; } }
    public GameObject Owner { get { return _owner; } }
    public PokemonFinalStatInfo FinalStatInfo { get { return _statInfo; } }

    public int CompareTo(Pokemon other)
    {
        return _order.CompareTo(other._order);
    }

    public Pokemon(string nickName, int level, int hp, int order, GameObject owner, PokemonFinalStatInfo statInfo)
    {
        _nickName = nickName;
        _level = level;
        _hp = hp;
        _exp = 0;
        _maxExp = level * 10;
        _order = order;
        _owner = owner;
        _statInfo = statInfo;
    }
}
