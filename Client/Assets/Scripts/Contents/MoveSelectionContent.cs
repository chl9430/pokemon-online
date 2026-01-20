using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum MoveSelectionContentState
{
    None = 0,
    Selecting_Move = 1,
    Asking_To_Quit = 2,
    Answering_To_Quit = 3,
}

public class MoveSelectionContent : ObjectContents
{
    PokemonMove _newMove;
    Pokemon _expPokemon;
    MoveSelectionContentState _state = MoveSelectionContentState.None;

    [SerializeField] SelectArea _moveSelectArea;
    [SerializeField] PokemonSummaryUI _pokemonSumUI;
    [SerializeField] TextMeshProUGUI _moveDescriptionText;
    [SerializeField] TextMeshProUGUI _movePowerText;
    [SerializeField] TextMeshProUGUI _moveAccuracyText;

    public MoveSelectionContentState State
    {
        set
        {
            _state = value;

            if (_state == MoveSelectionContentState.None)
            {
                gameObject.SetActive(false);

                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
            }
            else if (_state == MoveSelectionContentState.Selecting_Move)
            {
                _moveSelectArea.UIState = SelectAreaState.SELECTING;

                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
            }
            else if (_state == MoveSelectionContentState.Asking_To_Quit)
            {
                _moveSelectArea.UIState = SelectAreaState.NONE;
            }
            else if (_state == MoveSelectionContentState.Answering_To_Quit)
            {

            }
        }
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;
    }

    public override void SetNextAction(object value = null)
    {
        switch (_state)
        {
            case MoveSelectionContentState.None:
                {
                    gameObject.SetActive(true);

                    State = MoveSelectionContentState.Selecting_Move;
                }
                break;
            case MoveSelectionContentState.Selecting_Move:
                {
                    PokemonMove selectedMove = _moveSelectArea.GetSelectedBtnData() as PokemonMove;

                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (_moveSelectArea.GetSelectedIdx() == 0)
                            {
                                State = MoveSelectionContentState.Asking_To_Quit;
                                List<string> scripts = new List<string>()
                                {
                                    $"Stop trying to teach {selectedMove.MoveName}?"
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts);
                            }
                            else
                            {
                                _expPokemon.PokemonMoves[_moveSelectArea.GetSelectedIdx() - 1] = _newMove;

                                FinishContent();

                                C_ForgetAndLearnNewMove learnNewMovePacket = new C_ForgetAndLearnNewMove();
                                learnNewMovePacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                learnNewMovePacket.ForgetMoveOrder = _moveSelectArea.GetSelectedIdx() - 1;

                                Managers.Network.Send(learnNewMovePacket);
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = MoveSelectionContentState.Asking_To_Quit;
                            List<string> scripts = new List<string>()
                            {
                                $"Stop trying to teach {(_moveSelectArea.BtnGrid[0,0].BtnData as PokemonMove).MoveName}?"
                            };
                            ContentManager.Instance.BeginScriptTyping(scripts);
                        }
                    }
                    else
                    {
                        _moveDescriptionText.text = selectedMove.MoveDescription;
                        _movePowerText.text = selectedMove.MovePower.ToString();
                        _moveAccuracyText.text = selectedMove.MoveAccuracy.ToString();
                    }
                }
                break;
            case MoveSelectionContentState.Asking_To_Quit:
                {
                    State = MoveSelectionContentState.Answering_To_Quit;
                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btns, 1, 400, 100);
                }
                break;
            case MoveSelectionContentState.Answering_To_Quit:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (ContentManager.Instance.ScriptBox.ScriptSelectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                FinishContent();

                                C_ForgetAndLearnNewMove learnNewMovePacket = new C_ForgetAndLearnNewMove();
                                learnNewMovePacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                learnNewMovePacket.ForgetMoveOrder = -1;

                                Managers.Network.Send(learnNewMovePacket);
                            }
                            else if (ContentManager.Instance.ScriptBox.ScriptSelectBox.GetSelectedBtnData() as string == "No")
                            {
                                State = MoveSelectionContentState.Selecting_Move;
                            }
                        }
                        else
                        {
                            State = MoveSelectionContentState.Selecting_Move;
                        }
                    }
                }
                break;
        }
    }

    public override void FinishContent()
    {
        State = MoveSelectionContentState.None;

        Managers.Scene.CurrentScene.FinishContents(false);
    }

    public void SetMoveSelectionScene(Pokemon expPokemon, PokemonMove newMove)
    {
        _newMove = newMove;
        _expPokemon = expPokemon;

        // 포켓몬 정보 랜더링
        _pokemonSumUI.FillPokemonBasicInfo(expPokemon);

        // 기술 버튼 선택 기능 세팅
        List<object> moves = new List<object>();
        for (int i = 0; i < expPokemon.PokemonMoves.Count + 1; i++)
        {
            if (i == 0)
                moves.Add(newMove);
            else
            {
                PokemonMove move = expPokemon.PokemonMoves[i - 1];
                moves.Add(move);
            }
        }
        _moveSelectArea.FillButtonGrid(moves.Count, 1, moves);

        // 기술 버튼 위치 조정
        List<DynamicButton> btns = _moveSelectArea.ChangeBtnGridDataToList();
        for (int i = 0; i < btns.Count; i++)
        {
            DynamicButton btn = btns[i];
            RectTransform rt = btn.GetComponent<RectTransform>();

            if (i == 0)
            {
                PokemonMoveCard moveCard = btns[i].GetComponent<PokemonMoveCard>();
                moveCard.FillMoveCard(newMove);
                moveCard.MoveNameText.color = Color.red;

                rt.anchorMin = new Vector2(0, 0.8f);
                rt.anchorMax = new Vector2(1, 1);
            }
            else
            {
                rt.anchorMin = new Vector2(0, 1 - ((i + 1) * 0.2f));
                rt.anchorMax = new Vector2(1, 1 - (i * 0.2f));

                PokemonMoveCard moveCard = btns[i].GetComponent<PokemonMoveCard>();
                moveCard.FillMoveCard(expPokemon.PokemonMoves[i - 1]);
            }
        }

        // 첫번째 선택된 기술정보 랜더링
        PokemonMove selectedMove = _moveSelectArea.GetSelectedBtnData() as PokemonMove;

        _moveDescriptionText.text = selectedMove.MoveDescription;
        _movePowerText.text = selectedMove.MovePower.ToString();
        _moveAccuracyText.text = selectedMove.MoveAccuracy.ToString();
    }
}
