using UnityEngine;

public class DynamicAnimator : MonoBehaviour
{
    BaseScene _scene;

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
    }

    public void BroadcastToScene()
    {
        _scene.DoNextAction();
    }
}
