using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PokemonBattleRoom : JobSerializer
    {
        int _roomId;
        int _maxPlayerCount = 2;
        int _requestCount = 0;
        Random _random = new Random();

        Dictionary<Player, List<Pokemon>> _players = new Dictionary<Player, List<Pokemon>>();

        PriorityQueue<C_SendAction, float> _actionQueue = new PriorityQueue<C_SendAction, float>();

        public int RoomId { get { return _roomId; } set { _roomId = value; } }

        public List<Pokemon> GetPokemonListByPlayer(Player player)
        {
            return _players[player];
        }

        public PokemonBattleRoom(int maxPlayerCnt)
        {
            _maxPlayerCount = maxPlayerCnt;
        }

        public void EnterRoom(Player player)
        {
            List<Pokemon> battlePokemons = player.GetBattlePokemonOrderArray();

            _players.Add(player, battlePokemons);

            if (_players.Count == _maxPlayerCount)
                BeginBattle();
        }

        void BeginBattle()
        {
            foreach (Player player in _players.Keys)
            {
                S_EnterTrainerBattle s_EnterBattlePacket = new S_EnterTrainerBattle();
                //s_EnterBattlePacket.MyPlayerInfo = player.MakePlayerInfo();

                foreach (Player otherPlayer in _players.Keys)
                {
                    if (otherPlayer.Id != player.Id)
                    {
                        s_EnterBattlePacket.OtherPlayerInfos.Add(otherPlayer.MakeOtherPlayerInfo());
                        s_EnterBattlePacket.OtherPokemonSums.Add(_players[otherPlayer][0].MakePokemonSummary());
                    }
                }

                s_EnterBattlePacket.FirstBattlePokemonOrder = player.Pokemons.IndexOf(_players[player][0]);

                player.Session.Send(s_EnterBattlePacket);
            }
        }

        public void CheckAvailableMove(Player player)
        {
            Pokemon myBattlePokemon = _players[player][0];

            bool canUseMove = myBattlePokemon.CanUseMove();

            S_CheckAvailableMove checkMovePacket = new S_CheckAvailableMove();
            checkMovePacket.CanUseMove = canUseMove;

            player.Session.Send(checkMovePacket);
        }

        public void SetPlayerAction(Player player, IMessage actionPacket)
        {
            Pokemon myBattlePokemon = _players[player][0];

            int pokemonSpeed = myBattlePokemon.PokemonStat.Speed;

            int actionPriority = CalPriority(actionPacket);

            float randomTieBreaker = (float)_random.NextDouble();

            float totalPriorityScore = (float)(actionPriority + pokemonSpeed) + randomTieBreaker;

            _actionQueue.Enqueue(((C_SendAction)actionPacket), -totalPriorityScore);

            RequestNextBattleAction();
        }

        int CalPriority(IMessage actionPacket)
        {
            int actionPriority = 0;

            if (((C_SendAction)actionPacket).UseNoPPMove != null || ((C_SendAction)actionPacket).UseMove != null)
            {
                actionPriority += 10000;
            }
            else if (((C_SendAction)actionPacket).SwitchBattlePokemon != null)
            {
                actionPriority += 20000;
            }
            else if (((C_SendAction)actionPacket).FinishBattle != null)
            {
                actionPriority += 99999;
            }

                return actionPriority;
        }

        void ApplyAction()
        {
            S_SendAction actionPacket = new S_SendAction();

            C_SendAction battleAction = null;
            Pokemon attackPokemon = null;
            Pokemon defensePokemon = null;

            if (_actionQueue.Count > 0)
                battleAction = _actionQueue.Dequeue();

            foreach (Player player in _players.Keys)
            {
                if (player.Id == battleAction.PlayerId)
                {
                    attackPokemon = _players[player][0];
                }
                else
                {
                    defensePokemon = _players[player][0];
                }
            }

            switch (battleAction.SpecialInfoCase)
            {
                case C_SendAction.SpecialInfoOneofCase.UseNoPPMove:
                    {
                        actionPacket.UseMoveResult = UsePokemonMove(actionPacket, -1, attackPokemon, defensePokemon);
                    }
                    break;
                case C_SendAction.SpecialInfoOneofCase.UseMove:
                    {
                        actionPacket.UseMoveResult = UsePokemonMove(actionPacket, battleAction.UseMove.SelectedMoveOrder, attackPokemon, defensePokemon);
                    }
                    break;
                case C_SendAction.SpecialInfoOneofCase.SwitchBattlePokemon:
                    {
                        actionPacket.SwitchPokemonResult = SwitchBattlePokemon(actionPacket, battleAction.PlayerId, battleAction.SwitchBattlePokemon.FromIdx, battleAction.SwitchBattlePokemon.ToIdx);
                    }
                    break;
                case C_SendAction.SpecialInfoOneofCase.FinishBattle:
                    {
                        actionPacket.FinishBattleResult = FinishTrainerBattle(battleAction.PlayerId);
                    }
                    break;
            }

            actionPacket.TurnPlayerId = battleAction.PlayerId;

            foreach (Player player in _players.Keys)
            {
                player.Session.Send(actionPacket);
            }
        }

        UseMoveResult UsePokemonMove(S_SendAction actionPacket, int moveOrder, Pokemon attackPokemon, Pokemon defensePokemon)
        {
            UseMoveResult useMoveResult = new UseMoveResult();
            PokemonMove attackMove = attackPokemon.PokemonMoves[moveOrder];

            // 기술이 명중하였다면
            if (attackPokemon.UseMove(moveOrder))
            {
                useMoveResult.IsHit = true;

                float typeEffectiveModifier = GetTypeEffectivenessMultiplier(attackPokemon, defensePokemon, attackMove);
                float criticalModifier = GetCriticalModifier(attackMove);
                float typeEqualModifier = GetTypeEqualModifier(attackMove, attackPokemon);
                float damageModifier = typeEffectiveModifier * criticalModifier * typeEqualModifier;

                useMoveResult.TypeEffectiveness = typeEffectiveModifier;
                useMoveResult.IsCriticalHit = criticalModifier == 1.5f ? true : false;
                ChangeBattlePokemonHp(attackMove, attackPokemon, defensePokemon, damageModifier);

                // 공격 받은 포켓몬이 기절 시
                if (defensePokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                {
                    if (_actionQueue.Count > 0)
                        _actionQueue.Dequeue();

                    useMoveResult.DefensePokemonSum = defensePokemon.MakePokemonSummary();
                    useMoveResult.UsedMoveSum = attackMove.MakePokemonMoveSummary();
                    useMoveResult.UsedMoveOrder = moveOrder;
                    actionPacket.IsTurnFinish = true;

                    return useMoveResult;
                }
            }
            else
            {
                useMoveResult.IsHit = false;
            }

            useMoveResult.DefensePokemonSum = defensePokemon.MakePokemonSummary();
            useMoveResult.UsedMoveSum = attackMove.MakePokemonMoveSummary();
            useMoveResult.UsedMoveOrder = moveOrder;

            // 턴이 끝났는 지 확인
            if (_actionQueue.Count > 0)
            {
                actionPacket.IsTurnFinish = false;
            }
            else // 턴이 끝났다면 턴 초기화
            {
                actionPacket.IsTurnFinish = true;
            }

            return useMoveResult;
        }

        float GetTypeEffectivenessMultiplier(Pokemon attackPokemon, Pokemon defensePokemon, PokemonMove attackMove)
        {
            float multiplier = 1f;

            multiplier *= PokemonTypeChecker.Instance.GetEffectiveness(attackMove.MoveType, defensePokemon.PokemonInfo.Type1);

            if (defensePokemon.PokemonInfo.Type2 != PokemonType.TypeNone)
                multiplier *= PokemonTypeChecker.Instance.GetEffectiveness(attackMove.MoveType, defensePokemon.PokemonInfo.Type2);

            return multiplier;
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

        void ChangeBattlePokemonHp(PokemonMove move, Pokemon attackPokemon, Pokemon defensePokemon, float damageModifier)
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

        public void RequestNextBattleAction()
        {
            _requestCount++;

            if (_requestCount == _maxPlayerCount)
            {
                _requestCount = 0;

                ApplyAction();
            }
        }

        public void SwitchDiePokemon(Player player, int fromIdx, int toIdx)
        {
            C_SendAction actionPacket = new C_SendAction();
            actionPacket.PlayerId = player.Id;
            actionPacket.SwitchBattlePokemon = new SwitchBattlePokemon();
            actionPacket.SwitchBattlePokemon.FromIdx = fromIdx;
            actionPacket.SwitchBattlePokemon.ToIdx = toIdx;

            int actionPriority = CalPriority(actionPacket);

            _actionQueue.Enqueue(((C_SendAction)actionPacket), -actionPriority);

            RequestNextBattleAction();
        }

        public void CheckAvailablePokemon(Player player)
        {
            S_CheckAvailablePokemon checkPokemonPacket = new S_CheckAvailablePokemon();

            bool canFight = false;
            foreach (Pokemon pokemon in _players[player])
            {
                if (pokemon.PokemonInfo.PokemonStatus != PokemonStatusCondition.Fainting)
                {
                    canFight = true;
                    break;
                }
            }

            if (canFight)
            {
                checkPokemonPacket.CanFight = canFight;
                player.Session.Send(checkPokemonPacket);
            }
            else
            {
                C_SendAction actionPacket = new C_SendAction();
                actionPacket.PlayerId = player.Id;
                actionPacket.FinishBattle = new FinishBattle();

                int actionPriority = CalPriority(actionPacket);

                _actionQueue.Enqueue(((C_SendAction)actionPacket), -actionPriority);

                RequestNextBattleAction();
            }
        }

        SwitchBattlePokemonResult SwitchBattlePokemon(S_SendAction actionPacket, int playerId, int fromIdx, int toIdx)
        {
            SwitchBattlePokemonResult result = new SwitchBattlePokemonResult();

            Player foundPlayer = null;
            foreach (Player player in _players.Keys)
            {
                if (player.Id == playerId)
                {
                    foundPlayer = player;
                    break;
                }
            }

            List<Pokemon> pokemons = null;
            if (foundPlayer != null)
                pokemons = _players[foundPlayer];

            Pokemon prevPokemon = pokemons[fromIdx];
            pokemons[fromIdx] = pokemons[toIdx];
            pokemons[toIdx] = prevPokemon;

            result.PrevPokemonSum = pokemons[toIdx].MakePokemonSummary();
            result.NewPokemonSum = pokemons[fromIdx].MakePokemonSummary();

            // 턴이 끝났는 지 확인
            if (_actionQueue.Count > 0)
            {
                actionPacket.IsTurnFinish = false;
            }
            else // 턴이 끝났다면 턴 초기화
            {
                actionPacket.IsTurnFinish = true;
            }

            return result;
        }

        public FinishBattleResult FinishTrainerBattle(int playerId)
        {
            FinishBattleResult finishBattleResult = new FinishBattleResult();

            foreach (Player battlePlayer in _players.Keys)
            {
                if (battlePlayer.Id == playerId)
                    finishBattleResult.LosePlayerId = battlePlayer.Id;
                else
                    finishBattleResult.WinPlayerId = battlePlayer.Id;
            }

            return finishBattleResult;
        }

        public void SurrenderBattle(Player surrenderPlayer)
        {
            foreach (Player player in _players.Keys)
            {
                S_SurrenderTrainerBattle surrenderPacket = new S_SurrenderTrainerBattle();
                surrenderPacket.SurrenderPlayerId = surrenderPlayer.Id;

                player.Session.Send(surrenderPacket);
            }
        }
    }
}
