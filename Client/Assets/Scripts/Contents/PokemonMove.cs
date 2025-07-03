using Google.Protobuf.Protocol;
using UnityEngine;

public class PokemonMove
{
    int _curPp;
    int _maxPp;
    int _movePower;
    int _moveAccuracy;
    string _moveName;
    Texture2D _hitEffectImg;
    PokemonType _moveType;
    MoveCategory _moveCategory;

    public int CurPP { get { return _curPp; } set { _curPp = value; } }
    public int MaxPP { get { return _maxPp; } }
    public int MovePower { get { return _movePower; } }
    public int MoveAccuracy { get { return _moveAccuracy; } }
    public string MoveName { get { return _moveName; } }
    public Texture2D HitEffectImage { get { return _hitEffectImg; } }
    public PokemonType MoveType { get { return _moveType; } }
    public MoveCategory MoveCategory { get { return _moveCategory; } }

    public PokemonMove(PokemonMoveSummary moveSum)
    {
        _curPp = moveSum.CurPP;
        _maxPp = moveSum.MaxPP;
        _movePower = moveSum.MovePower;
        _moveAccuracy = moveSum.MoveAccuracy;
        _moveName = moveSum.MoveName;
        _moveType = moveSum.MoveType;
        _moveCategory = moveSum.MoveCategory;

        _hitEffectImg = Managers.Resource.Load<Texture2D>($"Textures/Effect/Physical_Hit_{MoveType.ToString()}");
    }
}