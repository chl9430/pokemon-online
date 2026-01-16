using Google.Protobuf.Protocol;
using Google.Protobuf;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public enum PokemonEvolutionContentState
{
    NONE = 0,
    EVOLUTION_SCRIPTING = 1,
    EVOLUTION_ANIMATION = 2,
    WHITE_FADE_OUT = 3,
    WHITE_FADE_IN = 4,
    AFTER_EVOLUTION_SCRIPTING = 5,
    ASKING_TO_LEARN_NEW_MOVE = 6,
    LEARNED_NEW_MOVE_SCRIPTING = 7,
    ANSWERING_TO_LEARN_NEW_MOVE = 8,
    AFTER_MOVE_SELECT_SCRIPTING = 9,
    FADING_OUT = 10,
    Inactiving = 11,
}

public class PokemonEvolutionContent : ObjectContents
{
    PokemonEvolutionContentState _state = PokemonEvolutionContentState.NONE;
    Pokemon _evolutionPokemon;
    PokemonMove _newMove;
    bool _isEvolution;

    [SerializeField] EvolutionController _controller;

    public PokemonEvolutionContentState State
    {
        set
        {
            _state = value;

            if (_state == PokemonEvolutionContentState.NONE)
            {
                gameObject.SetActive(false);

                ContentManager.Instance.ScriptBox.HideSelectBox();
                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
            }
            else if (_state == PokemonEvolutionContentState.Inactiving)
            {
                gameObject.SetActive(false);
            }
            else if (_state == PokemonEvolutionContentState.AFTER_MOVE_SELECT_SCRIPTING)
            {
                gameObject.SetActive(true);
            }
        }
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;

        if (_packet is S_PokemonEvolution)
        {
            _controller.ChangeEvolvePokemonImage();
            _controller.EvolutionFadeIn();

            State = PokemonEvolutionContentState.WHITE_FADE_IN;
        }
        else if (_packet is S_CheckPokemonEvolution)
        {
            S_CheckPokemonEvolution checkEvolvePacket = packet as S_CheckPokemonEvolution;
            string evolvePokemonName = checkEvolvePacket.EvolutionPokemonName;
            int evolvePokemonIdx = checkEvolvePacket.EvolvePokemonIdx;

            FinishContent();

            if (evolvePokemonIdx != -1)
            {
                GameContentManager.Instance.OpenPokemonEvolution(Managers.Object.MyPlayerController.MyPokemons[evolvePokemonIdx], evolvePokemonName);
            }
            else
            {
                C_ReturnGame returnGamePacket = new C_ReturnGame();
                returnGamePacket.PlayerId = Managers.Object.MyPlayerController.Id;

                Managers.Scene.AsyncUnLoadScene(Define.Scene.Battle, () =>
                {
                    ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
                    Managers.Network.Send(returnGamePacket);
                    Managers.Scene.CurrentScene = GameObject.FindFirstObjectByType<GameScene>();
                });
            }
        }
        else if (_packet is S_ForgetAndLearnNewMove)
        {
            S_ForgetAndLearnNewMove learnNewMovePacket = packet as S_ForgetAndLearnNewMove;
            string prevMoveName = learnNewMovePacket.PrevMoveName;
            string newMoveName = learnNewMovePacket.NewMoveName;

            if (prevMoveName == "")
            {
                List<string> scripts = new List<string>()
                {
                    $"{_evolutionPokemon.PokemonInfo.NickName} did not learn the move {newMoveName}."
                };
                ContentManager.Instance.BeginScriptTyping(scripts);
                State = PokemonEvolutionContentState.AFTER_MOVE_SELECT_SCRIPTING;
            }
            else
            {
                List<string> scripts = new List<string>()
                {
                    $"1, 2, and... ... ... Poof!",
                    $"{_evolutionPokemon.PokemonInfo.NickName} forgot how to use {prevMoveName}.",
                    "And...",
                    $"{_evolutionPokemon.PokemonInfo.NickName} learned {newMoveName}!",
                };
                ContentManager.Instance.BeginScriptTyping(scripts);
                State = PokemonEvolutionContentState.AFTER_MOVE_SELECT_SCRIPTING;
            }
        }
    }

