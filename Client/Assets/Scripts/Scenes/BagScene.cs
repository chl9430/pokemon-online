using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BagSceneState
{
    NONE = 0,
    WAITING_INPUT = 1,
    WAITING_ACTION = 2,
}

public class BagScene : BaseScene
{
    bool _loadingPacket = false;
    string _selectedAction;
    Item _selectedItem;
    ItemCategory _selectedCategory;
    BagSceneState _sceneState = BagSceneState.NONE;

    PlayerInfo _playerInfo;
    Dictionary<ItemCategory, List<Item>> _bag;

    [SerializeField] CategorySlider _categorySlider;
    [SerializeField] List<Image> _sliderIndicatorImgs;
    [SerializeField] ScrollSelectBox _scrollSelectBox;
    [SerializeField] List<Image> _scrollBoxArrow;
    [SerializeField] GridSelectBox _gridSelectBox;
    [SerializeField] Image _itemImg;
    [SerializeField] TextMeshProUGUI _itemDescription;


    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Bag;

        _bag = new Dictionary<ItemCategory, List<Item>>();

        // �׽�Ʈ �� ���.
        if (Managers.Network.Packet == null)
        {
            C_EnterPlayerBagScene enterBagScenePacket = new C_EnterPlayerBagScene();
            enterBagScenePacket.PlayerId = -1;

            Managers.Network.Send(enterBagScenePacket);
        }
        else
            Managers.Network.SendSavedPacket();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void UpdateData(IMessage packet)
    {
        switch (_sceneState)
        {
            case BagSceneState.NONE:
                {
                    _sceneState = BagSceneState.WAITING_INPUT;
                    ActiveUIBySceneState(_sceneState);

                    S_EnterPlayerBagScene enterBagScenePacket = packet as S_EnterPlayerBagScene;
                    PlayerInfo info = enterBagScenePacket.PlayerInfo;
                    MapField<int, CategoryInventory> inventory = enterBagScenePacket.Inventory;

                    // �÷��̾� ���� ����
                    _playerInfo = info;

                    // �κ��丮 ä���
                    foreach (var pair in inventory)
                    {
                        ItemCategory itemCategory = (ItemCategory)pair.Key;
                        _bag[itemCategory] = new List<Item>();

                        List<ItemSummary> categoryItems = new List<ItemSummary>(pair.Value.CategoryItemSums);

                        foreach (ItemSummary itemSum in categoryItems)
                        {
                            _bag[itemCategory].Add(new Item(itemSum));
                        }
                    }

                    // ������ ī�װ� �����̴� ����
                    List<object> _itemCategories = Enum.GetValues(typeof(ItemCategory))
                                            .Cast<ItemCategory>()
                                            .Where(key => _bag.ContainsKey(key))
                                            .Select(key => (object)key)
                    .ToList();

                    // ������ ��ǲ�� �ޱ� �����Ѵ�.
                    _categorySlider.SetSliderContents(_itemCategories);

                    // ����Ʈ �ڽ� ������ ä���
                    if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                        _gridSelectBox.SetButtonDatas(new List<object>() { "Use", "Give", "Toss", "Cancel" });
                }
                break;
        }
    }

