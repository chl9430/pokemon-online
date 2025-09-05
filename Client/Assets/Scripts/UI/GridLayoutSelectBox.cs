using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GridLayoutSelectBoxState
{
    NONE = 0,
    SELECTING = 1,
}

public class GridLayoutSelectBox : MonoBehaviour
{
    int _x;
    int _y;
    int _row;
    int _col;
    BaseScene _scene;
    RectTransform _rt;
    GridLayoutGroup _gridLayoutGroup;
    DynamicButton[,] _btnGrid;
    GridLayoutSelectBoxState _uiState = GridLayoutSelectBoxState.NONE;

    [SerializeField] DynamicButton _btn;

    public GridLayoutSelectBoxState UIState
    {
        set
        {
            _uiState = value;

            if (_uiState == GridLayoutSelectBoxState.SELECTING)
            {
                _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

                _x = 0;
                _y = 0;

                _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

                _scene.DoNextAction(_x * _col + _y);
            }
        }
    }

    private void Start()
    {
        _rt = GetComponent<RectTransform>();
        _gridLayoutGroup = GetComponent<GridLayoutGroup>();
        _scene = Managers.Scene.CurrentScene;
    }

    void Update()
    {
        switch (_uiState)
        {
            case GridLayoutSelectBoxState.SELECTING:
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

            _scene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_y == _col - 1 || _btnGrid[_x, _y + 1] == null)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _y++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            _scene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_x == _row - 1 || _btnGrid[_x + 1, _y] == null)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _x++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            _scene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_x == 0 || _btnGrid[_x - 1, _y] == null)
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

    public void SetSelectBoxContent(List<DynamicButton> btns, int row, int col)
    {
        if (_scene == null)
            _scene = Managers.Scene.CurrentScene;

        if (_gridLayoutGroup == null)
            _gridLayoutGroup = GetComponent<GridLayoutGroup>();

        GridLayoutGroup.Constraint constraint = _gridLayoutGroup.constraint;

        if (constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            _row = row;
            _col = _gridLayoutGroup.constraintCount;
        }
        else if (constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            _row = _gridLayoutGroup.constraintCount;
            _col = col;
        }

        if (_btnGrid == null)
            _btnGrid = new DynamicButton[_row, _col];

        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                if (_row > _col)
                {
                    if (i * _col + j < btns.Count)
                        _btnGrid[i, j] = btns[i * _col + j];
                }
                else
                {
                    if (i * _row + j < btns.Count)
                        _btnGrid[i, j] = btns[i * _row + j];
                }
            }
        }

        _btnGrid[_x, _y].SetSelectedOrNotSelected(true);
    }

    public void CreateButtons(List<string> btnName, int col, int btnWidth, int btnHeight, List<object> btnDatas = null)
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

        if (_scene == null)
            _scene = Managers.Scene.CurrentScene;

        if (_gridLayoutGroup == null)
            _gridLayoutGroup = GetComponent<GridLayoutGroup>();

        if (_rt == null)
            _rt = GetComponent<RectTransform>();

        _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayoutGroup.constraintCount = col;

        _row = (btnName.Count - 1) / col + 1;
        _col = col;

        _gridLayoutGroup.cellSize = new Vector2(btnWidth, btnHeight);
        _rt.sizeDelta = new Vector2(_rt.sizeDelta.x, btnHeight * _row + (btnHeight / 2));

        DynamicButton[,] btnGrid = new DynamicButton[_row, _col];
        _btnGrid = btnGrid;

        _x = 0;
        _y = 0;

        for (int i = 0; i < _btnGrid.GetLength(0); i++)
        {
            for (int j = 0; j < _btnGrid.GetLength(1); j++)
            {
                if (_row > _col)
                {
                    if (i * _col + j < btnName.Count)
                    {
                        DynamicButton btn = GameObject.Instantiate(_btn, gameObject.transform);
                        _btnGrid[i, j] = btn;

                        TextMeshProUGUI tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ContentText", true);
                        tmp.text = btnName[i * _col + j];
                        btn.BtnData = btnDatas == null ? tmp.text : btnDatas[i * _col + j];
                    }
                }
                else
                {
                    if (i * _row + j < btnName.Count)
                    {
                        DynamicButton btn = GameObject.Instantiate(_btn, gameObject.transform);
                        _btnGrid[i, j] = btn;

                        TextMeshProUGUI tmp = Util.FindChild<TextMeshProUGUI>(btn.gameObject, "ContentText", true);
                        tmp.text = btnName[i * _row + j];
                        btn.BtnData = btnDatas == null ? tmp.text : btnDatas[i * _row + j];
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

    public DynamicButton GetSelectedBtn()
    {
        return _btnGrid[_x, _y];
    }
}
