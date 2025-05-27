using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public class PokemonMove
{
    int _maxPP;
    int _pp;
    int _movePower;
    int _accuracy;
    string _moveName;
    PokemonType _moveType;
    MoveCategory _moveCategory;

    Texture2D _hitEffectImg;

    public int PP { get { return _pp; } set { _pp = value; } }
    public int MaxPP { get { return _maxPP; } }
    public int MovePower { get { return _movePower; } }
    public int Accuracy { get { return _accuracy; } }
    public string MoveName { get { return _moveName; } }
    public PokemonType MoveType { get { return _moveType; } }
    public MoveCategory MoveCategory { get { return _moveCategory; } }
    public Texture2D HitEffectImage { get { return _hitEffectImg; } }

    public PokemonMove(int maxPP, int movePower, int accuracy, string moveName, PokemonType moveType, MoveCategory moveCategory)
    {
        _pp = maxPP;
        _maxPP = maxPP;
        _movePower = movePower;
        _accuracy = accuracy;
        _moveName = moveName;
        _moveType = moveType;
        _moveCategory = moveCategory;

        _hitEffectImg = Managers.Resource.Load<Texture2D>("Textures/Effect/Hit");
    }
}
