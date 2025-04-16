using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class PlayerController : CreatureController
{
    protected float moveTimer = 0f;
    protected Vector3 initPos;
    protected Vector3 dist;

    public float MoveTimer
    {
        get { return moveTimer; }
        set { moveTimer = value; }
    }

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                SyncPos();
                // moveTimer = 0;
                break;
            case CreatureState.Walk:
                MoveToNextPos();
                break;
        }
    }

    protected virtual void MoveToNextPos()
    {
        string curAnimName = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        if (PosInfo.MoveDir == MoveDir.Up && !(curAnimName == "WALK_UP"))
        {
            return;
        }
        else if (PosInfo.MoveDir == MoveDir.Down && !(curAnimName == "WALK_DOWN"))
        {
            return;
        }
        else if (PosInfo.MoveDir == MoveDir.Left && !(curAnimName == "WALK_LEFT"))
        {
            return;
        }
        else if (PosInfo.MoveDir == MoveDir.Right && !(curAnimName == "WALK_LEFT"))
        {
            return;
        }

        float curAnimLength = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        if (moveTimer == 0)
        {
            initPos = transform.position;
            dist = CellPos - transform.position;
        }

        moveTimer += Time.deltaTime;

        float t = moveTimer / curAnimLength;

        if (t > 1)
            t = 1;

        transform.position = initPos + dist * t;

        if (moveTimer > curAnimLength)
        {
            moveTimer = 0f;
            transform.position = CellPos;
        }
    }

    protected virtual void CheckUpdatedFlag()
    {

    }
}
