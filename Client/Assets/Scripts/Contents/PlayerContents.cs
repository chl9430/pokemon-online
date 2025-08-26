using Google.Protobuf;
using Google.Protobuf.Protocol;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerContentState
{
    NONE = 0,

    // My Side
    RECEIVER_SAY_HELLO = 1,
    SELECTING_ACTION = 2,
    RECEIVER_ACCEPT_TO_BATTLE = 4,
    RECEIVER_ACCEPT_TO_EXCHANGE = 5,
    RECEIVER_REJECT_ANSWER = 6,
    RECEIVER_CANCEL_ANSWER = 7,


    // Receiver Side
    SENDER_SAY_HELLO = 11,
    SENDER_ASKING_BATTLE = 12,
    SENDER_ASKING_EXCHANGE = 13,
    SELECTING_ANSWER_TO_BATTLE = 15,
    SELECTING_ANSWER_TO_EXCHANGE = 16,
    SENDER_REJECTING_ACTION = 17,
    SENDER_CANCEL_ACTION = 18,

    MOVING_TO_THE_BATTLE_SCENE = 30,
    MOVING_TO_THE_EXCHANGE_SCENE = 31,
}

public class PlayerContents : ObjectContents
{
    bool _isLoading;
    ScriptBoxUI _scriptBox;
    BaseScene _scene;
    PlayerContentState _state = PlayerContentState.NONE;

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
        _scriptBox = GameObject.FindAnyObjectByType<ScriptBoxUI>(FindObjectsInactive.Include);
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;

