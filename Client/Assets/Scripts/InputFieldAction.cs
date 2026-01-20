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
    [SerializeField] List<TMP_InputField> _inputFields;
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
                foreach (TMP_InputField inputField in _inputFields)
                    inputField.interactable = true;

                _logInButton.interactable = true;
            }
            else if (_state == InputFieldState.Submitted)
            {
                foreach (TMP_InputField inputField in _inputFields)
                    inputField.interactable = false;

                _logInButton.interactable = false;
            }
        }
    }

    public string GetTextByIndex(int index)
    {
        if (index >= _inputFields.Count || index < 0)
            return "";

        return _inputFields[index].text;
    }

    public void ButtonAction()
    {
        State = InputFieldState.Submitted;

        Managers.Scene.CurrentScene.DoNextAction();
    }
}
