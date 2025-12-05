using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BagContentState
{
    None = 0,
    Slider_Ready = 1,
    Scroll_Box_Ready = 2,
    Waiting_Action = 3,
    Asking_Quantity_To_Sell = 4,
    Selecting_Quantity_To_Sell = 5,
    Asking_To_Sell = 6,
    Selecting_To_Sell = 7,
    Success_Sell_Scripting = 8,
    Inactiving = 9,
}

public class BagContent : ObjectContents
{
    Dictionary<ItemCategory, List<Item>> _items;
    BagContentState _state = BagContentState.None;

    [SerializeField] CategorySlider _categorySlider;
    [SerializeField] SlideAndScrollBox _scrollBox;
    [SerializeField] List<Image> _sliderIndicatorImgs;
    [SerializeField] GridLayoutSelectBox _gridSelectBox;
    [SerializeField] Image _itemImg;
    [SerializeField] TextMeshProUGUI _itemDescription;
    [SerializeField] CountingBox _countingBox;
    [SerializeField] TextMeshProUGUI _countingText;
    [SerializeField] TextMeshProUGUI _totalSellPrice;
    [SerializeField] GameObject _moneyBox;
    [SerializeField] TextMeshProUGUI _moneyText;

    public BagContentState State
    {
        set
        {
            _state = value;

            if (_state == BagContentState.Slider_Ready)
            {
                gameObject.SetActive(true);

                _categorySlider.SliderState = SliderState.WAITING_INPUT;
            }
            if (_state == BagContentState.Scroll_Box_Ready)
            {
                _scrollBox.State = SlideAndScrollBoxState.SELECTING;

                _gridSelectBox.UIState = GridLayoutSelectBoxState.NONE;
                _gridSelectBox.gameObject.SetActive(false);

                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
                ContentManager.Instance.ScriptBox.HideSelectBox();

                _countingBox.gameObject.SetActive(false);
                _countingBox.State = CountingBoxState.NONE;

                _moneyBox.gameObject.SetActive(false);
            }
            else if (_state == BagContentState.Waiting_Action)
            {
                _categorySlider.SliderState = SliderState.NONE;

                _scrollBox.State = SlideAndScrollBoxState.NONE;

                _gridSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
                _gridSelectBox.gameObject.SetActive(true);
            }
            else if (_state == BagContentState.Asking_Quantity_To_Sell)
            {
                _categorySlider.SliderState = SliderState.NONE;

                _scrollBox.State = SlideAndScrollBoxState.NONE;

                ContentManager.Instance.ScriptBox.gameObject.SetActive(true);
            }
            else if (_state == BagContentState.Selecting_Quantity_To_Sell)
            {
                _countingBox.gameObject.SetActive(true);
                _countingBox.State = CountingBoxState.SELECTING;

                _moneyBox.gameObject.SetActive(true);
            }
            else if (_state == BagContentState.Asking_To_Sell)
            {
                _countingBox.gameObject.SetActive(false);
                _countingBox.State = CountingBoxState.NONE;
            }
            else if (_state == BagContentState.Selecting_To_Sell)
            {

            }
            else if (_state == BagContentState.Success_Sell_Scripting)
            {
                ContentManager.Instance.ScriptBox.HideSelectBox();
            }
            else if (_state == BagContentState.Inactiving)
            {
                gameObject.SetActive(false);

                _gridSelectBox.UIState = GridLayoutSelectBoxState.NONE;

                _countingBox.State = CountingBoxState.NONE;
            }
            else if (_state == BagContentState.None)
            {
                gameObject.SetActive(false);

                _categorySlider.SliderState = SliderState.NONE;

                _scrollBox.State = SlideAndScrollBoxState.NONE;
            }
        }
    }


    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;

