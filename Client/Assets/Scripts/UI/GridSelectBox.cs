using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GridSelectBoxState
{
    NONE = 0,
    SELECTING = 1
}

public class GridSelectBox : MonoBehaviour
{
    int _x = 0;
    int _y = 0;
    int _row;
    int _col;
    GridSelectBoxState _uiState = GridSelectBoxState.NONE;
    DynamicButton[,] _btnGrid;
    BaseScene _scene;

    

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
                _btnGrid[i, j] = btns[j * _row + i];
            }
        }

        _btnGrid[_x, _y].SetSelectedOrNotSelected(true);
    }

    void ChooseAction()
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

    void Update()
    {
        switch (_uiState)
        {
            case GridSelectBoxState.SELECTING:
                ChooseAction();
                break;
        }
    }

    public void HideAllArow()
    {
        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                _btnGrid[i, j].SetSelectedOrNotSelected(false);
            }
        }
    }
}
