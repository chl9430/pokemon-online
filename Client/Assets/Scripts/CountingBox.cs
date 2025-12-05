using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public enum CountingBoxState
{
    NONE = 0,
    SELECTING = 1,
}

public class CountingBox : MonoBehaviour
{
    int _count = 1;
    int _maxValue = 1;
    CountingBoxState _state = CountingBoxState.NONE;

    public CountingBoxState State
    {
        set
        {
            _state = value;

            if (_state == CountingBoxState.SELECTING)
            {
                _count = 1;

                Managers.Scene.CurrentScene.DoNextAction(_count);
            }
        }
    }

    void Update()
    {
        switch (_state)
        {
            case CountingBoxState.SELECTING:
                ChooseAction();
                break;
        }
    }

    void ChooseAction()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_count == 1)
                _count = _maxValue;
            else
                _count -= 1;

            Managers.Scene.CurrentScene.DoNextAction(_count);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_count == _maxValue)
                _count = 1;
            else
                _count += 1;

            Managers.Scene.CurrentScene.DoNextAction(_count);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_count < 10)
                _count = 1;
            else
                _count -= 10;

            Managers.Scene.CurrentScene.DoNextAction(_count);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_count > _maxValue - 10)
                _count = _maxValue;
            else
                _count += 10;

            Managers.Scene.CurrentScene.DoNextAction(_count);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Managers.Scene.CurrentScene.DoNextAction(Define.InputSelectBoxEvent.SELECT);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Managers.Scene.CurrentScene.DoNextAction(Define.InputSelectBoxEvent.BACK);
        }
    }

    public int GetCurrentCount()
    {
        return _count;
    }

    public void SetMaxValue(int value)
    {
        _maxValue = value;
    }
}
