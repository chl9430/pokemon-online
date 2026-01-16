using Google.Protobuf;
using Google.Protobuf.Protocol;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerContentState
{
    NONE = 0,

    // My Side
    RECEIVER_SAY_HELLO = 1,
    SELECTING_ACTION = 2,
    RECEIVER_BUSY_SCRIPTING = 3,
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
    PlayerContentState _state = PlayerContentState.NONE;

    [SerializeField] GameObject _screenEffecter;

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;

        if (packet is S_SendTalk)
        {
            S_SendTalk sendTalkPacket = packet as S_SendTalk;
            OtherPlayerInfo otherPlayer = sendTalkPacket.OtherPlayerInfo;

            Managers.Object.MyPlayerController.NPC = Managers.Object.FindById(otherPlayer.ObjectInfo.ObjectId).GetComponent<PlayerController>();
            Managers.Object.MyPlayerController.State = CreatureState.Talk;
            Managers.Object.MyPlayerController.IsLoading = false;

            if (otherPlayer.ObjectInfo.PosInfo.State == CreatureState.Idle)
            {
                List<string> scripts = new List<string>() {
                    $"Hello! How are you doing?",
                    $"......",
                    $"Alright! You are {Managers.Object.MyPlayerController.PlayerName}!",
                    $"Nice to meet you {Managers.Object.MyPlayerController.PlayerName}! I am {otherPlayer.PlayerName}.",
                    $"What do you want to do with me?"
                };
                _state = PlayerContentState.RECEIVER_SAY_HELLO;
                ContentManager.Instance.ScriptBox.gameObject.SetActive(true);
                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);
            }
            else if (otherPlayer.ObjectInfo.PosInfo.State == CreatureState.Fight)
            {
                List<string> scripts = new List<string>() {
                    $"It seems like {(otherPlayer.PlayerGender == PlayerGender.PlayerMale ? "He" : "She")} is busy!",
                    $"Let's try again later..."
                };
                _state = PlayerContentState.RECEIVER_BUSY_SCRIPTING;
                ContentManager.Instance.ScriptBox.gameObject.SetActive(true);
                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);
            }
        }
        else if (packet is S_ReceiveTalk)
        {
            S_ReceiveTalk receiveTalkPacket = packet as S_ReceiveTalk;
            OtherPlayerInfo otherPlayer = receiveTalkPacket.OtherPlayerInfo;

            Managers.Object.MyPlayerController.NPC = Managers.Object.FindById(otherPlayer.ObjectInfo.ObjectId).GetComponent<PlayerController>();
            Managers.Object.MyPlayerController.State = CreatureState.Talk;
            Managers.Object.MyPlayerController.IsLoading = false;

            List<string> scripts = new List<string>() {
                $"Hello! I am {otherPlayer.PlayerName}!",
                $"......",
                $"Nice to meet you {Managers.Object.MyPlayerController.PlayerName}!",
                $"I would like to do something with you. ",
                $"If you don't mind, Could you...",
            };
            ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);

            _state = PlayerContentState.SENDER_SAY_HELLO;

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
                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.SENDER_ASKING_BATTLE;
            }
            else if (talkRequestType == TalkRequestType.RequestExchange)
            {
                List<string> scripts = new List<string>()
                {
                    "exchange Pokemon with me?"
                };
                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.SENDER_ASKING_EXCHANGE;
            }
            else if (talkRequestType == TalkRequestType.AcceptBattle)
            {
                List<string> scripts = new List<string>()
                {
                    "Alright Let's do it!"
                };
                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts);

                _state = PlayerContentState.RECEIVER_ACCEPT_TO_BATTLE;
            }
            else if (talkRequestType == TalkRequestType.AcceptExchange)
            {
                List<string> scripts = new List<string>()
                {
                    "Alright Let's do it!"
                };
                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.RECEIVER_ACCEPT_TO_EXCHANGE;
            }
            else if (talkRequestType == TalkRequestType.Reject)
            {
                List<string> scripts = new List<string>()
                {
                    "I am sorry. I am busy right now. Please ask me again later."
                };
                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.RECEIVER_REJECT_ANSWER;
            }
            else if (talkRequestType == TalkRequestType.CancelTalk)
            {
                List<string> scripts = new List<string>()
                {
                    "I am sorry. I got a wrong people...!"
                };
                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);

                _state = PlayerContentState.SENDER_CANCEL_ACTION;
            }
        }
        else if (_packet is S_ReturnGame)
        {
            ContentManager.Instance.PlayScreenEffecter("FadeIn_NonBroading");

            IList<ObjectInfo> players = ((S_ReturnGame)packet).OtherPlayers;

            foreach (ObjectInfo player in players)
            {
                GameObject obj = Managers.Object.FindById(player.ObjectId);

                BaseController bc = obj.GetComponent<BaseController>();

                bc.CellPos = new Vector3Int(player.PosInfo.PosX, player.PosInfo.PosY);
                bc.Dir = player.PosInfo.MoveDir;
                bc.State = CreatureState.Idle;
            }

            FinishContent();
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
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btnNames, 1, 400, 100);
                }
                break;
            case PlayerContentState.SELECTING_ACTION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;
                            if (selectBox.GetSelectedBtnData() as string == "Battle")
                            {
                                ContentManager.Instance.ScriptBox.HideSelectBox();
                                if (!_isLoading)
                                {
                                    C_PlayerTalk talkPacket = new C_PlayerTalk();
                                    talkPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                    talkPacket.TalkRequestType = TalkRequestType.RequestBattle;

                                    Managers.Network.Send(talkPacket);
                                }
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "Exchange")
                            {
                                ContentManager.Instance.ScriptBox.HideSelectBox();
                                if (!_isLoading)
                                {
                                    C_PlayerTalk talkPacket = new C_PlayerTalk();
                                    talkPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                    talkPacket.TalkRequestType = TalkRequestType.RequestExchange;

                                    Managers.Network.Send(talkPacket);
                                }
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "Cancel")
                            {
                                C_PlayerTalk talkPacket = new C_PlayerTalk();
                                talkPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                talkPacket.TalkRequestType = TalkRequestType.CancelTalk;

                                Managers.Network.Send(talkPacket);

                                _state = PlayerContentState.RECEIVER_CANCEL_ANSWER;
                                List<string> scripts = new List<string>()
                                {
                                    "Oh... Alright...!"
                                };
                                ContentManager.Instance.ScriptBox.HideSelectBox();
                                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);
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
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btnNames, 1, 400, 100);
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
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btnNames, 1, 400, 100);
                }
                break;
            case PlayerContentState.SELECTING_ANSWER_TO_BATTLE:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;
                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                C_PlayerTalk talkPacket = new C_PlayerTalk();
                                talkPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                talkPacket.TalkRequestType = TalkRequestType.AcceptBattle;

                                Managers.Network.Send(talkPacket);

                                _state = PlayerContentState.RECEIVER_ACCEPT_TO_BATTLE;

                                List<string> scripts = new List<string>()
                                {
                                    "Thank you for accpeting! Let's begin!"
                                };
                                ContentManager.Instance.ScriptBox.HideSelectBox();
                                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts);
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                C_PlayerTalk talkPacket = new C_PlayerTalk();
                                talkPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                talkPacket.TalkRequestType = TalkRequestType.Reject;

                                Managers.Network.Send(talkPacket);

                                _state = PlayerContentState.SENDER_REJECTING_ACTION;
                                List<string> scripts = new List<string>()
                                {
                                    "It is okay! See ya!"
                                };
                                ContentManager.Instance.ScriptBox.HideSelectBox();
                                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);
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
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;
                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                C_PlayerTalk talkPacket = new C_PlayerTalk();
                                talkPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                talkPacket.TalkRequestType = TalkRequestType.AcceptExchange;

                                Managers.Network.Send(talkPacket);

                                _state = PlayerContentState.RECEIVER_ACCEPT_TO_EXCHANGE;

                                List<string> scripts = new List<string>()
                                {
                                    "Thank you for accpeting! Let's begin!"
                                };
                                ContentManager.Instance.ScriptBox.HideSelectBox();
                                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                C_PlayerTalk talkPacket = new C_PlayerTalk();
                                talkPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                talkPacket.TalkRequestType = TalkRequestType.Reject;

                                Managers.Network.Send(talkPacket);

                                _state = PlayerContentState.SENDER_REJECTING_ACTION;
                                List<string> scripts = new List<string>()
                                {
                                    "It is okay! See ya!"
                                };
                                ContentManager.Instance.ScriptBox.HideSelectBox();
                                ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);
                            }
                        }
                    }
                }
                break;
            case PlayerContentState.RECEIVER_BUSY_SCRIPTING:
                {
                    FinishContent();
                }
                break;
            case PlayerContentState.RECEIVER_ACCEPT_TO_BATTLE:
                {
                    C_EnterTrainerBattle enterBattle = new C_EnterTrainerBattle();
                    enterBattle.PlayerId = Managers.Object.MyPlayerController.Id;

                    Managers.Network.SavePacket(enterBattle);

                    ContentManager.Instance.PlayScreenEffecter("PokemonAppear");

                    _state = PlayerContentState.MOVING_TO_THE_BATTLE_SCENE;
                }
                break;
            case PlayerContentState.RECEIVER_ACCEPT_TO_EXCHANGE:
                {
                    C_EnterPokemonExchangeScene enterExchangePacket = new C_EnterPokemonExchangeScene();
                    enterExchangePacket.PlayerId = Managers.Object.MyPlayerController.Id;

                    Managers.Network.SavePacket(enterExchangePacket);

                    ContentManager.Instance.PlayScreenEffecter("FadeOut");

                    _state = PlayerContentState.MOVING_TO_THE_EXCHANGE_SCENE;
                }
                break;
            case PlayerContentState.RECEIVER_REJECT_ANSWER:
                {
                    FinishContent();
                }
                break;
            case PlayerContentState.SENDER_REJECTING_ACTION:
                {
                    FinishContent();
                }
                break;
            case PlayerContentState.RECEIVER_CANCEL_ANSWER:
                {
                    FinishContent();
                }
                break;
            case PlayerContentState.SENDER_CANCEL_ACTION:
                {
                    FinishContent();
                }
                break;
            case PlayerContentState.MOVING_TO_THE_BATTLE_SCENE:
                {
                    if (Managers.Network.Packet is C_EnterTrainerBattle)
                        Managers.Scene.AsyncLoadScene(Define.Scene.Battle, () => {
                            ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
                            Managers.Scene.CurrentScene = GameObject.FindFirstObjectByType<BattleScene>();
                        }, LoadSceneMode.Additive);
                }
                break;
            case PlayerContentState.MOVING_TO_THE_EXCHANGE_SCENE:
                {
                    // ¾À º¯°æ
                    Managers.Scene.AsyncLoadScene(Define.Scene.PokemonExchange, () => {
                        ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
                        Managers.Scene.CurrentScene = GameObject.FindFirstObjectByType<PokemonExchangeScene>();
                    }, LoadSceneMode.Additive);
                }
                break;
        }
    }

    public override void FinishContent()
    {
        _state = PlayerContentState.NONE;

        Managers.Scene.CurrentScene.FinishContents();

        Managers.Object.MyPlayerController.State = CreatureState.Idle;
        Managers.Object.MyPlayerController.NPC = null;

        ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
    }
}