using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public enum PokemonCenterContentState
{
    NONE = 0,
    GREETING_SCRIPTING = 1,
    SELECTING_ACTION = 2,
    GOOD_BYE_SCRIPTING = 3,
    TAKING_POKEMON_SCRIPTING = 4,
    NURSE_TURNING_LEFT = 5,
    HEALING_MACHINE_ANIMATION = 6,
    THANK_YOU_SCRIPTING = 7,
    NURSE_THANK_ANIMATION = 8,
}

public class PokemonCenterContent : ObjectContents
{
    [SerializeField] ScriptBoxUI _scriptBox;
    [SerializeField] RecoveryMachine _recoveryMachine;

    PokemonCenterContentState _state = PokemonCenterContentState.NONE;

    public PokemonCenterContentState State
    {
        set
        {
            _state = value;

            if (_scene == null)
            {
                _scene = Managers.Scene.CurrentScene;
            }

            if (_state == PokemonCenterContentState.GREETING_SCRIPTING)
            {
                _scriptBox.gameObject.SetActive(true);
            }
            else if (_state == PokemonCenterContentState.SELECTING_ACTION)
            {
                _scriptBox.gameObject.SetActive(true);
            }
            else if (_state == PokemonCenterContentState.GOOD_BYE_SCRIPTING)
            {
                _scriptBox.gameObject.SetActive(true);
                _scriptBox.HideSelectBox();
            }
            else if (_state == PokemonCenterContentState.TAKING_POKEMON_SCRIPTING)
            {
                _scriptBox.gameObject.SetActive(true);
                _scriptBox.HideSelectBox();
            }
            else if (_state == PokemonCenterContentState.NONE)
            {
                _scriptBox.gameObject.SetActive(false);
                _scriptBox.HideSelectBox();
            }
        }
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;

        if (_packet is S_GetNpcTalk)
        {
            if (_scene == null)
            {
                _scene = Managers.Scene.CurrentScene;
            }

            _scene.MyPlayer.State = CreatureState.Shopping;
            State = PokemonCenterContentState.GREETING_SCRIPTING;

            List<string> scripts = new List<string>()
            {
                "Hello, and welcome to the Pokemon Center.",
                "We restore your tired Pokemon to full health.",
                "Would you like to rest your Pokemon?",
            };
            _scriptBox.BeginScriptTyping(scripts);

            _recoveryMachine = FindFirstObjectByType<RecoveryMachine>();
        }
        else if (_packet is S_RestorePokemon)
        {
            int pokemonCount = ((S_RestorePokemon)_packet).PokemonCount;
            _recoveryMachine.StartHeal(pokemonCount);

            State = PokemonCenterContentState.HEALING_MACHINE_ANIMATION;
        }
    }

    public override void SetNextAction(object value)
    {
        switch (_state)
        {
            case PokemonCenterContentState.GREETING_SCRIPTING:
                {
                    State = PokemonCenterContentState.SELECTING_ACTION;

                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    _scriptBox.CreateSelectBox(btns, btns.Count, 1, 400, 100);
                }
                break;
            case PokemonCenterContentState.SELECTING_ACTION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                State = PokemonCenterContentState.TAKING_POKEMON_SCRIPTING;

                                List<string> scripts = new List<string>()
                                {
                                    "Okay, I will take your Pokemon for a few seconds.",
                                };
                                _scriptBox.BeginScriptTyping(scripts);
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                List<string> scrtips = new List<string>()
                                {
                                    "We hope to see you again!"
                                };
                                _scriptBox.BeginScriptTyping(scrtips);
                                State = PokemonCenterContentState.GOOD_BYE_SCRIPTING;
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            List<string> scrtips = new List<string>()
                            {
                                "We hope to see you again!"
                            };
                            _scriptBox.BeginScriptTyping(scrtips);
                            State = PokemonCenterContentState.GOOD_BYE_SCRIPTING;
                        }
                    }
                }
                break;
            case PokemonCenterContentState.GOOD_BYE_SCRIPTING:
                {
                    if (!_isLoading)
                    {
                        _isLoading = true;

                        C_FinishNpcTalk finishTalk = new C_FinishNpcTalk();
                        finishTalk.PlayerId = _scene.MyPlayer.Id;

                        Managers.Network.Send(finishTalk);
                    }

                    State = PokemonCenterContentState.NONE;

                    ((GameScene)_scene).FinishContents();

                    _scene.MyPlayer.State = CreatureState.Idle;
                }
                break;
            case PokemonCenterContentState.TAKING_POKEMON_SCRIPTING:
                {
                    Animator anim = GetComponent<Animator>();
                    anim.Play("Nurse_Left");

                    State = PokemonCenterContentState.NURSE_TURNING_LEFT;
                }
                break;
            case PokemonCenterContentState.NURSE_TURNING_LEFT:
                {
                    if (!_isLoading)
                    {
                        _isLoading = true;

                        C_RestorePokemon restorePacket = new C_RestorePokemon();
                        restorePacket.PlayerId = _scene.MyPlayer.Id;

                        Managers.Network.Send(restorePacket);
                    }
                }
                break;
            case PokemonCenterContentState.HEALING_MACHINE_ANIMATION:
                {
                    if (_recoveryMachine.CountBallAnimFinsh())
                    {
                        State = PokemonCenterContentState.THANK_YOU_SCRIPTING;

                        _recoveryMachine.DestroyMachineBall();

                        Animator anim = GetComponent<Animator>();
                        anim.Play("Nurse_Default");

                        List<string> scripts = new List<string>()
                        {
                            "Thank you for waiting.",
                            "We have restored your Pokemon to full health."
                        };
                        _scriptBox.BeginScriptTyping(scripts);
                    }
                }
                break;
            case PokemonCenterContentState.THANK_YOU_SCRIPTING:
                {
                    Animator anim = GetComponent<Animator>();
                    anim.Play("Nurse_ThankYou");

                    State = PokemonCenterContentState.NURSE_THANK_ANIMATION;
                }
                break;
            case PokemonCenterContentState.NURSE_THANK_ANIMATION:
                {
                    State = PokemonCenterContentState.GOOD_BYE_SCRIPTING;

                    List<string> scrtips = new List<string>()
                    {
                        "We hope to see you again!"
                    };
                    _scriptBox.BeginScriptTyping(scrtips);
                }
                break;
        }
    }
}
