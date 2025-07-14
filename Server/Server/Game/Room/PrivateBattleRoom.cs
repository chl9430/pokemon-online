using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PrivateBattleRoom
    {
        int _remainedExp;
        int _escapeTurnCnt;
        Random _random;
        List<Pokemon> _pokemons;
        Player _player;
        Pokemon _myPokemon;
        Pokemon _wildPokemon;

        bool _myTurn = false;
        bool _enemyTurn = false;
        Pokemon _attackPokemon;
        Pokemon _defensePokemon;

        public PrivateBattleRoom(Player player, List<Pokemon> pokemons)
        {
            _escapeTurnCnt = 1;
            _random = new Random();
            _player = player;
            _pokemons = new List<Pokemon>();
            player.BattleRoom = this;

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
        }

        public List<Pokemon> Pokemons {  get { return _pokemons; } }

        public Pokemon MyPokemon { get { return _myPokemon; } }

        public Pokemon WildPokemon { get { return _wildPokemon; } }

        public S_CheckAndApplyRemainedExp CheckAndApplyExp()
        {
            S_CheckAndApplyRemainedExp s_CheckAndApplyExpPacket = new S_CheckAndApplyRemainedExp();

            int finalExp = 0;

            if (_remainedExp > 0)
            {
                if (_remainedExp > _myPokemon.ExpInfo.RemainExpToNextLevel)
                    finalExp = _myPokemon.ExpInfo.RemainExpToNextLevel;
                else
                    finalExp = _remainedExp;

                _remainedExp -= finalExp;

                s_CheckAndApplyExpPacket.FinalExp = finalExp;
            }
            else
            {
                return s_CheckAndApplyExpPacket;
            }

            _myPokemon.GetExp(finalExp, s_CheckAndApplyExpPacket);

            return s_CheckAndApplyExpPacket;
        }

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

        public void SwitchBattlePokemon(int from, int to)
        {
            if (from == to)
            {
                Console.WriteLine("Cannot change with same pokemons");
                return;
            }

            if (_pokemons[0].PokemonInfo.PokemonStatus != PokemonStatusCondition.Fainting)
            {
                _myTurn = true;
                _attackPokemon = WildPokemon;
                _defensePokemon = _pokemons[to];
                SetWildPokemonSelectedMove();
            }

            Pokemon temp = _pokemons[from];
            _pokemons[from] = _pokemons[to];
            _pokemons[to] = temp;

            _myPokemon = _pokemons[0];
        }

        public void SetBattlePokemonMove(int moveOrder)
        {
            if (moveOrder != -1)
                _myPokemon.SetSelectedMove(moveOrder);
            else
                _myPokemon.SetNoPPMoveToSelectedMove();

            SetWildPokemonSelectedMove();
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

        void SetWildPokemonSelectedMove()
        {
            List<PokemonMove> enemyPKMAvailableMoves = FindAvailableMove(_wildPokemon);

            if (enemyPKMAvailableMoves.Count > 0)
                _wildPokemon.SelectedMove = enemyPKMAvailableMoves[_random.Next(0, enemyPKMAvailableMoves.Count)];
            else
            {
                _wildPokemon.SetNoPPMoveToSelectedMove();
            }
        }

        public S_UsePokemonMove UseBattlePokemonMove()
        {
            if (!_myTurn && !_enemyTurn)
                SetAttackAndDefensePokemon();

            S_UsePokemonMove s_UseMovePacket = new S_UsePokemonMove();

            // 턴 체크
            if (_attackPokemon == _myPokemon)
            {
                s_UseMovePacket.IsMyPokemon = true;
                _myTurn = true;
            }
            else
            {
                s_UseMovePacket.IsMyPokemon = false;
                _enemyTurn = true;
            }

            if (_attackPokemon.DidSelectedMoveHit())
            {
                s_UseMovePacket.IsHit = true;
                s_UseMovePacket.RemainedPP = _attackPokemon.SelectedMove.CurPP;

                ChangeBattlePokemonHp(s_UseMovePacket);

                if (_defensePokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                {
                    s_UseMovePacket.IsTurnFinish = true;
                    _myTurn = false;
                    _enemyTurn = false;

                    return s_UseMovePacket;
                }
            }
            else
            {
                s_UseMovePacket.IsHit = false;
                s_UseMovePacket.RemainedPP = _attackPokemon.SelectedMove.CurPP;
            }

            // 공격, 수비 포켓몬 바꾸기
            if (!_myTurn || !_enemyTurn)
            {
                s_UseMovePacket.IsTurnFinish = false;

                Pokemon temp = _attackPokemon;
                _attackPokemon = _defensePokemon;
                _defensePokemon = temp;
            }
            else // 턴이 끝났다면 턴 초기화
            {
                _escapeTurnCnt++;

                if (_escapeTurnCnt > 10)
                    _escapeTurnCnt = 10;

                s_UseMovePacket.IsTurnFinish = true;
                _myTurn = false;
                _enemyTurn = false;
            }
            
            return s_UseMovePacket;
        }

        public void ChangeBattlePokemonHp(S_UsePokemonMove useMovePacket)
        {
            int finalDamage = 0;
            float multiplier = CalDamageModifier(useMovePacket);

            if (multiplier == 0f)
                return;

            finalDamage = CalFinalDamage(_attackPokemon.SelectedMove, _attackPokemon, _defensePokemon);
            
            finalDamage = (int)((float)finalDamage * multiplier);

            _defensePokemon.GetDamaged(finalDamage);

            useMovePacket.RemainedHp = _defensePokemon.PokemonStat.Hp;

            useMovePacket.PokemonStatus = _defensePokemon.PokemonInfo.PokemonStatus;
        }

        public float GetTypeEffectivenessMultiplier(Pokemon attackPokemon, Pokemon defensePokemon)
        {
            float multiplier = 1f;

            multiplier *= PokemonTypeChecker.Instance.GetEffectiveness(attackPokemon.SelectedMove.MoveType, defensePokemon.PokemonInfo.Type1);

            if (defensePokemon.PokemonInfo.Type2 != PokemonType.TypeNone)
                multiplier *= PokemonTypeChecker.Instance.GetEffectiveness(attackPokemon.SelectedMove.MoveType, defensePokemon.PokemonInfo.Type2);

            return multiplier;
        }

        public int GetExp()
        {
            //int exp = (112 * _wildPokemon.PokemonInfo.Level) / 7;
            int exp = 50;

            _remainedExp = exp;

            return exp;
        }

        int CalFinalDamage(PokemonMove attackMove, Pokemon attackPokemon, Pokemon defensePokemon)
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

            if ((int)finalDamage <= 0)
                finalDamage = 1f;

            return (int)finalDamage;
        }

        float CalDamageModifier(S_UsePokemonMove useMovePacket)
        {
            float typeEffectivenessModifier = GetTypeEffectivenessMultiplier(_attackPokemon, _defensePokemon);
            useMovePacket.TypeEffectiveness = typeEffectivenessModifier;

            float typeEqualModifier;
            float criticalModifier;

            if (_attackPokemon.PokemonInfo.Type1 == _attackPokemon.SelectedMove.MoveType)
                typeEqualModifier = 1.5f;
            else if (_attackPokemon.PokemonInfo.Type2 == _attackPokemon.SelectedMove.MoveType)
                typeEqualModifier = 1.5f;
            else
                typeEqualModifier = 1f;

            float rawRandom = (float)(_random.NextDouble() * (100.00 - 0.00) + 0.00);
            float ran = (float)Math.Round(rawRandom, 2, MidpointRounding.AwayFromZero);

            if (ran <= _attackPokemon.SelectedMove.CriticalRate)
            {
                criticalModifier = 1.5f;
                useMovePacket.IsCriticalHit = true;
            }
            else
            {
                criticalModifier = 1;
                useMovePacket.IsCriticalHit = false;
            }

            return typeEffectivenessModifier * typeEqualModifier * criticalModifier;
        }

        void SetAttackAndDefensePokemon()
        {
            int myPokemonSpeed = _myPokemon.PokemonStat.Speed;
            int enemyPokemonSpeed = _wildPokemon.PokemonStat.Speed;

            if (myPokemonSpeed > enemyPokemonSpeed)
            {
                _attackPokemon = _myPokemon;
                _defensePokemon = _wildPokemon;
            }
            else if (myPokemonSpeed < enemyPokemonSpeed)
            {
                _attackPokemon = _wildPokemon;
                _defensePokemon = _myPokemon;
            }
            else
            {
                int ran = _random.Next(1, 101);

                if (ran > 50)
                {
                    _attackPokemon = _myPokemon;
                    _defensePokemon = _wildPokemon;
                }
                else
                {
                    _attackPokemon = _wildPokemon;
                    _defensePokemon = _myPokemon;
                }
            }
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
                        _attackPokemon = WildPokemon;
                        _defensePokemon = _myPokemon;
                        SetWildPokemonSelectedMove();
                    }

                    return false;
                }
            }
        }
    }
}
