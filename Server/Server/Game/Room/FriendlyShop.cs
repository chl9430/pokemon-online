using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class FriendlyShop : GameRoom
    {
        List<Item> _shopItems;

        public List<Item> ShopItems { get { return _shopItems; } }

        public FriendlyShop(RoomType roomType, int roomId) : base(roomType, roomId)
        {
            _shopItems = new List<Item>();

            if (DataManager.ShopItemDict.TryGetValue(roomId, out ShopItemInfo[] value))
            {
                foreach (ShopItemInfo info in value)
                {
                    if (info.itemCategory == ItemCategory.PokeBall)
                    {
                        _shopItems.Add(new PokeBall(info.itemName, 1));
                    }
                }
            }

            // 프렌들리숍 스태프 생성
            if (roomType == RoomType.FriendlyShop)
            {
                NPC npc = ObjectManager.Instance.Add<NPC>();
                {
                    npc.Info.PosInfo.State = CreatureState.Idle;
                    npc.Info.PosInfo.MoveDir = MoveDir.Right;
                    npc.Name = "Staff";
                    npc.Room = this;
                }

                if (_objs.ContainsKey(GameObjectType.Npc) == false)
                    _objs.Add(GameObjectType.Npc, new Dictionary<int, GameObject>());

                _objs[GameObjectType.Npc].Add(npc.Id, npc);

                Vector2Int npcPos = Map.GetTilePos(2, 3);
                npc.PosInfo.PosX = npcPos.x;
                npc.PosInfo.PosY = npcPos.y;

                Map.SetObj(3, 2, npc);
            }
        }

        public void BuyItem(Player player, int itemIdx, int itemQuantity)
        {
            S_BuyItem buyItemPacket = new S_BuyItem();

            int money = player.Money;

            Item selectedItem = _shopItems[itemIdx];

            int totalPrice = selectedItem.Price * itemQuantity;

            if (money >= totalPrice)
            {
                money -= totalPrice;
                player.AddItem(selectedItem.ItemCategory, selectedItem.ItemName, itemQuantity);
                buyItemPacket.IsBuy = true;
            }
            else
            {
                buyItemPacket.IsBuy = false;
            }

            player.Session.Send(buyItemPacket);
        }
    }
}
