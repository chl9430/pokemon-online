using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PokemonMove
    {
        int _curPp;
        int _maxPp;
        int _movePower;
        int _moveAccuracy;
        float _criticalRate;
        string _moveName;
        PokemonType _moveType;
        MoveCategory _moveCategory;

        public int CurPP { get { return _curPp; } set { _curPp = value; } }
        public int MaxPP { get { return _maxPp; } }
        public int MovePower { get { return _movePower; } }
        public int MoveAccuracy { get { return _moveAccuracy; } }
        public float CriticalRate { get { return _criticalRate; } }
        public string MoveName { get { return _moveName; } }
        public PokemonType MoveType { get { return _moveType; } }
        public MoveCategory MoveCategory { get { return _moveCategory; } }

        public PokemonMove(int maxPp, int movePower, int moveAccuracy, string moveName, PokemonType moveType, MoveCategory moveCategory, float criticalRate = 6.25f)
        {
            _curPp = maxPp;
            _maxPp = maxPp;
            _movePower = movePower;
            _moveAccuracy = moveAccuracy;
            _criticalRate = criticalRate;
            _moveName = moveName;
            _moveType = moveType;
            _moveCategory = moveCategory;
        }

        public PokemonMoveSummary MakePokemonMoveSummary()
        {
            PokemonMoveSummary moveSum = new PokemonMoveSummary();
            moveSum.CurPP = _curPp;
            moveSum.MaxPP = _maxPp;
            moveSum.MovePower = _movePower;
            moveSum.MoveAccuracy = _moveAccuracy;
            moveSum.CriticalRate = _criticalRate;
            moveSum.MoveName = _moveName;
            moveSum.MoveType = _moveType;
            moveSum.MoveCategory = _moveCategory;

            return moveSum;
        }
    }
}
