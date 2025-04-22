using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    [SerializeField] ScreenChanger screenChanger;
    public ScreenChanger ScreenChanger {  get { return screenChanger; } }
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;

	void Awake()
	{
		Init();
	}

	protected virtual void Init()
    {
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
            Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";

        screenChanger = Managers.Resource.Instantiate("UI/ScreenChanger").GetComponent<ScreenChanger>();
    }

    public abstract void Clear();
}
