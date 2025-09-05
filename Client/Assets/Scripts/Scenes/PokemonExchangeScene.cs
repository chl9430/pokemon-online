using Google.Protobuf;
using Google.Protobuf.Protocol;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PokemonExchangeSceneState
{
    NONE = 0,
    SELECTING_POKEMON = 1,
    SELECTING_ACTION = 2,
    MOVING_TO_SUMMARY_SCENE = 3,
    WAITING_EXCHANGE_ANSWER = 4,
    ASKING_TO_EXCHANGE = 5,
    SELECTING_EXCHANGE_ACTION = 6,
    WAITING_FINAL_ANSWER = 7,
    FINISH_EXCHANGE_SCRIPTING = 8,
    EXIT_EXCHANGE_SCRIPTING = 9,
    MOVING_TO_GAME_SCENE = 10,
}

public class PokemonExchangeScene : BaseScene
{
    PlayerInfo _myPlayerInfo;
    PlayerInfo _otherPlayerInfo;
    object _data;
    PokemonExchangeSceneState _state = PokemonExchangeSceneState.NONE;

    public PokemonExchangeSceneState State
    {
        set
        {
            _state = value;

            if (_state == PokemonExchangeSceneState.SELECTING_POKEMON)
            {
                _pokemonSelectArea.State = CancelSelectAreaState.SELECTING;
                _actionSelectBox.gameObject.SetActive(false);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;
                _scriptBox.gameObject.SetActive(false);
            }
            else if (_state == PokemonExchangeSceneState.SELECTING_ACTION)
            {
                _pokemonSelectArea.State = CancelSelectAreaState.NONE;
                _actionSelectBox.gameObject.SetActive(true);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
            }
            else if (_state == PokemonExchangeSceneState.MOVING_TO_SUMMARY_SCENE)
            {
                _enterEffect.PlayEffect("FadeOut");
                _pokemonSelectArea.State = CancelSelectAreaState.NONE;
                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;
                _scriptBox.ScriptSelectBox.UIState = GridLayoutSelectBoxState.NONE;
            }
            else if (_state == PokemonExchangeSceneState.FINISH_EXCHANGE_SCRIPTING)
            {
                _scriptBox.HideSelectBox();
            }
            else if (_state == PokemonExchangeSceneState.EXIT_EXCHANGE_SCRIPTING)
            {
                _pokemonSelectArea.State = CancelSelectAreaState.NONE;
                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;
                _actionSelectBox.gameObject.SetActive(false);
            }
        }
    }

