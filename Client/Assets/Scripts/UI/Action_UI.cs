using UnityEngine;

public class Action_UI : MonoBehaviour
{
    protected int selectedIdx;
    protected BaseScene scene;

    void Start()
    {
        scene = Managers.Scene.CurrentScene;
    }

    public virtual void ChooseAction()
    {

    }
}
