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
    SLIDER_READY = 1,
    SCROLL_BOX_READY = 2,
    WAITING_ACTION = 3,
    MOVING_TO_SCENE = 4
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
    [SerializeField] Image _scrollUpArrow;
    [SerializeField] Image _scrollDownArrow;
    [SerializeField] GridLayoutSelectBox _gridSelectBox;
    [SerializeField] Image _itemImg;
    [SerializeField] TextMeshProUGUI _itemDescription;


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

            // 스크롤 상,하 화살표 표시
            if (_bag[0].Count > _scrollBox.ScrollBoxMaxView)
            {
                _scrollUpArrow.gameObject.SetActive(false);
                _scrollDownArrow.gameObject.SetActive(true);
            }
            else
            {
                _scrollUpArrow.gameObject.SetActive(false);
                _scrollDownArrow.gameObject.SetActive(false);
            }

            _sceneState = BagSceneState.NONE;
        }
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);
        switch (_sceneState)
        {
            case BagSceneState.NONE:
                {
                    _sceneState = BagSceneState.SLIDER_READY;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case BagSceneState.SLIDER_READY:
                {
                    _sceneState = BagSceneState.SCROLL_BOX_READY;
                    ActiveUIBySceneState(_sceneState);
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

                        // 스크롤 상, 하 화살표 표시
                        if (_bag[(ItemCategory)value].Count > _scrollBox.ScrollBoxMaxView)
                        {
                            _scrollUpArrow.gameObject.SetActive(false);
                            _scrollDownArrow.gameObject.SetActive(true);
                        }
                        else
                        {
                            _scrollUpArrow.gameObject.SetActive(false);
                            _scrollDownArrow.gameObject.SetActive(false);
                        }

                        _sceneState = BagSceneState.SCROLL_BOX_READY;
                        ActiveUIBySceneState(_sceneState);
                    }
                    else if (value is Item)
                    {
                        List<DynamicButton> btns = _scrollBox.ChangeBtnGridDataToList();
                        int scrollcnt = _scrollBox.ScrollCnt;
                        int maxview = _scrollBox.ScrollBoxMaxView;

                        // 아이템 리스트 칸 상, 하 화살표 설정
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

                                _sceneState = BagSceneState.MOVING_TO_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                C_ItemSceneToBattleScene itemToBattlePacket = new C_ItemSceneToBattleScene();
                                itemToBattlePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                itemToBattlePacket.ItemCategory = (ItemCategory)_categorySlider.GetSelectedContentData();
                                itemToBattlePacket.ItemOrder = -1;

                                Managers.Network.SavePacket(itemToBattlePacket);

                                _sceneState = BagSceneState.MOVING_TO_SCENE;
                                ActiveUIBySceneState(_sceneState);
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
                                    _sceneState = BagSceneState.WAITING_ACTION;
                                    ActiveUIBySceneState(_sceneState);
                                }
                                else
                                {
                                    // 액션 버튼 데이터 채우기
                                    List<string> btnNames = new List<string>()
                                    {
                                        "Cancel"
                                    };
                                    _gridSelectBox.CreateButtons(btnNames, 1, 400, 100);
                                    _sceneState = BagSceneState.WAITING_ACTION;
                                    ActiveUIBySceneState(_sceneState);
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
                            _sceneState = BagSceneState.SLIDER_READY;
                            ActiveUIBySceneState(_sceneState);
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

                                    _sceneState = BagSceneState.MOVING_TO_SCENE;
                                    ActiveUIBySceneState(_sceneState);
                                }
                            }
                            else if (selectedAction == "Cancel")
                            {
                                _sceneState = BagSceneState.SLIDER_READY;
                                ActiveUIBySceneState(_sceneState);
                            }
                        }
                    }
                }
                break;
            case BagSceneState.MOVING_TO_SCENE:
                {
                    if (Managers.Network.Packet is C_ItemSceneToBattleScene)
                        Managers.Scene.LoadScene(Define.Scene.Battle);
                    //else if (Managers.Network.Packet is C_ReturnGame)
                    //    Managers.Scene.LoadScene(Define.Scene.Game);
                }
                break;
        }
    }

    void ActiveUIBySceneState(BagSceneState state)
    {
        if (state == BagSceneState.SLIDER_READY)
        {
            _gridSelectBox.UIState = GridLayoutSelectBoxState.NONE;
            _gridSelectBox.gameObject.SetActive(false);
            _categorySlider.SliderState = SliderState.WAITING_INPUT;
        }
        if (state == BagSceneState.SCROLL_BOX_READY)
        {
            _scrollBox.State = SlideAndScrollBoxState.SELECTING;
        }
        else if (state == BagSceneState.WAITING_ACTION)
        {
            _categorySlider.SliderState = SliderState.NONE;
            _scrollBox.State = SlideAndScrollBoxState.NONE;
            _gridSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
            _gridSelectBox.gameObject.SetActive(true);
        }
    }


    public override void Clear()
    {
    }
}
