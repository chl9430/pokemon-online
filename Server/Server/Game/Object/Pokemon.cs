using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Pokemon : GameObject, IComparable<Pokemon>
    {
        string _nickName;
        int _level;
        int _hp;
        int _exp;
        int _maxExp;
        int _order;
        Player _owner;
        PokemonFinalStatInfo _statInfo;
        
        public string NickName { get { return _nickName; } }
        public int Level { get { return _level; } }
        public int Hp { get { return _hp; } }
        public int Exp { get { return _exp; } }
        public int MaxExp { get { return _maxExp; } }
        public int Order { set { _order = value; }  get { return _order; } }
        public Player Owner { get { return _owner; } }
        public PokemonFinalStatInfo FinalStatInfo { get { return _statInfo; } }

        public Pokemon(string nickName, string pokemonName, int level, int hp, Player owner)
        {
            ObjectType = GameObjectType.Pokemon;
            _nickName = nickName;
            _owner = owner;
            _level = level;
            _hp = hp;
            _exp = 0;
            _maxExp = level * 10;

            PokemonBaseStatInfo baseStatInfo;
            if (DataManager.PokemonStatDict.TryGetValue(pokemonName, out baseStatInfo))
            {
                float rate = (float)level / 10.0f;
                _statInfo = new PokemonFinalStatInfo()
                {
                    PokemonName = baseStatInfo.pokemonName,
                    MaxHp =(int)(baseStatInfo.maxHp * rate),
                    Attack = (int)(baseStatInfo.attack * rate),
                    Defense = (int)(baseStatInfo.defense * rate),
                    SpecialAttack = (int)(baseStatInfo.specialAttack * rate),
                    SpecialDefense = (int)(baseStatInfo.specialAttack * rate),
                    Speed = (int)(baseStatInfo.speed * rate),
                };
            }
            else
            {
                Console.WriteLine("Cannot find Pokemon Base Stat!");
                return;
            }

            _order = 0;
        }

        public int CompareTo(Pokemon other)
        {
            return _order.CompareTo(other._order);
        }
    }
}