    public override void DoNextAction(object value = null)
    {
        switch (_sceneState)
        {
            case BagSceneState.WAITING_INPUT:
                {
                    if (value is ScrollSelectBox)
                    {
                        ScrollSelectBox scrollBox = value as ScrollSelectBox;

                        if (scrollBox.ScrollBoxContents.Count == 0)
                        {
                            _scrollBoxArrow[0].gameObject.SetActive(false);
                            _scrollBoxArrow[1].gameObject.SetActive(false);

                            _itemImg.sprite = null;

                            _itemDescription.text = "";
                            return;
                        }

                        if (scrollBox.ScrollCnt == 0)
                        {
                            _scrollBoxArrow[0].gameObject.SetActive(false);
                            _scrollBoxArrow[1].gameObject.SetActive(true);
                        }
                        else if (scrollBox.ScrollCnt == scrollBox.ScrollBoxContents.Count - scrollBox.ViewCount)
                        {
                            _scrollBoxArrow[0].gameObject.SetActive(true);
                            _scrollBoxArrow[1].gameObject.SetActive(false);
                        }
                        else
                        {
                            _scrollBoxArrow[0].gameObject.SetActive(true);
                            _scrollBoxArrow[1].gameObject.SetActive(true);
                        }

                        Item selectedItem = scrollBox.SelectedContent.BtnData as Item;

                        Texture2D image = selectedItem.ItemImg;

                        _itemImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
                        _itemImg.SetNativeSize();

                        _itemDescription.text = selectedItem.ItemDescription;
                    }
                    else if (value is CategorySlider) // ī�װ� �����̴��� ����������
                    {
                        ItemCategory selectedCategory = (ItemCategory)((value as CategorySlider).GetSelectedContentData());
                        _selectedCategory = selectedCategory;

                        for (int i = 0; i < _sliderIndicatorImgs.Count; i++)
                        {
                            Image img = _sliderIndicatorImgs[i];
                            RectTransform rt = _sliderIndicatorImgs[i].GetComponent<RectTransform>();

                            if (i == (int)selectedCategory)
                            {
                                rt.sizeDelta = new Vector2(30, 30);
                                img.color = Color.red;
                            }
                            else
                            {
                                rt.sizeDelta = new Vector2(15, 15);
                                img.color = Color.white;
                            }
                        }

                        // ������ �������� ī�װ� �� ������ ����Ʈ ����
                        List<ArrowButton> scrollBoxContents = _scrollSelectBox.ScrollBoxContents;

                        foreach (ArrowButton obj in scrollBoxContents)
                        {
                            Managers.Resource.Destroy(obj.gameObject);
                        }

                        scrollBoxContents.Clear();

                        // ī�װ� �� ������ ����Ʈ ����
                        foreach (Item item in _bag[selectedCategory])
                        {
                            ArrowButton scrollBoxContent = Managers.Resource.Instantiate("UI/BagScene/ScrollContentZone", _scrollSelectBox.transform).GetComponent<ArrowButton>();

                            Util.FindChild<TextMeshProUGUI>(scrollBoxContent.gameObject, "ItemName", true).text = item.ItemName;

                            Util.FindChild<TextMeshProUGUI>(scrollBoxContent.gameObject, "ItemCount", true).text = "��" + item.ItemCnt;

                            scrollBoxContent.BtnData = item;

                            scrollBoxContents.Add(scrollBoxContent);
                        }

                        // ������ ��ǲ�� �ޱ� �����Ѵ�.
                        _categorySlider.SliderState = SliderState.WAITING_INPUT;

                        _scrollSelectBox.CreateScrollBoxItems(scrollBoxContents);
                        _scrollSelectBox.ScrollBoxState = ScrollBoxState.WAITING_INPUT;
                    }
                    else
                    {
                        Item item = value as Item;

                        _selectedItem = item;

                        _sceneState = BagSceneState.WAITING_ACTION;
                        ActiveUIBySceneState(_sceneState);
                    }
                }
                break;
            case BagSceneState.WAITING_ACTION:
                {
                    string input = value as string;

                    if (input == "Select")
                    {
                        Debug.Log(_playerInfo.ObjectInfo.PosInfo.State);
                        if (_selectedAction == "Use")
                        {
                            if (!_loadingPacket)
                            {
                                if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                                {
                                    C_UseItem c_useItem = new C_UseItem();
                                    c_useItem.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    c_useItem.ItemCategory = _selectedCategory;
                                    c_useItem.ItemOrder = _bag[_selectedCategory].FindIndex(item => item == _selectedItem);

                                    Managers.Network.SavePacket(c_useItem);

                                    ScreenChanger.ChangeAndFadeOutScene(Define.Scene.Battle);
                                }

                                _loadingPacket = true;
                            }
                        }
                        else if (_selectedAction == "Toss")
                        {
                            Debug.Log("Toss");
                        }
                        else if (_selectedAction == "Give")
                        {
                            Debug.Log("Give");
                        }
                        else if (_selectedAction == "Cancel")
                        {
                            _sceneState = BagSceneState.WAITING_INPUT;
                            ActiveUIBySceneState(_sceneState);
                        }
                    }
                    else if (input == "Back")
                    {
                        Debug.Log($"Back!");
                        
                        _sceneState = BagSceneState.WAITING_INPUT;
                        ActiveUIBySceneState(_sceneState);
                    }
                    else
                    {
                        _selectedAction = input;
                    }
                }
                break;
        }
    }

    void ActiveUIBySceneState(BagSceneState state)
    {
        if (state == BagSceneState.WAITING_INPUT)
        {
            _categorySlider.SliderState = SliderState.WAITING_INPUT;
            _scrollSelectBox.ScrollBoxState = ScrollBoxState.WAITING_INPUT;
            _gridSelectBox.ChangeUIState(GridSelectBoxState.NONE, false);
        }
        else if (state == BagSceneState.WAITING_ACTION)
        {
            _categorySlider.SliderState = SliderState.NONE;
            _scrollSelectBox.ScrollBoxState = ScrollBoxState.NONE;
            _gridSelectBox.ChangeUIState(GridSelectBoxState.SELECTING, true);
        }
    }


    public override void Clear()
    {
    }
}
