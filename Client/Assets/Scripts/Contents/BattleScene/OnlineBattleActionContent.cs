using Google.Protobuf.Protocol;
using Google.Protobuf;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum OnlineBattleActionContentState
{
    None = 0,
    Selecting_Action = 1,
    Selecting_Move = 2,
    Cannot_Use_Move_Scripting = 3,
    AskingSurrender = 5,
    AnsweringSurrender = 6,
    Inactiving = 9,
}

public class OnlineBattleActionContent : ObjectContents
{
    Pokemon _myPokemon;
    OnlineBattleActionContentState _state = OnlineBattleActionContentState.None;

    [SerializeField] GridLayoutSelectBox _actionSelectBox;
    [SerializeField] GridLayoutSelectBox _moveSelectBox;
    [SerializeField] GameObject _moveInfoBox;
    [SerializeField] TextMeshProUGUI _instructorText;
    [SerializeField] TextMeshProUGUI _movePPText;
    [SerializeField] TextMeshProUGUI _moveTypeText;

    public OnlineBattleActionContentState State
    {
        set
        {
            _state = value;

            if (_state == OnlineBattleActionContentState.None)
            {
                _moveInfoBox.gameObject.SetActive(false);
            }
            else if (_state == OnlineBattleActionContentState.Selecting_Action)
            {
                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                _actionSelectBox.gameObject.SetActive(true);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;

                _moveInfoBox.gameObject.SetActive(false);

                _instructorText.gameObject.SetActive(true);
                _instructorText.text = $"What will {_myPokemon.PokemonInfo.NickName} do?";
            }
            else if (_state == OnlineBattleActionContentState.Selecting_Move)
            {
                _moveSelectBox.gameObject.SetActive(true);
                _moveSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;

                _moveInfoBox.gameObject.SetActive(true);
            }
            else if (_state == OnlineBattleActionContentState.Cannot_Use_Move_Scripting)
            {
                _moveInfoBox.gameObject.SetActive(false);
            }
        }
    }

    public override void UpdateData(IMessage packet)
    {
        base.UpdateData(packet);

        _packet = packet;
        _isLoading = false;

        if (_packet is S_CheckAvailableMove)
        {
            bool canUseMove = ((S_CheckAvailableMove)_packet).CanUseMove;

            if (canUseMove)
            {
                State = OnlineBattleActionContentState.Selecting_Move;
            }
            else
            {
                FinishContent();

                if (!_isLoading)
                {
                    _isLoading = true;

                    C_SendAction actionPacket = new C_SendAction();
                    actionPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                    actionPacket.UseNoPPMove = new UseNoPPMove();

                    Managers.Network.Send(actionPacket);
                }
            }
        }
    }

