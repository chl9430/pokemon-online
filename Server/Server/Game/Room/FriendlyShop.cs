using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class FriendlyShop : GameRoom
    {
        List<ItemBase> _shopItems;

        public List<ItemBase> ShopItems { get { return _shopItems; } }

        public FriendlyShop(RoomType roomType, int roomId) : base(roomType, roomId)
        {
            _shopItems = new List<ItemBase>();

            if (DataManager.ShopItemDict.TryGetValue(roomId, out ShopItemInfo[] value))
            {
                foreach (ShopItemInfo info in value)
                {
                    if (DataManager.ItemBaseDict.TryGetValue(info.itemCategory, out List<ItemBase> itemBases))
                    {
                        var settings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects
                        };

                        ItemBase itemBase = itemBases.Find(item => item._name == info.itemName);
                        string json = JsonConvert.SerializeObject(itemBase, settings);
                        ItemBase newItemBase = JsonConvert.DeserializeObject<ItemBase>(json, settings);

                        _shopItems.Add(newItemBase);
                    }
                }
            }
        }

        public void BuyItem(Player player, int itemIdx, int itemQuantity)
        {
            S_BuyItem buyItemPacket = new S_BuyItem();

            int money = player.Money;

            ItemBase selectedItem = _shopItems[itemIdx];

            int totalPrice = selectedItem._price * itemQuantity;

            if (money >= totalPrice)
            {
                money -= totalPrice;
                player.Money = money;
                player.AddItem(selectedItem._itemCategory, selectedItem._name, itemQuantity, buyItemPacket);
                buyItemPacket.IsBuy = true;
                buyItemPacket.Money = money;
            }
            else
            {
                buyItemPacket.IsBuy = false;
                buyItemPacket.Money = money;
            }

            player.Session.Send(buyItemPacket);
        }
    }
}
