using UnityEngine;
using UnityEngine.Playables;

public enum MoveSceneType
{
    None = 0,
    MovingNewScene = 1,
    OpenUI = 2,
    UnloadCurScene = 3,
    CloseUI = 4,
}

public class ScreenEffecter : MonoBehaviour
{
    Animator _anim;

    MoveSceneType _moveSceneType;

    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    public void BroadcastToScene()
    {
        if (_moveSceneType == MoveSceneType.MovingNewScene)
            ContentManager.Instance.MoveToAnotherScene();
        else if (_moveSceneType == MoveSceneType.UnloadCurScene)
            ContentManager.Instance.UnloadCurScene();
        else if (_moveSceneType == MoveSceneType.OpenUI)
            GameContentManager.Instance.OpenNextUI();
        else if (_moveSceneType == MoveSceneType.CloseUI)
            GameContentManager.Instance.PopCurUI();
    }

    public void PlayEffect(string animName)
    {
        if (_anim == null)
            _anim = GetComponent<Animator>();

        _anim.Play(animName);
    }

    public void SetFadeIn()
    {
        _anim.SetTrigger("fadeIn");
    }

    public void SetMoveSceneType(MoveSceneType moveSceneType)
    {
        _moveSceneType = moveSceneType;
    }
}
