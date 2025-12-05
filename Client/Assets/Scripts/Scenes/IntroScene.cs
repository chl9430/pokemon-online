using Google.Protobuf.Protocol;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public enum IntroSceneState
{
    NONE = 0,
    INTRO_TALKING = 1,
    SHOWING_NAME_UI = 2,
    INPUTING_NAME = 3,
    HIDING_NAME_UI = 4,
    ASKING_GENDER = 5,
    SHOWING_GENDER_UI = 6,
    INPUTING_GENDER = 7,
    HIDING_GENDER_UI = 8,
    LAST_TALKING = 9,
    MOVING_TO_GAME_SCENE = 10,
}

public class IntroScene : BaseScene
{
    string _playerName;
    int _selectedGenderBtnIdx;
    PlayableDirector _playableDirector;
    IntroSceneState _sceneState = IntroSceneState.NONE;
    [SerializeField] InputBox _inputBox;
    [SerializeField] Button _inputEnterBtn;
    [SerializeField] SelectArea _genderSelectArea;
    [SerializeField] List<DynamicButton> _genderSelectBtns;
    [SerializeField] Animator _anim;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Intro;
    }

    protected override void Start()
    {
        base.Start();

        _playableDirector = GetComponent<PlayableDirector>();

        // 성별 버튼 데이터 채우기
        for (int i = 0; i < _genderSelectBtns.Count; i++)
        {
            _genderSelectBtns[i].BtnData = (PlayerGender)i;
        }
        // _genderSelectArea.FillButtonGrid(1, _genderSelectBtns.Count, _genderSelectBtns);

        ContentManager.Instance.PlayScreenEffecter("BlackFadeIn");
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);
        switch (_sceneState)
        {
            case IntroSceneState.NONE:
                {
                    // 씬 상태 변경
                    _sceneState = IntroSceneState.INTRO_TALKING;
                    ActiveUIBySceneState(_sceneState);

                    List<string> scripts = new List<string>()
                    {
                        "Hello! Welcome to the pokemon world!",
                        "My name is professor Oak!",
                        "What is your name?",
                    };

                    ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts);
                }
                break;
            case IntroSceneState.INTRO_TALKING:
                {
                    _playableDirector.Play();

                    _sceneState = IntroSceneState.SHOWING_NAME_UI;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case IntroSceneState.SHOWING_NAME_UI:
                {
                    _playableDirector.Pause();

                    _sceneState = IntroSceneState.INPUTING_NAME;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case IntroSceneState.INPUTING_NAME:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            _playableDirector.Resume();

                            _sceneState = IntroSceneState.HIDING_NAME_UI;
                            ActiveUIBySceneState(_sceneState);
                        }
                    }
                    else
                    {
                        _playerName = value as string;
                    }
                }
                break;
            case IntroSceneState.HIDING_NAME_UI:
                {
                    _playableDirector.Pause();

                    _sceneState = IntroSceneState.ASKING_GENDER;
                    ActiveUIBySceneState(_sceneState);

                    List<string> scripts = new List<string>()
                    {
                        $"{_playerName}!",
                        "Okay! Thank you for letting me know!",
                        "Are you man or woman?",
                    };

                    ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts);
                }
                break;
            case IntroSceneState.ASKING_GENDER:
                {
                    _playableDirector.Resume();

                    _sceneState = IntroSceneState.SHOWING_GENDER_UI;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case IntroSceneState.SHOWING_GENDER_UI:
                {
                    _playableDirector.Pause();

                    _sceneState = IntroSceneState.INPUTING_GENDER;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case IntroSceneState.INPUTING_GENDER:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            _playableDirector.Resume();

                            // gender ui 애니메이션 실행
                            if (_selectedGenderBtnIdx == 0)
                                _anim.Play("Male_Move");
                            else if (_selectedGenderBtnIdx == 1)
                                _anim.Play("Female_Move");

                            _sceneState = IntroSceneState.HIDING_GENDER_UI;
                            ActiveUIBySceneState(_sceneState);
                        }
                    }
                    else
                    {
                        _selectedGenderBtnIdx = (int)value;
                    }
                }
                break;
            case IntroSceneState.HIDING_GENDER_UI:
                {
                    string callName = (PlayerGender)_genderSelectBtns[_selectedGenderBtnIdx].BtnData == PlayerGender.PlayerMale ? "Male" : "Female";

                    List<string> scripts = new List<string>()
                    {
                        $"Okay! You are {callName}!",
                        "There you go! I am pretty sure It is time to ready to go to the pokemon world!",
                        "See you soon! I hope you will have a paid-off travel!"
                    };

                    ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts);

                    _sceneState = IntroSceneState.LAST_TALKING;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case IntroSceneState.LAST_TALKING:
                {
                    C_CreatePlayer createPacket = new C_CreatePlayer();
                    createPacket.Name = _playerName;
                    createPacket.Gender = (PlayerGender)_genderSelectBtns[_selectedGenderBtnIdx].BtnData;

                    Managers.Network.SavePacket(createPacket);

                    ContentManager.Instance.PlayScreenEffecter("BlackFadeOut");

                    _sceneState = IntroSceneState.MOVING_TO_GAME_SCENE;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case IntroSceneState.MOVING_TO_GAME_SCENE:
                {
                    // 씬 변경
                    //Managers.Scene.LoadScene(Define.Scene.Game);
                }
                break;
        }
    }

    void ActiveUIBySceneState(IntroSceneState state)
    {
        if (state == IntroSceneState.INTRO_TALKING)
        {
            ContentManager.Instance.ScriptBox.gameObject.SetActive(true);
        }

        if (state == IntroSceneState.SHOWING_NAME_UI)
        {
            _inputBox.gameObject.SetActive(true);
        }
        else if (state == IntroSceneState.INPUTING_NAME)
        {
            _inputBox.SetFieldInteractable(true);
        }
        else if (state == IntroSceneState.HIDING_NAME_UI)
        {
            _inputEnterBtn.interactable = false;
            _inputBox.SetFieldInteractable(false);
        }
        else
        {
            _inputBox.gameObject.SetActive(false);
            _inputBox.SetFieldInteractable(false);
        }

        if (state == IntroSceneState.SHOWING_GENDER_UI)
        {
            _genderSelectArea.gameObject.SetActive(true);
        }
        else if (state == IntroSceneState.INPUTING_GENDER)
        {
            _genderSelectArea.UIState = SelectAreaState.SELECTING;
        }
        else if (state == IntroSceneState.HIDING_GENDER_UI)
        {
            _genderSelectArea.UIState = SelectAreaState.NONE;
        }
    }

    public override void Clear()
    {
    }
}
