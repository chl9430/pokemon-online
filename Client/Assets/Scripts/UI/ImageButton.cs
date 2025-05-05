using UnityEngine;
using UnityEngine.UI;

public class ImageButton : MonoBehaviour
{
    Image buttonImg;

    [SerializeField] Sprite defaultImage;
    [SerializeField] Sprite selectedImage;

    void Start()
    {
        buttonImg = GetComponent<Image>();
    }

    public void ToggleSelected(bool _isSelected)
    {
        if (buttonImg == null)
            buttonImg = GetComponent<Image>();

        if (_isSelected)
            buttonImg.sprite = selectedImage;
        else
            buttonImg.sprite = defaultImage;
    }
}
