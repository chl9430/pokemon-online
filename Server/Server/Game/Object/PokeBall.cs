using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PokeBall : Item
    {
        float _catchRate;
        PokeBallDictData _ballItemDictData;

        public float CatchRate { get { return _catchRate; } }

        public PokeBall(string itemName, int itemCnt) : base(itemCnt)
        {
            if (DataManager.PokeBallItemDict.TryGetValue(itemName, out _ballItemDictData))
            {
                _itemCategory = ItemCategory.PokeBall;
                _itemName = _ballItemDictData.itemName;
                _itemDescription = _ballItemDictData.itemDescription;
                _catchRate = _ballItemDictData.catchRate;
            }
            else
            {
                Console.WriteLine("Cannot find pokeball data!");
            }
        }

        public override ItemSummary MakeItemSummary()
        {
            ItemSummary itemSum = new ItemSummary()
            {
                ItemCategory = ItemCategory.PokeBall,
                ItemName = _itemName,
                ItemDescription = _itemDescription,
                ItemCnt = _itemCnt,
            };

            return itemSum;
        }
    }
}
