using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server
{
    public class PrivateBattleRoom
    {
        int _expPerPokemon;
        int _remainedExp;
        int _escapeTurnCnt;
        Random _random;
        List<Pokemon> _pokemons;
        Player _player;
        Pokemon _myPokemon;
        Pokemon _wildPokemon;

        bool _myTurn = false;
        bool _enemyTurn = false;

        int _curExpIdx = 0;
        int _curEvolutionIdx = 0;
        List<Pokemon> _getExpPokemons;
        List<Pokemon> _evolvePokemons;
        Pokemon _curEvolvePokemon;

        PokemonMove _learnableMove;

        Item _usedItem;

        public PrivateBattleRoom(Player player, List<Pokemon> pokemons)
        {
            _escapeTurnCnt = 1;
            _random = new Random();
            _player = player;
            _pokemons = new List<Pokemon>();
            player.BattleRoom = this;

            _getExpPokemons = new List<Pokemon>();
            _evolvePokemons = new List<Pokemon>();

            foreach (Pokemon pokemon in pokemons)
            {
                _pokemons.Add(pokemon);
            }

            // 선두에 기절 포켓몬이 있으면 그렇지 않은 포켓몬과 교체해준다.
            if (_pokemons[0].PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
            {
                for (int i = 0; i < _pokemons.Count; i++)
                {
                    if (_pokemons[i].PokemonInfo.PokemonStatus != PokemonStatusCondition.Fainting)
                    {
                        Pokemon temp = _pokemons[0];
                        _pokemons[0] = _pokemons[i];
                        _pokemons[i] = temp;
                        break;
                    }
                }
            }

            _myPokemon = _pokemons[0];
            _getExpPokemons.Add(_myPokemon);
        }

        public List<Pokemon> Pokemons {  get { return _pokemons; } }

        public Pokemon MyPokemon { get { return _myPokemon; } }

        public Pokemon WildPokemon { get { return _wildPokemon; } }

        public bool MyTurn { set { _myTurn = value; } }

        public List<Pokemon> EvolvePokemons {  get { return _evolvePokemons; } }

        public PokemonMove LearnableMove { get { return _learnableMove; } set { _learnableMove = value; } }

        public Item UsedItem { set { _usedItem = value; } }

        public void MakeWildPokemon(int roomId, int bushNum)
        {
            if (DataManager.WildPKMLocationDict.TryGetValue(roomId, out BushInfo[] bushInfos))
            {
                BushInfo bushInfo = bushInfos[bushNum - 1];
                WildPokemonAppearInfo[] wildPokemonInfos = bushInfo.wildPokemons;
                WildPokemonAppearInfo wildPokemonInfo = wildPokemonInfos[0];

                int ran = _random.Next(100);

                int totalRateCnt = 0;

                for (int i = 0; i < wildPokemonInfos.Length; i++)
                {
                    int rateCnt = 0;
                    int appearRate = wildPokemonInfos[i].appearRate;

                    bool found = false;

                    while (rateCnt < appearRate)
                    {
                        if (totalRateCnt != ran)
                        {
                            totalRateCnt++;
                            rateCnt++;
                        }
                        else
                        {
                            wildPokemonInfo = wildPokemonInfos[i];
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        break;
                }

                _wildPokemon = new Pokemon(wildPokemonInfo.pokemonName, wildPokemonInfo.pokemonName, wildPokemonInfo.pokemonLevel, _player.Name, _player.Id);
            }
            else
            {
                Console.WriteLine("Cannot find Location Data!");
            }
        }

        public S_SwitchBattlePokemon SwitchBattlePokemon(int from, int to)
        {
            S_SwitchBattlePokemon switchPokemonPacket = new S_SwitchBattlePokemon();

            if (_pokemons[0].PokemonInfo.PokemonStatus != PokemonStatusCondition.Fainting)
            {
                _myTurn = true;
                //_attackPokemon = WildPokemon;
                //_defensePokemon = _pokemons[to];
                SetWildPokemonSelectedMove();
            }

            Pokemon prevPokemon = _pokemons[from];
            _pokemons[from] = _pokemons[to];
            _pokemons[to] = prevPokemon;

            _myPokemon = _pokemons[0];

            // 기절해서 벤치에 들어간 포켓몬을 exp리스트에서 삭제한다.
            if (prevPokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
            {
                _getExpPokemons.Remove(prevPokemon);
            }

            // 벤치에서 나온 포켓몬이 exp리스트에 존재하면 0번째로 옮긴다.
            if (_getExpPokemons.Contains(_myPokemon) == true)
            {
                _getExpPokemons.Remove(_myPokemon);
                _getExpPokemons.Insert(0, _myPokemon);
            }
            // 벤치에서 나온 포켓몬이 exp리스트에 존재하지 않으면 0번째에 삽입한다.
            else
            {
                _getExpPokemons.Insert(0, _myPokemon);
            }

            switchPokemonPacket.PlayerInfo = _player.MakePlayerInfo();
            switchPokemonPacket.EnemyPokemonSum = _wildPokemon.MakePokemonSummary();

            foreach (Pokemon pokemon in _pokemons)
                switchPokemonPacket.MyPokemonSums.Add(pokemon.MakePokemonSummary());

            switchPokemonPacket.PrevPokemonSum = prevPokemon.MakePokemonSummary();

            return switchPokemonPacket;
        }

        public S_ProcessTurn ProcessTurn(int moveOrder, Player player)
        {
            S_ProcessTurn processTurnPacket = new S_ProcessTurn();
            processTurnPacket.CanUseMove = true;

            if (!_myTurn && !_enemyTurn)
            {
                if (moveOrder != -1)
                {
                    if (_myPokemon.PokemonMoves[moveOrder].CurPP == 0)
                    {
                        processTurnPacket.CanUseMove = false;
                        return processTurnPacket;
                    }
                }

                if (IsMyPokemonFast())
                {
                    processTurnPacket.IsMyPokemon = true;
                }
                else
                {
                    processTurnPacket.IsMyPokemon = false;
                }
            }
            else if (_myTurn)
            {
                processTurnPacket.IsMyPokemon = false;
            }
            else if (_enemyTurn)
            {
                processTurnPacket.IsMyPokemon = true;
            }

            UsePokemonMove(processTurnPacket, moveOrder, processTurnPacket.IsMyPokemon);

            return processTurnPacket;
        }

        public void UsePokemonMove(S_ProcessTurn processTurnPacket, int moveOrder, bool isMyPokemon)
        {
            Pokemon attackPokemon = isMyPokemon == true ? _myPokemon : _wildPokemon;
            Pokemon defensePokemon = isMyPokemon == true ? _wildPokemon : _myPokemon;
            PokemonMove attackMove;
            int usedMoveOrder;

            if (isMyPokemon)
            {
                _myTurn = true;
                attackMove = moveOrder != -1 ? attackPokemon.PokemonMoves[moveOrder] : attackPokemon.NoPPMove;
                usedMoveOrder = moveOrder;
                processTurnPacket.UsedMoveOrder = usedMoveOrder;
            }
            else
            {
                _enemyTurn = true;
                attackMove = SetWildPokemonSelectedMove();
                usedMoveOrder = _wildPokemon.FindMoveIndex(attackMove);
                processTurnPacket.UsedMoveOrder = usedMoveOrder;
            }

            processTurnPacket.DefensePokemonSum = defensePokemon.MakePokemonSummary();

            if (attackPokemon.UseMove(usedMoveOrder))
            {
                processTurnPacket.IsHit = true;

                float typeEffectiveModifier = GetTypeEffectivenessMultiplier(attackPokemon, defensePokemon, attackMove);
                float criticalModifier = GetCriticalModifier(attackMove);
                float typeEqualModifier = GetTypeEqualModifier(attackMove, attackPokemon);
                float damageModifier = typeEffectiveModifier * criticalModifier * typeEqualModifier;

                processTurnPacket.TypeEffectiveness = typeEffectiveModifier;
                processTurnPacket.IsCriticalHit = criticalModifier == 1.5f ? true : false;
                ChangeBattlePokemonHp(attackMove, attackPokemon, defensePokemon, damageModifier);

                if (defensePokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                {
                    processTurnPacket.UsedMoveSum = attackMove.MakePokemonMoveSummary();
                    processTurnPacket.IsTurnFinish = true;
                    _myTurn = false;
                    _enemyTurn = false;

                    return;
                }
            }
            else
            {
                processTurnPacket.IsHit = false;
            }

            processTurnPacket.UsedMoveSum = attackMove.MakePokemonMoveSummary();

            // 공격, 수비 포켓몬 바꾸기
            if (!_myTurn || !_enemyTurn)
            {
                processTurnPacket.IsTurnFinish = false;
            }
            else // 턴이 끝났다면 턴 초기화
            {
                _escapeTurnCnt++;

                if (_escapeTurnCnt > 10)
                    _escapeTurnCnt = 10;

                processTurnPacket.IsTurnFinish = true;
                _myTurn = false;
                _enemyTurn = false;
            }
        }

        List<PokemonMove> FindAvailableMove(Pokemon pokemon)
        {
            List<PokemonMove> availableMoves = new List<PokemonMove>();

            for (int i = 0; i < pokemon.PokemonMoves.Count; i++)
            {
                PokemonMove move = pokemon.PokemonMoves[i];

                if (move.CurPP == 0)
                    continue;
                else
                    availableMoves.Add(move);
            }

            return availableMoves;
        }

        PokemonMove SetWildPokemonSelectedMove()
        {
            List<PokemonMove> availableMoves = FindAvailableMove(_wildPokemon);

            int ranMoveOrder = _random.Next(0, availableMoves.Count);

            if (availableMoves.Count > 0)
                return availableMoves[ranMoveOrder];
            else
                return _wildPokemon.NoPPMove;
        }

        public void ChangeBattlePokemonHp(PokemonMove move, Pokemon attackPokemon, Pokemon defensePokemon, float damageModifier)
        {
            int finalDamage = 0;

            if (damageModifier == 0f)
            {
                return;
            }
            else
            {
                finalDamage = CalFinalDamage(move, attackPokemon, defensePokemon, damageModifier);

                defensePokemon.GetDamaged(finalDamage);
            }
        }

        float GetCriticalModifier(PokemonMove move)
        {
            float criticalModifier;

            float rawRandom = (float)(_random.NextDouble() * (100.00 - 0.00) + 0.00);
            float ran = (float)Math.Round(rawRandom, 2, MidpointRounding.AwayFromZero);

            if (ran <= move.CriticalRate)
            {
                criticalModifier = 1.5f;
            }
            else
            {
                criticalModifier = 1;
            }

            return criticalModifier;
        }

        float GetTypeEqualModifier(PokemonMove move, Pokemon attackPokemon)
        {
            float typeEqualModifier;

            if (attackPokemon.PokemonInfo.Type1 == move.MoveType)
                typeEqualModifier = 1.5f;
            else if (attackPokemon.PokemonInfo.Type2 == move.MoveType)
                typeEqualModifier = 1.5f;
            else
                typeEqualModifier = 1f;

            return typeEqualModifier;
        }

        float GetTypeEffectivenessMultiplier(Pokemon attackPokemon, Pokemon defensePokemon, PokemonMove attackMove)
        {
            float multiplier = 1f;

            multiplier *= PokemonTypeChecker.Instance.GetEffectiveness(attackMove.MoveType, defensePokemon.PokemonInfo.Type1);

            if (defensePokemon.PokemonInfo.Type2 != PokemonType.TypeNone)
                multiplier *= PokemonTypeChecker.Instance.GetEffectiveness(attackMove.MoveType, defensePokemon.PokemonInfo.Type2);

            return multiplier;
        }

        int CalFinalDamage(PokemonMove attackMove, Pokemon attackPokemon, Pokemon defensePokemon, float damageModifier)
        {
            float finalDamage = 0;

            if (attackMove.MoveCategory == MoveCategory.Physical)
                finalDamage = ((
                ((((float)attackPokemon.PokemonInfo.Level) * 2f / 5f) + 2f)
                * ((float)attackMove.MovePower)
                * ((float)attackPokemon.PokemonStat.Attack) / ((float)defensePokemon.PokemonStat.Defense)
                ) / 50f + 2f);
            else if (attackMove.MoveCategory == MoveCategory.Special)
                finalDamage = ((
                ((((float)attackPokemon.PokemonInfo.Level) * 2f / 5f) + 2f)
                * ((float)attackMove.MovePower)
                * ((float)attackPokemon.PokemonStat.SpecialAttack) / ((float)defensePokemon.PokemonStat.SpecialDefense)
                ) / 50f + 2f);

            finalDamage = finalDamage * damageModifier;

            if ((int)finalDamage <= 0)
                finalDamage = 1f;

            return (int)finalDamage;
        }

        public S_GetEnemyPokemonExp GetExp()
        {
            S_GetEnemyPokemonExp getExpPacket = new S_GetEnemyPokemonExp();
            getExpPacket.GotExpPokemonSum = _getExpPokemons[_curExpIdx].MakePokemonSummary();

            //int exp = (112 * _wildPokemon.PokemonInfo.Level) / 7;
            _remainedExp = (int)Math.Ceiling(1000f / ((float)_getExpPokemons.Count));

            getExpPacket.Exp = _remainedExp;

            return getExpPacket;
        }

        public S_CheckAndApplyRemainedExp CheckAndApplyExp()
        {
            S_CheckAndApplyRemainedExp s_CheckAndApplyExpPacket = new S_CheckAndApplyRemainedExp();

            Pokemon expPokemon = _getExpPokemons[_curExpIdx];
            int finalExp;

            if (_curExpIdx == 0)
                s_CheckAndApplyExpPacket.IsMainPokemon = true;
            else
                s_CheckAndApplyExpPacket.IsMainPokemon = false;

            if (_remainedExp > expPokemon.ExpInfo.RemainExpToNextLevel)
                finalExp = expPokemon.ExpInfo.RemainExpToNextLevel;
            else
                finalExp = _remainedExp;

            _remainedExp -= finalExp;

            //if (_remainedExp <= 0)
            //    _curExpIdx++;

            s_CheckAndApplyExpPacket.FinalExp = finalExp;

            bool isLevelUp = expPokemon.GetExpAndCheckLevelUp(finalExp);

            if (isLevelUp)
            {
                s_CheckAndApplyExpPacket.StatDiff = expPokemon.LevelUp();
                _learnableMove = expPokemon.CheckNewLearnableMove();

                if (_learnableMove != null)
                    s_CheckAndApplyExpPacket.NewMoveSum = _learnableMove.MakePokemonMoveSummary();

                if (expPokemon.CanPokemonEvolve() && !_evolvePokemons.Contains(expPokemon))
                    _evolvePokemons.Add(expPokemon);
            }

            s_CheckAndApplyExpPacket.ExpPokemonSum = expPokemon.MakePokemonSummary();

            return s_CheckAndApplyExpPacket;
        }

        public bool CheckExpPokemons()
        {
            _curExpIdx++;

            if (_getExpPokemons.Count <= _curExpIdx)
                return false;
            else
                return true;
        }

        public Pokemon GetExpPokemon()
        {
            return _getExpPokemons[_curExpIdx];
        }

        public S_IsSuccessPokeBallCatch CatchPokemon()
        {
            S_IsSuccessPokeBallCatch isSuccessCatchPacket = new S_IsSuccessPokeBallCatch();

            float hpModifier = (3.0f * _wildPokemon.PokemonStat.MaxHp - 2.0f * _wildPokemon.PokemonStat.Hp) / (3.0f * _wildPokemon.PokemonStat.MaxHp);

            float statusModifier = 1.0f;

            float a = hpModifier * _wildPokemon.PokemonSummaryDictData.baseCatchRate * ((PokeBall)_usedItem).CatchRate * statusModifier;

            if (a > 255)
                a = 255;

            bool isCatch = true;
            for (int i = 0; i < 4; i++)
            {
                // 0부터 65535 사이의 난수 생성
                int randomNumber = _random.Next(0, 65536);

                // 난수가 a * 256보다 크면 실패
                if (randomNumber > a * 256)
                {
                    Console.WriteLine($"포획 실패! (난수: {randomNumber}, 요구 값: {a * 256})");
                    isCatch = false;
                    break;
                }

                Console.WriteLine($"포획 성공! (난수: {randomNumber}, 요구 값: {a * 256})");
            }

            isSuccessCatchPacket.IsCatch = isCatch;

            if (isCatch)
            {
                _player.Pokemons.Add(_wildPokemon);
                isSuccessCatchPacket.CatchPokemonName = _wildPokemon.PokemonInfo.PokemonName;
            }

            return isSuccessCatchPacket;
        }

        public S_EnterMoveSelectionScene EnterMoveSelectionScene()
        {
            S_EnterMoveSelectionScene enterMoveScenePacket = new S_EnterMoveSelectionScene();

            enterMoveScenePacket.PlayerInfo = _player.MakePlayerInfo();

            PokemonSummary pokemonSum = null;

            if (_player.Info.PosInfo.State == CreatureState.Fight)
            {
                pokemonSum = _getExpPokemons[_curExpIdx].MakePokemonSummary();
            }
            else if (_player.Info.PosInfo.State == CreatureState.PokemonEvolving)
            {
                pokemonSum = _curEvolvePokemon.MakePokemonSummary();
            }

            enterMoveScenePacket.PokemonSum = pokemonSum;
            enterMoveScenePacket.LearnableMoveSum = _learnableMove.MakePokemonMoveSummary();

            return enterMoveScenePacket;
        }

        bool IsMyPokemonFast()
        {
            int myPokemonSpeed = _myPokemon.PokemonStat.Speed;
            int enemyPokemonSpeed = _wildPokemon.PokemonStat.Speed;

            if (myPokemonSpeed > enemyPokemonSpeed)
            {
                return true;
            }
            else if (myPokemonSpeed < enemyPokemonSpeed)
            {
                return false;
            }
            else
            {
                int ran = _random.Next(1, 101);

                if (ran > 50)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool FindAvailableBattlePokemon()
        {
            for (int i = 0; i < _pokemons.Count; i++)
            {
                if (i == 0)
                    continue;
                else
                {
                    Pokemon pokemon = _pokemons[i];
                    if (pokemon.PokemonInfo.PokemonStatus != PokemonStatusCondition.Fainting)
                        return true;
                }
            }

            return false;
        }

        public bool CalEscapeRate()
        {
            int mySpeed = _myPokemon.PokemonStat.Speed;
            int enemySpeed = _wildPokemon.PokemonStat.Speed;

            if (mySpeed >= enemySpeed)
                return true;
            else
            {
                float firstValue = ((float)mySpeed * 128f / (float)enemySpeed) + (30f * (float)_escapeTurnCnt);
                float finalProbabilityValue = firstValue % 256f;

                int ran = _random.Next(0, 256);

                if (ran < (int)finalProbabilityValue)
                    return true;
                else
                {
                    if (_myPokemon.PokemonInfo.PokemonStatus != PokemonStatusCondition.Fainting)
                    {
                        _escapeTurnCnt++;

                        _myTurn = true;
                        SetWildPokemonSelectedMove();
                    }

                    return false;
                }
            }
        }

        public bool CheckEvolutionPokemon()
        {
            if (_evolvePokemons.Count <= _curEvolutionIdx)
                return false;
            else
                return true;
        }

        public Pokemon SetEvolutionPokemon()
        {
            _curEvolvePokemon = _evolvePokemons[_curEvolutionIdx];
            _curEvolutionIdx++;
            return _curEvolvePokemon;
        }

        public Pokemon GetCurEvolvePokemon()
        {
            return _curEvolvePokemon;
        }

        public S_PokemonEvolution EvolvePokemon(bool isEvolution)
        {
            S_PokemonEvolution evolutionPacket = new S_PokemonEvolution();

            if (isEvolution)
            {
                _curEvolvePokemon.PokemonEvolution();
                _learnableMove = _curEvolvePokemon.CheckNewLearnableMove();

                evolutionPacket.EvolvePokemonSum = _curEvolvePokemon.MakePokemonSummary();

                if (_learnableMove != null)
                    evolutionPacket.NewMoveSum = _learnableMove.MakePokemonMoveSummary();
                //else
                //    _curEvolutionIdx++;
            }

            return evolutionPacket;
        }
    }
}
