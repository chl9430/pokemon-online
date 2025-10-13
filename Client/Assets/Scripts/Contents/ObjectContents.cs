using Google.Protobuf;
using UnityEngine;

public class ObjectContents : MonoBehaviour
{
    protected bool _isLoading;
    protected BaseScene _scene;
    protected IMessage _packet;

    public IMessage Packet { set { _packet = value; } }

    public virtual void UpdateData(IMessage packet)
    {

    }

    public virtual void SetNextAction(object value = null)
    {

    }
}
