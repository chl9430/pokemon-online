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
    int _curIdx;
    int _switchIdx;
    int _moveFinishCnt = 0;
    PokemonSelectAreaState _state;
    List<DynamicButton> _btnGrid = new List<DynamicButton>();

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
                Managers.Scene.CurrentScene.DoNextAction(_curIdx);
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

    protected void ChooseAction()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_curIdx == _btnGrid.Count - 1)
                return;

            _btnGrid[_curIdx].SetSelectedOrNotSelected(false);

            _curIdx++;

            _btnGrid[_curIdx].SetSelectedOrNotSelected(true);

            if (_state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                _btnGrid[_switchIdx].SetSelectedOrNotSelected(true);
            }

            Managers.Scene.CurrentScene.DoNextAction(_curIdx);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_curIdx == 0)
                return;

            _btnGrid[_curIdx].SetSelectedOrNotSelected(false);

            _curIdx--;

            _btnGrid[_curIdx].SetSelectedOrNotSelected(true);

            if (_state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                _btnGrid[_switchIdx].SetSelectedOrNotSelected(true);
            }

            Managers.Scene.CurrentScene.DoNextAction(_curIdx);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (_state != PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                _switchIdx = _curIdx;
            }
            else if (_state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                if (_curIdx == _switchIdx)
                    return;
            }

            Managers.Scene.CurrentScene.DoNextAction(Define.InputSelectBoxEvent.SELECT);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (_state == PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON)
            {
                ResetSelectedPokemon(false);
            }

            Managers.Scene.CurrentScene.DoNextAction(Define.InputSelectBoxEvent.BACK);
        }
    }

    public void CreateButton(List<Pokemon> myPokemons)
    {
        // 기존에 있던 버튼들 삭제
        for (int i = 0; i < _btnGrid.Count; i++)
        {
            if (i < _btnGrid.Count - 1)
                Destroy(_btnGrid[i].gameObject);
        }

        _btnGrid.Clear();

        for (int i = 0; i < myPokemons.Count; i++)
        {
            if (i == 0)
            {
                _btnGrid.Add(Instantiate(_mainPokemonCard, _cardZones[i]));
                _btnGrid[i].GetComponent<PokemonCard>().FillPokemonCard(myPokemons[i]);
                _btnGrid[i].BtnData = myPokemons[i];
            }
            else
            {
                _btnGrid.Add(Instantiate(_subPokemonCard, _cardZones[i]));
                _btnGrid[i].GetComponent<PokemonCard>().FillPokemonCard(myPokemons[i]);
                _btnGrid[i].BtnData = myPokemons[i];
            }
        }

        _cancelBtn.SetButtonName("Cancel");
        _cancelBtn.SetSelectedOrNotSelected(false);
        _btnGrid.Add(_cancelBtn);

        _btnGrid[0].SetSelectedOrNotSelected(true);
    }

    public DynamicButton GetSelectedBtn()
    {
        return _btnGrid[_curIdx];
    }

    public object GetSelectedBtnData()
    {
        return _btnGrid[_curIdx].BtnData;
    }

    public int GetSelectedIndex()
    {
        return _curIdx;
    }

    public void CountContentMoving()
    {
        _moveFinishCnt++;

        if (_moveFinishCnt == 2)
        {
            _moveFinishCnt = 0;

            Managers.Scene.CurrentScene.DoNextAction();
        }
    }

    public void ResetSelectedPokemon(bool resetCursor)
    {
        _btnGrid[_switchIdx].SetSelectedOrNotSelected(false);

        _btnGrid[_curIdx].SetSelectedOrNotSelected(false);

        if (resetCursor)
        {
            _curIdx = 0;
        }

        _btnGrid[_curIdx].SetSelectedOrNotSelected(true);
    }

    public void MoveAndSwitchPokemonCard()
    {
        int fromCardDir = 0;
        int toCardDir = 0;

        if (_switchIdx == 0)
            fromCardDir = -1;
        else
            fromCardDir = 1;

        if (_curIdx == 0)
            toCardDir = -1;
        else
            toCardDir = 1;

        Pokemon temp = _btnGrid[_switchIdx].BtnData as Pokemon;
        _btnGrid[_switchIdx].BtnData = _btnGrid[_curIdx].BtnData as Pokemon;
        _btnGrid[_curIdx].BtnData = temp;

        _btnGrid[_switchIdx].GetComponent<PokemonCard>().SetDirection(fromCardDir, 2);
        _btnGrid[_curIdx].GetComponent<PokemonCard>().SetDirection(toCardDir, 2);

        // 실제 리스트도 변경
        Managers.Object.MyPlayerController.SwitchPokemon(_curIdx, _switchIdx);
    }

    public bool IsFirstPokemonFainting()
    {
        Pokemon firstPokemon = _btnGrid[0].BtnData as Pokemon;

        if (firstPokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddNewPokemonBtn(Pokemon pokemon)
    {
        _btnGrid.Insert(_btnGrid.Count - 2, Instantiate(_subPokemonCard, _cardZones[_btnGrid.Count - 1]));
        _btnGrid[_btnGrid.Count - 2].BtnData = pokemon;
    }
}
