using Google.Protobuf.Protocol;
using System;
using System.Collections;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    [SerializeField] float moveTimer = 0f;
    float moveTimerLimit = 0.15f;

    Vector3 initPos;
    Vector3 dist;

    void LateUpdate()
    {
        // Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    protected override void UpdateController()
    {
        // base.UpdateController();

        switch (State)
        {
            case CreatureState.Idle:
                ChangeDir();
                ChangeToWalk();
                break;
            case CreatureState.Walk:
                MoveToNextPos();
                break;
        }
    }

    protected override void Init()
    {
        base.Init();
        Application.wantsToQuit += OnApplicationWantsToQuit;
    }

    void ChangeDir()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
    }

    void ChangeToWalk()
    {
        if (Input.GetKey(KeyCode.W))
        {
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit || LastDir == MoveDir.Up)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit || LastDir == MoveDir.Down)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit || LastDir == MoveDir.Left)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit || LastDir == MoveDir.Right)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
        else
        {
            moveTimer = 0;
        }
    }

    void MoveToNextPos()
    {
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

            if (Input.GetKey(KeyCode.W))
            {
                Dir = MoveDir.Up;
                SetToNextPos();
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Dir = MoveDir.Down;
                SetToNextPos();
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Dir = MoveDir.Left;
                SetToNextPos();
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Dir = MoveDir.Right;
                SetToNextPos();
            }
            else
            {
                State = CreatureState.Idle;
            }
        }
    }

    void SetToNextPos()
    {
        Vector3Int destPos = CellPos;

        switch (Dir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                destPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
        }

        // 장애물 검사
        CellPos = destPos;

        // CheckUpdatedFlag();
    }

    protected override void CheckUpdatedFlag()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }

    bool OnApplicationWantsToQuit()
    {
        C_ExitGame exitPacket = new C_ExitGame();
        exitPacket.ObjectId = Id;
        Managers.Network.Send(exitPacket);

        return true;
    }
}