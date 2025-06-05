using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct PokemonMoveDictData
{
    public string moveName;
    public int movePower;
    public int moveAccuracy;
    public int maxPP;
    public MoveCategory moveCategory;
    public PokemonType moveType;
}

public struct LearnableMoveData
{
    public int learnLevel;
    public string moveName;
}

public struct GenderRatioData
{
    public string gender;
    public float ratio;
}

public struct PokemonSummaryDictData
{
    public int dictionaryNum;
    public string pokemonName;
    public string type1;
    public string type2;
    public GenderRatioData[] genderRatio;
    public int maxHp;
    public int attack;
    public int defense;
    public int specialAttack;
    public int specialDefense;
    public int speed;
    public LearnableMoveData[] learnableMoves;
}

public struct WildPokemonAppearData
{
    public string pokemonName;
    public int pokemonLevel;
    public int appearRate;
}

public struct WildPokemonLocationDictData
{
    public int locationNum;
    public WildPokemonAppearData[] WildPokemons;
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

    #region WildPokemonLocation
    [Serializable]
    public class WildPokemonLocationData : ILoader<int, WildPokemonAppearData[]>
    {
        public List<WildPokemonLocationDictData> wildPKMLocations = new List<WildPokemonLocationDictData>();

        public Dictionary<int, WildPokemonAppearData[]> MakeDict()
        {
            Dictionary<int, WildPokemonAppearData[]> dict = new Dictionary<int, WildPokemonAppearData[]>();
            foreach (WildPokemonLocationDictData locationData in wildPKMLocations)
            {
                WildPokemonAppearData[] sortedDatas = locationData.WildPokemons.OrderBy(item => item.appearRate).ToArray();

                dict.Add(locationData.locationNum, sortedDatas);
            }
            return dict;
        }
    }
    #endregion

    #region PokemonMove
    [Serializable]
    public class PokemonMoveData : ILoader<string, PokemonMoveDictData>
    {
        public List<PokemonMoveDictData> pokemonMoves = new List<PokemonMoveDictData>();

        public Dictionary<string, PokemonMoveDictData> MakeDict()
        {
            Dictionary<string, PokemonMoveDictData> dict = new Dictionary<string, PokemonMoveDictData>();
            foreach (PokemonMoveDictData moveData in pokemonMoves)
            {
                dict.Add(moveData.moveName, moveData);
            }
            return dict;
        }
    }
    #endregion
}