using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;

public class ObjectContents : MonoBehaviour
{
    protected bool _isLoading;
    protected bool _isActionStop = false;
    protected IMessage _packet;

    public void SetIsActionStop(bool isActionStop)
    {
        _isActionStop = isActionStop;
    }

    public virtual void UpdateData(IMessage packet)
    {

    }

    public virtual void SetNextAction(object value = null)
    {

    }

    public virtual void FinishContent()
    {

    }

    public virtual void InactiveContent()
    {

    }
}
