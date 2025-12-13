using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum InputFieldState
{
    None = 0,
    Inputing = 1,
    Submitted = 2,
}

public class InputFieldAction : MonoBehaviour
{
    InputFieldState _state;
    [SerializeField] TMP_InputField _idInputField;
    [SerializeField] TMP_InputField _pwInputField;
    [SerializeField] Button _logInButton;

    public InputFieldState State
    {
        set
        {
            _state = value;

            if (_state == InputFieldState.None)
            {

            }
            else if (_state == InputFieldState.Inputing)
            {
                _idInputField.interactable = true;
                _pwInputField.interactable = true;
                _logInButton.interactable = true;
            }
            else if (_state == InputFieldState.Submitted)
            {
                _idInputField.interactable = false;
                _pwInputField.interactable = false;
                _logInButton.interactable = false;
            }
        }
    }

    public string GetID()
    {
        return _idInputField.text;
    }

    public string GetPassword()
    {
        return _pwInputField.text;
    }

    public void ButtonAction()
    {
        State = InputFieldState.Submitted;

        Managers.Scene.CurrentScene.DoNextAction();
    }
}
