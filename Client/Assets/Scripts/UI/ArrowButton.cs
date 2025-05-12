using UnityEngine;

public class ArrowButton : MonoBehaviour
{
    [SerializeField] string _btnData;
    [SerializeField] GameObject _arrow;

    public string BtnData
    {
        get
        {
            return _btnData;
        }
    }

    public void ToggleArrow(bool isSelected)
    {
        if (isSelected)
        {
            _arrow.SetActive(true);
        }
        else
        {
            _arrow.SetActive(false);
        }
    }
}
