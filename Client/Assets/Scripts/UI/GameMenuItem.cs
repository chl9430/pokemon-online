using UnityEngine;
using UnityEngine.UI;

public class GameMenuItem : MonoBehaviour
{
    [SerializeField] GameObject arrowImg;

    public void ToggleArrow(bool toggle)
    {
        arrowImg.SetActive(toggle);
    }
}