    [SerializeField] CancelSelectArea _pokemonSelectArea;
    [SerializeField] CancelSelectArea _otherSelectArea;
    [SerializeField] Transform _otherPokemonArea;
    [SerializeField] DynamicButton _pokemonCard;
    [SerializeField] TextMeshProUGUI _playerName;
    [SerializeField] TextMeshProUGUI _otherPlayerName;
    [SerializeField] DynamicButton _cancelButton;
    [SerializeField] GridLayoutSelectBox _actionSelectBox;
    [SerializeField] ScriptBoxUI _scriptBox;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.PokemonExchange;
    }

    protected override void Start()
    {
        // �׽�Ʈ �� ���.
        if (Managers.Network.Packet == null)
        {
            C_EnterPokemonExchangeScene enterExchangePacket = new C_EnterPokemonExchangeScene();
            enterExchangePacket.PlayerId = -1;

            Managers.Network.Send(enterExchangePacket);
        }
        else
            Managers.Network.SendSavedPacket();
    }

    public override void UpdateData(IMessage packet)
    {
        if (packet is S_EnterPokemonExchangeScene)
        {
            S_EnterPokemonExchangeScene enterExchangeScenePacket = packet as S_EnterPokemonExchangeScene;
            PlayerInfo playerInfo = enterExchangeScenePacket.PlayerInfo;
            PlayerInfo otherPlayerInfo = enterExchangeScenePacket.OtherPlayerInfo;
            IList myPokemonSums = enterExchangeScenePacket.MyPokemonSums;
            IList otherPokemonSums = enterExchangeScenePacket.OtherPokemonSums;
            ExchangeCursorPos myCursorPos = enterExchangeScenePacket.MyCursorPos;
            ExchangeCursorPos otherCursorPos = enterExchangeScenePacket.OtherCursorPos;
            PokemonSummary otherPokemonSum = enterExchangeScenePacket.OtherPokemonSum;

            Managers.Object.PlayerInfo = playerInfo;
            _myPlayerInfo = playerInfo;
            _otherPlayerInfo = otherPlayerInfo;

            _enterEffect.PlayEffect("FadeIn");

            // ���� ���� �̸� ������
            _playerName.text = playerInfo.PlayerName;
            _otherPlayerName.text = otherPlayerInfo.PlayerName;

            // ���� ���ϸ� ����Ʈ�� �����ְ�, ���� ��� ����
            List<object> datas = new List<object>();
            for (int i = 0; i < myPokemonSums.Count; i++)
            {
                datas.Add(myPokemonSums[i]);
            }
            _pokemonSelectArea.CreateButton(2, datas);

            // ���ϸ�ī�忡 ������ �ϱ�
            List<DynamicButton> btns = _pokemonSelectArea.ChangeBtnGridDataToList();
            for (int i = 0; i < btns.Count; i++)
            {
                btns[i].GetComponent<ExchangePokemonCard>().FillPokemonCard(myPokemonSums[i] as PokemonSummary);
            }

            // ����� ���ϸ� ui�� �����Ѵ�.
            _otherSelectArea.CreateButton(2, otherPokemonSums.Count);

            // ����� ���ϸ� ui�� ������ ������ �Ѵ�.
            List<DynamicButton> otherBtns = _otherSelectArea.ChangeBtnGridDataToList();
            for (int i = 0; i < otherBtns.Count; i++)
            {
                otherBtns[i].GetComponent<ExchangePokemonCard>().FillPokemonCard(otherPokemonSums[i] as PokemonSummary);
            }

            // Ŀ�� ��ġ�� �ʱ�ȭ �� ����
            _pokemonSelectArea.MoveCursor(myCursorPos.X, myCursorPos.Y);
            _otherSelectArea.MoveCursor(otherCursorPos.X, otherCursorPos.Y);

            // �׼� ��ư ����
            List<string> btnNames = new List<string>()
            {
                "Exchange",
                "Summary",
                "Cancel"
            };
            _actionSelectBox.CreateButtons(btnNames, 1, 400, 100);

            _data = enterExchangeScenePacket;
        }
        else if (packet is S_MoveExchangeCursor)
        {
            S_MoveExchangeCursor moveCursorPacket = packet as S_MoveExchangeCursor;
            int x = moveCursorPacket.X;
            int y = moveCursorPacket.Y;

            _otherSelectArea.MoveCursor(x, y);
        }
        else if (packet is S_ChooseExchangePokemon)
        {
            S_ChooseExchangePokemon chooseExchangePacket = packet as S_ChooseExchangePokemon;
            PokemonSummary otherPokemonSum = chooseExchangePacket.OtherPokemonSum;

            _state = PokemonExchangeSceneState.ASKING_TO_EXCHANGE;

            List<string> scripts = new List<string>() {
                $"Do you want to exchange with {otherPokemonSum.PokemonInfo.NickName}?"
            };
            _scriptBox.BeginScriptTyping(scripts, true);
            _data = otherPokemonSum;
        }
        else if (packet is S_FinalAnswerToExchange)
        {
            S_FinalAnswerToExchange finalAnswerPacket = packet as S_FinalAnswerToExchange;
            PokemonSummary myPokemonSum = finalAnswerPacket.MyPokemonSum;
            PokemonSummary otherPokemonSum = finalAnswerPacket.OtherPokemonSum;

            if (myPokemonSum == null || otherPokemonSum == null)
            {
                State = PokemonExchangeSceneState.FINISH_EXCHANGE_SCRIPTING;
                List<string> script = new List<string>()
                {
                    $"The other party canceled the exchange.",
                };
                _scriptBox.BeginScriptTyping(script, true);
            }
            else
            {
                DynamicButton btn = _pokemonSelectArea.GetSelectedButton();
                btn.GetComponent<ExchangePokemonCard>().FillPokemonCard(otherPokemonSum);

                DynamicButton otherBtn = _otherSelectArea.GetSelectedButton();
                otherBtn.GetComponent<ExchangePokemonCard>().FillPokemonCard(myPokemonSum);

                _pokemonSelectArea.GetSelectedButton().BtnData = otherPokemonSum;

                State = PokemonExchangeSceneState.FINISH_EXCHANGE_SCRIPTING;
                List<string> script = new List<string>()
                {
                    $"Got {otherPokemonSum.PokemonInfo.NickName} from {_otherPlayerInfo.PlayerName}!",
                    $"Treat {otherPokemonSum.PokemonInfo.NickName} dearly!",
                };
                _scriptBox.BeginScriptTyping(script, true);
            }
        }
        else if (packet is S_ExitPokemonExchangeScene)
        {
            S_ExitPokemonExchangeScene exitExchangePacket = packet as S_ExitPokemonExchangeScene;
            PlayerInfo exitPlayerInfo = exitExchangePacket.ExitPlayerInfo;

            State = PokemonExchangeSceneState.EXIT_EXCHANGE_SCRIPTING;
            List<string> scripts = new List<string>()
            {
                $"{exitPlayerInfo.PlayerName} has ended the exchange."
            };
            _scriptBox.BeginScriptTyping(scripts, true);
        }
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);

        switch (_state)
        {
            case PokemonExchangeSceneState.NONE:
                {
                    if (_data is S_EnterPokemonExchangeScene)
                    {
                        S_EnterPokemonExchangeScene enterExchangePacket = _data as S_EnterPokemonExchangeScene;
                        PokemonSummary otherPokemonSum = enterExchangePacket.OtherPokemonSum;

                        if (otherPokemonSum != null)
                        {
                            _state = PokemonExchangeSceneState.ASKING_TO_EXCHANGE;

                            List<string> scripts = new List<string>() {
                                $"Do you want to exchange with {otherPokemonSum.PokemonInfo.NickName}?"
                            };
                            _scriptBox.BeginScriptTyping(scripts, true);
                            _data = otherPokemonSum;
                        }
                        else
                        {
                            State = PokemonExchangeSceneState.SELECTING_POKEMON;
                        }
                    }
                }
                break;
            case PokemonExchangeSceneState.SELECTING_POKEMON:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if(_pokemonSelectArea.GetSelectedBtnData() as string == "Cancel")
                            {
                                C_ExitPokemonExchangeScene exitExchangePacket = new C_ExitPokemonExchangeScene();
                                exitExchangePacket.PlayerId = _myPlayerInfo.ObjectInfo.ObjectId;

                                Managers.Network.Send(exitExchangePacket);

                                State = PokemonExchangeSceneState.EXIT_EXCHANGE_SCRIPTING;
                                List<string> scripts = new List<string>()
                                {
                                    "Return to the previous screen."
                                };
                                _scriptBox.BeginScriptTyping(scripts, true);
                            }
                            else
                            {
                                State = PokemonExchangeSceneState.SELECTING_ACTION;
                            }
                        }
                    }
                    else
                    {
                        C_MoveExchangeCursor moveCursorPacket = new C_MoveExchangeCursor();
                        moveCursorPacket.PlayerId = _myPlayerInfo.ObjectInfo.ObjectId;
                        moveCursorPacket.X = _pokemonSelectArea.X;
                        moveCursorPacket.Y = _pokemonSelectArea.Y;

                        Managers.Network.Send(moveCursorPacket);
                    }
                }
                break;
            case PokemonExchangeSceneState.SELECTING_ACTION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (_actionSelectBox.GetSelectedBtnData() as string == "Exchange")
                            {
                                _state = PokemonExchangeSceneState.WAITING_EXCHANGE_ANSWER;

                                _pokemonSelectArea.State = CancelSelectAreaState.NONE;
                                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;
                                _actionSelectBox.gameObject.SetActive(false);

                                List<string> scripts = new List<string>() {
                                    "Please wait for the opponent's answer."
                                };
                                _scriptBox.BeginScriptTyping(scripts, true);
                            }
                            else if (_actionSelectBox.GetSelectedBtnData() as string == "Summary")
                            {
                                Managers.Scene.Data = _pokemonSelectArea.GetSelectedBtnData();

                                State = PokemonExchangeSceneState.MOVING_TO_SUMMARY_SCENE;
                            }
                            else if (_actionSelectBox.GetSelectedBtnData() as string == "Cancel")
                            {
                                State = PokemonExchangeSceneState.SELECTING_POKEMON;
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = PokemonExchangeSceneState.SELECTING_POKEMON;
                        }
                    }
                }
                break;
            case PokemonExchangeSceneState.WAITING_EXCHANGE_ANSWER:
                {
                    C_ChooseExchangePokemon choosePacket = new C_ChooseExchangePokemon();
                    choosePacket.PlayerId = _myPlayerInfo.ObjectInfo.ObjectId;
                    choosePacket.PokemonOrder = _pokemonSelectArea.GetSelectedIndex();

                    Managers.Network.Send(choosePacket);
                }
                break;
            case PokemonExchangeSceneState.ASKING_TO_EXCHANGE:
                {
                    List<string> btnNames = new List<string>()
                    {
                        "Yes",
                        "Summary",
                        "No",
                    };
                    _state = PokemonExchangeSceneState.SELECTING_EXCHANGE_ACTION;
                    _scriptBox.CreateSelectBox(btnNames, btnNames.Count, 1, 400, 100);
                }
                break;
            case PokemonExchangeSceneState.SELECTING_EXCHANGE_ACTION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;
                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                C_FinalAnswerToExchange answerPacket = new C_FinalAnswerToExchange();
                                answerPacket.PlayerId = _myPlayerInfo.ObjectInfo.ObjectId;
                                answerPacket.FinalAnswer = true;

                                Managers.Network.SavePacket(answerPacket);

                                List<string> scripts = new List<string>() {
                                    "Please wait for the opponent's answer."
                                };
                                _scriptBox.BeginScriptTyping(scripts, true);

                                _state = PokemonExchangeSceneState.WAITING_FINAL_ANSWER;
                                _scriptBox.HideSelectBox();
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "Summary")
                            {
                                if (_data is PokemonSummary)
                                {
                                    PokemonSummary otherPokemonSum = _data as PokemonSummary;
                                    Managers.Scene.Data = otherPokemonSum;

                                    State = PokemonExchangeSceneState.MOVING_TO_SUMMARY_SCENE;
                                }
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                C_FinalAnswerToExchange answerPacket = new C_FinalAnswerToExchange();
                                answerPacket.PlayerId = _myPlayerInfo.ObjectInfo.ObjectId;
                                answerPacket.FinalAnswer = false;

                                Managers.Network.Send(answerPacket);

                                State = PokemonExchangeSceneState.FINISH_EXCHANGE_SCRIPTING;
                                List<string> scripts = new List<string>() {
                                    "The exchange has been canceled."
                                };
                                _scriptBox.BeginScriptTyping(scripts, true);
                            }
                        }
                    }
                }
                break;
            case PokemonExchangeSceneState.WAITING_FINAL_ANSWER:
                {
                    Managers.Network.SendSavedPacket();
                }
                break;
            case PokemonExchangeSceneState.FINISH_EXCHANGE_SCRIPTING:
                {
                    State = PokemonExchangeSceneState.SELECTING_POKEMON;
                }
                break;
            case PokemonExchangeSceneState.MOVING_TO_SUMMARY_SCENE:
                {
                    // �� ����
                    Managers.Scene.LoadScene(Define.Scene.PokemonSummary);
                }
                break;
            case PokemonExchangeSceneState.EXIT_EXCHANGE_SCRIPTING:
                {
                    _enterEffect.PlayEffect("FadeOut");

                    _state = PokemonExchangeSceneState.MOVING_TO_GAME_SCENE;
                }
                break;
            case PokemonExchangeSceneState.MOVING_TO_GAME_SCENE:
                {
                    C_ReturnGame returnGamePacket = new C_ReturnGame();
                    returnGamePacket.PlayerId = _myPlayerInfo.ObjectInfo.ObjectId;

                    Managers.Network.SavePacket(returnGamePacket);

                    // �� ����
                    Managers.Scene.LoadScene(Define.Scene.Game);
                }
                break;
        }
    }

    public override void Clear()
    {
    }
}
