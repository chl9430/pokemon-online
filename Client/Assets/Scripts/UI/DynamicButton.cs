using UnityEngine;

public class DynamicButton : MonoBehaviour
{
    object _btnData;

    public object BtnData { get {  return _btnData; } set { _btnData = value; } }

    public virtual void SetSelectedOrNotSelected(bool isSelected)
    {
    }
}
