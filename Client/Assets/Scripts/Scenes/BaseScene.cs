using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    ScreenChanger _screenChanger;

    protected GameObject _ui;

    public ScreenChanger ScreenChanger { get { return _screenChanger; } }
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;

	void Awake()
	{
		Init();
	}

    void Start()
    {
        Managers.Scene.CurrentScene.ScreenChanger.FadeInScene();
    }

    protected virtual void Init()
    {
        Object obj = GameObject.FindFirstObjectByType(typeof(EventSystem));
        if (obj == null)
            Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";

        _screenChanger = Managers.Resource.Instantiate("UI/ScreenChanger").GetComponent<ScreenChanger>();

        _ui = new GameObject();

        _ui.name = "UI";

        _ui.transform.position = Vector3.zero;

        _screenChanger.transform.SetParent(_ui.transform);
    }

    public virtual void DoNextActionWithTimeline()
    {
    }

    public virtual void AfterFadeInAction()
    {
    }

    public virtual void DoNextAction(object value = null)
    {
    }

    public void AttachToTheUI(GameObject obj)
    {
        obj.transform.SetParent(_ui.transform);
    }

    public abstract void Clear();
}
