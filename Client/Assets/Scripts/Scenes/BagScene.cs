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
    MOVING_TO_BATTLE_SCENE = 3,
    MOVING_TO_GAME_SCENE = 4
}

public class BagScene : BaseScene
{
    int _selectedActionIdx;
    Item _selectedItem;
    ItemCategory _selectedCategory;
    BagSceneState _sceneState = BagSceneState.NONE;

    PlayerInfo _playerInfo;
    Dictionary<ItemCategory, List<Item>> _bag;
    List<DynamicButton> _scrollBoxContents;

    [SerializeField] SlideAndScrollBox _itemSlideAndScrollBox;
    [SerializeField] List<SliderContent> _sliderContents;
    [SerializeField] List<Image> _sliderIndicatorImgs;
    [SerializeField] List<Image> _scrollBoxArrow;
    [SerializeField] GridLayoutSelectBox _gridSelectBox;
    [SerializeField] List<DynamicButton> _actionBtns;
    [SerializeField] Image _itemImg;
    [SerializeField] TextMeshProUGUI _itemDescription;


    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Bag;

        _bag = new Dictionary<ItemCategory, List<Item>>();
        _scrollBoxContents = new List<DynamicButton>();
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
        switch (_sceneState)
        {
            case BagSceneState.NONE:
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
                    for (int i = 0; i < _sliderContents.Count; i++)
                    {
                        _sliderContents[i].ContentData = (ItemCategory)i;
                    }
                    _itemSlideAndScrollBox.UpdateSliderContents(_sliderContents);

                    // 아이템 카테고리 리스트 설정
                    foreach (Item item in _bag[(ItemCategory)0])
                    {
                        ArrowButton scrollBoxContent = Managers.Resource.Instantiate("UI/BagScene/ScrollContentZone", _itemSlideAndScrollBox.transform).GetComponent<ArrowButton>();

                        Util.FindChild<TextMeshProUGUI>(scrollBoxContent.gameObject, "ItemName", true).text = item.ItemName;

                        Util.FindChild<TextMeshProUGUI>(scrollBoxContent.gameObject, "ItemCount", true).text = "×" + item.ItemCnt;

                        scrollBoxContent.BtnData = item;

                        _scrollBoxContents.Add(scrollBoxContent);
                    }
                    _itemSlideAndScrollBox.UpdateScrollBoxContents(_scrollBoxContents);

                    // 스크롤 상,하 화살표 표시
                    if (_scrollBoxContents.Count > _itemSlideAndScrollBox.ScrollBoxMaxView)
                    {
                        _scrollBoxArrow[0].gameObject.SetActive(false);
                        _scrollBoxArrow[1].gameObject.SetActive(true);
                    }
                    else
                    {
                        _scrollBoxArrow[0].gameObject.SetActive(false);
                        _scrollBoxArrow[1].gameObject.SetActive(false);
                    }

                    // 액션 선택 박스 설정
                    for (int i = 0; i < _actionBtns.Count; i++)
                    {
                        _actionBtns[i].BtnData = Util.FindChild<TextMeshProUGUI>(_actionBtns[i].gameObject, "ContentText", true).text;
                    }
                    _gridSelectBox.SetSelectBoxContent(_actionBtns, 4, 1);
                }
                break;
        }
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);
        switch (_sceneState)
        {
            case BagSceneState.NONE:
                {
                    _sceneState = BagSceneState.WAITING_INPUT;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case BagSceneState.WAITING_INPUT:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.WatchMenu)
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                _sceneState = BagSceneState.MOVING_TO_GAME_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                            else if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                            {
                                _enterEffect.PlayEffect("FadeOut");

                                C_ReturnPokemonBattleScene returnBattleScenePacket = new C_ReturnPokemonBattleScene();
                                returnBattleScenePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(returnBattleScenePacket);

                                _sceneState = BagSceneState.MOVING_TO_BATTLE_SCENE;
                                ActiveUIBySceneState(_sceneState);
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            _sceneState = BagSceneState.WAITING_ACTION;
                            ActiveUIBySceneState(_sceneState);
                        }
                    }
                    else if (value as string == "ScrollBoxMove")
                    {
                        if (_scrollBoxContents.Count == 0)
                        {
                            _itemImg.sprite = null;

                            _itemDescription.text = "";
                            return;
                        }

                        int selectedItemIdx = _itemSlideAndScrollBox.CurScrollBoxIdx;
                        int scrollCnt = _itemSlideAndScrollBox.ScrollCnt;
                        int maxView = _itemSlideAndScrollBox.ScrollBoxMaxView;
                        Item selectedItem = (Item)_scrollBoxContents[selectedItemIdx].BtnData;

                        // 아이템 리스트 칸 상, 하 화살표 설정
                        if (scrollCnt == 0)
                        {
                            _scrollBoxArrow[0].gameObject.SetActive(false);
                            _scrollBoxArrow[1].gameObject.SetActive(true);
                        }
                        else if (scrollCnt == _scrollBoxContents.Count - maxView)
                        {
                            _scrollBoxArrow[0].gameObject.SetActive(true);
                            _scrollBoxArrow[1].gameObject.SetActive(false);
                        }
                        else
                        {
                            _scrollBoxArrow[0].gameObject.SetActive(true);
                            _scrollBoxArrow[1].gameObject.SetActive(true);
                        }

                        // 선택된 아이템 이미지, 설명 표시
                        Texture2D image = selectedItem.ItemImg;

                        _itemImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
                        _itemImg.SetNativeSize();

                        _itemDescription.text = selectedItem.ItemDescription;
                    }
                    else if (value as string == "SliderMove")
                    {
                        ItemCategory selectedCategory = (ItemCategory)_itemSlideAndScrollBox.CurIdx;

                        // 슬라이드 인디케이터 변경
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

                        // 이전에 보여지던 카테고리 내 아이템 리스트 삭제
                        foreach (ArrowButton obj in _scrollBoxContents)
                        {
                            Managers.Resource.Destroy(obj.gameObject);
                        }

                        _scrollBoxContents.Clear();

                        // 카테고리 내 아이템 리스트 설정
                        foreach (Item item in _bag[selectedCategory])
                        {
                            ArrowButton scrollBoxContent = Managers.Resource.Instantiate("UI/BagScene/ScrollContentZone", _itemSlideAndScrollBox.transform).GetComponent<ArrowButton>();

                            Util.FindChild<TextMeshProUGUI>(scrollBoxContent.gameObject, "ItemName", true).text = item.ItemName;

                            Util.FindChild<TextMeshProUGUI>(scrollBoxContent.gameObject, "ItemCount", true).text = "×" + item.ItemCnt;

                            scrollBoxContent.BtnData = item;

                            _scrollBoxContents.Add(scrollBoxContent);
                        }
                        _itemSlideAndScrollBox.UpdateScrollBoxContents(_scrollBoxContents);

                        // 스크롤 상,하 화살표 표시
                        if (_scrollBoxContents.Count > _itemSlideAndScrollBox.ScrollBoxMaxView)
                        {
                            _scrollBoxArrow[0].gameObject.SetActive(false);
                            _scrollBoxArrow[1].gameObject.SetActive(true);
                        }
                        else
                        {
                            _scrollBoxArrow[0].gameObject.SetActive(false);
                            _scrollBoxArrow[1].gameObject.SetActive(false);
                        }

                        ActiveUIBySceneState(_sceneState);
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
                            _sceneState = BagSceneState.WAITING_INPUT;
                            ActiveUIBySceneState(_sceneState);
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            Debug.Log(_actionBtns[_selectedActionIdx].BtnData as string);

                            if (_actionBtns[_selectedActionIdx].BtnData as string == "USE")
                            {
                                if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                                {
                                    C_UseItem c_useItem = new C_UseItem();
                                    c_useItem.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    c_useItem.ItemCategory = _selectedCategory;
                                    c_useItem.ItemOrder = _bag[_selectedCategory].FindIndex(item => item == _selectedItem);

                                    Managers.Network.SavePacket(c_useItem);

                                    _enterEffect.PlayEffect("FadeOut");

                                    _sceneState = BagSceneState.MOVING_TO_BATTLE_SCENE;
                                    ActiveUIBySceneState(_sceneState);
                                }
                            }
                            else if (_actionBtns[_selectedActionIdx].BtnData as string == "CANCEL")
                            {
                                _sceneState = BagSceneState.WAITING_INPUT;
                                ActiveUIBySceneState(_sceneState);
                            }
                        }
                    }
                    else
                    {
                        _selectedActionIdx = (int)value;
                    }
                }
                break;
            case BagSceneState.MOVING_TO_BATTLE_SCENE:
                {
                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.Battle);
                }
                break;
            case BagSceneState.MOVING_TO_GAME_SCENE:
                {
                    C_ReturnGame returnGamePacket = new C_ReturnGame();
                    returnGamePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                    Managers.Network.SavePacket(returnGamePacket);

                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.Game);
                }
                break;
        }
    }

    void ActiveUIBySceneState(BagSceneState state)
    {
        if (state == BagSceneState.WAITING_INPUT)
        {
            _itemSlideAndScrollBox.State = SliderState.WAITING_INPUT;
            _gridSelectBox.UIState = GridLayoutSelectBoxState.NONE;
            _gridSelectBox.gameObject.SetActive(false);
        }
        else if (state == BagSceneState.WAITING_ACTION)
        {
            _itemSlideAndScrollBox.State = SliderState.NONE;
            _gridSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
            _gridSelectBox.gameObject.SetActive(true);
        }
    }


    public override void Clear()
    {
    }
}
