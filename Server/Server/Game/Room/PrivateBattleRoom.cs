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
        List<Pokemon> _opponentPokemons;
        Player _player;
        TrainerNPC _trainerNPC;
        Pokemon _myPokemon;
        Pokemon _opponentPokemon;

        bool _myTurn = false;
        bool _enemyTurn = false;

        int _curExpIdx = 0;
        int _curEvolutionIdx = 0;
        List<Pokemon> _getExpPokemons;
        List<Pokemon> _evolvePokemons;
        Pokemon _curEvolvePokemon;

        PokemonMove _learnableMove;

        //ItemBase _usedItem;

        public PrivateBattleRoom(Player player, List<Pokemon> pokemons)
        {
            _escapeTurnCnt = 1;
            _random = new Random();
            _player = player;
            _pokemons = player.GetBattlePokemonOrderArray();
            player.BattleRoom = this;

            _getExpPokemons = new List<Pokemon>();
            _evolvePokemons = new List<Pokemon>();

            _myPokemon = _pokemons[0];
            _getExpPokemons.Add(_myPokemon);
        }

        public TrainerNPC TrainerNPC { get {  return _trainerNPC; } }

        public List<Pokemon> Pokemons {  get { return _pokemons; } }

        public Pokemon MyPokemon { get { return _myPokemon; } }

        public Pokemon OpponentPokemon { get { return _opponentPokemon; } }

        public List<Pokemon> EvolvePokemons {  get { return _evolvePokemons; } }

        public PokemonMove LearnableMove { get { return _learnableMove; } set { _learnableMove = value; } }

        //public ItemBase UsedItem { set { _usedItem = value; } get { return _usedItem; } }

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

                _opponentPokemon = new Pokemon(wildPokemonInfo.pokemonName, wildPokemonInfo.pokemonName, wildPokemonInfo.pokemonLevel, _player.Name, _player.Id);
            }
            else
            {
                Console.WriteLine("Cannot find Location Data!");
            }
        }

        public void GetBattleReady(TrainerNPC trainerNPC)
        {
            NPCPokemonInfo[] data = trainerNPC.BattleNPCDictData.battlePokemons;

            _trainerNPC = trainerNPC;
            _opponentPokemons = new List<Pokemon>();

            for (int i = 0; i < data.Length; i++)
            {
                _opponentPokemons.Add(new Pokemon(data[i].pokemonName, data[i].pokemonName, data[i].pokemonLevel, trainerNPC.Name, trainerNPC.Id));
            }

            _opponentPokemon = _opponentPokemons[0];
        }

        public S_SwitchBattlePokemon SwitchBattlePokemon(int from, int to)
        {
            S_SwitchBattlePokemon switchPokemonPacket = new S_SwitchBattlePokemon();

            if (_pokemons[0].PokemonInfo.PokemonStatus != PokemonStatusCondition.Fainting)
            {
                _myTurn = true;
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

            switchPokemonPacket.PrevPokemonSum = prevPokemon.MakePokemonSummary();

            return switchPokemonPacket;
        }

        public S_ProcessTurn ProcessTurn(int moveOrder, Player player)
        {
            S_ProcessTurn processTurnPacket = new S_ProcessTurn();

            Pokemon attackPokemon = null;
            Pokemon defensePokemon = null;

            if (!_myTurn && !_enemyTurn)
            {
                if (IsMyPokemonFast())
                {
                    attackPokemon = _myPokemon;
                    defensePokemon = _opponentPokemon;
                    processTurnPacket.IsMyPokemon = true;
                }
                else
                {
                    attackPokemon = _opponentPokemon;
                    defensePokemon = _myPokemon;
                }
            }
            else if (_myTurn && !_enemyTurn)
            {
                attackPokemon = _opponentPokemon;
                defensePokemon = _myPokemon;
            }
            else if (!_myTurn && _enemyTurn)
            {
                processTurnPacket.IsMyPokemon = true;
                attackPokemon = _myPokemon;
                defensePokemon = _opponentPokemon;
            }

            UsePokemonMove(processTurnPacket, moveOrder, attackPokemon, defensePokemon);

            return processTurnPacket;
        }

        public void UsePokemonMove(S_ProcessTurn processTurnPacket, int moveOrder, Pokemon attackPokemon, Pokemon defensePokemon)
        {
            PokemonMove attackMove;
            int usedMoveOrder;

            if (attackPokemon == _myPokemon)
            {
                _myTurn = true;
                usedMoveOrder = moveOrder;
                attackMove = _myPokemon.PokemonMoves[usedMoveOrder];
            }
            else
            {
                _enemyTurn = true;
                usedMoveOrder = GetWildPokemonSelectedMoveIndex();
                attackMove = _opponentPokemon.PokemonMoves[usedMoveOrder];
            }

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
                    //processTurnPacket.AttackPokemonSum = attackPokemon.MakePokemonSummary();
                    processTurnPacket.DefensePokemonSum = defensePokemon.MakePokemonSummary();
                    processTurnPacket.UsedMoveSum = attackMove.MakePokemonMoveSummary();
                    processTurnPacket.UsedMoveOrder = usedMoveOrder;
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

            //processTurnPacket.AttackPokemonSum = attackPokemon.MakePokemonSummary();
            processTurnPacket.DefensePokemonSum = defensePokemon.MakePokemonSummary();
            processTurnPacket.UsedMoveSum = attackMove.MakePokemonMoveSummary();
            processTurnPacket.UsedMoveOrder = usedMoveOrder;

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

        public void ProcessMyTurn()
        {
            _myTurn = true;
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

        PokemonMove SetWildPokemonSelectedMove()
        {
            List<PokemonMove> availableMoves = FindAvailableMove(_opponentPokemon);

            int ranMoveOrder = _random.Next(0, availableMoves.Count);

            if (availableMoves.Count > 0)
                return availableMoves[ranMoveOrder];
            else
                return _opponentPokemon.NoPPMove;
        }

        int GetWildPokemonSelectedMoveIndex()
        {
            List<PokemonMove> availableMoves = FindAvailableMove(_opponentPokemon);

            if (availableMoves.Count > 0)
                return _random.Next(0, availableMoves.Count);
            else
                return -1;
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
            getExpPacket.ExpPokemonName = _getExpPokemons[_curExpIdx].PokemonInfo.NickName;

            //int exp = (112 * _opponentPokemon.PokemonInfo.Level) / 7;
            _remainedExp = (int)Math.Ceiling(2100f / ((float)_getExpPokemons.Count));

            getExpPacket.Exp = _remainedExp;

            return getExpPacket;
        }

        public S_CheckAndApplyRemainedExp CheckAndApplyExp()
        {
            S_CheckAndApplyRemainedExp s_CheckAndApplyExpPacket = new S_CheckAndApplyRemainedExp();

            Pokemon expPokemon = _getExpPokemons[_curExpIdx];
            int finalExp;

            s_CheckAndApplyExpPacket.ExpPokemonOrder = _pokemons.IndexOf(expPokemon);

            if (_remainedExp > expPokemon.ExpInfo.RemainExpToNextLevel)
                finalExp = expPokemon.ExpInfo.RemainExpToNextLevel;
            else
                finalExp = _remainedExp;

            _remainedExp -= finalExp;

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
            else
            {
                _curExpIdx++;

                if (_getExpPokemons.Count <= _curExpIdx)
                    s_CheckAndApplyExpPacket.IsExpFinish = true;
                else
                    s_CheckAndApplyExpPacket.IsExpFinish = false;
            }

            s_CheckAndApplyExpPacket.ExpPokemonSum = expPokemon.MakePokemonSummary();

            return s_CheckAndApplyExpPacket;
        }

        public Pokemon GetExpPokemon()
        {
            if (_curExpIdx < _getExpPokemons.Count)
                return _getExpPokemons[_curExpIdx];
            else
            {
                if (_curEvolutionIdx < _evolvePokemons.Count)
                {
                    Pokemon evolutionPokemon = _evolvePokemons[_curEvolutionIdx];
                    _curEvolutionIdx++;

                    return evolutionPokemon;
                }
                else
                    return null;
            }
        }

        bool IsMyPokemonFast()
        {
            int myPokemonSpeed = _myPokemon.PokemonStat.Speed;
            int enemyPokemonSpeed = _opponentPokemon.PokemonStat.Speed;

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

        public PokemonSummary SetNextOpponentPokemon()
        {
            _opponentPokemons.RemoveAt(0);

            _getExpPokemons.Clear();
            _getExpPokemons.Add(_myPokemon);
            _curExpIdx = 0;


            if (_opponentPokemons.Count > 0 )
            {
                _opponentPokemon = _opponentPokemons[0];
                return _opponentPokemon.MakePokemonSummary();
            }
            else
                return null;
        }

        public bool CalEscapeRate()
        {
            int mySpeed = _myPokemon.PokemonStat.Speed;
            int enemySpeed = _opponentPokemon.PokemonStat.Speed;

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

        public Pokemon SetEvolutionPokemon()
        {
            if (_curEvolutionIdx < _evolvePokemons.Count)
            {
                _curEvolvePokemon = _evolvePokemons[_curEvolutionIdx];
                return _curEvolvePokemon;
            }
            else
                return null;
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
                else
                    _curEvolutionIdx++;
            }
            else
                _curEvolutionIdx++;

            return evolutionPacket;
        }

        public S_GetRewardInfo FillRewardInfoPacket()
        {
            S_GetRewardInfo getRewardPacket = new S_GetRewardInfo();

            if (_trainerNPC != null)
            {
                int rewardMoney = _trainerNPC.BattleNPCDictData.rewardMoney;

                _player.NPCNumber++;
                _player.Money += rewardMoney;

                getRewardPacket.Money = rewardMoney;
                string[] scripts = _trainerNPC.GetTalk(_player.NPCNumber);

                foreach (string script in scripts)
                {
                    getRewardPacket.AfterBattleScripts.Add(script);
                }
            }
            else
            {
                getRewardPacket.Money = 0;
            }

            _player.TalkingNPC = null;

            return getRewardPacket;
        }
    }
}
