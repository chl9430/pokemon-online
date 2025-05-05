using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Pokemon : GameObject, IComparable<Pokemon>
    {
        int _order;
        PokemonSummary _pokemonSummary;

        public int Order { set { _order = value; } }

        public Pokemon(PokemonInfo info, PokemonSkill skill, PokemonBattleMove battleMove)
        {
            ObjectType = GameObjectType.Pokemon;

            PokemonSummary summary = new PokemonSummary()
            {
                Info = info,
                Skill = skill,
                BattleMove = battleMove
            };

            _order = 0;
        }

        public int CompareTo(Pokemon other)
        {
            return _order.CompareTo(other._order);
        }
    }
}
