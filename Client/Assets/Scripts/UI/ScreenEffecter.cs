using UnityEngine;
using UnityEngine.Playables;

public class ScreenEffecter : MonoBehaviour
{
    BaseScene _scene;
    Animator _anim;

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
        _anim = GetComponent<Animator>();
    }

    public void BroadcastToScene()
    {
        _scene.DoNextAction();
    }

    public void PlayEffect(string _animName)
    {
        if (_anim == null)
            _anim = GetComponent<Animator>();

        _anim.Play(_animName);
    }
}
