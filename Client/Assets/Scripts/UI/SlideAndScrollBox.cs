using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
struct SlideAndScrollBoxData
{
    public int slideIdx;
    public int scrollBoxIdx;
    public int scrollCnt;
    public int maxView;
}

public class SlideAndScrollBox : CategorySlider
{
    int _curScrollBoxIdx;
    int _curPosInScrollBox = 0;
    int _scrollCnt = 0;
    float _heightPerContent;
    List<DynamicButton> _scrollBoxContents;

    [SerializeField] int _scrollBoxMaxView;

    public int CurScrollBoxIdx { get { return _curScrollBoxIdx; } }
    public int ScrollCnt { get { return _scrollCnt; } }
    public int ScrollBoxMaxView { get { return  _scrollBoxMaxView; } }
    public SliderState State
    {
        set
        {
            _sliderState = value;

            if (_sliderState == SliderState.WAITING_INPUT)
            {
                if (_scene == null)
                    _scene = Managers.Scene.CurrentScene;
                
                _scene.DoNextAction("ScrollBoxMove");
            }
        }
    }

    void Start()
    {
    }

    protected override void Update()
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

    protected override void ChooseAction()
    {
        base.ChooseAction();

        if (_scrollBoxContents.Count == 1 || _scrollBoxContents.Count == 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_curScrollBoxIdx == 0)
                return;

            _scrollBoxContents[_curScrollBoxIdx].SetSelectedOrNotSelected(false);

            _curScrollBoxIdx--;

            _scrollBoxContents[_curScrollBoxIdx].SetSelectedOrNotSelected(true);

            if (_curPosInScrollBox == 0)
            {
                _scrollCnt--;

                foreach (ArrowButton content in _scrollBoxContents)
                {
                    RectTransform rt = content.GetComponent<RectTransform>();

                    rt.anchorMin = new Vector2(0, rt.anchorMin.y - _heightPerContent);
                    rt.anchorMax = new Vector2(1, rt.anchorMax.y - _heightPerContent);
                }
            }
            else
                _curPosInScrollBox--;

            _scene.DoNextAction("ScrollBoxMove");
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_curScrollBoxIdx == _scrollBoxContents.Count - 1)
                return;

            _scrollBoxContents[_curScrollBoxIdx].SetSelectedOrNotSelected(false);

            _curScrollBoxIdx++;

            _scrollBoxContents[_curScrollBoxIdx].SetSelectedOrNotSelected(true);

            if (_curPosInScrollBox == _scrollBoxMaxView - 1)
            {
                _scrollCnt++;

                foreach (ArrowButton content in _scrollBoxContents)
                {
                    RectTransform rt = content.GetComponent<RectTransform>();

                    rt.anchorMin = new Vector2(0, rt.anchorMin.y + _heightPerContent);
                    rt.anchorMax = new Vector2(1, rt.anchorMax.y + _heightPerContent);
                }
            }
            else
                _curPosInScrollBox++;

            _scene.DoNextAction("ScrollBoxMove");
        }
    }

    public void UpdateScrollBoxContents(List<DynamicButton> scrollBoxContents)
    {
        float curHeight = 1f;
        _heightPerContent = 1f / ((float)_scrollBoxMaxView);

        _scrollBoxContents = scrollBoxContents;
        _curScrollBoxIdx = 0;
        _curPosInScrollBox = 0;
        _scrollCnt = 0;

        for (int i = 0; i < _scrollBoxContents.Count; i++)
        {
            RectTransform rt = _scrollBoxContents[i].GetComponent<RectTransform>();

            float height = curHeight - _heightPerContent;

            rt.anchorMin = new Vector2(0, height);
            rt.anchorMax = new Vector2(1, curHeight);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            curHeight -= _heightPerContent;

            _scrollBoxContents[i].SetSelectedOrNotSelected(false);
        }

        if (_scrollBoxContents.Count > 0)
        {
            _scrollBoxContents[_curScrollBoxIdx].SetSelectedOrNotSelected(true);
        }
    }
}
