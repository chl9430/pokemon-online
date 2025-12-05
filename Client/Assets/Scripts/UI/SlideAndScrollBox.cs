using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum SlideAndScrollBoxState
{
    NONE = 0,
    SELECTING = 1,
}

public class SlideAndScrollBox : MonoBehaviour
{
    int _curPosInScrollBox = 0;
    int _scrollCnt = 0;
    float _heightPerContent;
    int _curIdx;
    protected List<DynamicButton> _btnGrid = new List<DynamicButton>();
    SlideAndScrollBoxState _state;

    [SerializeField] int _scrollBoxMaxView;
    [SerializeField] DynamicButton _btn;
    [SerializeField] GameObject _scrollUpArrow;
    [SerializeField] GameObject _scrollDownArrow;
    [SerializeField] Transform _btnPos;

    public SlideAndScrollBoxState State
    {
        get
        {
            return _state;
        }

        set
        {
            _state = value;

            if (_state == SlideAndScrollBoxState.SELECTING)
            {
                if (_btnGrid.Count == 0)
                    Managers.Scene.CurrentScene.DoNextAction(null);
                else
                    Managers.Scene.CurrentScene.DoNextAction(_btnGrid[_curIdx].BtnData);
            }
        }
    }

    public int ScrollCnt { get { return _scrollCnt; } }
    public int ScrollBoxMaxView { get { return  _scrollBoxMaxView; } }

    void Start()
    {
    }

    void Update()
    {
        switch (_state)
        {
            case SlideAndScrollBoxState.SELECTING:
                {
                    ChooseAction();
                }
                break;
        }
    }

    void ChooseAction()
    {
        if (_btnGrid.Count <= 1)
            return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_curIdx == _btnGrid.Count - 1 || _btnGrid[_curIdx + 1] == null)
                return;

            _btnGrid[_curIdx].SetSelectedOrNotSelected(false);

            _curIdx++;

            _btnGrid[_curIdx].SetSelectedOrNotSelected(true);

            if (_curPosInScrollBox == _scrollBoxMaxView - 1)
            {
                _scrollCnt++;

                foreach (DynamicButton btn in _btnGrid)
                {
                    RectTransform rt = _btnGrid[_curIdx].GetComponent<RectTransform>();

                    rt.anchorMin = new Vector2(0, rt.anchorMin.y + _heightPerContent);
                    rt.anchorMax = new Vector2(1, rt.anchorMax.y + _heightPerContent);
                }
            }
            else
                _curPosInScrollBox++;

            // 아이템 리스트 칸 상, 하 화살표 갱신
            if (_btnGrid.Count > _scrollBoxMaxView)
            {
                if (_scrollCnt == 0)
                {
                    _scrollUpArrow.gameObject.SetActive(false);
                    _scrollDownArrow.gameObject.SetActive(true);
                }
                else if (_scrollCnt == _btnGrid.Count - _scrollBoxMaxView)
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

            Managers.Scene.CurrentScene.DoNextAction(_btnGrid[_curIdx].BtnData);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_curIdx == 0 || _btnGrid[_curIdx - 1] == null)
                return;

            _btnGrid[_curIdx].SetSelectedOrNotSelected(false);

            _curIdx--;

            _btnGrid[_curIdx].SetSelectedOrNotSelected(true);

            if (_curPosInScrollBox == 0)
            {
                _scrollCnt--;

                foreach (DynamicButton btn in _btnGrid)
                {
                    RectTransform rt = _btnGrid[_curIdx].GetComponent<RectTransform>();

                    rt.anchorMin = new Vector2(0, rt.anchorMin.y + _heightPerContent);
                    rt.anchorMax = new Vector2(1, rt.anchorMax.y + _heightPerContent);
                }
            }
            else
                _curPosInScrollBox--;

