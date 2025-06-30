using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputBox : MonoBehaviour
{
    TMP_InputField _inputField;
    BaseScene _scene;

    void Start()
    {
        _inputField = GetComponent<TMP_InputField>();
        _inputField.onValueChanged.AddListener(ChangedValue);
        _scene = Managers.Scene.CurrentScene;
        _inputField.interactable = false;
    }

    public void ChangedValue(string newText)
    {
        _scene.DoNextAction(newText);
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

    // �ν����� �� ��ư�� �Ҵ�� �Լ�
    public void FinishEntering()
    {
        if (_inputField.text != "")
            _scene.DoNextAction(Define.InputSelectBoxEvent.SELECT);
    }
}
