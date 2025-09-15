using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public enum SliderState
{
    NONE = 0,
    WAITING_INPUT = 1,
    MOVING = 2,
}

public class CategorySlider : MonoBehaviour
{
    int _dir;
    int _moveFinishCnt;
    List<SliderContent> _sliderContents;

    protected SliderState _sliderState = SliderState.NONE;
    protected int _curIdx;
    protected BaseScene _scene;

    [SerializeField] SliderContent _sliderContent;

    public int CurIdx { get { return _curIdx; } }

    public List<SliderContent> SliderContents { get { return _sliderContents; } }

    public SliderState SliderState 
    { 
        get 
        { 
            return _sliderState; 
        }
        
        set 
        { 
            _sliderState = value;

            if (_sliderState == SliderState.WAITING_INPUT)
            {
                if (_scene == null)
                    _scene = Managers.Scene.CurrentScene;

                _scene.DoNextAction(_sliderContents[_curIdx].ContentData);
            }
        } 
    }

    void Awake()
    {
        _sliderContents = new List<SliderContent>();
    }

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
    }

    protected virtual void Update()
    {
        switch (_sliderState)
        {
            case SliderState.WAITING_INPUT:
                {
                    ChooseAction();
                }
                break;
        }
    }

    protected virtual void ChooseAction()
    {
        if (_sliderContents.Count == 1 || _sliderContents.Count == 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_sliderState == SliderState.MOVING)
            {
                return;
            }

            foreach (SliderContent content in _sliderContents)
            {
                RectTransform rt = content.GetComponent<RectTransform>();

                if (rt.anchorMax.x == _sliderContents.Count)
                {
                    rt.anchorMin = new Vector2(-1, rt.anchorMin.y);
                    rt.anchorMax = new Vector2(0, rt.anchorMax.y);
                }
            }

            _dir = 1;
            _curIdx--;

            if (_curIdx < 0)
            {
                _curIdx = _sliderContents.Count - 1;
            }

            _sliderState = SliderState.MOVING;

            for (int i = 0; i < _sliderContents.Count; i++)
            {
                SliderContent category = _sliderContents[i];

                category.MoveContent(5f, _dir);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_sliderState == SliderState.MOVING)
            {
                return;
            }

            _dir = -1;
            _curIdx++;

            if (_curIdx == _sliderContents.Count)
            {
                _curIdx = 0;
            }

            _sliderState = SliderState.MOVING;

            for (int i = 0; i < _sliderContents.Count; i++)
            {
                SliderContent category = _sliderContents[i];

                category.MoveContent(5f, _dir);
            }
        }
    }

    public void CountContentMoving()
    {
        _moveFinishCnt++;

        if (_moveFinishCnt == _sliderContents.Count)
        {
            _moveFinishCnt = 0;

            if (_dir == -1)
            {
                for (int i = 0; i < _sliderContents.Count; i++)
                {
                    SliderContent category = _sliderContents[i];
                    RectTransform rt = category.GetComponent<RectTransform>();

                    if (rt.anchorMax.x == 0)
                    {
                        rt.anchorMin = new Vector2(_sliderContents.Count - 1, rt.anchorMin.y);
                        rt.anchorMax = new Vector2(_sliderContents.Count, rt.anchorMax.y);
                    }
                }
            }

            SliderState = SliderState.WAITING_INPUT;
        }
    }

    public void UpdateSliderContents(List<SliderContent> sliderContents)
    {
        _curIdx = 0;
        _sliderContents = sliderContents;
    }

    public void CreateSlideContents(List<object> contents)
    {
        for (int i = 0; i < contents.Count; i++)
        {
            _sliderContents.Add(Instantiate(_sliderContent, gameObject.transform));
            RectTransform rt = _sliderContents[i].GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(i, 0);
            rt.anchorMax = new Vector2(i + 1, 1);

            _sliderContents[i].Slider = this;
            _sliderContents[i].ContentData = contents[i];
        }
    }

    public object GetSelectedContentData()
    {
        return _sliderContents[_curIdx].ContentData;
    }
}