        if (packet is S_SendTalk)
        {
            S_SendTalk sendTalkPacket = packet as S_SendTalk;
            PlayerInfo otherPlayer = sendTalkPacket.OtherPlayerInfo;

            if (otherPlayer == null)
                return;
            else
                Managers.Object.MyPlayer.State = CreatureState.Talk;

            if (otherPlayer.ObjectInfo.PosInfo.State == CreatureState.Idle)
            {
                List<string> scripts = new List<string>() {
                    $"Hello! How are you doing?",
                    $"......",
                    $"Alright! You are {Managers.Object.PlayerInfo.PlayerName}!",
                    $"Nice to meet you {Managers.Object.PlayerInfo.PlayerName}! I am {otherPlayer.PlayerName}.",
                    $"What do you want to do with me?"
                };
                _state = PlayerContentState.RECEIVER_SAY_HELLO;
                _scriptBox.gameObject.SetActive(true);
                _scriptBox.BeginScriptTyping(scripts, true);
            }
            else if (otherPlayer.ObjectInfo.PosInfo.State == CreatureState.Fight)
            {
                List<string> scripts = new List<string>() {
                    $"It seems like {(otherPlayer.PlayerGender == PlayerGender.PlayerMale ? "He" : "She")} is fighting!",
                    $"Let's try again later..."
                };
                _scriptBox.gameObject.SetActive(true);
                _scriptBox.BeginScriptTyping(scripts, true);
            }
            else if (otherPlayer.ObjectInfo.PosInfo.State == CreatureState.WatchMenu)
            {
                List<string> scripts = new List<string>() {
                    $"It seems like {(otherPlayer.PlayerGender == PlayerGender.PlayerMale ? "He" : "She")} is in personal business...",
                    $"Let's try again later..."
                };
                _scriptBox.gameObject.SetActive(true);
                _scriptBox.BeginScriptTyping(scripts, true);
            }
        }
        else if (packet is S_ReceiveTalk)
        {
            S_ReceiveTalk receiveTalkPacket = packet as S_ReceiveTalk;
            PlayerInfo otherPlayer = receiveTalkPacket.PlayerInfo;

            if (otherPlayer == null)
                return;
            else
                Managers.Object.MyPlayer.State = CreatureState.Talk;

            List<string> scripts = new List<string>() {
                $"Hello! I am {otherPlayer.PlayerName}!",
                $"......",
                $"Nice to meet you {Managers.Object.PlayerInfo.PlayerName}!",
                $"I would like to do something with you. ",
                $"If you don't mind, Could you...",
            };
            _state = PlayerContentState.SENDER_SAY_HELLO;

            _scriptBox.gameObject.SetActive(true);
            _scriptBox.BeginScriptTyping(scripts, true);
        }
        else if (packet is S_SendTalkRequest)
        {
            S_SendTalkRequest senderTalkPacket = packet as S_SendTalkRequest;
            TalkRequestType talkRequestType = senderTalkPacket.TalkRequestType;

            if (talkRequestType == TalkRequestType.RequestBattle)
            {
                List<string> scripts = new List<string>()
                {
                    "play a Pokemon battle with me?"
                };
                _scriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.SENDER_ASKING_BATTLE;
            }
            else if (talkRequestType == TalkRequestType.RequestExchange)
            {
                List<string> scripts = new List<string>()
                {
                    "exchange Pokemon with me?"
                };
                _scriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.SENDER_ASKING_EXCHANGE;
            }
            else if (talkRequestType == TalkRequestType.AcceptBattle)
            {
                List<string> scripts = new List<string>()
                {
                    "Alright Let's do it!"
                };
                _scriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.RECEIVER_ACCEPT_TO_BATTLE;
            }
            else if (talkRequestType == TalkRequestType.AcceptExchange)
            {
                List<string> scripts = new List<string>()
                {
                    "Alright Let's do it!"
                };
                _scriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.RECEIVER_ACCEPT_TO_EXCHANGE;
            }
            else if (talkRequestType == TalkRequestType.Reject)
            {
                List<string> scripts = new List<string>()
                {
                    "I am sorry. I am busy right now. Please ask me again later."
                };
                _scriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.RECEIVER_REJECT_ANSWER;
            }
            else if (talkRequestType == TalkRequestType.CancelTalk)
            {
                List<string> scripts = new List<string>()
                {
                    "I am sorry. I got a wrong people...!"
                };
                _scriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.SENDER_CANCEL_ACTION;
            }
        }
    }

    public override void SetNextAction(object value = null)
    {
        switch(_state)
        {
            case PlayerContentState.RECEIVER_SAY_HELLO:
                {
                    List<string> btnNames = new List<string>()
                    {
                        "Battle",
                        "Exchange",
                        "Cancel"
                    };
                    _state = PlayerContentState.SELECTING_ACTION;
                    _scriptBox.CreateSelectBox(btnNames, 3, 1, 400, 100);
                }
                break;
            case PlayerContentState.SELECTING_ACTION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;
                            if (selectBox.GetSelectedBtnData() as string == "Battle")
                            {
                                _scriptBox.HideSelectBox();
                                if (!_isLoading)
                                {
                                    C_PlayerTalk talkPacket = new C_PlayerTalk();
                                    talkPacket.PlayerId = Managers.Object.MyPlayer.Id;
                                    talkPacket.TalkRequestType = TalkRequestType.RequestBattle;

                                    Managers.Network.Send(talkPacket);
                                }
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "Exchange")
                            {
                                _scriptBox.HideSelectBox();
                                if (!_isLoading)
                                {
                                    C_PlayerTalk talkPacket = new C_PlayerTalk();
                                    talkPacket.PlayerId = Managers.Object.MyPlayer.Id;
                                    talkPacket.TalkRequestType = TalkRequestType.RequestExchange;

                                    Managers.Network.Send(talkPacket);
                                }
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "Cancel")
                            {
                                C_PlayerTalk talkPacket = new C_PlayerTalk();
                                talkPacket.PlayerId = Managers.Object.MyPlayer.Id;
                                talkPacket.TalkRequestType = TalkRequestType.CancelTalk;

                                Managers.Network.Send(talkPacket);

                                _state = PlayerContentState.RECEIVER_CANCEL_ANSWER;
                                List<string> scripts = new List<string>()
                                {
                                    "Oh... Alright...!"
                                };
                                _scriptBox.HideSelectBox();
                                _scriptBox.BeginScriptTyping(scripts, true);
                            }
                        }
                    }
                }
                break;
            case PlayerContentState.SENDER_ASKING_BATTLE:
                {
                    List<string> btnNames = new List<string>()
                    {
                        "Yes",
                        "No",
                    };
                    _state = PlayerContentState.SELECTING_ANSWER_TO_BATTLE;
                    _scriptBox.CreateSelectBox(btnNames, 2, 1, 400, 100);
                }
                break;
            case PlayerContentState.SENDER_ASKING_EXCHANGE:
                {
                    List<string> btnNames = new List<string>()
                    {
                        "Yes",
                        "No",
                    };
                    _state = PlayerContentState.SELECTING_ANSWER_TO_EXCHANGE;
                    _scriptBox.CreateSelectBox(btnNames, 2, 1, 400, 100);
                }
                break;
            case PlayerContentState.SELECTING_ANSWER_TO_BATTLE:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;
                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                C_PlayerTalk talkPacket = new C_PlayerTalk();
                                talkPacket.PlayerId = Managers.Object.MyPlayer.Id;
                                talkPacket.TalkRequestType = TalkRequestType.AcceptBattle;

                                Managers.Network.Send(talkPacket);

                                _state = PlayerContentState.RECEIVER_ACCEPT_TO_BATTLE;

                                List<string> scripts = new List<string>()
                                {
                                    "Thank you for accpeting! Let's begin!"
                                };
                                _scriptBox.HideSelectBox();
                                _scriptBox.BeginScriptTyping(scripts, true);
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                C_PlayerTalk talkPacket = new C_PlayerTalk();
                                talkPacket.PlayerId = Managers.Object.MyPlayer.Id;
                                talkPacket.TalkRequestType = TalkRequestType.Reject;

                                Managers.Network.Send(talkPacket);

                                _state = PlayerContentState.SENDER_REJECTING_ACTION;
                                List<string> scripts = new List<string>()
                                {
                                    "It is okay! See ya!"
                                };
                                _scriptBox.HideSelectBox();
                                _scriptBox.BeginScriptTyping(scripts, true);
                            }
                        }
                    }
                }
                break;
            case PlayerContentState.SELECTING_ANSWER_TO_EXCHANGE:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;
                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                C_PlayerTalk talkPacket = new C_PlayerTalk();
                                talkPacket.PlayerId = Managers.Object.MyPlayer.Id;
                                talkPacket.TalkRequestType = TalkRequestType.AcceptExchange;

                                Managers.Network.Send(talkPacket);

                                _state = PlayerContentState.RECEIVER_ACCEPT_TO_EXCHANGE;

                                List<string> scripts = new List<string>()
                                {
                                    "Thank you for accpeting! Let's begin!"
                                };
                                _scriptBox.HideSelectBox();
                                _scriptBox.BeginScriptTyping(scripts, true);
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                C_PlayerTalk talkPacket = new C_PlayerTalk();
                                talkPacket.PlayerId = Managers.Object.MyPlayer.Id;
                                talkPacket.TalkRequestType = TalkRequestType.Reject;

                                Managers.Network.Send(talkPacket);

                                _state = PlayerContentState.SENDER_REJECTING_ACTION;
                                List<string> scripts = new List<string>()
                                {
                                    "It is okay! See ya!"
                                };
                                _scriptBox.HideSelectBox();
                                _scriptBox.BeginScriptTyping(scripts, true);
                            }
                        }
                    }
                }
                break;
            case PlayerContentState.RECEIVER_ACCEPT_TO_BATTLE:
                {
                    ScreenEffecter effecter = Managers.Resource.Load<ScreenEffecter>("Prefabs/UI/GameScene/PokemonAppearEffect");
                    _scene.ScreenEffecter = GameObject.Instantiate(effecter, _scene.ScreenEffecterZone);

                    _scene.ScreenEffecter.PlayEffect("PokemonAppear");

                    _state = PlayerContentState.MOVING_TO_THE_BATTLE_SCENE;
                }
                break;
            case PlayerContentState.RECEIVER_ACCEPT_TO_EXCHANGE:
                {
                    C_EnterPokemonExchangeScene enterExchangePacket = new C_EnterPokemonExchangeScene();
                    enterExchangePacket.PlayerId = Managers.Object.PlayerInfo.ObjectInfo.ObjectId;

                    Managers.Network.SavePacket(enterExchangePacket);

                    _scene.ScreenEffecter.PlayEffect("FadeOut");

                    _state = PlayerContentState.MOVING_TO_THE_EXCHANGE_SCENE;
                }
                break;
            case PlayerContentState.RECEIVER_REJECT_ANSWER:
                {
                    _state = PlayerContentState.NONE;
                    _scene.FinishContents();
                    _scriptBox.gameObject.SetActive(false);
                    Managers.Object.MyPlayer.State = CreatureState.Idle;
                }
                break;
            case PlayerContentState.SENDER_REJECTING_ACTION:
                {
                    _state = PlayerContentState.NONE;
                    _scene.FinishContents();
                    _scriptBox.gameObject.SetActive(false);
                    Managers.Object.MyPlayer.State = CreatureState.Idle;
                }
                break;
            case PlayerContentState.RECEIVER_CANCEL_ANSWER:
                {
                    _state = PlayerContentState.NONE;
                    _scene.FinishContents();
                    _scriptBox.gameObject.SetActive(false);
                    Managers.Object.MyPlayer.State = CreatureState.Idle;
                }
                break;
            case PlayerContentState.SENDER_CANCEL_ACTION:
                {
                    _state = PlayerContentState.NONE;
                    _scene.FinishContents();
                    _scriptBox.gameObject.SetActive(false);
                    Managers.Object.MyPlayer.State = CreatureState.Idle;
                }
                break;
            case PlayerContentState.MOVING_TO_THE_BATTLE_SCENE:
                {
                    Debug.Log("Battle Begin");
                }
                break;
            case PlayerContentState.MOVING_TO_THE_EXCHANGE_SCENE:
                {
                    // ¾À º¯°æ
                    Managers.Scene.LoadScene(Define.Scene.PokemonExchange);
                }
                break;
        }
    }
}