using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum FriendlyShopContentsState
{
    NONE = 0,
    GREETING_SCRIPTING = 1,
    SELECTING_ACTION = 2,
    GOOD_BYE_SCRIPTING = 3,
    SELECTING_ITEM_TO_BUY = 4,
    ASKING_QUANTITY = 5,
    SELECTING_QUANTITY = 6,
    ASKING_TO_BUY = 7,
    SELECTING_TO_BUY = 8,
    SUCCESS_BUY_SCRIPTING = 9,
    FAILED_BUY_SCRIPTING = 10,
    MOVING_SCENE = 11,
}

public class FriendlyShopContents : ObjectContents
{
    [SerializeField] ScriptBoxUI _scriptBox;
    [SerializeField] SlideAndScrollBox _shopListBox;
    [SerializeField] Image _itemImg;
    [SerializeField] TextMeshProUGUI _itemDescription;
    [SerializeField] Image _scrollUpArrow;
    [SerializeField] Image _scrollDownArrow;
    [SerializeField] GameObject _shopUIZone;
    [SerializeField] GameObject _inBagCountBox;
    [SerializeField] CountingBox _countingBox;
    [SerializeField] TextMeshProUGUI _countingText;
    [SerializeField] TextMeshProUGUI _totalPrice;
    [SerializeField] GameObject _moneyTextBox;
    [SerializeField] TextMeshProUGUI _moneyText;

    FriendlyShopContentsState _state = FriendlyShopContentsState.NONE;

    List<Item> _shopItems;

