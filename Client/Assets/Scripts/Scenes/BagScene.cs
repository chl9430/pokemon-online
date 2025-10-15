using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BagSceneState
{
    NONE = 0,
    SLIDER_READY = 1,
    SCROLL_BOX_READY = 2,
    WAITING_ACTION = 3,
    ASKING_QUANTITY_TO_SELL = 4,
    SELECTING_QUANTITY_TO_SELL = 5,
    ASKING_TO_SELL = 6,
    SELECTING_TO_SELL = 7,
    SUCCESS_SELL_SCRIPTING = 8,
    MOVING_TO_SCENE = 9,
}

public class BagScene : BaseScene
{
    bool _isLoading = false;
    IMessage _packet;
    BagSceneState _sceneState = BagSceneState.NONE;

    PlayerInfo _playerInfo;
    Dictionary<ItemCategory, List<Item>> _bag;

    [SerializeField] CategorySlider _categorySlider;
    [SerializeField] SlideAndScrollBox _scrollBox;
    [SerializeField] List<Image> _sliderIndicatorImgs;
    [SerializeField] GridLayoutSelectBox _gridSelectBox;
    [SerializeField] Image _itemImg;
    [SerializeField] TextMeshProUGUI _itemDescription;
    [SerializeField] ScriptBoxUI _scriptBox;
    [SerializeField] CountingBox _countingBox;
    [SerializeField] TextMeshProUGUI _countingText;
    [SerializeField] TextMeshProUGUI _totalSellPrice;
    [SerializeField] GameObject _moneyBox;
    [SerializeField] TextMeshProUGUI _moneyText;

    public BagSceneState State
    {
        set
        {
            _sceneState = value;

            if (_sceneState == BagSceneState.SLIDER_READY)
            {
                _categorySlider.SliderState = SliderState.WAITING_INPUT;
            }
            if (_sceneState == BagSceneState.SCROLL_BOX_READY)
            {
                _scrollBox.State = SlideAndScrollBoxState.SELECTING;

                _gridSelectBox.UIState = GridLayoutSelectBoxState.NONE;
                _gridSelectBox.gameObject.SetActive(false);

                _scriptBox.gameObject.SetActive(false);
                _scriptBox.HideSelectBox();

                _countingBox.gameObject.SetActive(false);
                _countingBox.State = CountingBoxState.NONE;

                _moneyBox.gameObject.SetActive(false);
            }
            else if (_sceneState == BagSceneState.WAITING_ACTION)
            {
                _categorySlider.SliderState = SliderState.NONE;

                _scrollBox.State = SlideAndScrollBoxState.NONE;

                _gridSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
                _gridSelectBox.gameObject.SetActive(true);
            }
            else if (_sceneState == BagSceneState.ASKING_QUANTITY_TO_SELL)
            {
                _categorySlider.SliderState = SliderState.NONE;

                _scrollBox.State = SlideAndScrollBoxState.NONE;

                _scriptBox.gameObject.SetActive(true);
            }
            else if (_sceneState == BagSceneState.SELECTING_QUANTITY_TO_SELL)
            {
                _countingBox.gameObject.SetActive(true);
                _countingBox.State = CountingBoxState.SELECTING;

                _moneyBox.gameObject.SetActive(true);
            }
            else if (_sceneState == BagSceneState.ASKING_TO_SELL)
            {
                _countingBox.gameObject.SetActive(false);
                _countingBox.State = CountingBoxState.NONE;
            }
            else if (_sceneState == BagSceneState.SELECTING_TO_SELL)
            {

            }
            else if (_sceneState == BagSceneState.SUCCESS_SELL_SCRIPTING)
            {
                _scriptBox.HideSelectBox();
            }
            else if (_sceneState == BagSceneState.MOVING_TO_SCENE)
            {
                _categorySlider.SliderState = SliderState.NONE;

                _scrollBox.State = SlideAndScrollBoxState.NONE;
            }
        }
    }


    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Bag;

