using System.Collections.Generic;
using System.Xml.Linq;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;

namespace Server
{
    public enum RecoveryMethod
    {
        RecoveryMethodNone = 0,
        Fixed = 1,
        Full = 2,
    }

    public enum ItemType
    {
        ItemTypeNone = 0,
        HPRecoveryItem = 1,
        StatusCureItem = 2,
        ReviveItem = 3,
    }

    public abstract class ItemBase
    {
        public string _name;
        public string _description;
        public int _itemCnt;
        public int _price;
        public ItemCategory _itemCategory;
        public CreatureState[] _useState;

        public virtual void UseItem(S_UseItemInListScene useItemPacket, Pokemon pokemon, Player player)
        {
        }

        public virtual ItemSummary MakeItemSummary()
        {
            ItemSummary itemSum = new ItemSummary()
            {
                ItemCategory = _itemCategory,
                ItemName = _name,
                ItemDescription = _description,
                ItemCnt = _itemCnt,
                ItemPrice = _price,
            };

            foreach (CreatureState state in _useState)
                itemSum.UseState.Add(state);

            return itemSum;
        }
    }

    public class HPRecoveryItem : ItemBase
    {
        public int _recoveryAmount;
        public RecoveryMethod _recoveryMethod;
        public ItemType _itemType;

        public override void UseItem(S_UseItemInListScene useItemPacket, Pokemon pokemon, Player player)
        {
            if (_recoveryMethod == RecoveryMethod.Fixed)
            {
                int recoveryAmount = 0;

                if (pokemon.PokemonInfo.PokemonStatus != PokemonStatusCondition.Fainting &&
                    pokemon.PokemonStat.Hp < pokemon.PokemonStat.MaxHp)
                {
                    useItemPacket.ItemUseResult = new ItemUseResult();

                    if (pokemon.PokemonStat.Hp + _recoveryAmount <= pokemon.PokemonStat.MaxHp)
                        recoveryAmount = _recoveryAmount;
                    else
                        recoveryAmount = pokemon.PokemonStat.MaxHp - pokemon.PokemonStat.Hp;

                    pokemon.PokemonStat.Hp += recoveryAmount;

                    useItemPacket.ItemUseResult.HpRecoveryItemUseResult = new HPRecoveryItemUseResult();
                    useItemPacket.ItemUseResult.HpRecoveryItemUseResult.RealRecoveryAmt = recoveryAmount;
                }
            }
        }
    }

    public class StatusCureItem : ItemBase
    {
        public List<PokemonStatusCondition> _statusesToCure;
        public ItemType _itemType;
    }

    public class ReviveItem : ItemBase
    {
        public int _reviveHPRate;
        public ItemType _itemType;
    }

    public class PokeBall : ItemBase
    {
        public float _catchRate;

        Random _random = new Random();

        public override void UseItem(S_UseItemInListScene useItemPacket, Pokemon pokemon, Player player)
        {
            Pokemon wildPokemon = player.BattleRoom.OpponentPokemon;
            float hpModifier = (3.0f * wildPokemon.PokemonStat.MaxHp - 2.0f * wildPokemon.PokemonStat.Hp) / (3.0f * wildPokemon.PokemonStat.MaxHp);

            float statusModifier = 1.0f;

            float a = hpModifier * wildPokemon.PokemonSummaryDictData.baseCatchRate * _catchRate * statusModifier;

            if (a > 255)
                a = 255;

            bool isCatch = true;
            for (int i = 0; i < 4; i++)
            {
                // 0부터 65535 사이의 난수 생성
                int randomNumber = _random.Next(0, 65536);

                // 난수가 a * 256보다 크면 실패
                if (randomNumber > a * 256)
                {
                    Console.WriteLine($"포획 실패! (난수: {randomNumber}, 요구 값: {a * 256})");
                    isCatch = false;
                    break;
                }

                Console.WriteLine($"포획 성공! (난수: {randomNumber}, 요구 값: {a * 256})");
            }

            useItemPacket.ItemUseResult = new ItemUseResult();
            useItemPacket.ItemUseResult.PokeBallUseResult = new PokeBallUseResult();
            useItemPacket.ItemUseResult.PokeBallUseResult.DidCatch = isCatch;

            if (isCatch)
            {
                player.AddPokemon(wildPokemon);
            }
        }
    }
}