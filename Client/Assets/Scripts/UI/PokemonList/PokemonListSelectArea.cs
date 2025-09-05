using Google.Protobuf.Protocol;
using NUnit.Framework.Constraints;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum PokemonSelectAreaState
{
    NONE = 0,
    SELECTING_POKEMON = 1,
    SELECTING_TO_SWITCH_POKEMON = 2,
}

public class PokemonListSelectArea : MonoBehaviour
{
    int _row;
    int _col;
    int _x = 0;
    int _y = 0;
    int _selectedX = 0;
    int _selectedY = 0;
    int _moveFinishCnt = 0;
    PokemonSelectAreaState _state;
    BaseScene _scene;
    DynamicButton[,] _btnGrid;

    [SerializeField] List<Transform> _cardZones;
    [SerializeField] DynamicButton _mainPokemonCard;
    [SerializeField] DynamicButton _subPokemonCard;
    [SerializeField] DynamicButton _cancelBtn;

    public PokemonSelectAreaState State
    {
        set
        {
            _state = value;

            if (_state == PokemonSelectAreaState.SELECTING_POKEMON || _state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
                _scene.DoNextAction(_x * _col + _y);
        }
    }

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
    }

    void Update()
    {
        switch (_state)
        {
            case PokemonSelectAreaState.SELECTING_POKEMON:
                ChooseAction();
                break;
            case PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON:
                ChooseAction();
                break;
        }
    }

    protected void ChooseAction()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_y == 0)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _y--;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            if (_state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                _btnGrid[_selectedX, _selectedY].SetSelectedOrNotSelected(true);
            }

            _scene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_y == _col - 1)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _y++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            if (_state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                _btnGrid[_selectedX, _selectedY].SetSelectedOrNotSelected(true);
            }

            _scene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_x == _row - 1)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _x++;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            if (_state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                _btnGrid[_selectedX, _selectedY].SetSelectedOrNotSelected(true);
            }

            _scene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_x == 0)
                return;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(false);

            _x--;

            _btnGrid[_x, _y].SetSelectedOrNotSelected(true);

            if (_state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                _btnGrid[_selectedX, _selectedY].SetSelectedOrNotSelected(true);
            }

            _scene.DoNextAction(_x * _col + _y);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (_state != PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                _selectedX = _x;
                _selectedY = _y;
            }
            else if (_state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                if (_x == _selectedX && _y == _selectedY)
                    return;
            }

            _scene.DoNextAction(Define.InputSelectBoxEvent.SELECT);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (_state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                ResetSelectedPokemon();
            }

            _scene.DoNextAction(Define.InputSelectBoxEvent.BACK);
        }
    }

    public void CreateButton(List<object> btnDatas, int col = 1)
    {
        if (_scene == null)
            _scene = Managers.Scene.CurrentScene;

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
                        DynamicButton btn;

                        if (i == 0 && j == 0)
                            btn = GameObject.Instantiate(_mainPokemonCard, _cardZones[i * _col + j]);
                        else
                            btn = GameObject.Instantiate(_subPokemonCard, _cardZones[i * _col + j]);

                        _btnGrid[i, j] = btn;

                        btn.BtnData = btnDatas[i * _col + j];

                        lastY = j;
                    }
                }
                else
                {
                    if (i * _row + j < btnDatas.Count)
                    {
                        DynamicButton btn;

                        if (i == 0 && j == 0)
                            btn = GameObject.Instantiate(_mainPokemonCard, _cardZones[i * _row + j]);
                        else
                            btn = GameObject.Instantiate(_subPokemonCard, _cardZones[i * _row + j]);

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

    public object GetSelectedBtnData()
    {
        return _btnGrid[_x, _y].BtnData;
    }

    public object GetSelectedSwitchBtnData()
    {
        return _btnGrid[_selectedX, _selectedY].BtnData;
    }

    public int GetSelectedIndex()
    {
        return _x * _col + _y;
    }

    public void CountContentMoving()
    {
        _moveFinishCnt++;

        if (_moveFinishCnt == 2)
        {
            _moveFinishCnt = 0;

            _scene.DoNextAction();
        }
    }

    public void ResetSelectedPokemon()
    {
        _btnGrid[_selectedX, _selectedY].SetSelectedOrNotSelected(false);
        _btnGrid[_x, _y].SetSelectedOrNotSelected(true);
    }

    public void MoveAndSwitchPokemonCard()
    {
        int fromCardDir = 0;
        int toCardDir = 0;

        if (_selectedX == 0 && _selectedY == 0)
            fromCardDir = -1;
        else
            fromCardDir = 1;

        if (_x == 0 && _y == 0)
            toCardDir = -1;
        else
            toCardDir = 1;

        PokemonSummary temp = _btnGrid[_selectedX, _selectedY].BtnData as PokemonSummary;
        _btnGrid[_selectedX, _selectedY].BtnData = _btnGrid[_x, _y].BtnData as PokemonSummary;
        _btnGrid[_x, _y].BtnData = temp;

        _btnGrid[_selectedX, _selectedY].GetComponent<PokemonCard>().SetDirection(fromCardDir, 2);
        _btnGrid[_x, _y].GetComponent<PokemonCard>().SetDirection(toCardDir, 2);
    }
}
