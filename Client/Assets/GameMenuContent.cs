using Google.Protobuf;
using Google.Protobuf.Protocol;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using TMPro;
using UnityEngine;

public enum GameMenuContentState
{
    None = 0,
    Choosing_Menu = 1,
    Inactiving = 2,
}

public class GameMenuContent : ObjectContents
{
    GameMenuContentState _state = GameMenuContentState.None;

    [SerializeField] GridLayoutSelectBox _selectBox;

    public GameMenuContentState State
    {
        set
        {
            _state = value;

            if (_state == GameMenuContentState.None)
            {
                gameObject.SetActive(false);

                _selectBox.UIState = GridLayoutSelectBoxState.NONE;
            }
            else if (_state == GameMenuContentState.Choosing_Menu)
            {
                _selectBox.UIState = GridLayoutSelectBoxState.SELECTING;
            }
            else if (_state == GameMenuContentState.Inactiving)
            {
                _selectBox.UIState = GridLayoutSelectBoxState.NONE;
            }
        }
    }

    public override void SetNextAction(object value = null)
    {
        switch (_state)
        {
            case GameMenuContentState.None:
                {
                    State = GameMenuContentState.Choosing_Menu;

                    gameObject.SetActive(true);
                }
                break;
            case GameMenuContentState.Choosing_Menu:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            FinishContent();
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (_selectBox.GetSelectedBtnData() as string == "Pokemon")
                            {
                                List<string> actionBtnNames = new List<string>()
                                {
                                    "Summary",
                                    "Switch",
                                    "Cancel"
                                };
                                ContentManager.Instance.OpenPokemonList(Managers.Object.MyPlayerController.MyPokemons, actionBtnNames);

                                State = GameMenuContentState.Inactiving;
                            }
                            else if (_selectBox.GetSelectedBtnData() as string == "Bag")
                            {
                                ContentManager.Instance.OpenBag();

                                State = GameMenuContentState.Inactiving;
                            }
                            else if (_selectBox.GetSelectedBtnData() as string == "Cancel")
                            {
                                FinishContent();
                            }
                        }
                    }
                }
                break;
            case GameMenuContentState.Inactiving:
                {
                    State = GameMenuContentState.Choosing_Menu;
                }
                break;
        }
    }

    public override void FinishContent()
    {
        State = GameMenuContentState.None;

        Managers.Scene.CurrentScene.FinishContents();

        Managers.Object.MyPlayerController.State = CreatureState.Idle;
    }

    public void SetMenuButtons()
    {
        List<string> btnNames = new List<string>()
        {
            "Pokemon",
            "Bag",
            "Cancel"
        };
        _selectBox.CreateButtons(btnNames, 1, 400, 100);
    }
}
