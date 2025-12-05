using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum ScriptBoxUIState
{
    TEXT_TYPING = 0,
    WAITING_NEXT_SENTENCE = 1,
    NONE = 2,
}

public class ScriptBoxUI : MonoBehaviour
{
    ScriptBoxUIState _uiState = ScriptBoxUIState.NONE;
    int _curScriptIdx;
    string _sentence;
    bool _isStatic;
    bool _autoSkip;
    float _autoSkipTime;
    List<string> _scripts;

    TextMeshProUGUI _tmp;
    Image _nextBtn;

    [SerializeField] float _typeSpeed = 0.05f;
    [SerializeField] GridLayoutSelectBox _selectBox;

    public GridLayoutSelectBox ScriptSelectBox { get { return _selectBox; } }

    void Start()
    {
        LoadComponent();

        _nextBtn.gameObject.SetActive(false);
    }

    void LoadComponent()
    {
        if (_tmp == null)
            _tmp = Util.FindChild<TextMeshProUGUI>(gameObject, "ScriptBoxText", true);

        if (_nextBtn == null)
            _nextBtn = Util.FindChild<Image>(gameObject, "NextButton", true);
    }

    void Update()
    {
        switch (_uiState)
        {
            case ScriptBoxUIState.TEXT_TYPING:
                ShowFullSentence();
                break;
            case ScriptBoxUIState.WAITING_NEXT_SENTENCE:
                WaitToTheNextSentence();
                break;
        }
    }

    public void CreateSelectBox(List<string> btnNames, int col, int btnWidth, int btnHeight)
    {
        _selectBox.CreateButtons(btnNames, col, btnWidth, btnHeight);

        _selectBox.gameObject.SetActive(true);
        _selectBox.UIState = GridLayoutSelectBoxState.SELECTING;
    }

    public void HideSelectBox()
    {
        _selectBox.gameObject.SetActive(false);
        _selectBox.UIState = GridLayoutSelectBoxState.NONE;
    }

    public void SetScriptWihtoutTyping(string script)
    {
        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        LoadComponent();

        _tmp.text = script;
    }

    public void BeginScriptTyping(List<string> scripts, bool autoSkip = false, float autoSkipTime = 1f, bool isStatic = false)
    {
        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        LoadComponent();

        _uiState = ScriptBoxUIState.TEXT_TYPING;
        _scripts = scripts;
        _autoSkipTime = autoSkipTime;
        _isStatic = isStatic;
        _autoSkip = autoSkip;
        _sentence = _scripts[_curScriptIdx];
        _tmp.text = ""; // 이전 텍스트 초기화

        HideSelectBox();
        StopAllCoroutines();
        StartCoroutine(TypeText());
    }

    void TypingNextScript()
    {
        _uiState = ScriptBoxUIState.TEXT_TYPING;
        _sentence = _scripts[_curScriptIdx];
        _tmp.text = ""; // 이전 텍스트 초기화

        StopAllCoroutines();
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        for (int i = 0; i < _sentence.Length; i++)
        {
            _tmp.text += _sentence[i];
            yield return new WaitForSeconds(_typeSpeed);
        }

        if (!_autoSkip)
            _nextBtn.gameObject.SetActive(true);

        _uiState = ScriptBoxUIState.WAITING_NEXT_SENTENCE;
    }

    void ShowFullSentence()
    {
        if (Input.GetKeyDown(KeyCode.D) && !_autoSkip)
        {
            StopAllCoroutines();
            _tmp.text = _sentence;
            _uiState = ScriptBoxUIState.WAITING_NEXT_SENTENCE;
            _nextBtn.gameObject.SetActive(true);
        }
    }

    void WaitToTheNextSentence()
    {
        if (_autoSkip)
        {
            StartCoroutine(ShowNextSentence());
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(ShowNextSentence());
        }
    }

    IEnumerator ShowNextSentence()
    {
        _nextBtn.gameObject.SetActive(false);
        _tmp.text = _sentence;
        _uiState = ScriptBoxUIState.TEXT_TYPING;
        _curScriptIdx++;

        if (_curScriptIdx == _scripts.Count)
        {
            _curScriptIdx = 0;

            if (_autoSkip)
                yield return new WaitForSeconds(_autoSkipTime);

            _uiState = ScriptBoxUIState.NONE;

            if (_isStatic)
                Managers.Scene.CurrentScene.DoNextStaticAction();
            else
                Managers.Scene.CurrentScene.DoNextAction();
            yield break;
        }

        if (_autoSkip)
            yield return new WaitForSeconds(_autoSkipTime);

        TypingNextScript();
    }
}
