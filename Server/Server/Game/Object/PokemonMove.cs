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
        string _moveDescription;

        PokemonMoveDictData _moveDictData;

        public int CurPP { get { return _curPp; } set { _curPp = value; } }
        public int MaxPP { get { return _maxPp; } }
        public int MovePower { get { return _movePower; } }
        public int MoveAccuracy { get { return _moveAccuracy; } }
        public float CriticalRate { get { return _criticalRate; } }
        public string MoveName { get { return _moveName; } }
        public PokemonType MoveType { get { return _moveType; } }
        public MoveCategory MoveCategory { get { return _moveCategory; } }

        public PokemonMove(string moveName, float criticalRate = 6.25f)
        {
            if (DataManager.PokemonMoveDict.TryGetValue(moveName, out _moveDictData))
            {
                _curPp = _moveDictData.maxPP;
                _maxPp = _moveDictData.maxPP;
                _movePower = _moveDictData.movePower;
                _moveAccuracy = _moveDictData.moveAccuracy;
                _criticalRate = criticalRate;
                _moveName = moveName;
                _moveType = _moveDictData.moveType;
                _moveCategory = _moveDictData.moveCategory;
                _moveDescription = _moveDictData.moveDescription;
            }
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
            moveSum.MoveDescription = _moveDescription;

            return moveSum;
        }
    }
}
