using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Item
    {
        protected ItemCategory _itemCategory;
        protected string _itemName;
        protected string _itemDescription;
        protected int _itemCnt;

        public string ItemName { get { return _itemName; } }
        public int ItemCount { get { return _itemCnt; } set { _itemCnt = value; } }

        public Item(int itemCnt) 
        {
            _itemCnt = itemCnt;
        }

        public ItemSummary MakeItemSummary()
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