    FriendlyShopContentsState State
    {
        set
        {
            _state = value;

            if (_scene == null)
            {
                _scene = Managers.Scene.CurrentScene;
            }

            if (_state == FriendlyShopContentsState.GREETING_SCRIPTING)
            {
                _scriptBox.gameObject.SetActive(true);

                _shopListBox.State = SlideAndScrollBoxState.NONE;
                _shopUIZone.gameObject.SetActive(false);

                _inBagCountBox.gameObject.SetActive(false);

                _countingBox.State = CountingBoxState.NONE;
                _countingBox.gameObject.SetActive(false);
            }
            else if (_state == FriendlyShopContentsState.SELECTING_ACTION)
            {
                _scriptBox.gameObject.SetActive(true);
            }
            else if (_state == FriendlyShopContentsState.GOOD_BYE_SCRIPTING)
            {
                _scriptBox.gameObject.SetActive(true);
                _scriptBox.HideSelectBox();

                _shopListBox.State = SlideAndScrollBoxState.NONE;
                _shopUIZone.gameObject.SetActive(false);

                _inBagCountBox.gameObject.SetActive(false);

                _countingBox.State = CountingBoxState.NONE;
                _countingBox.gameObject.SetActive(false);
            }
            else if (_state == FriendlyShopContentsState.SELECTING_ITEM_TO_BUY)
            {
                _scriptBox.gameObject.SetActive(false);
                _scriptBox.HideSelectBox();

                _shopListBox.State = SlideAndScrollBoxState.SELECTING;
                _shopUIZone.gameObject.SetActive(true);

                _inBagCountBox.gameObject.SetActive(false);

                _countingBox.State = CountingBoxState.NONE;
                _countingBox.gameObject.SetActive(false);

                _moneyTextBox.gameObject.SetActive(false);
            }
            else if (_state == FriendlyShopContentsState.ASKING_QUANTITY)
            {
                _scriptBox.gameObject.SetActive(true);

                _shopListBox.State = SlideAndScrollBoxState.NONE;
                _shopUIZone.gameObject.SetActive(true);

                _inBagCountBox.gameObject.SetActive(false);

                _countingBox.State = CountingBoxState.NONE;
                _countingBox.gameObject.SetActive(false);
            }
            else if (_state == FriendlyShopContentsState.SELECTING_QUANTITY)
            {
                _scriptBox.gameObject.SetActive(true);
                _scriptBox.HideSelectBox();

                _shopListBox.State = SlideAndScrollBoxState.NONE;
                _shopUIZone.gameObject.SetActive(true);

                _inBagCountBox.gameObject.SetActive(true);

                _countingBox.State = CountingBoxState.SELECTING;
                _countingBox.gameObject.SetActive(true);

                _moneyTextBox.gameObject.SetActive(true);
            }
            else if (_state == FriendlyShopContentsState.ASKING_TO_BUY)
            {
                _scriptBox.gameObject.SetActive(true);

                _shopListBox.State = SlideAndScrollBoxState.NONE;
                _shopUIZone.gameObject.SetActive(true);

                _inBagCountBox.gameObject.SetActive(false);

                _countingBox.State = CountingBoxState.NONE;
                _countingBox.gameObject.SetActive(false);
            }
            else if (_state == FriendlyShopContentsState.SELECTING_TO_BUY)
            {
                _scriptBox.gameObject.SetActive(true);

                _shopListBox.State = SlideAndScrollBoxState.NONE;
                _shopUIZone.gameObject.SetActive(true);

                _inBagCountBox.gameObject.SetActive(false);

                _countingBox.State = CountingBoxState.NONE;
                _countingBox.gameObject.SetActive(false);
            }
            else if (_state == FriendlyShopContentsState.SUCCESS_BUY_SCRIPTING)
            {
                _scriptBox.gameObject.SetActive(true);
                _scriptBox.HideSelectBox();

                _shopListBox.State = SlideAndScrollBoxState.NONE;
                _shopUIZone.gameObject.SetActive(true);

                _inBagCountBox.gameObject.SetActive(false);

                _countingBox.State = CountingBoxState.NONE;
                _countingBox.gameObject.SetActive(false);
            }
            else if (_state == FriendlyShopContentsState.FAILED_BUY_SCRIPTING)
            {
                _scriptBox.gameObject.SetActive(true);
                _scriptBox.HideSelectBox();

                _shopListBox.State = SlideAndScrollBoxState.NONE;
                _shopUIZone.gameObject.SetActive(true);

                _inBagCountBox.gameObject.SetActive(false);

                _countingBox.State = CountingBoxState.NONE;
                _countingBox.gameObject.SetActive(false);
            }
            else if (_state == FriendlyShopContentsState.MOVING_SCENE)
            {
                _scriptBox.gameObject.SetActive(true);
                _scriptBox.ScriptSelectBox.UIState = GridLayoutSelectBoxState.NONE;

                _shopListBox.State = SlideAndScrollBoxState.NONE;
                _shopUIZone.gameObject.SetActive(false);

                _inBagCountBox.gameObject.SetActive(false);

                _countingBox.State = CountingBoxState.NONE;
                _countingBox.gameObject.SetActive(false);
            }
            else if (_state == FriendlyShopContentsState.NONE)
            {
                _scriptBox.gameObject.SetActive(false);
                _scriptBox.HideSelectBox();

                _shopListBox.State = SlideAndScrollBoxState.NONE;
                _shopUIZone.gameObject.SetActive(false);

                _inBagCountBox.gameObject.SetActive(false);

                _countingBox.State = CountingBoxState.NONE;
                _countingBox.gameObject.SetActive(false);
            }
        }
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;

        if (_packet is S_GetNpcTalk)
        {
            if (_scene == null)
            {
                _scene = Managers.Scene.CurrentScene;
            }

            _scene.MyPlayer.State = CreatureState.Shopping;
            State = FriendlyShopContentsState.GREETING_SCRIPTING;

            List<string> scripts = new List<string>()
            {
                "Welcome!",
                "How may I serve you?",
            };
            _scriptBox.BeginScriptTyping(scripts);
        }
        else if (_packet is S_ShopItemList)
        {
            IList<ItemSummary> shopItemSums = ((S_ShopItemList)packet).ShopItemSums;

            if (_shopItems == null)
                _shopItems = new List<Item>();

            foreach (ItemSummary itemSum in shopItemSums)
                _shopItems.Add(new Item(itemSum));

            List<object> datas = new List<object>();
            foreach (Item item in _shopItems)
                datas.Add(item);
            _shopListBox.CreateScrollAreaButtons(datas, datas.Count, 1);


            // 아이템 랜더링
            List<DynamicButton> btns = _shopListBox.ChangeBtnGridDataToList();
            foreach (DynamicButton btn in btns)
            {
                TextMeshProUGUI tmp = Util.FindChild(btn.gameObject, "ItemName", true).GetComponent<TextMeshProUGUI>();
                tmp.text = ((Item)btn.BtnData).ItemName;

                tmp = Util.FindChild(btn.gameObject, "ItemPrice", true).GetComponent<TextMeshProUGUI>();
                tmp.text = $"${((Item)btn.BtnData).ItemPrice}";
            }

            // 스크롤 상,하 화살표 표시
            if (_shopItems.Count > _shopListBox.ScrollBoxMaxView)
            {
                _scrollUpArrow.gameObject.SetActive(false);
                _scrollDownArrow.gameObject.SetActive(true);
            }
            else
            {
                _scrollUpArrow.gameObject.SetActive(false);
                _scrollDownArrow.gameObject.SetActive(false);
            }

            State = FriendlyShopContentsState.SELECTING_ITEM_TO_BUY;
        }
        else if (_packet is S_GetItemCount)
        {
            Item selectedItem = _shopListBox.GetScrollBoxContent() as Item;

            List<string> scripts = new List<string>()
            {
                $"{selectedItem.ItemName}? Certainly. How many would you like?"
            };
            _scriptBox.BeginScriptTyping(scripts, true);

            State = FriendlyShopContentsState.ASKING_QUANTITY;
        }
        else if (_packet is S_BuyItem)
        {
            bool isBuy = (_packet as S_BuyItem).IsBuy;
            int money = (_packet as S_BuyItem).Money;

            if (isBuy)
            {
                _moneyText.text = "$ " + money.ToString();

                State = FriendlyShopContentsState.SUCCESS_BUY_SCRIPTING;

                List<string> scripts = new List<string>()
                {
                    $"Here you go!\nThank you very much."
                };
                _scriptBox.BeginScriptTyping(scripts);
            }
            else
            {
                State = FriendlyShopContentsState.FAILED_BUY_SCRIPTING;

                List<string> scripts = new List<string>()
                {
                    $"You don't have enough money..."
                };
                _scriptBox.BeginScriptTyping(scripts);
            }
        }
        else if (_packet is S_EnterRoom)
        {
            if (_scene == null)
            {
                _scene = Managers.Scene.CurrentScene;
            }

            _scene.MyPlayer.State = CreatureState.Shopping;

            State = FriendlyShopContentsState.SELECTING_ACTION;

            List<string> btns = new List<string>()
            {
                "Buy",
                "Sell",
                "Quit"
            };
            _scriptBox.CreateSelectBox(btns, btns.Count, 1, 400, 100);
            _scriptBox.SetScriptWihtoutTyping("How may I serve you?");
        }
    }

