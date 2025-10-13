using Google.Protobuf.Protocol;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    protected ScreenEffecter _enterEffect;

    [SerializeField] Transform _screenEffecterZone;
    public ScreenEffecter ScreenEffecter { set { _enterEffect = value; } get { return _enterEffect; } }
    public Transform ScreenEffecterZone { get { return _screenEffecterZone; } }
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;

    protected MyPlayerController _myPlayer;

    public MyPlayerController MyPlayer { get { return _myPlayer; } }

	void Awake()
	{
		Init();
	}

    protected virtual void Init()
    {
        Object obj = GameObject.FindFirstObjectByType(typeof(EventSystem));
        if (obj == null)
            Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";

        _enterEffect = Managers.Resource.Instantiate("UI/Fading", _screenEffecterZone).GetComponent<ScreenEffecter>();
    }

    protected virtual void Start()
    {
    }

    // 씬 내 여러 ui와 상호작용하기 위해 호출되는 함수
    public virtual void DoNextAction(object value = null)
    {
    }

    // 씬 진입 후 서버로부터 패킷을 받아 렌더링 정보를 업데이트하는 함수
    public virtual void UpdateData(IMessage packet)
    {
    }

    public virtual void FinishContents()
    {
    }

    public abstract void Clear();
}
