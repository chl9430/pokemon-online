using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PokemonTypeChecker
    {
        public static PokemonTypeChecker Instance { get; } = new PokemonTypeChecker();

        Dictionary<PokemonType, EffectivenessData[]> _typeEffectiveness;

        PokemonTypeChecker()
        {
            _typeEffectiveness = DataManager.TypeEffectivenessDict;
        }

        public float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
        {
            _typeEffectiveness.TryGetValue(attackType, out EffectivenessData[] effectiveness);

            for (int i = 0; i < effectiveness.Length; i++)
            {
                PokemonType type;
                Enum.TryParse(effectiveness[i].targetType, true, out type);

                if (type == defenseType)
                    return effectiveness[i].multiplier;
            }

            return 1.0f;
        }
    }
}
