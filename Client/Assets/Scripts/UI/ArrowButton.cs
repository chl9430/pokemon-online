using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArrowButton : DynamicButton
{
    [SerializeField] Image _arrow;

    public override void SetSelectedOrNotSelected(bool isSelected)
    {
        if (_arrow == null)
            _arrow = Util.FindChild<Image>(gameObject, "Image", true);

        if (isSelected)
            _arrow.gameObject.SetActive(true);
        else
            _arrow.gameObject.SetActive(false);
    }
}