    public override void SetNextAction(object value)
    {
        switch (_state)
        {
            case FriendlyShopContentsState.GREETING_SCRIPTING:
                {
                    State = FriendlyShopContentsState.SELECTING_ACTION;

                    List<string> btns = new List<string>()
                    {
                        "Buy",
                        "Sell",
                        "Quit"
                    };
                    _scriptBox.CreateSelectBox(btns, btns.Count, 1, 400, 100);
                }
                break;
            case FriendlyShopContentsState.SELECTING_ACTION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Buy")
                            {
                                if (!_isLoading)
                                {
                                    _isLoading = true;

                                    C_ShopItemList itemListPacket = new C_ShopItemList();
                                    itemListPacket.PlayerId = _scene.MyPlayer.Id;

                                    Managers.Network.Send(itemListPacket);
                                }
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "Sell")
                            {
                                if (!_isLoading)
                                {
                                    C_EnterPlayerBagScene enterBagScene = new C_EnterPlayerBagScene();
                                    enterBagScene.PlayerId = _scene.MyPlayer.Id;

                                    Managers.Network.SavePacket(enterBagScene);

                                    _scene.ScreenEffecter.PlayEffect("FadeOut");

                                    State = FriendlyShopContentsState.MOVING_SCENE;
                                }
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "Quit")
                            {
                                State = FriendlyShopContentsState.GOOD_BYE_SCRIPTING;

                                List<string> scripts = new List<string>()
                                {
                                    "Please come again!"
                                };
                                _scriptBox.BeginScriptTyping(scripts);
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = FriendlyShopContentsState.GOOD_BYE_SCRIPTING;

                            List<string> scripts = new List<string>()
                            {
                                "Please come again!"
                            };
                            _scriptBox.BeginScriptTyping(scripts);
                        }
                    }
                }
                break;
            case FriendlyShopContentsState.GOOD_BYE_SCRIPTING:
                {
                    if (!_isLoading)
                    {
                        _isLoading = true;

                        C_FinishNpcTalk finishTalk = new C_FinishNpcTalk();
                        finishTalk.PlayerId = _scene.MyPlayer.Id;

                        Managers.Network.Send(finishTalk);
                    }

                    State = FriendlyShopContentsState.NONE;

                    ((GameScene)_scene).FinishContents();

                    _scene.MyPlayer.State = CreatureState.Idle;
                }
                break;
            case FriendlyShopContentsState.SELECTING_ITEM_TO_BUY:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = FriendlyShopContentsState.GOOD_BYE_SCRIPTING;

                            List<string> scripts = new List<string>()
                            {
                                "Please come again!"
                            };
                            _scriptBox.BeginScriptTyping(scripts);
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            Item selectedItem = _shopListBox.GetScrollBoxContent() as Item;

                            if (!_isLoading)
                            {
                                _isLoading = true;

                                C_GetItemCount getItemCount = new C_GetItemCount();
                                getItemCount.PlayerId = _scene.MyPlayer.Id;
                                getItemCount.ItemCategory = selectedItem.ItemCategory;
                                getItemCount.ItemName = selectedItem.ItemName;

                                Managers.Network.Send(getItemCount);
                            }

                            List<string> scripts = new List<string>()
                            {
                                $"{selectedItem.ItemName}? Certainly. How many would you like?"
                            };
                            _scriptBox.BeginScriptTyping(scripts, true);

