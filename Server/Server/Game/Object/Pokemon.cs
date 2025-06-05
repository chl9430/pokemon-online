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

        PokemonInfo _pokemonInfo;
        PokemonSkill _pokemonSkill;
        PokemonStat _pokemonStat;

        public PokemonSummary PokemonSummary
        {
            get { return _pokemonSummary; }
        }

        public PokemonInfo PokemonInfo
        {
            get { return _pokemonInfo; }
        }

        public PokemonSkill PokemonSkill
        {
            get { return _pokemonSkill; }
        }

        public PokemonStat PokemonStat
        {
            get { return _pokemonStat; }
        }

        public Pokemon(PokemonSummary summary)
        {
            ObjectType = GameObjectType.Pokemon;

            _pokemonSummary = summary;
            _pokemonInfo = summary.Info;
            _pokemonSkill = summary.Skill;
            _pokemonStat = summary.Skill.Stat;
        }

        public void GetExp(int exp)
        {
            if (_pokemonSkill.RemainLevelExp >= exp)
            {
                _pokemonSkill.CurExp += exp;
                _pokemonSkill.RemainLevelExp -= exp;
                _pokemonSkill.TotalExp += exp;
            }
            else
            {
                return;
            }
        }

        public void LevelUp()
        {
            

            if (_pokemonInfo.Level < 100)
            {
                _pokemonInfo.Level++;
                _pokemonSkill.CurExp = 0;
                _pokemonSkill.RemainLevelExp = ((int)Math.Pow(_pokemonInfo.Level, 3)) - ((int)Math.Pow(_pokemonInfo.Level - 1, 3));
            }
            else
            {
                return;
            }
        }
    }
}
