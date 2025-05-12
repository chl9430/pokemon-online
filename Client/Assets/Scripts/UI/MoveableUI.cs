using UnityEngine;

public enum MoveableUIState
{
    MOVING = 0,
    NONE = 1,
}

public class MoveableUI : MonoBehaviour
{
    float startTime;
    MoveableUIState uiState = MoveableUIState.NONE;
    Vector2 oldMinPos;
    Vector2 oldMaxPos;
    Vector2 destMinPos;
    Vector2 destMaxPos;
    RectTransform _rect;
    BaseScene _scene;

    [SerializeField] float moveSpeed;

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
        }
    }

    public void SetOldAndDestPos(Vector2 minDestPos, Vector2 maxDestPos)
    {
        if (_rect == null)
            _rect = GetComponent<RectTransform>();

        uiState = MoveableUIState.MOVING;
        startTime = Time.time;
        oldMinPos = _rect.anchorMin;
        oldMaxPos = _rect.anchorMax;

        destMinPos = minDestPos;
        destMaxPos = maxDestPos;
    }

    void MoveSlideContent()
    {
        float timeElapsed = Time.time - startTime;
        float t = Mathf.Clamp01(timeElapsed * moveSpeed);

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
}
