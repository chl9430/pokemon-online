using NUnit.Framework;
using System.Collections.Generic;
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
    int _curIdx;
    int _moveFinishCnt;
    SliderState _sliderState = SliderState.NONE;
    BaseScene _scene;
    List<SliderContent> _categoryList;

    public SliderState SliderState { get { return _sliderState; } set { _sliderState = value; } }

    [SerializeField] SliderContent _sliderContent;

    void Awake()
    {
        _categoryList = new List<SliderContent>();
    }

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
    }

    private void Update()
    {
        switch (_sliderState)
        {
            case SliderState.WAITING_INPUT:
                {
                    if (_categoryList.Count == 1)
                    {
                        return;
                    }

                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        if (_sliderState == SliderState.MOVING)
                        {
                            return;
                        }

                        foreach (SliderContent content in _categoryList)
                        {
                            RectTransform rt = content.GetComponent<RectTransform>();

                            if (rt.anchorMax.x == _categoryList.Count)
                            {
                                rt.anchorMin = new Vector2(-1, rt.anchorMin.y);
                                rt.anchorMax = new Vector2(0, rt.anchorMax.y);
                            }
                        }

                        _dir = 1;
                        _curIdx--;

                        if (_curIdx < 0)
                        {
                            _curIdx = _categoryList.Count - 1;
                        }

                        _sliderState = SliderState.MOVING;

                        for (int i = 0; i < _categoryList.Count; i++)
                        {
                            SliderContent category = _categoryList[i];

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

                        if (_curIdx == _categoryList.Count)
                        {
                            _curIdx = 0;
                        }

                        _sliderState = SliderState.MOVING;

                        for (int i = 0; i < _categoryList.Count; i++)
                        {
                            SliderContent category = _categoryList[i];

                            category.MoveContent(5f, _dir);
                        }
                    }
                }
                break;
        }
    }

    public void CountContentMoving()
    {
        _moveFinishCnt++;

        if (_moveFinishCnt == _categoryList.Count)
        {
            _moveFinishCnt = 0;
            _sliderState = SliderState.NONE;

            if (_dir == -1)
            {
                for (int i = 0; i < _categoryList.Count; i++)
                {
                    SliderContent category = _categoryList[i];
                    RectTransform rt = category.GetComponent<RectTransform>();

                    if (rt.anchorMax.x == 0)
                    {
                        rt.anchorMin = new Vector2(_categoryList.Count - 1, rt.anchorMin.y);
                        rt.anchorMax = new Vector2(_categoryList.Count, rt.anchorMax.y);
                    }
                }
            }

            _scene.DoNextAction(this);
        }
    }

    public object GetSelectedContentData()
    {
        return _categoryList[_curIdx].ContentData;
    }

    public void SetSliderContents(List<object> _sliderContents)
    {
        for (int i = 0; i < _sliderContents.Count; i++)
        {
            SliderContent sliderContent = GameObject.Instantiate(_sliderContent, gameObject.transform);
            sliderContent.SetContentName(_sliderContents[i].ToString());
            sliderContent.SetData(_sliderContents[i]);
            _categoryList.Add(sliderContent);

            RectTransform rt = sliderContent.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(i, 0);
            rt.anchorMax = new Vector2(i + 1, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        _scene.DoNextAction(this);
    }
}
