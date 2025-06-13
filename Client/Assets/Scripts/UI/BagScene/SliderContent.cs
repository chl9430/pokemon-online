using UnityEngine;

public enum SliderContentState
{
    NONE = 0,
    MOVING = 1,
}

public class SliderContent : MonoBehaviour
{
    float _startTime;
    float _moveSpeed;
    RectTransform _rect;
    CategorySlider _slider;
    Vector2 _prevMinPos;
    Vector2 _prevMaxPos;
    Vector2 _destMinPos;
    Vector2 _destMaxPos;
    SliderContentState _state = SliderContentState.NONE;

    void Start()
    {
        _slider = transform.parent.GetComponent<CategorySlider>();
    }

    void Update()
    {
        switch (_state)
        {
            case SliderContentState.MOVING:
                {
                    MoveSlideContent();
                }
                break;
        }
    }

    void MoveSlideContent()
    {
        float timeElapsed = Time.time - _startTime;
        float t = Mathf.Clamp01(timeElapsed * _moveSpeed);

        _rect.anchorMin = Vector2.Lerp(_prevMinPos, _destMinPos, t);
        _rect.anchorMax = Vector2.Lerp(_prevMaxPos, _destMaxPos, t);

        if (t >= 1f)
        {
            _rect.anchorMin = _destMinPos;
            _rect.anchorMax = _destMaxPos;
            _state = SliderContentState.NONE;

            _slider.CountContentMoving();
        }
    }

    public void MoveContent(float moveSpeed, int dir)
    {
        if (_rect == null)
            _rect = GetComponent<RectTransform>();

        _startTime = Time.time;
        _prevMinPos = _rect.anchorMin;
        _prevMaxPos = _rect.anchorMax;
        _moveSpeed = moveSpeed;
        _state = SliderContentState.MOVING;

        _destMinPos = new Vector2(_prevMinPos.x + dir, _prevMinPos.y);
        _destMaxPos = new Vector2(_prevMaxPos.x + dir, _prevMaxPos.y);
    }
}