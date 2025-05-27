using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum MoveableUIState
{
    MOVING = 0,
    MOVE_AND_COMEBACK = 1,
    BLINK = 2,
    NONE = 3,
}

public class MoveableUI : MonoBehaviour
{
    float startTime;
    int _dir;
    MoveableUIState uiState = MoveableUIState.NONE;
    Vector2 oldMinPos;
    Vector2 oldMaxPos;
    Vector2 destMinPos;
    Vector2 destMaxPos;
    RectTransform _rect;
    BaseScene _scene;

    [Header("Blink")]
    float _blinkTime = 1f;
    float _curBlinkTime = 0f;
    int _blinkCnt = 3;
    int _curBlinkCnt = 0;

    float _moveSpeed;

    [SerializeField]Image _uiObjectToBlink;

    void Start()
    {
        _rect = GetComponent<RectTransform>();
        _scene = Managers.Scene.CurrentScene;
    }

    void Update()
    {
        switch (uiState)
        {
            case MoveableUIState.MOVING:
                MoveSlideContent();
                break;
            case MoveableUIState.MOVE_AND_COMEBACK:
                MoveAndComeBack();
                break;
            case MoveableUIState.BLINK:
                {
                    _curBlinkTime += Time.deltaTime;

                    if (_curBlinkTime >= _blinkTime)
                    {
                        if (_curBlinkCnt == _blinkCnt)
                        {
                            SetAlpha(255f);
                            uiState = MoveableUIState.NONE;
                            _curBlinkCnt = 0;
                            _scene.DoNextAction();
                        }
                        else
                        {
                            if (_uiObjectToBlink.color.a == 0f)
                            {
                                SetAlpha(255f);
                            }
                            else
                            {
                                SetAlpha(0f);
                                _curBlinkCnt++;
                            }
                        }

                        _curBlinkTime = 0f;
                    }
                }
                break;
        }
    }

    public void StartBlink(int blinkCnt, float blinkTime)
    {
        _blinkCnt = blinkCnt; // 원본 색상 저장
        _blinkTime = blinkTime;
        uiState = MoveableUIState.BLINK;
    }

    public void SetOldAndDestPos(Vector2 minDestPos, Vector2 maxDestPos, MoveableUIState state, float moveSpeed)
    {
        if (_rect == null)
            _rect = GetComponent<RectTransform>();

        uiState = state;
        startTime = Time.time;
        oldMinPos = _rect.anchorMin;
        oldMaxPos = _rect.anchorMax;
        _moveSpeed = moveSpeed;

        destMinPos = minDestPos;
        destMaxPos = maxDestPos;

        _dir = 1;
    }

    void MoveSlideContent()
    {
        float timeElapsed = Time.time - startTime;
        float t = Mathf.Clamp01(timeElapsed * _moveSpeed);

        _rect.anchorMin = Vector2.Lerp(oldMinPos, destMinPos, t);
        _rect.anchorMax = Vector2.Lerp(oldMaxPos, destMaxPos, t);

        if (t >= 1f)
        {
            uiState = MoveableUIState.NONE;
            _rect.anchorMin = destMinPos;
            _rect.anchorMax = destMaxPos;
            _scene.DoNextAction();
        }
    }

    void MoveAndComeBack()
    {
        float timeElapsed = Time.time - startTime;
        float t = Mathf.Clamp01(timeElapsed * _moveSpeed);

        if (_dir == 1)
        {
            _rect.anchorMin = Vector2.Lerp(oldMinPos, destMinPos, t);
            _rect.anchorMax = Vector2.Lerp(oldMaxPos, destMaxPos, t);
        }
        else if (_dir == -1)
        {
            _rect.anchorMin = Vector2.Lerp(destMinPos, oldMinPos, t);
            _rect.anchorMax = Vector2.Lerp(destMaxPos, oldMaxPos, t);
        }

        if (t >= 1f)
        {
            if (_dir == 1)
            {
                _rect.anchorMin = destMinPos;
                _rect.anchorMax = destMaxPos;
                // _scene.DoNextAction();
            }
            else if (_dir == -1)
            {
                _rect.anchorMin = oldMinPos;
                _rect.anchorMax = oldMaxPos;

                _scene.DoNextAction();
                _dir = 0;
                uiState = MoveableUIState.NONE;
            }

            startTime = Time.time;

            _dir = -1;
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = _uiObjectToBlink.color;
        color.a = alpha;

        _uiObjectToBlink.color = color;
    }
}
