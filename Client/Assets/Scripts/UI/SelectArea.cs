using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum SelectAreaState
{
    NONE = 0,
    SELECTING = 1,
}

public class SelectArea : MonoBehaviour
{
    int _row;
    int _col;

    int _x = 0;
    int _y = 0;
    SelectAreaState _uiState = SelectAreaState.NONE;
    DynamicButton[,] _btnGrid;

    [SerializeField] DynamicButton _btn;
    [SerializeField] Transform _btnPos;

    public SelectAreaState UIState
    {
        set
        {
            _uiState = value;

            if (_uiState == SelectAreaState.SELECTING)
                Managers.Scene.CurrentScene.DoNextAction(_btnGrid[_x, _y].BtnData);
        }
    }

    public DynamicButton[,] BtnGrid { get  { return _btnGrid; } }

    void Update()
    {
        switch (_uiState)
        {
            case SelectAreaState.SELECTING:
                ChooseAction();
                break;
        }
    }

    protected virtual void ChooseAction()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_y == 0)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _y--;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            Managers.Scene.CurrentScene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_y == _col - 1)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _y++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            Managers.Scene.CurrentScene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_x == _row - 1)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _x++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            Managers.Scene.CurrentScene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_x == 0)
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

    public void FillButtonGrid(int row, int col, List<object> datas)
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

            _btnGrid = null;
        }

        _row = row;
        _col = col;

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
                    }
                }
                else
                {
                    if (i * _row + j < datas.Count)
                    {
                        DynamicButton btn = GameObject.Instantiate(_btn, _btnPos);

                        _btnGrid[i, j] = btn;
                        _btnGrid[i, j].BtnData = datas[i * _row + j];
                    }
                }
            }
        }

        _x = 0;
        _y = 0;

        _btnGrid[_x, _y].SetSelectedOrNotSelected(true);
    }

    public object GetSelectedBtnData()
    {
        return _btnGrid[_x, _y].BtnData;
    }

    public int GetSelectedIdx()
    {
        return _x * _col + _y;
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
}
