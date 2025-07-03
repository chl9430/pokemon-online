using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PrivateBattleRoom
    {
        Player _player;
        Pokemon _myPokemon;
        Pokemon _wildPokemon;

        public PrivateBattleRoom(Player player)
        {
            _player = player;
            player.BattleRoom = this;
        }
        public Pokemon MyPokemon { get { return _myPokemon; } set { _myPokemon = value; } }

        public Pokemon WildPokemon { get { return _wildPokemon; } }

        public void MakeWildPokemon(int locationNum)
        {
            if (DataManager.WildPKMLocationDict.TryGetValue(locationNum, out WildPokemonAppearData[] wildPokemonDatas))
            {
                WildPokemonAppearData wildPokemonData = wildPokemonDatas[0];

                Random random = new Random();
                int ran = random.Next(100);

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

        public void UseBattlePokemonMove(int myMoveOrder, int enemyMoveOrder)
        {
            if (myMoveOrder != -1)
            {
                _myPokemon.SelectedMove = _myPokemon.PokemonMoves[myMoveOrder];
                _myPokemon.SelectedMove.CurPP--;
            }

            if (enemyMoveOrder != -1)
            {
                _wildPokemon.SelectedMove = _wildPokemon.PokemonMoves[enemyMoveOrder];
                _wildPokemon.SelectedMove.CurPP--;
            }
        }

        public void ChangeBattlePokemonHp(bool isMyPokemon)
        {
            MoveCategory moveCategory;
            int movePower;
            int finalDamage = 0;

            if (isMyPokemon)
            {
                moveCategory = _wildPokemon.SelectedMove.MoveCategory;
                movePower = _wildPokemon.SelectedMove.MovePower;
                finalDamage = CalFinalDamage(moveCategory, _wildPokemon.PokemonInfo.Level, _wildPokemon.PokemonStat, _myPokemon.PokemonStat, movePower);

                _myPokemon.GetDamage(finalDamage);
            }
            else
            {
                moveCategory = _myPokemon.SelectedMove.MoveCategory;
                movePower = _myPokemon.SelectedMove.MovePower;
                finalDamage = CalFinalDamage(moveCategory, _myPokemon.PokemonInfo.Level, _myPokemon.PokemonStat, _wildPokemon.PokemonStat, movePower);

                _wildPokemon.GetDamage(finalDamage);
            }
        }

        public int GetExp()
        {
            // int exp = (112 * _wildPokemon.PokemonInfo.Level) / 7;
            int exp = 1000;

            return exp;
        }

        int CalFinalDamage(MoveCategory moveCategory, int attackPKMLevel, PokemonStat attackPKMStat, PokemonStat defensePKMStat, int movePower)
        {
            int finalDamage = 0;

            if (moveCategory == MoveCategory.Physical)
                finalDamage = (int)((
                ((((float)attackPKMLevel) * 2f / 5f) + 2f)
                * ((float)movePower)
                * ((float)attackPKMStat.Attack) / ((float)defensePKMStat.Defense)
                ) / 50f + 2f);
            else if (moveCategory == MoveCategory.Special)
                finalDamage = (int)((
                ((((float)attackPKMLevel) * 2f / 5f) + 2f)
                * ((float)movePower)
                * ((float)attackPKMStat.SpecialAttack) / ((float)defensePKMStat.SpecialDefense)
                ) / 50f + 2f);

            if (finalDamage <= 0)
                finalDamage = 1;

            return finalDamage;
        }
    }
}
