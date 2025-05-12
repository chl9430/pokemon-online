using Google.Protobuf.Protocol;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;

public enum IntroSceneState
{
    INTRO_TALKING = 0,
    MOVING_PROF_IMG = 1,
    FADING_IN_NAME_UI = 2,
    ENTERING_NAME = 3,
    FADING_OUT_NAME_UI = 4,
    MOVING_BACK_PROF_IMG = 5,
    ASKING_GENDER = 6,
    FADING_OUT_PROF_IMG = 7,
    MOVING_GENDER_UI = 8,
    FADING_IN_GENDER_UI = 9,
    ENTERING_GENDER = 10,
    FADING_OUT_UNSELECTED_GENDER = 11,
    MOVING_SELECTED_GENDER = 12,
    LAST_TALKING = 99,
}

public class IntroScene : BaseScene
{
    int _curScriptIdx;
    string _playerName;
    PlayerGender _playerGender;
    IntroSceneState _sceneState = IntroSceneState.INTRO_TALKING;
    List<List<string>> _scripts;

    [SerializeField] ScriptBoxUI scriptBox;
    [SerializeField] MoveableUI _profAndImgZone;
    [SerializeField] FadeInOutUI _nameFIOInputField;
    [SerializeField] TMP_InputField _inputField;
    [SerializeField] FadeInOutUI _profFIOImg;
    [SerializeField] FadeInOutUI _genderFIOZone;
    [SerializeField] HorizontalSelectBoxUI _genderHSelectBox;
    [SerializeField] FadeInOutUI _maleImgBtn;
    [SerializeField] FadeInOutUI _femaleImgBtn;

    protected override void Init()
    {
        base.Init();

        {
            _scripts = new List<List<string>>();

            List<string> scripts1 = new List<string>()
            {
                "Hello! Welcome to the pokemon world!",
                "My name is professor Oak!",
                "What is your name?",
            };

            _scripts.Add(scripts1);

            List<string> scripts2 = new List<string>()
            {
                "Okay! Thank you for letting me know!",
                "Are you man or woman?",
            };

            _scripts.Add(scripts2);

            List<string> scripts3 = new List<string>()
            {
                "Okay! You are ",
                "There you go! I am pretty sure It is time to ready to go to the pokemon world!",
                "See you soon! I hope you will have a paid-off travel!"
            };

            _scripts.Add(scripts3);
        }
    }

    public override void AfterFadeInAction()
    {
        scriptBox.gameObject.SetActive(true);
        scriptBox.SetScript(_scripts[_curScriptIdx]);
        scriptBox.ShowText();
    }

