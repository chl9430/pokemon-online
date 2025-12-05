using Google.Protobuf.Protocol;
using UnityEngine;

public class PokemonMove
{
    int _curPp;
    int _maxPp;
    int _movePower;
    int _moveAccuracy;
    string _moveName;
    string _moveDescription;
    Texture2D _hitEffectImg;
    Texture2D _moveTypeImg;
    PokemonType _moveType;
    MoveCategory _moveCategory;

    public int CurPP { get { return _curPp; } set { _curPp = value; } }
    public int MaxPP { get { return _maxPp; } }
    public int MovePower { get { return _movePower; } }
    public int MoveAccuracy { get { return _moveAccuracy; } }
    public string MoveName { get { return _moveName; } }
    public string MoveDescription { get { return _moveDescription; } }
    public Texture2D MoveTypeImage { get { return _moveTypeImg; } }
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
        _moveDescription = moveSum.MoveDescription;
        _moveType = moveSum.MoveType;
        _moveCategory = moveSum.MoveCategory;

        _hitEffectImg = Managers.Resource.Load<Texture2D>($"Textures/Effect/Physical_Hit_{MoveType.ToString()}");
        _moveTypeImg = Managers.Resource.Load<Texture2D>($"Textures/UI/{moveSum.MoveType}_Icon");
    }

    public void UpdatePokemonMoveSummary(PokemonMoveSummary moveSum)
    {
        _curPp = moveSum.CurPP;
        _maxPp = moveSum.MaxPP;
        _movePower = moveSum.MovePower;
        _moveAccuracy = moveSum.MoveAccuracy;
        _moveName = moveSum.MoveName;
        _moveDescription = moveSum.MoveDescription;
        _moveType = moveSum.MoveType;
        _moveCategory = moveSum.MoveCategory;
    }
}