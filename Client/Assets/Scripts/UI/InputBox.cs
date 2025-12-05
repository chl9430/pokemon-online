using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputBox : MonoBehaviour
{
    TMP_InputField _inputField;

    void Start()
    {
        _inputField = GetComponent<TMP_InputField>();
        _inputField.onValueChanged.AddListener(ChangedValue);
        _inputField.interactable = false;
    }

    public void ChangedValue(string newText)
    {
        Managers.Scene.CurrentScene.DoNextAction(newText);
    }

    public void SetFieldInteractable(bool isInteractable)
    {
        _inputField.interactable = isInteractable;

        if (isInteractable)
        {
            EventSystem.current.SetSelectedGameObject(_inputField.gameObject);
            _inputField.ActivateInputField();
        }
    }

    // 인스펙터 내 버튼에 할당될 함수
    public void FinishEntering()
    {
        if (_inputField.text != "")
            Managers.Scene.CurrentScene.DoNextAction(Define.InputSelectBoxEvent.SELECT);
    }
}
