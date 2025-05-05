using UnityEngine;

public class ArrowButton : MonoBehaviour
{
    [SerializeField] GameObject _arrow;

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
