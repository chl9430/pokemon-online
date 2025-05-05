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

        public static void LoadData()
        {
            PokemonSummaryDict = LoadJson<PokemonSummaryData, string, PokemonSummaryDictData>("PokemonSummaryData").MakeDict();
        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key,Value>
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
    }
}
