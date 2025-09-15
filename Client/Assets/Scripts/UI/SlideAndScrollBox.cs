using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
struct SlideAndScrollBoxData
{
    public int slideIdx;
    public int scrollBoxIdx;
    public int scrollCnt;
    public int maxView;
}

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
    BaseScene _scene;
    int _row;
    int _col;
    int _x;
    int _y;
    protected DynamicButton[,] _btnGrid;
    SlideAndScrollBoxState _state;

    [SerializeField] int _scrollBoxMaxView;
    [SerializeField] DynamicButton _btn;
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
                if (_scene == null)
                    _scene = Managers.Scene.CurrentScene;

                if (_btnGrid != null)
                    _scene.DoNextAction(_btnGrid[_x, _y].BtnData);
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
        if (_btnGrid == null)
            return;

        if (_btnGrid.GetLength(0) == 1 && _btnGrid.GetLength(1) == 1)
            return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_x == _row - 1 || _btnGrid[_x + 1, _y] == null)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _x++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            if (_curPosInScrollBox == _scrollBoxMaxView - 1)
            {
                _scrollCnt++;

                for (int i = 0; i < _btnGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < _btnGrid.GetLength(1); j++)
                    {
                        RectTransform rt = _btnGrid[i, j].GetComponent<RectTransform>();

                        rt.anchorMin = new Vector2(0, rt.anchorMin.y + _heightPerContent);
                        rt.anchorMax = new Vector2(1, rt.anchorMax.y + _heightPerContent);
                    }
                }
            }
            else
                _curPosInScrollBox++;

            _scene.DoNextAction(_btnGrid[_x, _y].BtnData);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_x == 0 || _btnGrid[_x - 1, _y] == null)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _x--;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            if (_curPosInScrollBox == 0)
            {
                _scrollCnt--;

                for (int i = 0; i < _btnGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < _btnGrid.GetLength(1); j++)
                    {
                        RectTransform rt = _btnGrid[i, j].GetComponent<RectTransform>();

                        rt.anchorMin = new Vector2(0, rt.anchorMin.y - _heightPerContent);
                        rt.anchorMax = new Vector2(1, rt.anchorMax.y - _heightPerContent);
                    }
                }
            }
            else
                _curPosInScrollBox--;

            _scene.DoNextAction(_btnGrid[_x, _y].BtnData);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _scene.DoNextAction(Define.InputSelectBoxEvent.SELECT);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            _scene.DoNextAction(Define.InputSelectBoxEvent.BACK);
        }
    }

    public void UpdateScrollBoxContents()
    {
        float curHeight = 1f;
        _heightPerContent = 1f / ((float)_scrollBoxMaxView);

        _curPosInScrollBox = 0;
        _scrollCnt = 0;

        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                RectTransform rt = _btnGrid[i, j].GetComponent<RectTransform>();

                float height = curHeight - _heightPerContent;

                rt.anchorMax = new Vector2(1, curHeight);
                rt.anchorMin = new Vector2(0, height);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
            }

            curHeight -= _heightPerContent;
        }
    }

    public void ClearBtnGrid()
    {
        // 기존에 있던 버튼들 삭제
        if (_btnGrid != null)
        {
            for (int i = 0; i < _btnGrid.GetLength(0); i++)
            {
                for (int j = 0; j < _btnGrid.GetLength(1); j++)
                {
                    if (_btnGrid[i, j] != null)
                        Destroy(_btnGrid[i, j].gameObject);
                }
            }
        }

        _btnGrid = null;
    }

    public void CreateScrollAreaButtons(List<object> datas, int row, int col)
    {
        _row = row;
        _col = col;
        _x = 0;
        _y = 0;

        if (_scene == null)
            _scene = Managers.Scene.CurrentScene;

        if (_btnGrid == null)
            _btnGrid = new DynamicButton[_row, _col];

        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                if (_row > _col)
                {
                    if (i * _col + j < datas.Count)
                    {
                        DynamicButton btn = GameObject.Instantiate(_btn, _btnPos);

                        _btnGrid[i, j] = btn;
                        _btnGrid[i, j].BtnData = datas[i * _col + j];
                        _btnGrid[i, j].SetSelectedOrNotSelected(false);
                    }
                }
                else
                {
                    if (i * _row + j < datas.Count)
                    {
                        DynamicButton btn = GameObject.Instantiate(_btn, _btnPos);

                        _btnGrid[i, j] = btn;
                        _btnGrid[i, j].BtnData = datas[i * _row + j];
                        _btnGrid[i, j].SetSelectedOrNotSelected(false);
                    }
                }
            }
        }

        _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

        UpdateScrollBoxContents();
    }

    public List<DynamicButton> ChangeBtnGridDataToList()
    {
        List<DynamicButton> btns = new List<DynamicButton>();

        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                if (_btnGrid[i, j] != null)
                    btns.Add(_btnGrid[i, j]);
            }
        }

        return btns;
    }

    public object GetScrollBoxContent()
    {
        return _btnGrid[_x, _y].BtnData;
    }
}
