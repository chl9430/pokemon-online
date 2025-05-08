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
    public class Pokemon : GameObject
    {
        PokemonSummary _pokemonSummary;

        public PokemonSummary PokemonSummary
        {
            get { return _pokemonSummary; }
        }

        public Pokemon(PokemonInfo info, PokemonSkill skill, PokemonBattleMove battleMove)
        {
            ObjectType = GameObjectType.Pokemon;

            PokemonSummary summary = new PokemonSummary()
            {
                Info = info,
                Skill = skill,
                BattleMove = battleMove
            };

            _pokemonSummary = summary;
        }
    }
}