    public override void DoNextAction()
    {
        switch (_sceneState)
        {
            case IntroSceneState.INTRO_TALKING:
                {
                    RectTransform rt = _profAndImgZone.GetComponent<RectTransform>();

                    Vector2 minDestPos = new Vector2(rt.anchorMin.x - 0.25f, rt.anchorMin.y);
                    Vector2 maxDestPos = new Vector2(rt.anchorMax.x - 0.25f, rt.anchorMax.y);

                    _profAndImgZone.SetOldAndDestPos(minDestPos, maxDestPos);
                    _sceneState = IntroSceneState.MOVING_PROF_IMG;
                }
                break;
            case IntroSceneState.MOVING_PROF_IMG:
                {
                    _nameFIOInputField.ChangeUIAlpha(1f);
                    _sceneState = IntroSceneState.FADING_IN_NAME_UI;
                }
                break;
            case IntroSceneState.FADING_IN_NAME_UI:
                {
                    _sceneState = IntroSceneState.ENTERING_NAME;
                }
                break;
            case IntroSceneState.FADING_OUT_NAME_UI:
                {
                    RectTransform rt = _profAndImgZone.GetComponent<RectTransform>();

                    Vector2 minDestPos = new Vector2(rt.anchorMin.x + 0.25f, rt.anchorMin.y);
                    Vector2 maxDestPos = new Vector2(rt.anchorMax.x + 0.25f, rt.anchorMax.y);

                    _profAndImgZone.SetOldAndDestPos(minDestPos, maxDestPos);
                    _sceneState = IntroSceneState.MOVING_BACK_PROF_IMG;
                }
                break;
            case IntroSceneState.MOVING_BACK_PROF_IMG:
                {
                    _curScriptIdx++;
                    scriptBox.SetScript(_scripts[_curScriptIdx]);
                    scriptBox.ShowText();
                    _sceneState = IntroSceneState.ASKING_GENDER;
                }
                break;
            case IntroSceneState.ASKING_GENDER:
                {
                    _profFIOImg.ChangeUIAlpha(0f);

                    _sceneState = IntroSceneState.FADING_OUT_PROF_IMG;
                }
                break;
            case IntroSceneState.FADING_OUT_PROF_IMG:
                {
                    RectTransform rt = _profAndImgZone.GetComponent<RectTransform>();

                    Vector2 minDestPos = new Vector2(rt.anchorMin.x - 0.5f, rt.anchorMin.y);
                    Vector2 maxDestPos = new Vector2(rt.anchorMax.x - 0.5f, rt.anchorMax.y);

                    _profAndImgZone.SetOldAndDestPos(minDestPos, maxDestPos);
                    _sceneState = IntroSceneState.MOVING_GENDER_UI;
                }
                break;
            case IntroSceneState.MOVING_GENDER_UI:
                {
                    _genderFIOZone.ChangeUIAlpha(1f);
                    _sceneState = IntroSceneState.FADING_IN_GENDER_UI;
                }
                break;
            case IntroSceneState.FADING_IN_GENDER_UI:
                {
                    _genderHSelectBox.UIState = HorizontalSelectBoxUIState.SELECTING;
                    _sceneState = IntroSceneState.ENTERING_GENDER;
                }
                break;
            case IntroSceneState.FADING_OUT_UNSELECTED_GENDER:
                {
                    if (_playerGender == PlayerGender.PlayerMale)
                    {
                        MoveableUI ui = _maleImgBtn.GetComponent<MoveableUI>();
                        RectTransform rt = _maleImgBtn.GetComponent<RectTransform>();

                        Vector2 minDestPos = new Vector2(rt.anchorMin.x + 0.5f, rt.anchorMin.y);
                        Vector2 maxDestPos = new Vector2(rt.anchorMax.x + 0.5f, rt.anchorMax.y);

                        ui.SetOldAndDestPos(minDestPos, maxDestPos);
                    }
                    else if (_playerGender == PlayerGender.PlayerFemale)
                    {
                        MoveableUI ui = _femaleImgBtn.GetComponent<MoveableUI>();
                        RectTransform rt = _femaleImgBtn.GetComponent<RectTransform>();

                        Vector2 minDestPos = new Vector2(rt.anchorMin.x - 0.5f, rt.anchorMin.y);
                        Vector2 maxDestPos = new Vector2(rt.anchorMax.x - 0.5f, rt.anchorMax.y);

                        ui.SetOldAndDestPos(minDestPos, maxDestPos);
                    }

                    _sceneState = IntroSceneState.MOVING_SELECTED_GENDER;
                }
                break;
            case IntroSceneState.MOVING_SELECTED_GENDER:
                {
                    _curScriptIdx++;
                    scriptBox.SetScript(_scripts[_curScriptIdx]);
                    scriptBox.ShowText();
                    _sceneState = IntroSceneState.LAST_TALKING;
                }
                break;
            case IntroSceneState.LAST_TALKING:
                {
                    C_CreatePlayer createPacket = new C_CreatePlayer();
                    createPacket.Name = _playerName;
                    createPacket.Gender = _playerGender;

                    Managers.Network.SavePacket(createPacket);

                    Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.Game);
                }
                break;
        }
    }

    public override void DoNextActionWithValue(object value)
    {
        switch (_sceneState)
        {
            case IntroSceneState.ENTERING_NAME:
                {
                    _playerName = (string)value;
                    _inputField.interactable = false;
                    _nameFIOInputField.ChangeUIAlpha(0f);

                    _sceneState = IntroSceneState.FADING_OUT_NAME_UI;

                    string textToAdd = $"{_scripts[_curScriptIdx + 1][0]} {_playerName}!";
                    _scripts[_curScriptIdx + 1][0] = textToAdd;
                }
                break;
            case IntroSceneState.ENTERING_GENDER:
                {
                    if ((string)value == "Male")
                        _playerGender = PlayerGender.PlayerMale;
                    else if ((string)value == "Female")
                        _playerGender = PlayerGender.PlayerFemale;

                    if (_playerGender == PlayerGender.PlayerMale)
                    {
                        _femaleImgBtn.ChangeUIAlpha(0f);
                    }
                    else if (_playerGender == PlayerGender.PlayerFemale)
                    {
                        _maleImgBtn.ChangeUIAlpha(0f);
                    }

                    _genderHSelectBox.HideAllArrow();
                    _sceneState = IntroSceneState.FADING_OUT_UNSELECTED_GENDER;

                    string textToAdd = $"{_scripts[_curScriptIdx + 1][0]}{(string)value}!";
                    _scripts[_curScriptIdx + 1][0] = textToAdd;
                }
                break;
        }
    }

    public override void Clear()
    {
    }
}
