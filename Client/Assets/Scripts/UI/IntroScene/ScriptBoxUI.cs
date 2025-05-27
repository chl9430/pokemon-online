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
    bool _autoSkip;
    float _autoSkipTime;
    List<string> _scripts;
    BaseScene _scene;

    [SerializeField] TextMeshProUGUI _tmp;
    [SerializeField] GameObject _nextBtn;
    [SerializeField] float _typeSpeed = 0.05f;

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
        _tmp.text = "";
        _nextBtn.SetActive(false);
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

    public void SetScriptWihtoutTyping(string script)
    {
        _tmp.text = script;
    }

    public void BeginScriptTyping(List<string> scripts, bool autoSkip = false, float autoSkipTime = 1f)
    {
        _uiState = ScriptBoxUIState.TEXT_TYPING;
        _scripts = scripts;
        _autoSkipTime = autoSkipTime;
        _autoSkip = autoSkip;
        _sentence = _scripts[_curScriptIdx];
        _tmp.text = ""; // 이전 텍스트 초기화

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
            _nextBtn.SetActive(true);

        _uiState = ScriptBoxUIState.WAITING_NEXT_SENTENCE;
    }

    void ShowFullSentence()
    {
        if (Input.GetKeyDown(KeyCode.D) && !_autoSkip)
        {
            StopAllCoroutines();
            _tmp.text = _sentence;
            _uiState = ScriptBoxUIState.WAITING_NEXT_SENTENCE;
            _nextBtn.SetActive(true);
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
        _nextBtn.SetActive(false);
        _tmp.text = _sentence;
        _uiState = ScriptBoxUIState.TEXT_TYPING;
        _curScriptIdx++;

        if (_curScriptIdx == _scripts.Count)
        {
            _curScriptIdx = 0;

            if (_autoSkip)
                yield return new WaitForSeconds(_autoSkipTime);

            _scene.DoNextAction();
            _uiState = ScriptBoxUIState.NONE;
            yield break;
        }

        if (_autoSkip)
            yield return new WaitForSeconds(_autoSkipTime);

        TypingNextScript();
    }
}
