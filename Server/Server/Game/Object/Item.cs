using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Item
    {
        protected ItemCategory _itemCategory;
        protected string _itemName;
        protected string _itemDescription;
        protected int _itemCnt;
        protected int _price;

        public ItemCategory ItemCategory { get { return _itemCategory; } }
        public string ItemName { get { return _itemName; } }
        public int ItemCount { get { return _itemCnt; } set { _itemCnt = value; } }
        public int Price { get { return _price; } }

        public Item(int itemCnt) 
        {
            _itemCnt = itemCnt;
        }

        public abstract ItemSummary MakeItemSummary();
    }
}
