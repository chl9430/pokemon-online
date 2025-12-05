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
    int _applyRateAmount;
    float _curChangeRateTime;
    float _changeRateTime = 0.25f;
    HPBarUIState _uiState = HPBarUIState.NONE;

    [SerializeField] RectTransform _gauge;
    [SerializeField] TextMeshProUGUI _gaugeText;

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

                        int restDiff = Mathf.Abs(_curRate - _destRate);
                        if (_curRate < _destRate)
                        {
                            if (restDiff >= _applyRateAmount)
                                _curRate += _applyRateAmount;
                            else
                                _curRate += restDiff;
                        }
                        else
                        {
                            if (restDiff >= _applyRateAmount)
                                _curRate -= _applyRateAmount;
                            else
                                _curRate -= restDiff;
                        }

                        _gauge.anchorMax = new Vector2(((float)_curRate / (float)_maxRate), _gauge.anchorMax.y);

                        if (_gaugeText != null)
                            _gaugeText.text = $"{_curRate} / {_maxRate}";

                        if (_curRate == _destRate)
                        {
                            _uiState = HPBarUIState.NONE;
                            Managers.Scene.CurrentScene.DoNextAction();
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

        int offset = Mathf.Abs(_curRate - destHP);

        _applyRateAmount = Mathf.CeilToInt((float)offset / 10f);

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
