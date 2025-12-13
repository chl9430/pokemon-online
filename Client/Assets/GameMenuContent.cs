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
    FadingScene = 2,
    AskingSaveData = 3,
    AnsweringSaveData = 4,
    SuccessSaveDataScripting = 5,
    Inactiving = 9,
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
                gameObject.SetActive(true);

                _selectBox.UIState = GridLayoutSelectBoxState.SELECTING;

                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
                ContentManager.Instance.ScriptBox.HideSelectBox();
            }
            else if (_state == GameMenuContentState.FadingScene)
            {
                _selectBox.UIState = GridLayoutSelectBoxState.NONE;
            }
            else if (_state == GameMenuContentState.AskingSaveData)
            {
                _selectBox.UIState = GridLayoutSelectBoxState.NONE;
                gameObject.SetActive(false);
            }
            else if (_state == GameMenuContentState.AnsweringSaveData)
            {

            }
            else if (_state == GameMenuContentState.SuccessSaveDataScripting)
            {
                ContentManager.Instance.ScriptBox.HideSelectBox();
            }
            else if (_state == GameMenuContentState.Inactiving)
            {
                _selectBox.UIState = GridLayoutSelectBoxState.NONE;
            }
        }
    }

    public override void UpdateData(IMessage packet)
    {
        base.UpdateData(packet);

        _packet = packet;
        _isLoading = false;

        if (_packet is S_SaveGameData)
        {
            bool isSuccess = ((S_SaveGameData)_packet).IsSuccess;

            if (isSuccess)
            {
                List<string> scripts = new List<string>()
                {
                    "Saved game data successfully!"
                };
                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);

                State = GameMenuContentState.SuccessSaveDataScripting;
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
                            if (_selectBox.GetSelectedBtnData() as string == "Cancel")
                            {
                                FinishContent();
                            }
                            else if (_selectBox.GetSelectedBtnData() as string == "Save")
                            {
                                List<string> scripts = new List<string>()
                                {
                                    "Do you wanna save game data?"
                                };
                                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts);

                                State = GameMenuContentState.AskingSaveData;
                            }
                            else
                            {
                                ContentManager.Instance.PlayScreenEffecter("FadeOut");

                                State = GameMenuContentState.FadingScene;
                            }
                        }
                    }
                }
                break;
            case GameMenuContentState.AskingSaveData:
                {
                    State = GameMenuContentState.AnsweringSaveData;

                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btns, 1, 400, 100);
                }
                break;
            case GameMenuContentState.AnsweringSaveData:
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
                            string data = ContentManager.Instance.ScriptBox.ScriptSelectBox.GetSelectedBtnData() as string;

                            if (data == "Yes")
                            {
                                if (!_isLoading)
                                {
                                    _isLoading = true;

                                    C_SaveGameData saveGamePacket = new C_SaveGameData();
                                    saveGamePacket.AccountId = Managers.Object._myAccountId;
                                    saveGamePacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                    Managers.Network.Send(saveGamePacket);
                                }
                                ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Saving data...");
                            }
                            else if (data == "No")
                            {
                                State = GameMenuContentState.Choosing_Menu;
                            }
                        }
                    }
                }
                break;
            case GameMenuContentState.SuccessSaveDataScripting:
                {
                    State = GameMenuContentState.Choosing_Menu;
                }
                break;
            case GameMenuContentState.Inactiving:
                {
                    State = GameMenuContentState.Choosing_Menu;
                }
                break;
            case GameMenuContentState.FadingScene:
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
                        ContentManager.Instance.OpenBag(Managers.Object.MyPlayerController.Items);

                        State = GameMenuContentState.Inactiving;
                    }
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
            "Save",
            "Cancel"
        };
        _selectBox.CreateButtons(btnNames, 1, 400, 100);
    }
}
