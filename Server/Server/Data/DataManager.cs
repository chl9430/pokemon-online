using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        public static Dictionary<string, PokemonSummaryDictData> PokemonSummaryDict { get; private set; } = new Dictionary<string, PokemonSummaryDictData>();
        public static Dictionary<int, WildPokemonAppearData[]> WildPKMLocationDict { get; private set; } = new Dictionary<int, WildPokemonAppearData[]>();
        public static Dictionary<string, PokemonMoveDictData> PokemonMoveDict { get; private set; } = new Dictionary<string, PokemonMoveDictData>();
        public static Dictionary<string, PokeBallDictData> PokeBallItemDict { get; private set; } = new Dictionary<string, PokeBallDictData>();
        public static Dictionary<PokemonType, EffectivenessData[]> TypeEffectivenessDict { get; private set; } = new Dictionary<PokemonType, EffectivenessData[]>();

        public static void LoadData()
        {
            PokemonSummaryDict = LoadJson<PokemonSummaryData, string, PokemonSummaryDictData>("PokemonSummaryData").MakeDict();
            WildPKMLocationDict = LoadJson<WildPokemonLocationData, int, WildPokemonAppearData[]>("WildPokemonLocationData").MakeDict();
            PokemonMoveDict = LoadJson<PokemonMoveData, string, PokemonMoveDictData>("PokemonMoveData").MakeDict();
            PokeBallItemDict = LoadJson<PokeBallData, string, PokeBallDictData>("PokeBallItemData").MakeDict();
            TypeEffectivenessDict = LoadJson<TypeEffectiveness, PokemonType, EffectivenessData[]>("PokemonTypeEffectivenessData").MakeDict();
        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key,Value>
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
    }
}
