using Google.Protobuf;
using Google.Protobuf.Protocol;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class LogInScene : BaseScene
{
    enum LogInSceneState
    {
        None = 0,
        InputingLogInInfo = 1,
        IncorrectIdScripting = 2,
        IncorrectPasswordScripting = 3,
        SuccessLogInScripting = 4,
        AnsweringToMakeAccount = 5,
        FailedMakingAccountScripting = 6,
        SuccessMakingAccountScripting = 7,
        AskingNewGame = 8,
        AnsweringNewGame = 9,
        AskingLoadData = 10,
        AnsweringLoadData = 11,
        AskingDeleteData = 12,
        AnsweringDeleteData = 13,
        StartNewGameScripting = 14,
        EnterCorrectInfoScripting = 17,
    }

    LogInSceneState _state = LogInSceneState.None;
    [SerializeField] InputFieldAction _loginAction;

    LogInSceneState State
    {
        set
        {
            _state = value;

            if (_state == LogInSceneState.InputingLogInInfo)
            {
                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                _loginAction.State = InputFieldState.Inputing;
            }
        }
    }

    protected override void Init()
    {
        base.Init();

        Managers.Scene.CurrentScene = this;

        Screen.SetResolution(1280, 720, false);
    }

    protected override void Start()
    {
        base.Start();

        State = LogInSceneState.InputingLogInInfo;
    }

    public override void UpdateData(IMessage packet)
    {
        _loadingPacket = false;
        _packet = packet;

        if (_packet is S_LogIn)
        {
            LogInResultType result = ((S_LogIn)_packet).LogInResult;

            switch (result)
            {
                case LogInResultType.Success:
                    {
                        Managers.Object._myAccountId = _loginAction.GetTextByIndex(0);

                        List<string> scripts = new List<string>()
                        {
                            "You have successfully logged in!"
                        };
                        ContentManager.Instance.BeginScriptTyping(scripts, true);

                        State = LogInSceneState.SuccessLogInScripting;
                    }
                    break;
                case LogInResultType.IdError:
                    {
                        List<string> scripts = new List<string>()
                        {
                            "The ID does not match. Do you want to create a new account with that information?"
                        };
                        ContentManager.Instance.BeginScriptTyping(scripts, true);

                        State = LogInSceneState.IncorrectIdScripting;
                    }
                    break;
                case LogInResultType.PassswordError:
                    {
                        List<string> scripts = new List<string>()
                        {
                            "Password doesn't match, please check the password."
                        };
                        ContentManager.Instance.BeginScriptTyping(scripts, true);

                        State = LogInSceneState.IncorrectPasswordScripting;
                    }
                    break;
            }
        }
        else if (_packet is S_CreateAccount)
        {
            bool isSuccess = ((S_CreateAccount)_packet).IsSuccess;

            if (isSuccess)
            {
                List<string> scripts = new List<string>()
                {
                    "The account was successfully created!"
                };
                ContentManager.Instance.BeginScriptTyping(scripts, true);

                State = LogInSceneState.SuccessMakingAccountScripting;
            }
            else
            {
                List<string> scripts = new List<string>()
                {
                    "Account creation failed, please try again."
                };
                ContentManager.Instance.BeginScriptTyping(scripts, true);

                State = LogInSceneState.FailedMakingAccountScripting;
            }
        }
        else if (_packet is S_CheckSaveData)
        {
            bool isDataFound = ((S_CheckSaveData)_packet).IsDataFound;

            if (isDataFound)
            {
                List<string> scripts = new List<string>()
                {
                    "There is an existing saved data. Do you want to go on and start the game?"
                };
                ContentManager.Instance.BeginScriptTyping(scripts, true);

                State = LogInSceneState.AskingLoadData;
            }
            else
            {
                List<string> scripts = new List<string>()
                {
                    "There is no existing saved data. Do you want to start a new game?"
                };
                ContentManager.Instance.BeginScriptTyping(scripts, true);

                State = LogInSceneState.AskingNewGame;
            }
        }
    }

    public override void DoNextAction(object value = null)
    {
        switch (_state)
        {
            case LogInSceneState.InputingLogInInfo:
                {
                    string id = _loginAction.GetTextByIndex(0);
                    string pw = _loginAction.GetTextByIndex(1);

                    if (id == "" || pw == "")
                    {
                        List<string> scripts = new List<string>()
                        {
                            "Please enter the correct ID and password."
                        };
                        ContentManager.Instance.BeginScriptTyping(scripts, true);

                        State = LogInSceneState.EnterCorrectInfoScripting;
                    }
                    else
                    {
                        if (!_loadingPacket)
                        {
                            _loadingPacket = true;

                            C_LogIn logInPacket = new C_LogIn();
                            logInPacket.Id = id;
                            logInPacket.Password = pw;

                            Managers.Network.Send(logInPacket);
                        }
                        ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Please wait...");
                    }
                }
                break;
            case LogInSceneState.IncorrectIdScripting:
                {
                    State = LogInSceneState.AnsweringToMakeAccount;

                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btns, 1, 400, 100);
                }
                break;
            case LogInSceneState.IncorrectPasswordScripting:
                {
                    State = LogInSceneState.InputingLogInInfo;
                }
                break;
            case LogInSceneState.SuccessLogInScripting:
                {
                    // 저장 데이터 확인
                    if (!_loadingPacket)
                    {
                        _loadingPacket = true;

                        C_CheckSaveData checkDataPacket = new C_CheckSaveData();
                        checkDataPacket.Id = _loginAction.GetTextByIndex(0);

                        Managers.Network.Send(checkDataPacket);
                    }
                    ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Checking data...");
                }
                break;
            case LogInSceneState.AnsweringToMakeAccount:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                if (!_loadingPacket)
                                {
                                    _loadingPacket = true;

                                    C_CreateAccount createAccountPacket = new C_CreateAccount();
                                    createAccountPacket.Id = _loginAction.GetTextByIndex(0);
                                    createAccountPacket.Password = _loginAction.GetTextByIndex(0);

                                    Managers.Network.Send(createAccountPacket);
                                }
                                ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Please wait...");
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                State = LogInSceneState.InputingLogInInfo;
                            }
                        }
                    }
                }
                break;
            case LogInSceneState.FailedMakingAccountScripting:
                {
                    State = LogInSceneState.InputingLogInInfo;
                }
                break;
            case LogInSceneState.SuccessMakingAccountScripting:
                {
                    // 저장 데이터 확인
                    if (!_loadingPacket)
                    {
                        _loadingPacket = true;

                        C_CheckSaveData checkDataPacket = new C_CheckSaveData();
                        checkDataPacket.Id = _loginAction.GetTextByIndex(0);

                        Managers.Network.Send(checkDataPacket);
                    }
                    ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Checking data...");
                }
                break;
            case LogInSceneState.AskingLoadData:
                {
                    State = LogInSceneState.AnsweringLoadData;

                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btns, 1, 400, 100);
                }
                break;
            case LogInSceneState.AnsweringLoadData:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                C_LoadGameData loadGamePacket = new C_LoadGameData();
                                loadGamePacket.AccountId = Managers.Object._myAccountId;

                                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                                ContentManager.Instance.FadeOutSceneToMove(Define.Scene.Game, "FadeOut", loadGamePacket);

                                State = LogInSceneState.None;
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                List<string> scripts = new List<string>()
                                {
                                    "Do you want to erase the existing saved data and start a new game?"
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts, true);

                                State = LogInSceneState.AskingDeleteData;
                            }
                        }
                    }
                }
                break;
            case LogInSceneState.AskingDeleteData:
                {
                    State = LogInSceneState.AnsweringDeleteData;

                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btns, 1, 400, 100);
                }
                break;
            case LogInSceneState.AnsweringDeleteData:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                // 게임 새로 시작
                                List<string> scripts = new List<string>()
                                {
                                    "Create new game data and start a new game."
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts, true);

                                State = LogInSceneState.StartNewGameScripting;
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                State = LogInSceneState.InputingLogInInfo;
                            }
                        }
                    }
                }
                break;
            case LogInSceneState.AskingNewGame:
                {
                    State = LogInSceneState.AnsweringNewGame;

                    List<string> btns = new List<string>()
                    {
                        "Yes",
                        "No"
                    };
                    ContentManager.Instance.ScriptBox.CreateSelectBox(btns, 1, 400, 100);
                }
                break;
            case LogInSceneState.AnsweringNewGame:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                // 게임 새로 시작
                                List<string> scripts = new List<string>()
                                {
                                    "Create new game data and start a new game."
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts, true);

                                State = LogInSceneState.StartNewGameScripting;
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                State = LogInSceneState.InputingLogInInfo;
                            }
                        }
                    }
                }
                break;
            case LogInSceneState.StartNewGameScripting:
                {
                    if (_packet is S_CheckSaveData)
                    {
                        string foundDataId = ((S_CheckSaveData)_packet).FoundDataId;

                        Managers.Object._myAccountId = foundDataId;

                        ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                        ContentManager.Instance.FadeOutSceneToMove(Define.Scene.Intro, "FadeOut");

                        State = LogInSceneState.None;
                    }
                }
                break;
            case LogInSceneState.EnterCorrectInfoScripting:
                {
                    State = LogInSceneState.InputingLogInInfo;
                }
                break;
        }
    }

    public override void Clear()
    {
    }
}