        _bag = new Dictionary<ItemCategory, List<Item>>();
    }

    protected override void Start()
    {
        base.Start();

        // 테스트 시 사용.
        if (Managers.Network.Packet == null)
        {
            C_EnterPlayerBagScene enterBagScenePacket = new C_EnterPlayerBagScene();
            enterBagScenePacket.PlayerId = -1;

            Managers.Network.Send(enterBagScenePacket);
        }
        else
            Managers.Network.SendSavedPacket();
    }

    public override void UpdateData(IMessage packet)
    {
        _isLoading = false;

        _packet = packet;

        if (packet is S_EnterPlayerBagScene)
        {
            _enterEffect.PlayEffect("FadeIn");

            S_EnterPlayerBagScene enterBagScenePacket = packet as S_EnterPlayerBagScene;
            PlayerInfo info = enterBagScenePacket.PlayerInfo;
            MapField<int, CategoryInventory> inventory = enterBagScenePacket.Inventory;

            // 플레이어 정보 저장
            _playerInfo = info;

            _moneyText.text = info.Money.ToString();

            // 인벤토리 채우기
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

            // 아이템 카테고리 슬라이더 설정
            var sortedKeys = _bag.Keys.OrderBy(key => key);
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

            // 아이템 리스트 나열
            if (_bag[0].Count > 0)
            {
                List<object> datas = new List<object>();
                foreach (Item item in _bag[0])
                {
                    datas.Add(item);
                }
                _scrollBox.CreateScrollAreaButtons(datas, datas.Count, 1);
            }

            // 아이템 랜더링
            if (_bag[0].Count > 0)
            {
                List<DynamicButton> btns = _scrollBox.ChangeBtnGridDataToList();
                foreach (DynamicButton btn in btns)
                {
                    TextMeshProUGUI tmp = btn.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
                    tmp.text = ((Item)btn.BtnData).ItemName;

                    tmp = btn.transform.Find("ItemCount").GetComponent<TextMeshProUGUI>();
                    tmp.text = $"x{((Item)btn.BtnData).ItemCnt}";
                }
            }

            _sceneState = BagSceneState.NONE;
        }
        else if (_packet is S_SellItem)
        {
            int money = ((S_SellItem)_packet).Money;
            int itemQuantity = ((S_SellItem)_packet).ItemQuantity;
            Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;
            int count = _countingBox.GetCurrentCount();

            _moneyText.text = money.ToString();

            if (itemQuantity <= 0)
            {
                int selectedItemIdx = _scrollBox.GetSelectedIdx();

                _scrollBox.DeleteBtn(selectedItemIdx);
            }
            else
                selectedItem.ItemCnt = itemQuantity;

            // 아이템 이미지, 설명 갱신
            DynamicButton btn = _scrollBox.GetSelectedBtn();
            Item newSelectedItem = _scrollBox.GetScrollBoxContent() as Item;
            Texture2D image = newSelectedItem.ItemImg;

            _itemImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
            _itemImg.SetNativeSize();

            _itemDescription.text = (newSelectedItem).ItemDescription;

            TextMeshProUGUI tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemName", true);
            tmp.text = newSelectedItem.ItemName;

            tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ItemCount", true);
            tmp.text = $"x{newSelectedItem.ItemCnt}";

            // 상태 변경
            State = BagSceneState.SUCCESS_SELL_SCRIPTING;

            List<string> scripts = new List<string>()
            {
                $"Turned over the {selectedItem.ItemName} and received ${selectedItem.ItemPrice / 2 * count}."
            };
            _scriptBox.BeginScriptTyping(scripts);
        }
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);
        switch (_sceneState)
        {
            case BagSceneState.NONE:
                {
                    State = BagSceneState.SLIDER_READY;
                }
                break;
            case BagSceneState.SLIDER_READY:
                {
                    State = BagSceneState.SCROLL_BOX_READY;
                }
                break;
            case BagSceneState.SCROLL_BOX_READY:
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
                        if (_bag[(ItemCategory)value].Count > 0)
                        {
                            List<object> btns = new List<object>();
                            foreach (Item item in _bag[(ItemCategory)value])
                            {
                                btns.Add(item);
                            }
                            _scrollBox.CreateScrollAreaButtons(btns, btns.Count, 1);
                        }

                        // 아이템 랜더링
                        if (_bag[(ItemCategory)value].Count > 0)
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

                        State = BagSceneState.SCROLL_BOX_READY;
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
                            if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.WatchMenu)
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                C_ReturnGame returnGamePacket = new C_ReturnGame();
                                returnGamePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(returnGamePacket);

                                State = BagSceneState.MOVING_TO_SCENE;
                            }
                            else if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                C_ItemSceneToBattleScene itemToBattlePacket = new C_ItemSceneToBattleScene();
                                itemToBattlePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                itemToBattlePacket.ItemCategory = (ItemCategory)_categorySlider.GetSelectedContentData();
                                itemToBattlePacket.ItemOrder = -1;

                                Managers.Network.SavePacket(itemToBattlePacket);

                                State = BagSceneState.MOVING_TO_SCENE;
                            }
                            else if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Shopping)
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                C_ReturnGame returnGamePacket = new C_ReturnGame();
                                returnGamePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(returnGamePacket);

                                State = BagSceneState.MOVING_TO_SCENE;
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;

                            if (selectedItem.ItemCategory == ItemCategory.PokeBall)
                            {
                                if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                                {
                                    // 액션 버튼 데이터 채우기
                                    List<string> btnNames = new List<string>()
                                    {
                                        "Use","Cancel"
                                    };
                                    _gridSelectBox.CreateButtons(btnNames, 1, 400, 100);
                                    State = BagSceneState.WAITING_ACTION;
                                }
                                else if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Shopping)
                                {
                                    State = BagSceneState.ASKING_QUANTITY_TO_SELL;

                                    List<string> scripts = new List<string>()
                                    {
                                        $"{selectedItem.ItemName}?\nHow many would you like to sell?"
                                    };
                                    _scriptBox.BeginScriptTyping(scripts);
                                }
                                else
                                {
                                    // 액션 버튼 데이터 채우기
                                    List<string> btnNames = new List<string>()
                                    {
                                        "Cancel"
                                    };
                                    _gridSelectBox.CreateButtons(btnNames, 1, 400, 100);
                                    State = BagSceneState.WAITING_ACTION;
                                }
                            }
                        }
                    }
                }
                break;
            case BagSceneState.WAITING_ACTION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = BagSceneState.SLIDER_READY;
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            string selectedAction = _gridSelectBox.GetSelectedBtnData() as string;

                            if (selectedAction == "Use")
                            {
                                if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                                {
                                    C_ItemSceneToBattleScene itemToBattlePacket = new C_ItemSceneToBattleScene();
                                    itemToBattlePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    itemToBattlePacket.ItemCategory = (ItemCategory)_categorySlider.GetSelectedContentData();
                                    itemToBattlePacket.ItemOrder = _bag[(ItemCategory)_categorySlider.GetSelectedContentData()].FindIndex(item => item == _scrollBox.GetScrollBoxContent());

                                    Managers.Network.SavePacket(itemToBattlePacket);

                                    _enterEffect.PlayEffect("FadeOut");

                                    _gridSelectBox.UIState = GridLayoutSelectBoxState.NONE;

                                    State = BagSceneState.MOVING_TO_SCENE;
                                }
                            }
                            else if (selectedAction == "Cancel")
                            {
                                State = BagSceneState.SLIDER_READY;
                            }
                        }
                    }
                }
                break;
            case BagSceneState.MOVING_TO_SCENE:
                {
                    if (Managers.Network.Packet is C_ItemSceneToBattleScene)
                        Managers.Scene.LoadScene(Define.Scene.Battle);
                    else if (Managers.Network.Packet is C_ReturnGame)
                        Managers.Scene.LoadScene(Define.Scene.Game);
                }
                break;
            case BagSceneState.ASKING_QUANTITY_TO_SELL:
                {
                    Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;

                    _countingBox.SetMaxValue(selectedItem.ItemCnt);

                    State = BagSceneState.SELECTING_QUANTITY_TO_SELL;
                }
                break;
            case BagSceneState.SELECTING_QUANTITY_TO_SELL:
                {
                    Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;
                    int count = _countingBox.GetCurrentCount();

                    int totalSellPrice = (selectedItem.ItemPrice / 2) * count;

                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = BagSceneState.SLIDER_READY;
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            List<string> scripts = new List<string>()
                            {
                                $"I can pay ${totalSellPrice}.\nWould that be okay?"
                            };
                            _scriptBox.BeginScriptTyping(scripts);

                            State = BagSceneState.ASKING_TO_SELL;
                        }
                    }
                    else
                    {
                        _countingText.text = $"×{count}";
                        _totalSellPrice.text = $"${totalSellPrice.ToString()}";
                    }
                }
                break;
            case BagSceneState.ASKING_TO_SELL:
                {
                    State = BagSceneState.SELECTING_TO_SELL;

                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    _scriptBox.CreateSelectBox(btns, btns.Count, 1, 400, 100);
                }
                break;
            case BagSceneState.SELECTING_TO_SELL:
                {
                    Item selectedItem = _scrollBox.GetScrollBoxContent() as Item;
                    int selectedItemIdx = _scrollBox.GetSelectedIdx();
                    int count = _countingBox.GetCurrentCount();

                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = BagSceneState.SLIDER_READY;
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                if (!_isLoading)
                                {
                                    _isLoading = true;

                                    C_SellItem sellItem = new C_SellItem();
                                    sellItem.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    sellItem.ItemCategory = selectedItem.ItemCategory;
                                    sellItem.ItemQuantity = count;
                                    sellItem.ItemIdx = selectedItemIdx;

                                    Managers.Network.Send(sellItem);
                                }
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                State = BagSceneState.SLIDER_READY;
                            }
                        }
                    }
                }
                break;
            case BagSceneState.SUCCESS_SELL_SCRIPTING:
                {
                    State = BagSceneState.SLIDER_READY;
                }
                break;
        }
    }


    public override void Clear()
    {
    }
}
