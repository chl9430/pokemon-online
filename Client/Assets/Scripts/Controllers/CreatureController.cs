using Google.Protobuf.Protocol;
using UnityEngine;

public class CreatureController : BaseController
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        CheckUpdatedFlag();
    }

    protected virtual void CheckUpdatedFlag()
    {
        if (_updated)
        {
            SendPosInfoPacket();
            _updated = false;
        }
    }
}
