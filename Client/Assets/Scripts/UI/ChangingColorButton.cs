using UnityEngine;
using UnityEngine.UI;

public class ChangingColorButton : DynamicButton
{
    Image _img;

    [SerializeField] Color _selectedColor;
    [SerializeField] Color _nonSelectedColor;

    public Color SelectedColor { get { return _selectedColor; } set { _selectedColor = value; } }
    public Color NonSelectedColor { get { return _nonSelectedColor; } set { _nonSelectedColor = value; } }

    public override void SetSelectedOrNotSelected(bool isSelected)
    {
        if (_img == null)
            _img = GetComponent<Image>();

        if (isSelected)
            _img.color = _selectedColor;
        else
            _img.color = _nonSelectedColor;
    }
}
