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
        //Pokemon _attackPokemon;
        //Pokemon _defensePokemon;

        int _curExpIdx;
        int _curEvolutionIdx;
        List<Pokemon> _getExpPokemons;
        List<Pokemon> _evolvePokemons;

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

        public List<Pokemon> EvolvePokemons {  get { return _evolvePokemons; } }

        public void MakeWildPokemon(int locationNum)
        {
            if (DataManager.WildPKMLocationDict.TryGetValue(locationNum, out WildPokemonAppearData[] wildPokemonDatas))
            {
                WildPokemonAppearData wildPokemonData = wildPokemonDatas[0];

                int ran = _random.Next(100);

                int totalRateCnt = 0;

                for (int i = 0; i < wildPokemonDatas.Length; i++)
                {
                    int rateCnt = 0;
                    int appearRate = wildPokemonDatas[i].appearRate;

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
                            wildPokemonData = wildPokemonDatas[i];
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        break;
                }

                _wildPokemon = new Pokemon(wildPokemonData.pokemonName, wildPokemonData.pokemonName, wildPokemonData.pokemonLevel, _player.Name, _player.Id);
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
            _remainedExp = (int)Math.Ceiling(300f / ((float)_getExpPokemons.Count));

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

            if (_remainedExp <= 0)
                _curExpIdx++;

            s_CheckAndApplyExpPacket.FinalExp = finalExp;

            bool isLevelUp = expPokemon.GetExpAndCheckLevelUp(finalExp);

            if (isLevelUp)
            {
                s_CheckAndApplyExpPacket.StatDiff = expPokemon.LevelUp();
                s_CheckAndApplyExpPacket.NewMoveSum = expPokemon.CheckNewLearnableMove();

                if (expPokemon.CanPokemonEvolve())
                    _evolvePokemons.Add(expPokemon);
            }

            s_CheckAndApplyExpPacket.ExpPokemonSum = expPokemon.MakePokemonSummary();

            return s_CheckAndApplyExpPacket;
        }

        public bool CheckExpPokemons()
        {
            if (_getExpPokemons.Count > _curExpIdx)
                return true;
            else
                return false;
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
            if (_evolvePokemons.Count == _curEvolutionIdx)
                return false;
            else
                return true;
        }

        public S_EnterPokemonEvolutionScene GetEvolutionPokemon()
        {
            S_EnterPokemonEvolutionScene enterEvolutionPacket = new S_EnterPokemonEvolutionScene();
            enterEvolutionPacket.PlayerInfo = _player.MakePlayerInfo();
            enterEvolutionPacket.PokemonSum =  _evolvePokemons[_curEvolutionIdx].MakePokemonSummary();
            enterEvolutionPacket.EvolvePokemonName = _evolvePokemons[_curEvolutionIdx].GetEvolvePokemonName();

            return enterEvolutionPacket;
        }

        public S_PokemonEvolution EvolvePokemon(bool isEvolution)
        {
            S_PokemonEvolution evolutionPacket = new S_PokemonEvolution();

            if (isEvolution)
            {
                _evolvePokemons[_curEvolutionIdx].PokemonEvolution();
                evolutionPacket.NewMoveSum = _evolvePokemons[_curEvolutionIdx].CheckNewLearnableMove();
            }

            evolutionPacket.EvolvePokemonSum = _evolvePokemons[_curEvolutionIdx].MakePokemonSummary();

            _curEvolutionIdx++;

            return evolutionPacket;
        }
    }
}