                            State = FriendlyShopContentsState.ASKING_QUANTITY;
                        }
                    }
                    else
                    {
                        List<DynamicButton> btns = _shopListBox.ChangeBtnGridDataToList();
                        int scrollcnt = _shopListBox.ScrollCnt;
                        int maxview = _shopListBox.ScrollBoxMaxView;


                        // 아이템 리스트 칸 상, 하 화살표 설정
                        if (_shopItems.Count > _shopListBox.ScrollBoxMaxView)
                        {
                            if (scrollcnt == 0)
                            {
                                _scrollUpArrow.gameObject.SetActive(false);
                                _scrollDownArrow.gameObject.SetActive(true);
                            }
                            else if (scrollcnt == btns.Count - maxview)
                            {
                                _scrollUpArrow.gameObject.SetActive(true);
                                _scrollDownArrow.gameObject.SetActive(false);
                            }
                            else
                            {
                                _scrollUpArrow.gameObject.SetActive(true);
                                _scrollDownArrow.gameObject.SetActive(true);
                            }
                        }

                        // 선택된 아이템 이미지, 설명 표시
                        Texture2D image = ((Item)_shopListBox.GetScrollBoxContent()).ItemImg;

                        _itemImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
                        _itemImg.SetNativeSize();

                        _itemDescription.text = ((Item)_shopListBox.GetScrollBoxContent()).ItemDescription;
                    }
                }
                break;
            case FriendlyShopContentsState.ASKING_QUANTITY:
                {
                    if (_packet is S_GetItemCount)
                    {
                        int itemCount = (_packet as S_GetItemCount).ItemCount;
                        int money = (_packet as S_GetItemCount).Money;

                        _moneyText.text = "$ " + money.ToString();

                        TextMeshProUGUI inBagCountText = Util.FindChild<TextMeshProUGUI>(_inBagCountBox);
                        inBagCountText.text = $"In Bag : {itemCount}";

                        _countingBox.SetMaxValue(999);

                        State = FriendlyShopContentsState.SELECTING_QUANTITY;
                    }
                }
                break;
            case FriendlyShopContentsState.SELECTING_QUANTITY:
                {
                    Item selectedItem = _shopListBox.GetScrollBoxContent() as Item;
                    int quantity = _countingBox.GetCurrentCount();

                    int totalPrice = selectedItem.ItemPrice * quantity;

                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = FriendlyShopContentsState.SELECTING_ITEM_TO_BUY;
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            State = FriendlyShopContentsState.ASKING_TO_BUY;

                            List<string> scripts = new List<string>()
                            {
                                $"{selectedItem.ItemName}? And you wanted {quantity}?\nThat will be {selectedItem.ItemPrice * quantity}$."
                            };
                            _scriptBox.BeginScriptTyping(scripts, true);
                        }
                    }
                    else
                    {
                        _countingText.text = $"×{quantity}";
                        _totalPrice.text = $"${totalPrice.ToString()}";
                    }
                }
                break;
            case FriendlyShopContentsState.ASKING_TO_BUY:
                {
                    State = FriendlyShopContentsState.SELECTING_TO_BUY;

                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    _scriptBox.CreateSelectBox(btns, btns.Count, 1, 400, 100);
                }
                break;
            case FriendlyShopContentsState.SELECTING_TO_BUY:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = FriendlyShopContentsState.SELECTING_QUANTITY;
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                if (!_isLoading)
                                {
                                    C_BuyItem buyItemPacket = new C_BuyItem();
                                    buyItemPacket.PlayerId = _scene.MyPlayer.Id;
                                    buyItemPacket.ItemIdx = _shopListBox.GetSelectedIdx();
                                    buyItemPacket.ItemQuantity = _countingBox.GetCurrentCount();

                                    Managers.Network.Send(buyItemPacket);
                                }
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                string script = $"How many would you like?";
                                _scriptBox.SetScriptWihtoutTyping(script);

                                State = FriendlyShopContentsState.SELECTING_QUANTITY;
                            }
                        }
                    }
                }
                break;
            case FriendlyShopContentsState.SUCCESS_BUY_SCRIPTING:
                {
                    State = FriendlyShopContentsState.SELECTING_ITEM_TO_BUY;
                }
                break;
            case FriendlyShopContentsState.FAILED_BUY_SCRIPTING:
                {
                    string script = $"How many would you like?";
                    _scriptBox.SetScriptWihtoutTyping(script);

                    State = FriendlyShopContentsState.SELECTING_QUANTITY;
                }
                break;
            case FriendlyShopContentsState.MOVING_SCENE:
                {
                    if (Managers.Network.Packet is C_EnterPlayerBagScene)
                        Managers.Scene.LoadScene(Define.Scene.Bag);
                }
                break;
        }
    }
}
