using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct PokemonSummaryDictData
{
    public int dictionaryNum;
    public string pokemonName;
    public string type1;
    public string type2;
    public int maxHp;
    public int attack;
    public int defense;
    public int specialAttack;
    public int specialDefense;
    public int speed;
}

namespace Server
{
    #region PokemonSummary
    [Serializable]
    public class PokemonSummaryData : ILoader<string, PokemonSummaryDictData>
    {
        public List<PokemonSummaryDictData> pokemonSummaries = new List<PokemonSummaryDictData>();

        public Dictionary<string, PokemonSummaryDictData> MakeDict()
        {
            Dictionary<string, PokemonSummaryDictData> dict = new Dictionary<string, PokemonSummaryDictData>();
            foreach (PokemonSummaryDictData summary in pokemonSummaries)
            {
                dict.Add(summary.pokemonName, summary);
            }
            return dict;
        }
    }
    #endregion
}