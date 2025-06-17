using Google.Protobuf.Protocol;
using UnityEngine;

public class Item
{
    ItemCategory _itemCategory;
    string _itemName;
    string _itemDescription;
    int _itemCnt;
    Texture2D _itemImg;

    public string ItemName { get { return _itemName; } }
    public int ItemCnt { get { return _itemCnt; } }
    public string ItemDescription { get { return _itemDescription; } }
    public Texture2D ItemImg { get { return _itemImg; } }

    public Item(ItemSummary itemSum)
    {
        _itemCategory = itemSum.ItemCategory;
        _itemName = itemSum.ItemName;
        _itemDescription = itemSum.ItemDescription;
        _itemCnt = itemSum.ItemCnt;

        _itemImg = Managers.Resource.Load<Texture2D>($"Textures/Item/{_itemCategory}/{_itemName}");
    }
}
