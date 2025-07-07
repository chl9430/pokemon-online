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
        List<Pokemon> _pokemons;
        Player _player;
        Pokemon _myPokemon;
        Pokemon _wildPokemon;

        public PrivateBattleRoom(Player player, List<Pokemon> pokemons)
        {
            _player = player;
            _pokemons = new List<Pokemon>();
            player.BattleRoom = this;

            foreach (Pokemon pokemon in pokemons)
            {
                _pokemons.Add(pokemon);
            }
        }

        public List<Pokemon> Pokemons {  get { return _pokemons; } }
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

        public void SwitchBattlePokemon(int from, int to)
        {
            if (from == to)
            {
                Console.WriteLine("Cannot change with same pokemons");
                return;
            }

            Pokemon temp = _pokemons[from];
            _pokemons[from] = _pokemons[to];
            _pokemons[to] = temp;

            _myPokemon = _pokemons[0];
        }

        public int UseBattlePokemonMove(int moveOrder, bool isMyPokemon)
        {
            if (isMyPokemon)
            {
                if (moveOrder != -1)
                {
                    _myPokemon.SelectedMove = _myPokemon.PokemonMoves[moveOrder];
                    _myPokemon.SelectedMove.CurPP--;
                }
                else
                {
                    _myPokemon.SetNoPPMove();
                }

                return _myPokemon.SelectedMove.CurPP;
            }
            else
            {
                if (moveOrder != -1)
                {
                    _wildPokemon.SelectedMove = _wildPokemon.PokemonMoves[moveOrder];
                    _wildPokemon.SelectedMove.CurPP--;
                }
                else
                {
                    _wildPokemon.SetNoPPMove();
                }

                return _wildPokemon.SelectedMove.CurPP;
            }
        }

        public bool ChangeBattlePokemonHp(bool isMyPokemon)
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

                if (_myPokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                    return true;
                else
                    return false;
            }
            else
            {
                moveCategory = _myPokemon.SelectedMove.MoveCategory;
                movePower = _myPokemon.SelectedMove.MovePower;
                finalDamage = CalFinalDamage(moveCategory, _myPokemon.PokemonInfo.Level, _myPokemon.PokemonStat, _wildPokemon.PokemonStat, movePower);

                _wildPokemon.GetDamage(finalDamage);

                if (_wildPokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                    return true;
                else
                    return false;
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
