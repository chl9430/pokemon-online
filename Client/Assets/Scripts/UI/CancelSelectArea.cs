using UnityEngine;
using System.Collections.Generic;
using System.Data;

public enum CancelSelectAreaState
{
    NONE = 0,
    SELECTING = 1,
}

public class CancelSelectArea : MonoBehaviour
{
    int _x;
    int _y;
    int _row;
    int _col;
    DynamicButton[,] _btnGrid;
    CancelSelectAreaState _state = CancelSelectAreaState.NONE;

    [SerializeField] DynamicButton _btn;
    [SerializeField] DynamicButton _cancelBtn;

    public int X { get { return _x; } }
    public int Y { get { return _y; } }
    public CancelSelectAreaState State 
    { 
        set  
        { 
            _state = value;

            if (_state == CancelSelectAreaState.SELECTING)
                Managers.Scene.CurrentScene.DoNextAction(_x * _col + _y);
        } 
    }

    public List<DynamicButton> ChangeBtnGridDataToList()
    {
        List<DynamicButton> btns = new List<DynamicButton>();

        for (int i = 0; i < _btnGrid.GetLength(0) - 1; i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                if (_btnGrid[i, j] != null)
                    btns.Add(_btnGrid[i, j]);
            }
        }

        return btns;
    }

    void Update()
    {
        switch (_state)
        {
            case CancelSelectAreaState.SELECTING:
                ChooseAction();
                break;
        }
    }

    void ChooseAction()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_y == 0 || _btnGrid[_x, _y - 1] == null)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _y--;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            Managers.Scene.CurrentScene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_y == _col - 1 || _btnGrid[_x, _y + 1] == null)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _y++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            Managers.Scene.CurrentScene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_x == _row - 1 || _btnGrid[_x + 1, _y] == null)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _x++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            Managers.Scene.CurrentScene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_x == 0 || _btnGrid[_x - 1, _y] == null)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _x--;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            Managers.Scene.CurrentScene.DoNextAction(_x * _col + _y);
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

    public void CreateButton(int col, int btnCount)
    {
        _row = (btnCount - 1) / col + 2;
        _col = col;

        DynamicButton[,] btnGrid = new DynamicButton[_row, _col];
        _btnGrid = btnGrid;

        _x = 0;
        _y = 0;

        int lastY = 0;

        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                if (_row > _col)
                {
                    if (i * _col + j < btnCount)
                    {
                        DynamicButton btn = GameObject.Instantiate(_btn, gameObject.transform);
                        _btnGrid[i, j] = btn;

                        lastY = j;
                    }
                }
                else
                {
                    if (i * _row + j < btnCount)
                    {
                        DynamicButton btn = GameObject.Instantiate(_btn, gameObject.transform);
                        _btnGrid[i, j] = btn;

                        lastY = j;
                    }
                }
            }
        }

        _btnGrid[_btnGrid.GetLength(0) - 1, lastY] = _cancelBtn;
        _btnGrid[_btnGrid.GetLength(0) - 1, lastY].SetButtonName("Cancel");

        _btnGrid[_x, _y].SetSelectedOrNotSelected(true);
    }

    public void CreateButton(int col, List<object> btnDatas)
    {
        _row = (btnDatas.Count - 1) / col + 2;
        _col = col;

        DynamicButton[,] btnGrid = new DynamicButton[_row, _col];
        _btnGrid = btnGrid;

        _x = 0;
        _y = 0;

        int lastY = 0;

        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                if (_row > _col)
                {
                    if (i * _col + j < btnDatas.Count)
                    {
                        DynamicButton btn = GameObject.Instantiate(_btn, gameObject.transform);
                        _btnGrid[i, j] = btn;

                        btn.BtnData = btnDatas[i * _col + j];

                        lastY = j;
                    }
                }
                else
                {
                    if (i * _row + j < btnDatas.Count)
                    {
                        DynamicButton btn = GameObject.Instantiate(_btn, gameObject.transform);
                        _btnGrid[i, j] = btn;

                        btn.BtnData = btnDatas[i * _row + j];

                        lastY = j;
                    }
                }
            }
        }

        _btnGrid[_btnGrid.GetLength(0) - 1, lastY] = _cancelBtn;
        _btnGrid[_btnGrid.GetLength(0) - 1, lastY].SetButtonName("Cancel");

        _btnGrid[_x, _y].SetSelectedOrNotSelected(true);
    }

    public void MoveCursor(int x, int y)
    {
        if (x >= _btnGrid.GetLength(0))
            return;

        if (y >= _btnGrid.GetLength(1))
            return;

        _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

        _x = x;
        _y = y;

        if (_x < _btnGrid.GetLength(0) - 1)
            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);
    }

    public DynamicButton GetSelectedButton()
    {
        return _btnGrid[_x, _y];
    }

    public object GetSelectedBtnData()
    {
        return _btnGrid[_x, _y].BtnData;
    }

    public int GetSelectedIndex()
    {
        return _x * _col + _y;
    }
}