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
    bool _broadcastToScene;
    SliderState _sliderState = SliderState.NONE;
    BaseScene _scene;

    [SerializeField] List<SliderContent> categoryList;

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;

        _scene.DoNextAction(_curIdx);
    }

    private void Update()
    {
        switch (_sliderState)
        {
            case SliderState.WAITING_INPUT:
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        if (_sliderState == SliderState.MOVING)
                        {
                            return;
                        }

                        _dir = 1;
                        _curIdx--;

                        if (_curIdx < 0)
                        {
                            _curIdx = categoryList.Count - 1;
                        }

                        _sliderState = SliderState.MOVING;

                        for (int i = 0; i < categoryList.Count; i++)
                        {
                            SliderContent category = categoryList[i];

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

                        if (_curIdx == categoryList.Count)
                        {
                            _curIdx = 0;
                        }

                        _sliderState = SliderState.MOVING;

                        for (int i = 0; i < categoryList.Count; i++)
                        {
                            SliderContent category = categoryList[i];

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

        if (_moveFinishCnt == categoryList.Count)
        {
            _moveFinishCnt = 0;
            _sliderState = SliderState.NONE;

            if (_dir == -1)
            {
                for (int i = 0; i < categoryList.Count; i++)
                {
                    SliderContent category = categoryList[i];
                    RectTransform rt = category.GetComponent<RectTransform>();

                    if (rt.anchorMax.x == -2)
                    {
                        rt.anchorMin = new Vector2(categoryList.Count - 3, rt.anchorMin.y);
                        rt.anchorMax = new Vector2(categoryList.Count - 2, rt.anchorMax.y);
                    }
                }
            }
            else
            {
                for (int i = 0; i < categoryList.Count; i++)
                {
                    SliderContent category = categoryList[i];
                    RectTransform rt = category.GetComponent<RectTransform>();

                    if (rt.anchorMin.x == categoryList.Count - 2)
                    {
                        rt.anchorMin = new Vector2(-2, rt.anchorMin.y);
                        rt.anchorMax = new Vector2(-1, rt.anchorMax.y);
                    }
                }
            }

            if (_broadcastToScene)
            {
                _scene.DoNextAction(_curIdx);
            }
        }
    }

    public void WaitUserInputForSlider(bool broadcastToScene)
    {
        _sliderState = SliderState.WAITING_INPUT;
        _broadcastToScene = broadcastToScene;
    }
}
