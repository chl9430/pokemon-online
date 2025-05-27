using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GridSelectBoxState
{
    SELECTING = 0,
    NONE = 1,
}

public class GridSelectBox : MonoBehaviour
{
    int _x = 0;
    int _y = 0;
    GridSelectBoxState _uiState = GridSelectBoxState.NONE;
    ArrowButton[,] _btnGrid;
    BaseScene _scene;

    [SerializeField] int _row;
    [SerializeField] int _col;

    public ArrowButton[,] BtnGrid
    {
        get
        {
            return _btnGrid;
        }
    }

    void Awake()
    {
        _btnGrid = new ArrowButton[_col, _row];
    }

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;

        FillButtonGrid();

        _btnGrid[_x, _y].ToggleArrow(true);
    }

    void FillButtonGrid()
    {
        if (_btnGrid == null)
            _btnGrid = new ArrowButton[_col, _row];

        ArrowButton[] _btns = gameObject.GetComponentsInChildren<ArrowButton>();

        if (_btns.Length != _row * _col)
            Debug.Log("Button count is incorrect.");

        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                _btnGrid[i, j] = _btns[i * _row + j];
            }
        }
    }

    void ChooseAction()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_y == 0)
                return;

            _btnGrid[_x, _y].ToggleArrow(false);

            _y--;

            _btnGrid[_x, _y].ToggleArrow(true);
            _scene.DoNextAction(_btnGrid[_x, _y].BtnData);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_y == _col - 1)
                return;

            _btnGrid[_x, _y].ToggleArrow(false);

            _y++;

            _btnGrid[_x, _y].ToggleArrow(true);
            _scene.DoNextAction(_btnGrid[_x, _y].BtnData);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_x == _row - 1)
                return;

            _btnGrid[_x, _y].ToggleArrow(false);

            _x++;

            _btnGrid[_x, _y].ToggleArrow(true);
            _scene.DoNextAction(_btnGrid[_x, _y].BtnData);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_x == 0)
                return;

            _btnGrid[_x, _y].ToggleArrow(false);

            _x--;

            _btnGrid[_x, _y].ToggleArrow(true);
            _scene.DoNextAction(_btnGrid[_x, _y].BtnData);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _scene.DoNextAction("Select");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            _scene.DoNextAction("Back");
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

    public void SetButtonNames(List<string> names)
    {
        if (_btnGrid == null)
            FillButtonGrid();

        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                _btnGrid[i, j].SetButtonName(names[i * _btnGrid.GetLength(0) + j]);
            }
        }
    }

    public void SetButtonDatas(List<object> datas)
    {
        if (_btnGrid == null)
            FillButtonGrid();

        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                _btnGrid[i, j].BtnData = datas[i * _btnGrid.GetLength(0) + j];
            }
        }
    }

    public void ChangeUIState(GridSelectBoxState state, bool isActive)
    {
        _uiState = state;

        if (_scene == null)
            _scene = Managers.Scene.CurrentScene;

        if (isActive)
            _scene.DoNextAction(_btnGrid[_x, _y].BtnData);

        if (isActive)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
}