    public override void SetNextAction(object value = null)
    {
        switch (_state)
        {
            case PokemonEvolutionContentState.NONE:
                {
                    ContentManager.Instance.PlayScreenEffecter("FadeIn_NonBroading");

                    gameObject.SetActive(true);

                    List<string> scripts = new List<string>()
                    {
                        $"What?\n{_evolutionPokemon.PokemonInfo.NickName} is evolving!"
                    };
                    ContentManager.Instance.BeginScriptTyping(scripts);

                    State = PokemonEvolutionContentState.EVOLUTION_SCRIPTING;
                }
                break;
            case PokemonEvolutionContentState.EVOLUTION_SCRIPTING:
                {
                    _controller.PokemonEvolution();
                    _controller.ControllerState = EvolutionControllerState.SELECTING_EVOLUTION;

                    State = PokemonEvolutionContentState.EVOLUTION_ANIMATION;
                }
                break;
            case PokemonEvolutionContentState.EVOLUTION_ANIMATION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            _isEvolution = false;
                        }
                    }
                    else
                    {
                        _isEvolution = true;
                    }

                    _controller.EvolutionFadeOut();
                    _controller.ControllerState = EvolutionControllerState.NONE;

                    State = PokemonEvolutionContentState.WHITE_FADE_OUT;
                }
                break;
            case PokemonEvolutionContentState.WHITE_FADE_OUT:
                {
                    C_PokemonEvolution evolutionPacket = new C_PokemonEvolution();
                    evolutionPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                    evolutionPacket.IsEvolution = _isEvolution;

                    Managers.Network.Send(evolutionPacket);

                    if (!_isEvolution)
                    {
                        _controller.EvolutionFadeIn();
                        _controller.ChangePrevPokemonImage();
                        State = PokemonEvolutionContentState.WHITE_FADE_IN;
                    }
                }
                break;
            case PokemonEvolutionContentState.WHITE_FADE_IN:
                {
                    if (_packet is S_PokemonEvolution)
                    {
                        S_PokemonEvolution evolutionPacket = _packet as S_PokemonEvolution;
                        PokemonSummary evolvePokemonSum = evolutionPacket.EvolvePokemonSum;

                        _evolutionPokemon.UpdatePokemonSummary(evolvePokemonSum);

                        List<string> scripts = new List<string>()
                        {
                            $"Congratulations! Your {_evolutionPokemon.PokemonInfo.NickName} evolved into {_evolutionPokemon.PokemonInfo.PokemonName}!"
                        };
                        ContentManager.Instance.BeginScriptTyping(scripts);

                        State = PokemonEvolutionContentState.AFTER_EVOLUTION_SCRIPTING;
                    }
                    else
                    {
                        List<string> scripts = new List<string>()
                        {
                            $"Huh? {_evolutionPokemon.PokemonInfo.NickName} stopped evolving!"
                        };
                        ContentManager.Instance.BeginScriptTyping(scripts);
                        State = PokemonEvolutionContentState.AFTER_EVOLUTION_SCRIPTING;
                    }
                }
                break;
            case PokemonEvolutionContentState.AFTER_EVOLUTION_SCRIPTING:
                {
                    if (_packet is S_PokemonEvolution)
                    {
                        S_PokemonEvolution evolutionPacket = _packet as S_PokemonEvolution;
                        PokemonMoveSummary newMoveSum = evolutionPacket.NewMoveSum;

                        if (newMoveSum != null)
                        {
                            _newMove = new PokemonMove(newMoveSum);

                            if (_evolutionPokemon.PokemonMoves.Count == 4)
                            {
                                List<string> scripts = new List<string>()
                                {
                                    $"{_evolutionPokemon.PokemonInfo.NickName} wants to learn the move {_newMove.MoveName}.",
                                    $"However, {_evolutionPokemon.PokemonInfo.NickName} already knows four moves.",
                                    $"Sould a move be deleted and replaced with {_newMove.MoveName}?"
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts);
                                State = PokemonEvolutionContentState.ASKING_TO_LEARN_NEW_MOVE;
                            }
                            else
                            {
                                _evolutionPokemon.PokemonMoves.Add(_newMove);

                                List<string> scripts = new List<string>()
                                {
                                    $"{_evolutionPokemon.PokemonInfo.NickName} learned {_newMove.MoveName}!"
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts);
                                State = PokemonEvolutionContentState.LEARNED_NEW_MOVE_SCRIPTING;
                            }
                        }
                        else
                        {
                            ContentManager.Instance.PlayScreenEffecter("FadeOut");
                            State = PokemonEvolutionContentState.FADING_OUT;
                        }
                    }
                    else // 진화를 취소했을 경유
                    {
                        ContentManager.Instance.PlayScreenEffecter("FadeOut");
                        State = PokemonEvolutionContentState.FADING_OUT;
                    }
                }
                break;
            case PokemonEvolutionContentState.ASKING_TO_LEARN_NEW_MOVE:
                {
                    State = PokemonEvolutionContentState.ANSWERING_TO_LEARN_NEW_MOVE;

                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No",
                    };
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btns, 1, 400, 100);
                }
                break;
            case PokemonEvolutionContentState.LEARNED_NEW_MOVE_SCRIPTING:
                {
                    ContentManager.Instance.PlayScreenEffecter("FadeOut");
                    State = PokemonEvolutionContentState.FADING_OUT;
                }
                break;
            case PokemonEvolutionContentState.ANSWERING_TO_LEARN_NEW_MOVE:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                GameContentManager.Instance.OpenMoveSelection(_evolutionPokemon, _newMove);

                                State = PokemonEvolutionContentState.Inactiving;
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                if (_packet is S_PokemonEvolution)
                                {
                                    S_PokemonEvolution evolutionPacket = _packet as S_PokemonEvolution;
                                    PokemonMoveSummary newMoveSum = evolutionPacket.NewMoveSum;

                                    ContentManager.Instance.ScriptBox.HideSelectBox();
                                    List<string> scripts = new List<string>()
                                    {
                                        $"{_evolutionPokemon.PokemonInfo.NickName} did not learn the move {_newMove.MoveName}."
                                    };
                                    ContentManager.Instance.BeginScriptTyping(scripts);
                                    State = PokemonEvolutionContentState.AFTER_MOVE_SELECT_SCRIPTING;
                                }
                            }
                        }
                    }
                }
                break;
            case PokemonEvolutionContentState.AFTER_MOVE_SELECT_SCRIPTING:
                {
                    ContentManager.Instance.PlayScreenEffecter("FadeOut");
                    State = PokemonEvolutionContentState.FADING_OUT;
                }
                break;
            case PokemonEvolutionContentState.FADING_OUT:
                {
                    C_RequestDataById c_RequestDataPacket = new C_RequestDataById();
                    c_RequestDataPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                    c_RequestDataPacket.RequestType = RequestType.CheckPokemonEvolution;

                    Managers.Network.Send(c_RequestDataPacket);
                }
                break;
            case PokemonEvolutionContentState.Inactiving:
                {
                }
                break;
        }
    }

    public override void FinishContent()
    {
        State = PokemonEvolutionContentState.NONE;

        Managers.Scene.CurrentScene.FinishContents();
    }

    public void SetEvolutionPokemon(Pokemon pokemon, string evolutionPokemonName)
    {
        _evolutionPokemon = pokemon;

        _controller.SetPrevPokemonImage(_evolutionPokemon.PokemonInfo.PokemonName);
        _controller.SetEvolvePokemonImage(evolutionPokemonName);
    }
}
