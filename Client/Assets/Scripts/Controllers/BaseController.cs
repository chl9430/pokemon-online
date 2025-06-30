using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngineInternal;

public class BaseController : MonoBehaviour
{
    PositionInfo _positionInfo = new PositionInfo();

    protected bool _updated = false;
    protected Animator _animator;
    protected SpriteRenderer _sprite;

    public int Id { get; set; }
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
                return;

            CellPos = new Vector3Int(value.PosX, value.PosY, 0);
            State = value.State;
            Dir = value.MoveDir;
        }
    }
    public Vector3Int CellPos
    {
        get { return new Vector3Int(PosInfo.PosX, PosInfo.PosY, 0); }

        set
        {
            if (PosInfo.PosX == value.x && PosInfo.PosY == value.y)
                return;

            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            _updated = true;
        }
    }
    public CreatureState State
    {
        get { return PosInfo.State; }

        set
        {
            if (PosInfo.State == value)
                return;

            PosInfo.State = value;
            UpdateAnimation();
            _updated = true;
        }
    }
    public MoveDir Dir
    {
        get { return PosInfo.MoveDir; }
        set
        {
            // lastDir = Dir;
            if (PosInfo.MoveDir == value)
                return;

            PosInfo.MoveDir = value;
            UpdateAnimation();
            _updated = true;
        }
    }

    protected virtual void Start()
    {
        Init();
    }

    void Update()
    {
        UpdateController();
    }

    protected virtual void Init()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;

        UpdateAnimation();
    }

    protected virtual void UpdateController()
    {
    }

    protected virtual void UpdateAnimation()
    {
        if (_animator == null || _sprite == null)
            return;

        if (State == CreatureState.Idle)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("IDLE_UP");
                    break;
                case MoveDir.Down:
                    _animator.Play("IDLE_DOWN");
                    break;
                case MoveDir.Left:
                    _sprite.flipX = false;
                    _animator.Play("IDLE_LEFT");
                    break;
                case MoveDir.Right:
                    _sprite.flipX = true;
                    _animator.Play("IDLE_LEFT");
                    break;
            }
        }
        else if (State == CreatureState.Walk)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("WALK_UP");
                    break;
                case MoveDir.Down:
                    _animator.Play("WALK_DOWN");
                    break;
                case MoveDir.Left:
                    _sprite.flipX = false;
                    _animator.Play("WALK_LEFT");
                    break;
                case MoveDir.Right:
                    _sprite.flipX = true;
                    _animator.Play("WALK_LEFT");
                    break;
            }
        }
        else
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("IDLE_UP");
                    break;
                case MoveDir.Down:
                    _animator.Play("IDLE_DOWN");
                    break;
                case MoveDir.Left:
                    _sprite.flipX = false;
                    _animator.Play("IDLE_LEFT");
                    break;
                case MoveDir.Right:
                    _sprite.flipX = true;
                    _animator.Play("IDLE_LEFT");
                    break;
            }
        }
    }

    protected virtual void SendPosInfoPacket()
    {
        C_Move movePacket = new C_Move();
        movePacket.PosInfo = PosInfo;
        Managers.Network.Send(movePacket);
    }

    public void SyncPos()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = destPos;
    }
}