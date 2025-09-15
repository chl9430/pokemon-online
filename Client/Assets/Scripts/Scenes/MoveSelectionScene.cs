using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum MoveSelectionSceneState
{
    NONE = 0,
    SELECTING_MOVE = 1,
    ASKING_TO_QUIT = 2,
    ANSWERING_TO_QUIT = 3,
    MOVING_SCENE = 4,
}

public class MoveSelectionScene : BaseScene
{
    PokemonSummary _pokemonSum;
    PlayerInfo _playerInfo;
    IMessage _packet;
    MoveSelectionSceneState _state;

    [SerializeField] SelectArea _moveSelectArea;
    [SerializeField] PokemonSummaryUI _pokemonSumUI;
    [SerializeField] TextMeshProUGUI _moveDescriptionText;
    [SerializeField] TextMeshProUGUI _movePowerText;
    [SerializeField] TextMeshProUGUI _moveAccuracyText;
    [SerializeField] ScriptBoxUI _scriptBoxUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.MoveSelection;
    }

    protected override void Start()
    {
        // 테스트 시 사용.
        if (Managers.Network.Packet == null)
        {
            C_EnterMoveSelectionScene enterevolutionScene = new C_EnterMoveSelectionScene();
            enterevolutionScene.PlayerId = -1;

            Managers.Network.Send(enterevolutionScene);
        }
        else
            Managers.Network.SendSavedPacket();
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;

        if (packet is S_EnterMoveSelectionScene)
        {
            _enterEffect.PlayEffect("FadeIn");

            S_EnterMoveSelectionScene enterMoveScenePacket = packet as S_EnterMoveSelectionScene;
            PokemonMoveSummary learnableMoveSum = enterMoveScenePacket.LearnableMoveSum;
            _playerInfo = enterMoveScenePacket.PlayerInfo;
            _pokemonSum = enterMoveScenePacket.PokemonSum;

            // 포켓몬 정보 랜더링
            _pokemonSumUI.FillPokemonBasicInfo(_pokemonSum);

            // 기술 버튼 선택 기능 세팅
            List<object> moves = new List<object>();
            for (int i = 0; i < _pokemonSum.PokemonMoves.Count + 1; i++)
            {
                if (i == 0)
                    moves.Add(learnableMoveSum);
                else
                {
                    PokemonMoveSummary moveSum = _pokemonSum.PokemonMoves[i - 1];
                    moves.Add(moveSum);
                }
            }
            _moveSelectArea.FillButtonGrid(_pokemonSum.PokemonMoves.Count + 1, 1, moves);

            // 기술 버튼 위치 조정
            List<DynamicButton> btns = _moveSelectArea.ChangeBtnGridDataToList();
            for (int i = 0; i < btns.Count; i++)
            {
                DynamicButton btn = btns[i];
                RectTransform rt = btn.GetComponent<RectTransform>();

                if (i == 0)
                {
                    PokemonMoveCard moveCard = btns[i].GetComponent<PokemonMoveCard>();
                    moveCard.FillMoveCard(learnableMoveSum);
                    moveCard.MoveNameText.color = Color.red;

                    rt.anchorMin = new Vector2(0, 0.8f);
                    rt.anchorMax = new Vector2(1, 1);
                }
                else
                {
                    rt.anchorMin = new Vector2(0, 1 - ((i + 1) * 0.2f));
                    rt.anchorMax = new Vector2(1, 1 - (i * 0.2f));

                    PokemonMoveCard moveCard = btns[i].GetComponent<PokemonMoveCard>();
                    moveCard.FillMoveCard(_pokemonSum.PokemonMoves[i - 1]);
                }
            }

            // 첫번째 선택된 기술정보 랜더링
            PokemonMoveSummary selectedMoveSum = _moveSelectArea.GetSelectedBtnData() as PokemonMoveSummary;

            _moveDescriptionText.text = selectedMoveSum.MoveDescription;
            _movePowerText.text = selectedMoveSum.MovePower.ToString();
            _moveAccuracyText.text = selectedMoveSum.MoveAccuracy.ToString();
        }
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);
        switch (_state)
        {
            case MoveSelectionSceneState.NONE:
                {
                    _state = MoveSelectionSceneState.SELECTING_MOVE;
                    _moveSelectArea.UIState = SelectAreaState.SELECTING;
                }
                break;
            case MoveSelectionSceneState.SELECTING_MOVE:
                {
                    PokemonMoveSummary selectedMoveSum = _moveSelectArea.GetSelectedBtnData() as PokemonMoveSummary;

                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (_moveSelectArea.GetSelectedIdx() == 0)
                            {
                                _state = MoveSelectionSceneState.ASKING_TO_QUIT;
                                List<string> scripts = new List<string>()
                                {
                                    $"Stop trying to teach {selectedMoveSum.MoveName}?"
                                };
                                _scriptBoxUI.BeginScriptTyping(scripts);
                            }
                            else
                            {
                                if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                                {
                                    C_MoveSceneToBattleScene packet = new C_MoveSceneToBattleScene();
                                    packet.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    packet.PrevMoveIdx = _moveSelectArea.GetSelectedIdx() - 1;

                                    Managers.Network.SavePacket(packet);

                                    _enterEffect.PlayEffect("FadeOut");
                                    _state = MoveSelectionSceneState.MOVING_SCENE;
                                }
                                else if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.PokemonEvolving)
                                {
                                    C_MoveSceneToEvolveScene packet = new C_MoveSceneToEvolveScene();
                                    packet.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    packet.PrevMoveIdx = _moveSelectArea.GetSelectedIdx() - 1;

                                    Managers.Network.SavePacket(packet);

                                    _enterEffect.PlayEffect("FadeOut");
                                    _state = MoveSelectionSceneState.MOVING_SCENE;
                                }
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            _state = MoveSelectionSceneState.ASKING_TO_QUIT;
                            List<string> scripts = new List<string>()
                            {
                                $"Stop trying to teach {(_moveSelectArea.BtnGrid[0,0].BtnData as PokemonMoveSummary).MoveName}?"
                            };
                            _scriptBoxUI.BeginScriptTyping(scripts);
                        }

                        _moveSelectArea.UIState = SelectAreaState.NONE;
                    }
                    else
                    {
                        _moveDescriptionText.text = selectedMoveSum.MoveDescription;
                        _movePowerText.text = selectedMoveSum.MovePower.ToString();
                        _moveAccuracyText.text = selectedMoveSum.MoveAccuracy.ToString();
                    }
                }
                break;
            case MoveSelectionSceneState.ASKING_TO_QUIT:
                {
                    _state = MoveSelectionSceneState.ANSWERING_TO_QUIT;
                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    _scriptBoxUI.CreateSelectBox(btns, btns.Count, 1, 400, 100);
                }
                break;
            case MoveSelectionSceneState.ANSWERING_TO_QUIT:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (_scriptBoxUI.ScriptSelectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.Fight)
                                {
                                    C_MoveSceneToBattleScene packet = new C_MoveSceneToBattleScene();
                                    packet.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    packet.PrevMoveIdx = -1;

                                    Managers.Network.SavePacket(packet);

                                    _enterEffect.PlayEffect("FadeOut");
                                    _state = MoveSelectionSceneState.MOVING_SCENE;
                                }
                                else if (_playerInfo.ObjectInfo.PosInfo.State == CreatureState.PokemonEvolving)
                                {
                                    C_MoveSceneToEvolveScene packet = new C_MoveSceneToEvolveScene();
                                    packet.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    packet.PrevMoveIdx = -1;

                                    Managers.Network.SavePacket(packet);

                                    _enterEffect.PlayEffect("FadeOut");
                                    _state = MoveSelectionSceneState.MOVING_SCENE;
                                }
                            }
                            else if (_scriptBoxUI.ScriptSelectBox.GetSelectedBtnData() as string == "No")
                            {
                                _state = MoveSelectionSceneState.SELECTING_MOVE;
                                _scriptBoxUI.HideSelectBox();
                                _moveSelectArea.UIState = SelectAreaState.SELECTING;
                                _scriptBoxUI.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            _state = MoveSelectionSceneState.SELECTING_MOVE;
                            _scriptBoxUI.HideSelectBox();
                            _moveSelectArea.UIState = SelectAreaState.SELECTING;
                            _scriptBoxUI.gameObject.SetActive(false);
                        }
                    }
                }
                break;
            case MoveSelectionSceneState.MOVING_SCENE:
                {
                    if (Managers.Network.Packet is C_MoveSceneToEvolveScene)
                        Managers.Scene.LoadScene(Define.Scene.Evolution);
                    else if (Managers.Network.Packet is C_MoveSceneToBattleScene)
                        Managers.Scene.LoadScene(Define.Scene.Battle);
                }
                break;
        }
    }

    public override void Clear()
    {
    }
}
