using System.Collections.Generic;
using UnityEngine;

public enum ScrollBoxState
{
    NONE = 0,
    WAITING_INPUT = 1,
}

public class ScrollSelectBox : MonoBehaviour
{
    int _curIdx = 0;
    int _curPosInScrollBox = 0;
    int _scrollCnt = 0;
    // bool _broadcastToScene;
    float _heightPerContent;
    BaseScene _scene;
    List<ArrowButton> _scrollBoxContents;
    ArrowButton _selectedScrollContent;
    ScrollBoxState _scrollBoxState = ScrollBoxState.NONE;

    [SerializeField] int _viewCount;

    public int CurIdx {  get { return _curIdx; } }
    public int CurPosInScrollBox { get {  return _curPosInScrollBox; } }
    public int ScrollCnt { get { return _scrollCnt; } }
    public List<ArrowButton> ScrollBoxContents { get {  return _scrollBoxContents; } }
    public ScrollBoxState ScrollBoxState { get { return _scrollBoxState; } set { _scrollBoxState = value; } }
    public ArrowButton SelectedContent { get { return _selectedScrollContent; } }

    public int ViewCount { get { return _viewCount; } }

    void Awake()
    {
        _scrollBoxContents = new List<ArrowButton>();
    }

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
    }

    void Update()
    {
        switch (_scrollBoxState)
        {
            case ScrollBoxState.WAITING_INPUT:
                {
                    if (_scrollBoxContents.Count == 1 || _scrollBoxContents.Count == 0)
                    {
                        return;
                    }

                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if (_curIdx == 0)
                            return;

                        _selectedScrollContent.ToggleArrow(false);

                        _curIdx--;

                        _selectedScrollContent = _scrollBoxContents[_curIdx];

                        _selectedScrollContent.ToggleArrow(true);

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

                        _scene.DoNextAction(this);
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if (_curIdx == _scrollBoxContents.Count - 1)
                            return;

                        _selectedScrollContent.ToggleArrow(false);

                        _curIdx++;

                        _selectedScrollContent = _scrollBoxContents[_curIdx];

                        _selectedScrollContent.ToggleArrow(true);

                        if (_curPosInScrollBox == _viewCount - 1)
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

                        _scene.DoNextAction(this);
                    }
                    else if (Input.GetKeyDown(KeyCode.D))
                    {
                        _scrollBoxState = ScrollBoxState.NONE;
                        _scene.DoNextAction(_selectedScrollContent.BtnData);
                    }
                }
                break;
        }
    }

    public void CreateScrollBoxItems(List<ArrowButton> contents)
    {
        float curHeight = 1f;
        _heightPerContent = 1f / ((float)_viewCount);

        _scrollBoxContents = contents;
        _curIdx = 0;
        _curPosInScrollBox = 0;
        _scrollCnt = 0;

        for (int i = 0; i < contents.Count; i++)
        {
            ArrowButton content = contents[i];

            RectTransform rt = content.GetComponent<RectTransform>();

            float height = curHeight - _heightPerContent;

            rt.anchorMin = new Vector2(0, height);
            rt.anchorMax = new Vector2(1, curHeight);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            curHeight -= _heightPerContent;

            content.ToggleArrow(false);
        }

        if (_scrollBoxContents.Count > 0)
        {
            _selectedScrollContent = _scrollBoxContents[0];
            _selectedScrollContent.ToggleArrow(true);
        }

        _scene.DoNextAction(this);
    }
}
