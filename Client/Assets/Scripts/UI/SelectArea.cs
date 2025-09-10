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
    protected int _row;
    protected int _col;

    protected int _x = 0;
    protected int _y = 0;
    SelectAreaState _uiState = SelectAreaState.NONE;
    protected DynamicButton[,] _btnGrid;
    protected BaseScene _scene;

    [SerializeField] DynamicButton _btn;
    [SerializeField] Transform _btnPos;

    public SelectAreaState UIState
    {
        set
        {
            _uiState = value;

            if (_uiState == SelectAreaState.SELECTING)
                _scene.DoNextAction(_x * _col + _y);
        }
    }

    public DynamicButton[,] BtnGrid { get  { return _btnGrid; } }

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
    }

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

    public void FillButtonGrid(int row, int col, List<object> datas)
    {
        // 기존에 있던 버튼들 삭제
        if (_btnGrid != null)
        {
            for (int i = 0; i < _btnGrid.GetLength(0); i++)
            {
                for (int j = 0; j < _btnGrid.GetLength(1); j++)
                {
                    Destroy(_btnGrid[i, j].gameObject);
                }
            }
        }

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
