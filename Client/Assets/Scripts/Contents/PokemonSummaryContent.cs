using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PokemonSummaryContentState
{
    None = 0,
    Waiting_Input = 1,
    Exit_Exchange_Scripting = 2,
    Exchange_Canceled_Scripting = 3,
}

public class PokemonSummaryContent : ObjectContents
{
    PokemonSummaryContentState _state = PokemonSummaryContentState.None;

    [SerializeField] PokemonSummaryUI summaryUI;
    [SerializeField] SelectArea _moveSelectArea;
    [SerializeField] TextMeshProUGUI _moveDescriptionText;
    [SerializeField] TextMeshProUGUI _movePowerText;
    [SerializeField] TextMeshProUGUI _moveAccuracyText;
    [SerializeField] CategorySlider _slider;
    [SerializeField] RectTransform _indicator;

    public PokemonSummaryContentState State
    {
        set
        {
            _state = value;

            if (_state == PokemonSummaryContentState.None)
            {
                _moveSelectArea.UIState = SelectAreaState.NONE;
                _slider.SliderState = SliderState.NONE;
            }
            else if (_state == PokemonSummaryContentState.Waiting_Input)
            {
                _slider.SliderState = SliderState.WAITING_INPUT;
            }
            else if (_state == PokemonSummaryContentState.Exit_Exchange_Scripting)
            {
                _slider.SliderState = SliderState.NONE;
                _moveSelectArea.UIState = SelectAreaState.NONE;
            }
            else if (_state == PokemonSummaryContentState.Exchange_Canceled_Scripting)
            {
                _slider.SliderState = SliderState.NONE;
                _moveSelectArea.UIState = SelectAreaState.NONE;
            }
        }
    }

    public override void UpdateData(IMessage packet)
    {
        if (packet is S_ExitPokemonExchangeScene)
        {
            S_ExitPokemonExchangeScene exitExchangePacket = packet as S_ExitPokemonExchangeScene;
            PlayerInfo exitPlayerInfo = exitExchangePacket.ExitPlayerInfo;

            State = PokemonSummaryContentState.Exit_Exchange_Scripting;
            List<string> scripts = new List<string>()
            {
                $"{exitPlayerInfo.PlayerName} has ended the exchange."
            };
            ContentManager.Instance.BeginScriptTyping(scripts, true);
        }
        else if (packet is S_FinalAnswerToExchange)
        {
            S_FinalAnswerToExchange finalAnswerPacket = packet as S_FinalAnswerToExchange;
            PokemonSummary myPokemonSum = finalAnswerPacket.MyPokemonSum;
            PokemonSummary otherPokemonSum = finalAnswerPacket.OtherPokemonSum;

            if (myPokemonSum == null || otherPokemonSum == null)
            {
                State = PokemonSummaryContentState.Exchange_Canceled_Scripting;
                List<string> script = new List<string>()
                {
                    $"The other party canceled the exchange.",
                };
                ContentManager.Instance.BeginScriptTyping(script, true);
            }
        }
    }

    public override void SetNextAction(object value = null)
    {
        switch (_state)
        {
            case PokemonSummaryContentState.None:
                {
                    State = PokemonSummaryContentState.Waiting_Input;

                    gameObject.SetActive(true);
                }
                break;
            case PokemonSummaryContentState.Waiting_Input:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            FinishContent();
                        }
                    }
                    else if (value is PokemonMove)
                    {
                        PokemonMove move = value as PokemonMove;

                        _moveDescriptionText.text = move.MoveDescription;
                        _movePowerText.text = move.MovePower.ToString();
                        _moveAccuracyText.text = move.MoveAccuracy.ToString();
                    }
                    else
                    {
                        int selectedIdx = _slider.CurIdx;

                        _indicator.anchorMax = new Vector2(((float)selectedIdx + 1f) / (float)_slider.SliderContents.Count, 1f);

                        if (selectedIdx == 2)
                            _moveSelectArea.UIState = SelectAreaState.SELECTING;
                        else
                            _moveSelectArea.UIState = SelectAreaState.NONE;
                    }
                }
                break;
            case PokemonSummaryContentState.Exit_Exchange_Scripting:
                {
                    FinishContent();
                }
                break;
            case PokemonSummaryContentState.Exchange_Canceled_Scripting:
                {
                    FinishContent();
                }
                break;
        }
    }

    public override void FinishContent()
    {
        State = PokemonSummaryContentState.None;

        Managers.Scene.CurrentScene.FinishContents(false);
    }

    public void SetPokemonSummary(Pokemon pokemon)
    {
        summaryUI.FillPokemonBasicInfo(pokemon);
        summaryUI.FillPokemonSummary(pokemon);

        // 기술 버튼 선택 기능 세팅
        List<object> moves = new List<object>();
        for (int i = 0; i < pokemon.PokemonMoves.Count; i++)
        {
            moves.Add(pokemon.PokemonMoves[i]);
        }
        _moveSelectArea.FillButtonGrid(moves.Count, 1, moves);

        // 기술 버튼 위치 조정
        List<DynamicButton> btns = _moveSelectArea.ChangeBtnGridDataToList();
        for (int i = 0; i < btns.Count; i++)
        {
            DynamicButton btn = btns[i];
            RectTransform rt = btn.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0, 1 - ((i + 1) * 0.25f));
            rt.anchorMax = new Vector2(1, 1 - (i * 0.25f));

            PokemonMoveCard moveCard = btns[i].GetComponent<PokemonMoveCard>();
            moveCard.FillMoveCard(pokemon.PokemonMoves[i]);
        }

        // ui 리셋
        _slider.ResetSliderContents();
    }
}
