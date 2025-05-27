using TMPro;
using UnityEngine;


public class InputFieldExtractor : MonoBehaviour
{
    BaseScene _scene;

    [SerializeField] TMP_InputField _inputField;

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
    }

    public void GetInput()
    {
        if (_inputField != null)
        {
            string inputText = _inputField.text;
            // _scene.DoNextActionWithValue(inputText);
        }
        else
        {
            Debug.LogError("TMP Input Field가 할당되지 않았습니다.");
        }
    }

}