            // 아이템 리스트 칸 상, 하 화살표 갱신
            if (_btnGrid.Count > _scrollBoxMaxView)
            {
                if (_scrollCnt == 0)
                {
                    _scrollUpArrow.gameObject.SetActive(false);
                    _scrollDownArrow.gameObject.SetActive(true);
                }
                else if (_scrollCnt == _btnGrid.Count - _scrollBoxMaxView)
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

            Managers.Scene.CurrentScene.DoNextAction(_btnGrid[_curIdx].BtnData);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Managers.Scene.CurrentScene.DoNextAction(Define.InputSelectBoxEvent.SELECT);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Managers.Scene.CurrentScene.DoNextAction(Define.InputSelectBoxEvent.BACK);
        }
    }

    public void UpdateScrollBoxContents()
    {
        float curHeight = 1f;
        _heightPerContent = 1f / ((float)_scrollBoxMaxView);

        _curPosInScrollBox = 0;
        _scrollCnt = 0;

        foreach (DynamicButton btn in _btnGrid)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();

            float height = curHeight - _heightPerContent;

            rt.anchorMax = new Vector2(1, curHeight);
            rt.anchorMin = new Vector2(0, height);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            curHeight -= _heightPerContent;
        }
    }

    public void ClearBtnGrid()
    {
        // 기존에 있던 버튼들 삭제
        if (_btnGrid.Count > 0)
        {
            foreach (DynamicButton btn in _btnGrid)
            {
                Destroy(btn.gameObject);
            }

            _btnGrid.Clear();
        }
    }

    public DynamicButton AddNewBtn(object data)
    {
        DynamicButton btn = GameObject.Instantiate(_btn, _btnPos);
        btn.BtnData = data;
        btn.SetSelectedOrNotSelected(false);

        _btnGrid.Add(btn);

        if (_btnGrid.Count == 1)
        {
            btn.SetSelectedOrNotSelected(true);
        }

        // 아이템 리스트 칸 상, 하 화살표 갱신
        if (_btnGrid.Count > _scrollBoxMaxView)
        {
            if (_scrollCnt == 0)
            {
                _scrollUpArrow.gameObject.SetActive(false);
                _scrollDownArrow.gameObject.SetActive(true);
            }
            else if (_scrollCnt == _btnGrid.Count - _scrollBoxMaxView)
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

        return btn;
    }

    public void CreateScrollAreaButtons(List<object> datas, int row, int col)
    {
        ClearBtnGrid();

        _curIdx = 0;

        foreach (object data in datas)
        {
            DynamicButton btn = GameObject.Instantiate(_btn, _btnPos);
            btn.BtnData =data;
            btn.SetSelectedOrNotSelected(false);

            _btnGrid.Add(btn);
        }

        _btnGrid[_curIdx].SetSelectedOrNotSelected(true);

        UpdateScrollBoxContents();

        ShowUpAndDownArrows();
    }

    public void ShowUpAndDownArrows()
    {
        // 스크롤 상,하 화살표 표시
        if (_btnGrid.Count > _scrollBoxMaxView)
        {
            _scrollUpArrow.gameObject.SetActive(false);
            _scrollDownArrow.gameObject.SetActive(true);
        }
        else
        {
            _scrollUpArrow.gameObject.SetActive(false);
            _scrollDownArrow.gameObject.SetActive(false);
        }
    }

    public List<DynamicButton> ChangeBtnGridDataToList()
    {
        return _btnGrid;
    }

    public DynamicButton GetDynamicButton(int idx)
    {
        return _btnGrid[idx];
    }

    public object GetScrollBoxContent()
    {
        if (_btnGrid.Count == 0)
            return null;

        return _btnGrid[_curIdx].BtnData;
    }

    public int GetSelectedIdx()
    {
        return _curIdx;
    }

    public DynamicButton GetSelectedBtn()
    {
        return _btnGrid[_curIdx];
    }

    public void DeleteBtn(int idx)
    {
        if (_curIdx == _btnGrid.Count - 1)
            _curIdx--;

        Destroy(_btnGrid[idx].gameObject);
        _btnGrid.RemoveAt(idx);

        if (_btnGrid.Count == 0)
            return;

        _btnGrid[_curIdx].SetSelectedOrNotSelected(true);

        float curHeight = 1f;
        _heightPerContent = 1f / ((float)_scrollBoxMaxView);

        foreach (DynamicButton btn in _btnGrid)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();

            float height = curHeight - _heightPerContent;

            rt.anchorMax = new Vector2(1, curHeight);
            rt.anchorMin = new Vector2(0, height);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            curHeight -= _heightPerContent;
        }
    }
}
