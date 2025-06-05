using Google.Protobuf.Protocol;
using Google.Protobuf;
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

    protected virtual void Start()
    {
        Managers.Scene.CurrentScene.ScreenChanger.FadeInScene();
    }

    // 특정한 타임라인 애니메이션이 끝난 후 씬에 알려주기 위해 호출되는 함수
    public virtual void DoNextActionWithTimeline()
    {
    }

    // 씬 진입 시 페이드인 호 씬에 알려주기 위해 호출되는 함수
    public virtual void AfterFadeInAction()
    {
    }

    // 씬 내 여러 ui와 상호작용하기 위해 호출되는 함수
    public virtual void DoNextAction(object value = null)
    {
    }

    public void AttachToTheUI(GameObject obj)
    {
        obj.transform.SetParent(_ui.transform);
    }

    // 씬 진입 후 서버로부터 패킷을 받아 렌더링 정보를 업데이트하는 함수
    public virtual void UpdateData(IMessage packet)
    {
    }

    // 씬을 벗어나기 전 서버에 보낼 패킷을 등록하는 함수
    public virtual void RegisterPacket(IMessage packet)
    {
    }

    public abstract void Clear();
}
