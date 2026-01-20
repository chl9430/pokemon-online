using Google.Protobuf.Protocol;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    protected bool _loadingPacket = false;
    protected bool _ignoreContent = false;
    protected Stack<ObjectContents> _contentStack = new Stack<ObjectContents>();
    //protected MyPlayerController _playerController;
    protected IMessage _packet;

    //[SerializeField] protected Canvas _sceneCanvas;

    public Stack<ObjectContents> ContentStack { get { return _contentStack; } }
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;
    //public MyPlayerController PlayerController {  get { return _playerController; } }


	void Awake()
	{
		Init();
	}

    protected virtual void Init()
    {
        Object obj = GameObject.FindFirstObjectByType(typeof(EventSystem));

        if (obj == null)
            Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";
    }

    protected virtual void Start()
    {
    }

    // 씬 내 여러 ui와 상호작용하기 위해 호출되는 함수
    public virtual void DoNextAction(object value = null)
    {
    }

    public virtual void DoNextStaticAction(object value = null)
    {

    }

    // 씬 진입 후 서버로부터 패킷을 받아 렌더링 정보를 업데이트하는 함수
    public virtual void UpdateData(IMessage packet)
    {
    }

    public void PopUntilSpecificChild<TTarget>()
        where TTarget : ObjectContents
    {
        while (_contentStack.Count > 0)
        {
            ObjectContents item = _contentStack.Peek();

            if (item is TTarget)
            {
                item.SetNextAction();
                return;
            }
            else
            {
                item.FinishContent();
            }
        }
        //while (_contentStack.Count > 0)
        //{
        //    _contentStack.Pop();

        //    ObjectContents item = _contentStack.Peek();
        //    item.SetNextAction();

        //    if (item is TTarget targetItem)
        //    {
        //        return;
        //    }
        //}
    }

    public virtual void PopAllContents()
    {
        while (_contentStack.Count > 0)
        {
            ObjectContents content = _contentStack.Peek();
            content.FinishContent();
        }
    }

    public virtual void FinishContents(bool isActive)
    {
        _contentStack.Peek().gameObject.SetActive(isActive);
        _contentStack.Pop();

        if (_contentStack.Count > 0)
            _contentStack.Peek().SetNextAction();
    }

    public abstract void Clear();
}
