using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArrowButton : MonoBehaviour
{
    object _btnData;
    [SerializeField] Image _arrow;
    [SerializeField] TextMeshProUGUI _tmp;

    public object BtnData
    {
        get
        {
            return _btnData;
        }

        set
        {
            _btnData = value;
        }
    }

    public void ToggleArrow(bool isSelected)
    {
        if (isSelected)
        {
            _arrow.gameObject.SetActive(true);
        }
        else
        {
            _arrow.gameObject.SetActive(false);
        }
    }

    public void SetButtonName(string btnName)
    {
        _tmp.text = btnName;
    }
}
