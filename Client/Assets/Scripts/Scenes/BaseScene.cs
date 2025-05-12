using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    ScreenChanger screenChanger;
    public ScreenChanger ScreenChanger { get { return screenChanger; } set { screenChanger = value; } }
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

        screenChanger = Managers.Resource.Instantiate("UI/ScreenChanger").GetComponent<ScreenChanger>();
    }
    
    public virtual void AfterFadeInAction()
    {
    }

    public virtual void DoNextAction()
    {

    }

    public virtual void DoNextActionWithValue(object value)
    {

    }

    public abstract void Clear();
}
