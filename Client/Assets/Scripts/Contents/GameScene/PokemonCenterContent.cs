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
    [SerializeField] RecoveryMachine _recoveryMachine;

    PokemonCenterContentState _state = PokemonCenterContentState.NONE;

    public PokemonCenterContentState State
    {
        set
        {
            _state = value;

            if (_state == PokemonCenterContentState.GREETING_SCRIPTING)
            {
                ContentManager.Instance.ScriptBox.gameObject.SetActive(true);
            }
            else if (_state == PokemonCenterContentState.SELECTING_ACTION)
            {
                ContentManager.Instance.ScriptBox.gameObject.SetActive(true);
            }
            else if (_state == PokemonCenterContentState.GOOD_BYE_SCRIPTING)
            {
                ContentManager.Instance.ScriptBox.gameObject.SetActive(true);
            }
            else if (_state == PokemonCenterContentState.TAKING_POKEMON_SCRIPTING)
            {
                ContentManager.Instance.ScriptBox.gameObject.SetActive(true);
            }
            else if (_state == PokemonCenterContentState.NONE)
            {
                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
            }
        }
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;

        if (_packet is S_GetNpcTalk)
        {
            Managers.Object.MyPlayerController.State = CreatureState.Shopping;
            Managers.Object.MyPlayerController.IsLoading = false;
            State = PokemonCenterContentState.GREETING_SCRIPTING;

            List<string> scripts = new List<string>()
            {
                "Hello, and welcome to the Pokemon Center.",
                "We restore your tired Pokemon to full health.",
                "Would you like to rest your Pokemon?",
            };
            ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts);

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
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btns, 1, 400, 100);
                }
                break;
            case PokemonCenterContentState.SELECTING_ACTION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                State = PokemonCenterContentState.TAKING_POKEMON_SCRIPTING;

                                List<string> scripts = new List<string>()
                                {
                                    "Okay, I will take your Pokemon for a few seconds.",
                                };
                                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts);
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                List<string> scrtips = new List<string>()
                                {
                                    "We hope to see you again!"
                                };
                                ContentManager.Instance.ScriptBox.BeginScriptTyping(scrtips);
                                State = PokemonCenterContentState.GOOD_BYE_SCRIPTING;
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            List<string> scrtips = new List<string>()
                            {
                                "We hope to see you again!"
                            };
                            ContentManager.Instance.ScriptBox.BeginScriptTyping(scrtips);
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
                        finishTalk.PlayerId = Managers.Object.MyPlayerController.Id;

                        Managers.Network.Send(finishTalk);
                    }

                    FinishContent();
                }
                break;
            case PokemonCenterContentState.TAKING_POKEMON_SCRIPTING:
                {
                    CreatureController controller = GetComponent<CreatureController>();
                    controller.Dir = MoveDir.Left;

                    State = PokemonCenterContentState.NURSE_TURNING_LEFT;
                }
                break;
            case PokemonCenterContentState.NURSE_TURNING_LEFT:
                {
                    if (!_isLoading)
                    {
                        _isLoading = true;

                        C_RestorePokemon restorePacket = new C_RestorePokemon();
                        restorePacket.PlayerId = Managers.Object.MyPlayerController.Id;

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

                        CreatureController controller = GetComponent<CreatureController>();
                        controller.Dir = MoveDir.Down;

                        List<string> scripts = new List<string>()
                        {
                            "Thank you for waiting.",
                            "We have restored your Pokemon to full health."
                        };
                        ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts);
                    }
                }
                break;
            case PokemonCenterContentState.THANK_YOU_SCRIPTING:
                {
                    CreatureController controller = GetComponent<CreatureController>();
                    controller.State = CreatureState.NurseThankYou;

                    State = PokemonCenterContentState.NURSE_THANK_ANIMATION;
                }
                break;
            case PokemonCenterContentState.NURSE_THANK_ANIMATION:
                {
                    CreatureController controller = GetComponent<CreatureController>();
                    controller.State = CreatureState.Idle;

                    State = PokemonCenterContentState.GOOD_BYE_SCRIPTING;

                    List<string> scrtips = new List<string>()
                    {
                        "We hope to see you again!"
                    };
                    ContentManager.Instance.ScriptBox.BeginScriptTyping(scrtips);
                }
                break;
        }
    }

    public override void FinishContent()
    {
        State = PokemonCenterContentState.NONE;

        Managers.Scene.CurrentScene.FinishContents(true);

        Managers.Object.MyPlayerController.State = CreatureState.Idle;
    }
}
