using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public enum EvolutionSceneState
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
    MOVING_SCENE = 10,
}

public class EvolutionScene : BaseScene
{
    IMessage _packet;
    EvolutionSceneState _sceneState = EvolutionSceneState.NONE;
    PlayerInfo _playerInfo;
    PokemonSummary _myPokemonSum;
    bool _isEvolution;

    [SerializeField] EvolutionController _controller;
    [SerializeField] ScriptBoxUI _scriptBox;

    public EvolutionSceneState SceneState
    {
        set
        {
            _sceneState = value;
        }
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Evolution;
    }

    protected override void Start()
    {
        // 테스트 시 사용.
        if (Managers.Network.Packet == null)
        {
            C_EnterPokemonEvolutionScene enterevolutionScene = new C_EnterPokemonEvolutionScene();
            enterevolutionScene.PlayerId = -1;

            Managers.Network.Send(enterevolutionScene);
        }
        else
            Managers.Network.SendSavedPacket();
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;

        if (packet is S_EnterPokemonEvolutionScene)
        {
            _enterEffect.PlayEffect("FadeIn");

            S_EnterPokemonEvolutionScene enterEvolutionPacket = packet as S_EnterPokemonEvolutionScene;
            _playerInfo = enterEvolutionPacket.PlayerInfo;
            string evolvePokemonName = enterEvolutionPacket.EvolvePokemonName;
            _myPokemonSum = enterEvolutionPacket.PokemonSum;

            _controller.SetPrevPokemonImage(_myPokemonSum.PokemonInfo.PokemonName);
            _controller.SetEvolvePokemonImage(evolvePokemonName);
        }
        else if (packet is S_PokemonEvolution)
        {
            _controller.ChangeEvolvePokemonImage();
            _enterEffect.PlayEffect("WhiteFadeIn");
            _sceneState = EvolutionSceneState.WHITE_FADE_IN;
        }
        else if (packet is S_CheckPokemonEvolution)
        {
            S_CheckPokemonEvolution checkEvolvePacket = packet as S_CheckPokemonEvolution;
            bool goToEvolutionScene = checkEvolvePacket.GoToEvolutionScene;

            IMessage moveScenePacket;
            if (goToEvolutionScene)
            {
                moveScenePacket = new C_EnterPokemonEvolutionScene();
                (moveScenePacket as C_EnterPokemonEvolutionScene).PlayerId = _playerInfo.ObjectInfo.ObjectId;
            }
            else
            {
                moveScenePacket = new C_ReturnGame();
                (moveScenePacket as C_ReturnGame).PlayerId = _playerInfo.ObjectInfo.ObjectId;
            }

            Managers.Network.SavePacket(moveScenePacket);
            SceneState = EvolutionSceneState.MOVING_SCENE;
            _enterEffect.PlayEffect("FadeOut");
        }
        else if (packet is S_MoveSceneToEvolveScene)
        {
            _enterEffect.PlayEffect("FadeIn");

            S_MoveSceneToEvolveScene evolveScenePacket = packet as S_MoveSceneToEvolveScene;
            _playerInfo = evolveScenePacket.PlayerInfo;
            _myPokemonSum = evolveScenePacket.PokemonSum;

            _controller.SetPrevPokemonImage(_myPokemonSum.PokemonInfo.PokemonName);
        }
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);
        switch (_sceneState)
        {
            case EvolutionSceneState.NONE:
                {
                    if (_packet is S_MoveSceneToEvolveScene)
                    {
                        S_MoveSceneToEvolveScene evolveScenePacket = _packet as S_MoveSceneToEvolveScene;
                        string prevMoveName = evolveScenePacket.PrevMoveName;
                        string newMoveName = evolveScenePacket.NewMoveName;

                        if (prevMoveName == "")
                        {
                            _sceneState = EvolutionSceneState.AFTER_MOVE_SELECT_SCRIPTING;
                            List<string> scripts = new List<string>()
                            {
                                $"{_myPokemonSum.PokemonInfo.NickName} did not learn the move {newMoveName}."
                            };
                            _scriptBox.BeginScriptTyping(scripts);
                        }
                        else
                        {
                            _sceneState = EvolutionSceneState.AFTER_MOVE_SELECT_SCRIPTING;
                            List<string> scripts = new List<string>()
                            {
                                $"1, 2, and... ... ... Poof!",
                                $"{_myPokemonSum.PokemonInfo.NickName} forgot how to use {prevMoveName}.",
                                "And...",
                                $"{_myPokemonSum.PokemonInfo.NickName} learned {newMoveName}!",
                            };
                            _scriptBox.BeginScriptTyping(scripts);
                        }
                    }
                    else if (_packet is S_EnterPokemonEvolutionScene)
                    {
                        List<string> scripts = new List<string>()
                        {
                            $"What?\n{_myPokemonSum.PokemonInfo.NickName} is evolving!"
                        };
                        _scriptBox.BeginScriptTyping(scripts);
                        _sceneState = EvolutionSceneState.EVOLUTION_SCRIPTING;
                    }
                }
                break;
            case EvolutionSceneState.EVOLUTION_SCRIPTING:
                {
                    _controller.PlayEvolutionAnim("PokemonEvolution_Evolving");
                    _controller.ControllerState = EvolutionControllerState.SELECTING_EVOLUTION;
                    _sceneState = EvolutionSceneState.EVOLUTION_ANIMATION;
                }
                break;
            case EvolutionSceneState.EVOLUTION_ANIMATION:
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

                    _enterEffect.PlayEffect("WhiteFadeOut");
                    _controller.ControllerState = EvolutionControllerState.NONE;
                    _sceneState = EvolutionSceneState.WHITE_FADE_OUT;
                }
                break;
            case EvolutionSceneState.WHITE_FADE_OUT:
                {
                    _controller.PlayEvolutionAnim("PokemonEvolution_Default");

                    C_PokemonEvolution evolutionPacket = new C_PokemonEvolution();
                    evolutionPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                    evolutionPacket.IsEvolution = _isEvolution;

                    Managers.Network.Send(evolutionPacket);

                    if (!_isEvolution)
                    {
                        _enterEffect.PlayEffect("WhiteFadeIn");
                        _controller.ChangePrevPokemonImage();
                        _sceneState = EvolutionSceneState.WHITE_FADE_IN;
                    }
                }
                break;
            case EvolutionSceneState.WHITE_FADE_IN:
                {
                    if (_packet is S_PokemonEvolution)
                    {
                        S_PokemonEvolution evolutionPacket = _packet as S_PokemonEvolution;
                        PokemonSummary evolvePokemonSum = evolutionPacket.EvolvePokemonSum;

                        List<string> scripts = new List<string>()
                        {
                            $"Congratulations! Your {_myPokemonSum.PokemonInfo.NickName} evolved into {evolvePokemonSum.PokemonInfo.PokemonName}!"
                        };
                        _scriptBox.BeginScriptTyping(scripts);
                        _sceneState = EvolutionSceneState.AFTER_EVOLUTION_SCRIPTING;
                    }
                    else
                    {
                        List<string> scripts = new List<string>()
                        {
                            $"Huh? {_myPokemonSum.PokemonInfo.NickName} stopped evolving!"
                        };
                        _scriptBox.BeginScriptTyping(scripts);
                        _sceneState = EvolutionSceneState.AFTER_EVOLUTION_SCRIPTING;
                    }
                }
                break;
            case EvolutionSceneState.AFTER_EVOLUTION_SCRIPTING:
                {
                    if (_packet is S_PokemonEvolution)
                    {
                        S_PokemonEvolution evolutionPacket = _packet as S_PokemonEvolution;
                        PokemonMoveSummary newMoveSum = evolutionPacket.NewMoveSum;

                        if (newMoveSum != null)
                        {
                            if (_myPokemonSum.PokemonMoves.Count == 4)
                            {
                                List<string> scripts = new List<string>()
                                {
                                    $"{_myPokemonSum.PokemonInfo.NickName} wants to learn the move {newMoveSum.MoveName}.",
                                    $"However, {_myPokemonSum.PokemonInfo.NickName} already knows four moves.",
                                    $"Sould a move be deleted and replaced with {newMoveSum.MoveName}?"
                                };
                                _scriptBox.BeginScriptTyping(scripts);
                                _sceneState = EvolutionSceneState.ASKING_TO_LEARN_NEW_MOVE;
                            }
                            else
                            {
                                List<string> scripts = new List<string>()
                                {
                                    $"{_myPokemonSum.PokemonInfo.NickName} learned {newMoveSum.MoveName}!"
                                };
                                _scriptBox.BeginScriptTyping(scripts);
                                _sceneState = EvolutionSceneState.LEARNED_NEW_MOVE_SCRIPTING;
                            }
                        }
                        else
                        {
                            C_RequestDataById c_RequestDataPacket = new C_RequestDataById();
                            c_RequestDataPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                            c_RequestDataPacket.RequestType = RequestType.CheckPokemonEvolution;

                            Managers.Network.Send(c_RequestDataPacket);
                        }
                    }
                    else
                    {
                        C_RequestDataById c_RequestDataPacket = new C_RequestDataById();
                        c_RequestDataPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                        c_RequestDataPacket.RequestType = RequestType.CheckPokemonEvolution;

                        Managers.Network.Send(c_RequestDataPacket);
                    }
                }
                break;
            case EvolutionSceneState.ASKING_TO_LEARN_NEW_MOVE:
                {
                    _sceneState = EvolutionSceneState.ANSWERING_TO_LEARN_NEW_MOVE;
                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No",
                    };
                    _scriptBox.CreateSelectBox(btns, btns.Count, 1, 400, 100);
                }
                break;
            case EvolutionSceneState.LEARNED_NEW_MOVE_SCRIPTING:
                {
                    C_RequestDataById c_RequestDataPacket = new C_RequestDataById();
                    c_RequestDataPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                    c_RequestDataPacket.RequestType = RequestType.CheckPokemonEvolution;

                    Managers.Network.Send(c_RequestDataPacket);
                }
                break;
            case EvolutionSceneState.ANSWERING_TO_LEARN_NEW_MOVE:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                C_EnterMoveSelectionScene enterMoveScene = new C_EnterMoveSelectionScene();
                                enterMoveScene.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(enterMoveScene);

                                SceneState = EvolutionSceneState.MOVING_SCENE;
                                _enterEffect.PlayEffect("FadeOut");
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                if (_packet is S_PokemonEvolution)
                                {
                                    S_PokemonEvolution evolutionPacket = _packet as S_PokemonEvolution;
                                    PokemonMoveSummary newMoveSum = evolutionPacket.NewMoveSum;

                                    _scriptBox.HideSelectBox();

                                    List<string> scripts = new List<string>()
                                    {
                                        $"{_myPokemonSum.PokemonInfo.NickName} did not learn the move {newMoveSum.MoveName}."
                                    };
                                    _scriptBox.BeginScriptTyping(scripts);
                                    _sceneState = EvolutionSceneState.AFTER_MOVE_SELECT_SCRIPTING;
                                }
                            }
                        }
                    }
                }
                break;
            case EvolutionSceneState.AFTER_MOVE_SELECT_SCRIPTING:
                {
                    C_RequestDataById c_RequestDataPacket = new C_RequestDataById();
                    c_RequestDataPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                    c_RequestDataPacket.RequestType = RequestType.CheckPokemonEvolution;

                    Managers.Network.Send(c_RequestDataPacket);
                }
                break;
            case EvolutionSceneState.MOVING_SCENE:
                {
                    if (Managers.Network.Packet is C_EnterPokemonEvolutionScene)
                        Managers.Scene.LoadScene(Define.Scene.Evolution);
                    else if (Managers.Network.Packet is C_EnterMoveSelectionScene)
                        Managers.Scene.LoadScene(Define.Scene.MoveSelection);
                    //else if (Managers.Network.Packet is C_ReturnGame)
                    //    Managers.Scene.LoadScene(Define.Scene.Game);
                }
                break;
        }
    }

    public override void Clear()
    {
    }
}
