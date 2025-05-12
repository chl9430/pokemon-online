using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public enum HorizontalSelectBoxUIState
{
    SELECTING = 0,
    NONE = 1,
}

public class HorizontalSelectBoxUI : Action_UI
{
    HorizontalSelectBoxUIState _uiState = HorizontalSelectBoxUIState.NONE;

    [SerializeField] List<ArrowButton> _btns;

    public HorizontalSelectBoxUIState UIState
    {
        set
        {
            _uiState = value;
        }
    }

    void Start()
    {
        //_btns = new List<ArrowButton>();
        
        //ArrowButton btn = Util.FindChild<ArrowButton>(gameObject, "ArrowButton", true);
        //_btns.Add(btn);
    }

    void Update()
    {
        switch (_uiState)
        {
            case HorizontalSelectBoxUIState.SELECTING:
                SelectingOption();
                break;
        }
    }

    void SelectingOption()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _btns[selectedIdx].ToggleArrow(false);
            selectedIdx++;

            if (selectedIdx == _btns.Count)
            {
                selectedIdx = _btns.Count - 1;
            }
            _btns[selectedIdx].ToggleArrow(true);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _btns[selectedIdx].ToggleArrow(false);
            selectedIdx--;

            if (selectedIdx < 0)
            {
                selectedIdx = 0;
            }
            _btns[selectedIdx].ToggleArrow(true);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (scene == null)
                scene = Managers.Scene.CurrentScene;

            scene.DoNextActionWithValue(_btns[selectedIdx].BtnData);
        }
    }

    public void HideAllArrow()
    {
        for (int i = 0; i < _btns.Count; i++)
        {
            _btns[i].ToggleArrow(false);
        }
    }
}
