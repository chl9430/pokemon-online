using NUnit.Framework.Constraints;
using NUnit.Framework.Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public enum PokemonSelectAreaState
{
    NONE = 0,
    SELECTING_POKEMON = 1,
    SELECTING_TO_SWITCH_POKEMON = 2,
}

public class PokemonListSelectArea : SelectArea
{
    int _selectedX = 0;
    int _selectedY = 0;
    int _moveFinishCnt = 0;
    PokemonSelectAreaState _state;

    public PokemonSelectAreaState State
    {
        set
        {
            _state = value;

            if (_state == PokemonSelectAreaState.SELECTING_POKEMON || _state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
                _scene.DoNextAction(_x * _col + _y);
        }
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

    protected override void ChooseAction()
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

        Pokemon temp = _btnGrid[_selectedX, _selectedY].BtnData as Pokemon;
        _btnGrid[_selectedX, _selectedY].BtnData = _btnGrid[_x, _y].BtnData as Pokemon;
        _btnGrid[_x, _y].BtnData = temp;

        _btnGrid[_selectedX, _selectedY].GetComponent<PokemonCard>().SetDirection(fromCardDir, 2);
        _btnGrid[_x, _y].GetComponent<PokemonCard>().SetDirection(toCardDir, 2);
    }
}
