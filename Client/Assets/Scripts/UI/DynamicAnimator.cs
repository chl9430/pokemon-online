using UnityEngine;

public class DynamicAnimator : MonoBehaviour
{
    public void BroadcastToScene()
    {
        Managers.Scene.CurrentScene.DoNextAction();
    }
}
