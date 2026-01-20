using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ActionSelectContentState
{
    None = 0,
    Selecting_Action = 1,
    Selecting_Move = 2,
    Cannot_Use_Move_Scripting = 3,
    Cannot_Escape_Scripting = 4,
    AskingSurrender = 5,
    AnsweringSurrender = 6,
    Inactiving = 9,
}

public class ActionSelectContent : ObjectContents
{
    Pokemon _myPokemon;
    ActionSelectContentState _state = ActionSelectContentState.None;

    [SerializeField] GridLayoutSelectBox _actionSelectBox;
    [SerializeField] GridLayoutSelectBox _moveSelectBox;
    [SerializeField] GameObject _moveInfoBox;
    [SerializeField] TextMeshProUGUI _instructorText;
    [SerializeField] TextMeshProUGUI _movePPText;
    [SerializeField] TextMeshProUGUI _moveTypeText;

    public ActionSelectContentState State
    {
        set
        {
            _state = value;

            if (_state == ActionSelectContentState.None)
            {
                _moveInfoBox.gameObject.SetActive(false);
            }
            else if (_state == ActionSelectContentState.Selecting_Action)
            {
                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                _actionSelectBox.gameObject.SetActive(true);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;

                _moveInfoBox.gameObject.SetActive(false);

                _instructorText.gameObject.SetActive(true);
                _instructorText.text = $"What will {_myPokemon.PokemonInfo.NickName} do?";
            }
            else if (_state == ActionSelectContentState.Selecting_Move)
            {
                _moveSelectBox.gameObject.SetActive(true);
                _moveSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;

                _moveInfoBox.gameObject.SetActive(true);
            }
            else if (_state == ActionSelectContentState.Cannot_Use_Move_Scripting)
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
    }

    public override void SetNextAction(object value = null)
    {
        switch (_state)
        {
            case ActionSelectContentState.None:
                {
                    State = ActionSelectContentState.Selecting_Action;

                    gameObject.SetActive(true);
                }
                break;
            case ActionSelectContentState.Selecting_Action:
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
                                List<PokemonMove> availableMoves = FindAvailableMove();

                                if (availableMoves.Count > 0)
                                {
                                    State = ActionSelectContentState.Selecting_Move;
                                }
                                else
                                {
                                    C_ProcessTurn processTurnPacket = new C_ProcessTurn();
                                    processTurnPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                    processTurnPacket.MoveOrder = -1;

                                    Managers.Network.Send(processTurnPacket);

                                    FinishContent();
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

                                State = ActionSelectContentState.Inactiving;
                            }
                            else if (selectedAction == "Bag")
                            {
                                GameContentManager.Instance.OpenBag(Managers.Object.MyPlayerController.Items, "FadeOut");

                                State = ActionSelectContentState.Inactiving;
                            }
                            else if (selectedAction == "Run")
                            {
                                if (Managers.Object.MyPlayerController.NPC is PlayerController)
                                {
                                    List<string> scripts = new List<string>()
                                    {
                                        "Do you want to surrender?"
                                    };
                                    ContentManager.Instance.BeginScriptTyping(scripts);

                                    State = ActionSelectContentState.AskingSurrender;
                                }
                                else if (Managers.Object.MyPlayerController.NPC is CreatureController)
                                {
                                    List<string> scripts = new List<string>()
                                    {
                                        "No! There is no running from a Trainer battle!"
                                    };
                                    ContentManager.Instance.BeginScriptTyping(scripts);

                                    State = ActionSelectContentState.Cannot_Escape_Scripting;
                                }
                                else
                                {
                                    C_RequestDataById requestPacket = new C_RequestDataById();
                                    requestPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                    requestPacket.RequestType = RequestType.EscapeFromWildPokemon;

                                    Managers.Network.Send(requestPacket);

                                    FinishContent();
                                }
                            }
                        }
                    }
                }
                break;
            case ActionSelectContentState.Selecting_Move:
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

                                State = ActionSelectContentState.Cannot_Use_Move_Scripting;
                            }
                            else
                            {
                                C_ProcessTurn processTurnPacket = new C_ProcessTurn();
                                processTurnPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                processTurnPacket.MoveOrder = _moveSelectBox.GetSelectedIdx();

                                Managers.Network.Send(processTurnPacket);

                                FinishContent();
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = ActionSelectContentState.Selecting_Action;
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
            case ActionSelectContentState.Cannot_Use_Move_Scripting:
                {
                    State = ActionSelectContentState.Selecting_Move;
                }
                break;
            case ActionSelectContentState.Cannot_Escape_Scripting:
                {
                    State = ActionSelectContentState.Selecting_Action;
                }
                break;
            case ActionSelectContentState.AskingSurrender:
                {
                    State = ActionSelectContentState.AnsweringSurrender;
                    List<string> btnNames = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btnNames, 1, 400, 100);
                }
                break;
            case ActionSelectContentState.AnsweringSurrender:
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
                                State = ActionSelectContentState.Selecting_Action;
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = ActionSelectContentState.Selecting_Action;
                        }
                    }
                }
                break;
            case ActionSelectContentState.Inactiving:
                {
                    State = ActionSelectContentState.Selecting_Action;
                }
                break;
        }
    }

    public override void FinishContent()
    {
        base.FinishContent();

        State = ActionSelectContentState.None;

        Managers.Scene.CurrentScene.FinishContents(false);
    }

    public PokemonMove GetSelectedMove()
    {
        return _moveSelectBox.GetSelectedBtnData() as PokemonMove;
    }

    public int GetSelectMoveOrder()
    {
        return _moveSelectBox.GetSelectedIdx();
    }

    public List<PokemonMove> FindAvailableMove()
    {
        List<PokemonMove> availableMoves = new List<PokemonMove>();

        for (int i = 0; i < _myPokemon.PokemonMoves.Count; i++)
        {
            PokemonMove move = _myPokemon.PokemonMoves[i];

            if (move.CurPP == 0)
                continue;
            else
                availableMoves.Add(move);
        }

        return availableMoves;
    }

    public void CreateButton(Pokemon pokemon)
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

        CreateMoveButtons();
    }

    public void CreateMoveButtons()
    {
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
