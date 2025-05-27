using TMPro;
using UnityEngine;
public enum HPBarUIState
{
    CHANGING_HP = 1,
    NONE = 2,
}

public class GaugeUI : MonoBehaviour
{
    int _destRate;
    int _maxRate;
    int _curRate;
    float _curChangeRateTime;
    float _changeRateTime = 0.25f;
    HPBarUIState _uiState = HPBarUIState.NONE;
    BaseScene _scene;

    [SerializeField] RectTransform _gauge;
    [SerializeField] TextMeshProUGUI _gaugeText;

    void Start()
    {
        if (_scene == null)
            _scene = Managers.Scene.CurrentScene;
    }

    void Update()
    {
        switch (_uiState)
        {
            case HPBarUIState.CHANGING_HP:
                {
                    _curChangeRateTime += Time.deltaTime;

                    if (_curChangeRateTime >= _changeRateTime)
                    {
                        _curChangeRateTime = 0;

                        if (_curRate < _destRate)
                            _curRate++;
                        else
                            _curRate--;

                        _gauge.anchorMax = new Vector2(((float)_curRate / (float)_maxRate), _gauge.anchorMax.y);

                        if (_gaugeText != null)
                            _gaugeText.text = $"{_curRate} / {_maxRate}";

                        if (_curRate == _destRate)
                        {
                            _uiState = HPBarUIState.NONE;
                            _scene.DoNextAction();
                        }
                    }
                }
                break;
        }
    }

    public void ChangeGauge(int destHP, float changeHPTime)
    {
        _uiState = HPBarUIState.CHANGING_HP;
        _changeRateTime = changeHPTime;

        if (destHP <= 0)
            _destRate = 0;
        else
            _destRate = destHP;
    }

    public void SetGauge(int curRate, int maxRate)
    {
        _curRate = curRate;
        _maxRate = maxRate;
        _gauge.anchorMax = new Vector2(((float)_curRate / (float)_maxRate), _gauge.anchorMax.y);
        _gaugeText.text = $"{curRate} / {maxRate}";
    }
}
