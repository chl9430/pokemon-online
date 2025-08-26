using TMPro;
using UnityEngine;

public class DynamicButton : MonoBehaviour
{
    object _btnData;
    TextMeshProUGUI _btnName;

    public object BtnData { get {  return _btnData; } set { _btnData = value; } }

    public virtual void SetSelectedOrNotSelected(bool isSelected)
    {
    }

    public void SetButtonName(string name)
    {
        if (_btnName == null)
            _btnName = Util.FindChild<TextMeshProUGUI>(gameObject, "ContentText", true);

        if (_btnName != null)
        {
            _btnName.text = name;
            _btnData = name;
        }
    }
}
