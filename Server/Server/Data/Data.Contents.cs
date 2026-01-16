using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct EffectivenessData
{
    public string targetType;
    public float multiplier;
}

public struct TypeEffectivenessDictData
{
    public string type;
    public EffectivenessData[] effectiveness;
}

public struct PokeBallDictData
{
    public string itemName;
    public string itemDescription;
    public float catchRate;
    public int price;
}

public struct PokemonMoveDictData
{
    public string moveName;
    public int movePower;
    public int moveAccuracy;
    public int maxPP;
    public MoveCategory moveCategory;
    public PokemonType moveType;
    public string moveDescription;
}

public struct EvolutionChain
{
    public int evolutionLevel;
    public string evolutionPokemonName;
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
    public int baseCatchRate;
    public LearnableMoveData[] learnableMoves;
    public EvolutionChain evolutionChain;
}

public struct WildPokemonAppearInfo
{
    public string pokemonName;
    public int pokemonLevel;
    public int appearRate;
}

public struct BushInfo
{
    public int bushNum;
    public WildPokemonAppearInfo[] wildPokemons;
}

public struct WildPokemonLocationDictData
{
    public int roomId;
    public BushInfo[] bushes;
}

public struct DoorDestInfo
{
    public int doorId;
    public RoomType destRoomType;
    public int destRoomId;
}

public struct RoomInfo
{
    public int roomId;
    public DoorDestInfo[] doors;
}

public struct RoomDoorPathDictData
{
    public RoomType roomType;
    public RoomInfo[] rooms;
}

public struct ShopItemInfo
{
    public ItemCategory itemCategory;
    public string itemName;
    public int price;
}

public struct FriendlyShopItemDictData
{
    public int shopRoomId;
    public ShopItemInfo[] shopItemInfos;
}

public struct NPCPokemonInfo
{
    public string pokemonName;
    public int pokemonLevel;
}

public struct BattleNPCDictData
{
    public int npcId;
    public NPCPokemonInfo[] battlePokemons;
    public string[] beforeBattleScripts;
    public string[] afterBattleScripts;
    public int rewardMoney;
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
    public class WildPokemonLocationData : ILoader<int, BushInfo[]>
    {
        public List<WildPokemonLocationDictData> wildPKMLocations = new List<WildPokemonLocationDictData>();

        public Dictionary<int, BushInfo[]> MakeDict()
        {
            Dictionary<int, BushInfo[]> dict = new Dictionary<int, BushInfo[]>();
            foreach (WildPokemonLocationDictData locationData in wildPKMLocations)
            {
                for (int j = 0; j < locationData.bushes.Length; j++)
                {
                    BushInfo bushInfo = locationData.bushes[j];
                    WildPokemonAppearInfo[] sortedDatas = bushInfo.wildPokemons.OrderBy(item => item.appearRate).ToArray();

                    bushInfo.wildPokemons = sortedDatas;
                }

                dict.Add(locationData.roomId, locationData.bushes);
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

    #region PokeBall
    [Serializable]
    public class PokeBallData : ILoader<string, PokeBallDictData>
    {
        public List<PokeBallDictData> pokeBallItems = new List<PokeBallDictData>();

        public Dictionary<string, PokeBallDictData> MakeDict()
        {
            Dictionary<string, PokeBallDictData> dict = new Dictionary<string, PokeBallDictData>();
            foreach (PokeBallDictData itemData in pokeBallItems)
            {
                dict.Add(itemData.itemName, itemData);
            }
            return dict;
        }
    }
    #endregion

    #region TypeEffectiveness
    [Serializable]
    public class TypeEffectiveness : ILoader<PokemonType, EffectivenessData[]>
    {
        public List<TypeEffectivenessDictData> pokemonTypeEffectiveness = new List<TypeEffectivenessDictData>();

        public Dictionary<PokemonType, EffectivenessData[]> MakeDict()
        {
            Dictionary<PokemonType, EffectivenessData[]> dict = new Dictionary<PokemonType, EffectivenessData[]>();
            foreach (TypeEffectivenessDictData effectivenessData in pokemonTypeEffectiveness)
            {
                dict.Add((PokemonType)Enum.Parse(typeof(PokemonType), effectivenessData.type), effectivenessData.effectiveness);
            }
            return dict;
        }
    }
    #endregion

    #region RoomDoorPath
    [Serializable]
    public class RoomDoorPath : ILoader<RoomType, RoomInfo[]>
    {
        public List<RoomDoorPathDictData> roomPathDoorDatas = new List<RoomDoorPathDictData>();

        public Dictionary<RoomType, RoomInfo[]> MakeDict()
        {
            Dictionary<RoomType, RoomInfo[]> dict = new Dictionary<RoomType, RoomInfo[]>();
            foreach (RoomDoorPathDictData data in roomPathDoorDatas)
            {
                RoomInfo[] roomList = data.rooms;

                for (int i = 0; i < roomList.Length; i++)
                {
                    DoorDestInfo[] sortedList = roomList[i].doors.OrderBy(item => item.doorId).ToArray();
                    roomList[i].doors = sortedList;
                }

                RoomInfo[] sortedRoomList = roomList.OrderBy(item => item.roomId).ToArray();

                dict.Add(data.roomType, sortedRoomList);
            }
            return dict;
        }
    }
    #endregion

    #region FriendlyShopItem
    [Serializable]
    public class FriendlyShopItem : ILoader<int, ShopItemInfo[]>
    {
        public List<FriendlyShopItemDictData> shopItems = new List<FriendlyShopItemDictData>();

        public Dictionary<int, ShopItemInfo[]> MakeDict()
        {
            Dictionary<int, ShopItemInfo[]> dict = new Dictionary<int, ShopItemInfo[]>();
            foreach (FriendlyShopItemDictData data in shopItems)
            {
                dict.Add(data.shopRoomId, data.shopItemInfos);
            }
            return dict;
        }
    }
    #endregion

    #region BattleNPC
    [Serializable]
    public class BattleNPC : ILoader<int, BattleNPCDictData>
    {
        public List<BattleNPCDictData> battleNPCs = new List<BattleNPCDictData>();

        public Dictionary<int, BattleNPCDictData> MakeDict()
        {
            Dictionary<int, BattleNPCDictData> dict = new Dictionary<int, BattleNPCDictData>();
            foreach (BattleNPCDictData data in battleNPCs)
            {
                dict.Add(data.npcId, data);
            }
            return dict;
        }
    }
    #endregion

    #region ItemBaseLoader
    [Serializable]
    public class ItemBaseLoader : ILoader<ItemCategory, List<ItemBase>>
    {
        public List<ItemBase> itemBases = new List<ItemBase>();

        public Dictionary<ItemCategory, List<ItemBase>> MakeDict()
        {
            Dictionary<ItemCategory, List<ItemBase>> dict = new Dictionary<ItemCategory, List<ItemBase>>();
            foreach (var itemBase in itemBases)
            {
                if (dict.ContainsKey(itemBase._itemCategory) == false)
                    dict.Add(itemBase._itemCategory, new List<ItemBase>());
                
                dict[itemBase._itemCategory].Add(itemBase);
            }
            return dict;
        }
    }
    #endregion
}