    public override void SetNextAction(object value = null)
    {
        if (_isActionStop)
            return;

        switch (_state)
        {
            case OnlineBattleActionContentState.None:
                {
                    _isLoading = false;
                    _isActionStop = false;

                    SetActionButton(value as Pokemon);

                    State = OnlineBattleActionContentState.Selecting_Action;

                    gameObject.SetActive(true);
                }
                break;
            case OnlineBattleActionContentState.Selecting_Action:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            string selectedAction = _actionSelectBox.GetSelectedBtnData() as string;

                            if (selectedAction == "Fight")
                            {
                                // 내 포켓몬에 사용 가능한 기술이 있는 지 확인한다.
                                if (!_isLoading)
                                {
                                    _isLoading = true;

                                    C_CheckAvailableMove checkMovePacket = new C_CheckAvailableMove();
                                    checkMovePacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                    Managers.Network.Send(checkMovePacket);
                                }
                            }
                            else if (selectedAction == "Pokemon")
                            {
                                List<string> actionBtnNames = new List<string>()
                                {
                                    "Send Out",
                                    "Summary",
                                    "Cancel"
                                };
                                GameContentManager.Instance.OpenPokemonList(((BattleScene)Managers.Scene.CurrentScene).Pokemons, actionBtnNames, "FadeOut");

                                State = OnlineBattleActionContentState.Inactiving;
                            }
                            else if (selectedAction == "Bag")
                            {
                            }
                            else if (selectedAction == "Run")
                            {
                                List<string> scripts = new List<string>()
                                {
                                    "Do you want to surrender?"
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts);

                                State = OnlineBattleActionContentState.AskingSurrender;
                            }
                        }
                    }
                }
                break;
            case OnlineBattleActionContentState.Selecting_Move:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            PokemonMove selectedMove = _moveSelectBox.GetSelectedBtnData() as PokemonMove;

                            if (selectedMove.CurPP == 0)
                            {
                                List<string> scripts = new List<string>()
                                {
                                    "Cannot use this move!"
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts);

                                State = OnlineBattleActionContentState.Cannot_Use_Move_Scripting;
                            }
                            else
                            {
                                FinishContent();

                                if (!_isLoading)
                                {
                                    _isLoading = true;

                                    C_SendAction actionPacket = new C_SendAction();
                                    actionPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                    actionPacket.UseMove = new UseMove();
                                    actionPacket.UseMove.SelectedMoveOrder = _moveSelectBox.GetSelectedIdx();

                                    Managers.Network.Send(actionPacket);
                                }

                                ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Waiting for the other side's input...");
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = OnlineBattleActionContentState.Selecting_Action;
                        }
                    }
                    else
                    {
                        PokemonMove selectedMove = _moveSelectBox.GetSelectedBtnData() as PokemonMove;

                        _movePPText.text = $"{selectedMove.CurPP.ToString()} / {selectedMove.MaxPP.ToString()}";
                        _moveTypeText.text = $"TYPE / {selectedMove.MoveType.ToString()}";
                    }
                }
                break;
            case OnlineBattleActionContentState.Cannot_Use_Move_Scripting:
                {
                    State = OnlineBattleActionContentState.Selecting_Move;
                }
                break;
            case OnlineBattleActionContentState.AskingSurrender:
                {
                    State = OnlineBattleActionContentState.AnsweringSurrender;
                    List<string> btnNames = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btnNames, 1, 400, 100);
                }
                break;
            case OnlineBattleActionContentState.AnsweringSurrender:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            string selectedAnswer = ContentManager.Instance.ScriptBox.ScriptSelectBox.GetSelectedBtnData() as string;

                            if (selectedAnswer == "Yes")
                            {
                                FinishContent();

                                if (!_isLoading)
                                {
                                    _isLoading = true;

                                    C_SurrenderTrainerBattle surrenderPacket = new C_SurrenderTrainerBattle();
                                    surrenderPacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                    Managers.Network.Send(surrenderPacket);
                                }
                            }
                            else if (selectedAnswer == "No")
                            {
                                State = OnlineBattleActionContentState.Selecting_Action;
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = OnlineBattleActionContentState.Selecting_Action;
                        }
                    }
                }
                break;
            case OnlineBattleActionContentState.Inactiving:
                {
                    State = OnlineBattleActionContentState.Selecting_Action;
                }
                break;
        }
    }

    public override void FinishContent()
    {
        base.FinishContent();

        State = OnlineBattleActionContentState.None;

        Managers.Scene.CurrentScene.FinishContents(false);
    }

    void SetActionButton(Pokemon pokemon)
    {
        _myPokemon = pokemon;

        // 액션 버튼 데이터 채우기
        List<string> btnNames = new List<string>()
        {
            "Fight",
            "Bag",
            "Pokemon",
            "Run"
        };
        _actionSelectBox.CreateButtons(btnNames, 2, 400, 100);

        // 기술 버튼 데이터 채우기
        List<string> moveNames = new List<string>();
        foreach (PokemonMove move in _myPokemon.PokemonMoves)
            moveNames.Add(move.MoveName);

        List<object> moveDatas = new List<object>();
        foreach (PokemonMove move in _myPokemon.PokemonMoves)
            moveDatas.Add(move);

        _moveSelectBox.CreateButtons(moveNames, 2, 600, 100, moveDatas);
    }
}
