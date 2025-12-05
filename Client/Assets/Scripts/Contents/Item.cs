using Google.Protobuf.Protocol;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    ItemCategory _itemCategory;
    string _itemName;
    string _itemDescription;
    int _itemCnt;
    int _itemPrice;
    List<CreatureState> _useState;
    Texture2D _itemImg;
    Texture2D _inBattleImg;

    public ItemCategory ItemCategory {  get { return _itemCategory; } }
    public string ItemName { get { return _itemName; } }
    public int ItemCnt { get { return _itemCnt; } set { _itemCnt = value; } }
    public int ItemPrice { get { return _itemPrice; } }
    public string ItemDescription { get { return _itemDescription; } }
    public Texture2D ItemImg { get { return _itemImg; } }
    public Texture2D InBattleImg { get { return _inBattleImg; } }

    public Item(ItemSummary itemSum)
    {
        _itemCategory = itemSum.ItemCategory;
        _itemName = itemSum.ItemName;
        _itemDescription = itemSum.ItemDescription;
        _itemCnt = itemSum.ItemCnt;
        _itemPrice = itemSum.ItemPrice;
        _useState = new List<CreatureState>();

        foreach (CreatureState state in itemSum.UseState)
            _useState.Add(state);

        _itemImg = Managers.Resource.Load<Texture2D>($"Textures/Item/{_itemCategory}/{_itemName}");

        if (_itemCategory == ItemCategory.PokeBall)
            _inBattleImg = Managers.Resource.Load<Texture2D>($"Textures/Item/PokeBall/{_itemName}_Battle");
    }

    public bool CanUse(CreatureState playerState)
    {
        Debug.Log(playerState);
        if (_useState.Contains(playerState))
        {
            if (_itemCategory == ItemCategory.PokeBall)
            {
                if (playerState == CreatureState.Fight && Managers.Object.MyPlayerController.NPC == null)
                    return true;
                else
                    return false;
            }
            else
            {
                return true;
            }
        }
        else
            return false;
    }

    public void UpdateItemSummary(ItemSummary itemSum)
    {
        _itemCategory = itemSum.ItemCategory;
        _itemName = itemSum.ItemName;
        _itemDescription = itemSum.ItemDescription;
        _itemCnt = itemSum.ItemCnt;
        _itemPrice = itemSum.ItemPrice;
    }
}
