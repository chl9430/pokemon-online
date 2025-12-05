using UnityEngine;
using UnityEngine.Playables;

public class ScreenEffecter : MonoBehaviour
{
    string _curAnimName;
    Animator _anim;

    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    public void DestroyEffecter()
    {
        Destroy(gameObject);
    }

    public void BroadcastToScene()
    {
        Managers.Scene.CurrentScene.DoNextAction();
    }

    public void PlayEffect(string animName)
    {
        if (_anim == null)
            _anim = GetComponent<Animator>();

        _curAnimName = animName;
        _anim.Play(animName);
    }
}