        if (_packet is S_SellItem)
        {
            int money = ((S_SellItem)_packet).Money;
            int itemQuantity = ((S_SellItem)_packet).ItemQuantity;
            Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;
            int count = _countingBox.GetCurrentCount();

            Managers.Object.MyPlayerController.ChangeMoney(money);
            _moneyText.text = money.ToString();

            // 아이템 수량 변경
            selectedItem.ItemCnt = itemQuantity;

            if (itemQuantity <= 0)
            {
                RemoveSelectedItem();
            }
            else
                UpdateSelectedItem();

            // 아이템 이미지, 설명 갱신
            DynamicButton btn = _scrollBox.GetSelectedBtn();
            Item newSelectedItem = btn.BtnData as Item;
            Texture2D image = newSelectedItem.ItemImg;

            _itemImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
            _itemImg.SetNativeSize();

            _itemDescription.text = (newSelectedItem).ItemDescription;

            //TextMeshProUGUI tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemName", true);
            //tmp.text = newSelectedItem.ItemName;

            //tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemCount", true);
            //tmp.text = $"x{newSelectedItem.ItemCnt}";

            // 상태 변경
            State = BagContentState.Success_Sell_Scripting;

            List<string> scripts = new List<string>()
            {
                $"Turned over the {selectedItem.ItemName} and received ${selectedItem.ItemPrice / 2 * count}."
            };
            ContentManager.Instance.BeginScriptTyping(scripts);
        }
    }

    public override void SetNextAction(object value = null)
    {
        switch (_state)
        {
            case BagContentState.None:
                {
                    State = BagContentState.Slider_Ready;

                    gameObject.SetActive(true);
                }
                break;
            case BagContentState.Slider_Ready:
                {
                    State = BagContentState.Scroll_Box_Ready;
                }
                break;
            case BagContentState.Scroll_Box_Ready:
                {
                    if (value is ItemCategory)
                    {
                        // 슬라이드 인디케이터 변경
                        for (int i = 0; i < _sliderIndicatorImgs.Count; i++)
                        {
                            Image img = _sliderIndicatorImgs[i];
                            RectTransform rt = _sliderIndicatorImgs[i].GetComponent<RectTransform>();

                            if (i == (int)value)
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

                        // 아이템 목록 갱신
                        _scrollBox.ClearBtnGrid();
                        if (_items[(ItemCategory)value].Count > 0)
                        {
                            List<object> btns = new List<object>();
                            foreach (Item item in _items[(ItemCategory)value])
                            {
                                btns.Add(item);
                            }
                            _scrollBox.CreateScrollAreaButtons(btns, btns.Count, 1);
                        }

                        // 아이템 랜더링
                        if (_items[(ItemCategory)value].Count > 0)
                        {
                            List<DynamicButton> btns = _scrollBox.ChangeBtnGridDataToList();
                            foreach (DynamicButton btn in btns)
                            {
                                TextMeshProUGUI tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemName", true);
                                tmp.text = ((Item)btn.BtnData).ItemName;

                                tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemCount", true);
                                tmp.text = $"x{((Item)btn.BtnData).ItemCnt}";
                            }
                        }
                        else
                        {
                            _itemImg.sprite = null;
                            _itemDescription.text = "";
                        }

                        _scrollBox.ShowUpAndDownArrows();

                        State = BagContentState.Scroll_Box_Ready;
                    }
                    else if (value is Item)
                    {
                        // 선택된 아이템 이미지, 설명 표시
                        Texture2D image = ((Item)value).ItemImg;

                        _itemImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
                        _itemImg.SetNativeSize();

                        _itemDescription.text = ((Item)value).ItemDescription;
                    }
                    else if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = BagContentState.None;

                            FinishContent();
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;

                            if (selectedItem == null)
                                return;

                            if (selectedItem.CanUse(Managers.Object.MyPlayerController.State))
                            {
                                // 액션 버튼 데이터 채우기
                                List<string> btnNames = new List<string>()
                                {
                                    "Use","Cancel"
                                };
                                _gridSelectBox.CreateButtons(btnNames, 1, 400, 100);
                                State = BagContentState.Waiting_Action;
                            }
                            else
                            {
                                if (Managers.Object.MyPlayerController.State == CreatureState.Shopping)
                                {
                                    State = BagContentState.Asking_Quantity_To_Sell;

                                    List<string> scripts = new List<string>()
                                    {
                                        $"{selectedItem.ItemName}?\nHow many would you like to sell?"
                                    };
                                    ContentManager.Instance.BeginScriptTyping(scripts);
                                }
                                else
                                {
                                    // 액션 버튼 데이터 채우기
                                    List<string> btnNames = new List<string>()
                                    {
                                        "Cancel"
                                    };
                                    _gridSelectBox.CreateButtons(btnNames, 1, 400, 100);
                                    State = BagContentState.Waiting_Action;
                                }
                            }
                        }
                    }
                    else if (value == null)
                    {
                        _itemImg.sprite = null;

                        _itemDescription.text = "";
                    }
                }
                break;
            case BagContentState.Waiting_Action:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = BagContentState.Slider_Ready;
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            string selectedAction = _gridSelectBox.GetSelectedBtnData() as string;

                            if (selectedAction == "Use")
                            {
                                Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;

                                if (selectedItem == null)
                                    return;

                                if (selectedItem.ItemCategory == ItemCategory.PokeBall)
                                {
                                    State = BagContentState.None;

                                    Managers.Scene.CurrentScene.PopAllContents();

                                    C_UseItemInListScene useItemPacket = new C_UseItemInListScene();
                                    useItemPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                    useItemPacket.UsedItemCategory = GetSelectedItemCategory();
                                    useItemPacket.UsedItemOrder = GetSelectedItemOrder();

                                    Managers.Network.Send(useItemPacket);
                                }
                                else if (selectedItem.ItemCategory == ItemCategory.Item)
                                {
                                    List<string> actionBtnNames = new List<string>()
                                    {
                                        "Use",
                                        "Cancel"
                                    };
                                    List<Pokemon> pokemons;
                                    if (Managers.Object.MyPlayerController.State == CreatureState.Fight)
                                        pokemons = ((BattleScene)Managers.Scene.CurrentScene).Pokemons;
                                    else
                                        pokemons = Managers.Object.MyPlayerController.MyPokemons;

                                    ContentManager.Instance.OpenPokemonList(pokemons, actionBtnNames);

                                    State = BagContentState.Inactiving;
                                }

                            }
                            else if (selectedAction == "Cancel")
                            {
                                State = BagContentState.Slider_Ready;
                            }
                        }
                    }
                }
                break;
            case BagContentState.Asking_Quantity_To_Sell:
                {
                    Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;

                    _countingBox.SetMaxValue(selectedItem.ItemCnt);

                    State = BagContentState.Selecting_Quantity_To_Sell;
                }
                break;
            case BagContentState.Selecting_Quantity_To_Sell:
                {
                    Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;
                    int count = _countingBox.GetCurrentCount();

                    int totalSellPrice = (selectedItem.ItemPrice / 2) * count;

                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = BagContentState.Slider_Ready;
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            List<string> scripts = new List<string>()
                            {
                                $"I can pay ${totalSellPrice}.\nWould that be okay?"
                            };
                            ContentManager.Instance.BeginScriptTyping(scripts);

                            State = BagContentState.Asking_To_Sell;
                        }
                    }
                    else
                    {
                        _countingText.text = $"×{count}";
                        _totalSellPrice.text = $"${totalSellPrice.ToString()}";
                    }
                }
                break;
            case BagContentState.Asking_To_Sell:
                {
                    State = BagContentState.Selecting_To_Sell;

                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btns, 1, 400, 100);
                }
                break;
            case BagContentState.Selecting_To_Sell:
                {
                    Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;
                    int selectedItemIdx = _scrollBox.GetSelectedIdx();
                    int count = _countingBox.GetCurrentCount();

                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = BagContentState.Slider_Ready;
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                if (!_isLoading)
                                {
                                    _isLoading = true;

                                    C_SellItem sellItem = new C_SellItem();
                                    sellItem.PlayerId = Managers.Object.MyPlayerController.Id;
                                    sellItem.ItemCategory = selectedItem.ItemCategory;
                                    sellItem.ItemQuantity = count;
                                    sellItem.ItemIdx = selectedItemIdx;

                                    Managers.Network.Send(sellItem);
                                }
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                State = BagContentState.Slider_Ready;
                            }
                        }
                    }
                }
                break;
            case BagContentState.Success_Sell_Scripting:
                {
                    State = BagContentState.Slider_Ready;
                }
                break;
            case BagContentState.Inactiving:
                {
                    State = BagContentState.Slider_Ready;
                }
                break;
        }
    }

    public override void FinishContent()
    {
        State = BagContentState.None;

        Managers.Scene.CurrentScene.FinishContents();
    }

    public void AddNewItem(Item item)
    {
        DynamicButton btn = _scrollBox.AddNewBtn(item);

        TextMeshProUGUI tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemName", true);
        tmp.text = ((Item)btn.BtnData).ItemName;

        tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemCount", true);
        tmp.text = $"x{((Item)btn.BtnData).ItemCnt}";
    }

    public void UpdateItemInIndex(int idx)
    {
        DynamicButton btn = _scrollBox.GetDynamicButton(idx);

        TextMeshProUGUI tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemName", true);
        tmp.text = ((Item)btn.BtnData).ItemName;

        tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemCount", true);
        tmp.text = $"x{((Item)btn.BtnData).ItemCnt}";
    }

    public Item GetSelectedItem()
    {
        return _scrollBox.GetScrollBoxContent() as Item;
    }

    public ItemCategory GetSelectedItemCategory()
    {
        return (ItemCategory)_categorySlider.GetSelectedContentData();
    }

    public int GetSelectedItemOrder()
    {
        return _scrollBox.GetSelectedIdx();
    }

    public void UpdateSelectedItem()
    {
        DynamicButton btn = _scrollBox.GetSelectedBtn();
        TextMeshProUGUI tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemName", true);
        tmp.text = ((Item)btn.BtnData).ItemName;

        tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemCount", true);
        tmp.text = $"x{((Item)btn.BtnData).ItemCnt}";
    }

    public void RemoveSelectedItem()
    {
        int selectedItemIdx = _scrollBox.GetSelectedIdx();

        _scrollBox.DeleteBtn(selectedItemIdx);

        _items[(ItemCategory)_categorySlider.GetSelectedContentData()].RemoveAt(selectedItemIdx);
    }

    public void SetBagItems(Dictionary<ItemCategory, List<Item>> items)
    {
        _items = items;

        _moneyText.text = Managers.Object.MyPlayerController.Money.ToString();

        // 아이템 카테고리 슬라이더 설정
        var sortedKeys = _items.Keys.OrderBy(key => key);
        List<object> sliderContents = new List<object>();
        foreach (ItemCategory itemCategory in sortedKeys)
        {
            sliderContents.Add(itemCategory);
        }
        _categorySlider.CreateSlideContents(sliderContents);

        // 카테고리 슬라이더에 이름넣기
        for (int i = 0; i < _categorySlider.SliderContents.Count; i++)
        {
            TextMeshProUGUI tmp = _categorySlider.SliderContents[i].GetComponentInChildren<TextMeshProUGUI>();

            tmp.text = _categorySlider.SliderContents[i].ContentData.ToString();
        }

        if (_items[0].Count > 0)
        {
            // 아이템 리스트 나열
            List<object> datas = new List<object>();
            foreach (Item item in _items[0])
            {
                datas.Add(item);
            }
            _scrollBox.CreateScrollAreaButtons(datas, datas.Count, 1);

            // 아이템 랜더링
            List<DynamicButton> btns = _scrollBox.ChangeBtnGridDataToList();
            foreach (DynamicButton btn in btns)
            {
                TextMeshProUGUI tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemName", true);
                tmp.text = ((Item)btn.BtnData).ItemName;

                tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemCount", true);
                tmp.text = $"x{((Item)btn.BtnData).ItemCnt}";
            }

            // 선택된 아이템 이미지, 설명 표시
            Texture2D image = ((Item)datas[0]).ItemImg;

            _itemImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
            _itemImg.SetNativeSize();

            _itemDescription.text = (((Item)datas[0])).ItemDescription;
        }
    }
}
