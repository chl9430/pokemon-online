using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectArea : MonoBehaviour
{
    protected int _row;
    protected int _col;

    protected int _x = 0;
    protected int _y = 0;
    GridSelectBoxState _uiState = GridSelectBoxState.NONE;
    protected DynamicButton[,] _btnGrid;
    protected BaseScene _scene;

    public GridSelectBoxState UIState
    {
        set
        {
            _uiState = value;

            if (_uiState == GridSelectBoxState.SELECTING)
                _scene.DoNextAction(_x * _col + _y);
        }
    }

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
    }

    void Update()
    {
        switch (_uiState)
        {
            case GridSelectBoxState.SELECTING:
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

            _scene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_y == _col - 1)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _y++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            _scene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_x == _row - 1)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _x++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            _scene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_x == 0)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _x--;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            _scene.DoNextAction(_x * _col + _y);
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

    public void FillButtonGrid(int row, int col, List<DynamicButton> btns)
    {
        _row = row;
        _col = col;

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
                    if (i * _col + j < btns.Count)
                    {
                        _btnGrid[i, j] = btns[i * _col + j];
                    }
                }
                else
                {
                    if (i * _row + j < btns.Count)
                    {
                        _btnGrid[i, j] = btns[i * _row + j];
                    }
                }
            }
        }

        _btnGrid[_x, _y].SetSelectedOrNotSelected(true);
    }

    public int GetSelectedIdx()
    {
        return _x * _col + _y;
    }
}
