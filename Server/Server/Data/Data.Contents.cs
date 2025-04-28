using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct PokemonBaseStatInfo
{
    public string pokemonName;
    public int maxHp;
    public int attack;
    public int defense;
    public int specialAttack;
    public int specialDefense;
    public int speed;
}

namespace Server
{
    #region PokemonBaseStat
    [Serializable]
    public class PokemonStatData : ILoader<string, PokemonBaseStatInfo>
    {
        public List<PokemonBaseStatInfo> pokemonBaseStats = new List<PokemonBaseStatInfo>();

        public Dictionary<string, PokemonBaseStatInfo> MakeDict()
        {
            Dictionary<string, PokemonBaseStatInfo> dict = new Dictionary<string, PokemonBaseStatInfo>();
            foreach (PokemonBaseStatInfo stat in pokemonBaseStats)
            {
                dict.Add(stat.pokemonName, stat);
            }
            return dict;
        }
    }
    #endregion
}