using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MyPlayerController : PlayerController
{
    bool _isLoading = false;
    float moveTimerLimit = 0.15f;
    BaseScene _scene;
    PokemonAppearanceTile _pokemonTile;

    public bool IsLoading { set {  _isLoading = value; } }
    public PokemonAppearanceTile PokemonTile {  set { _pokemonTile = value; } }

    protected override void Start()
    {
        base.Start();

        _scene = Managers.Scene.CurrentScene;
    }

    protected override void Init()
    {
        base.Init();
        Application.wantsToQuit += OnApplicationWantsToQuit;
    }

    void LateUpdate()
    {
        // Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    protected override void UpdateController()
    {
        // base.UpdateController();

        if (_isLoading)
            return;

        switch (State)
        {
            case CreatureState.Idle:
                ChangeDir();
                ChangeToWalk();
                ToggleMenu(true);
                BeginTalk();
                break;
            case CreatureState.Walk:
                MoveToNextPos();
                break;
            case CreatureState.WatchMenu:
                ToggleMenu(false);
                break;
            case CreatureState.Fight:
                break;
            case CreatureState.Talk:
                break;
        }

        CheckUpdatedFlag();
    }

    void ChangeDir()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (Dir == MoveDir.Up)
            {
                State = CreatureState.Walk;
                SetToNextPos();
            }
            else
            {
                Dir = MoveDir.Up;
                C_Move movePacket = new C_Move();
                movePacket.PosInfo = PosInfo;
                Managers.Network.Send(movePacket);
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (Dir == MoveDir.Down)
            {
                State = CreatureState.Walk;
                SetToNextPos();
            }
            else
            {
                Dir = MoveDir.Down;
                C_Move movePacket = new C_Move();
                movePacket.PosInfo = PosInfo;
                Managers.Network.Send(movePacket);
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Dir == MoveDir.Left)
            {
                State = CreatureState.Walk;
                SetToNextPos();
            }
            else
            {
                Dir = MoveDir.Left;
                C_Move movePacket = new C_Move();
                movePacket.PosInfo = PosInfo;
                Managers.Network.Send(movePacket);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Dir == MoveDir.Right)
            {
                State = CreatureState.Walk;
                SetToNextPos();
            }
            else
            {
                Dir = MoveDir.Right;
                C_Move movePacket = new C_Move();
                movePacket.PosInfo = PosInfo;
                Managers.Network.Send(movePacket);
            }
        }
    }

    void ChangeToWalk()
    {
        if (State == CreatureState.Walk)
            return;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            Dir = MoveDir.Up;
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            Dir = MoveDir.Down;
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            Dir = MoveDir.Left;
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Dir = MoveDir.Right;
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit)
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

    void ToggleMenu(bool toggle)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (toggle)
            {
                State = CreatureState.WatchMenu;

                _scene.DoNextAction(State);
            }
            else
            {
                State = CreatureState.Idle;

                _scene.DoNextAction(State);
            }
        }
    }

    void BeginTalk()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (!_isLoading)
            {
                _isLoading = true;

                C_RequestDataById c_RequestDataPacket = new C_RequestDataById();
                c_RequestDataPacket.PlayerId = Managers.Object.MyPlayer.Id;
                c_RequestDataPacket.RequestType = RequestType.CheckObjectInMap;

                Managers.Network.Send(c_RequestDataPacket);
            }
        }
    }

    protected override void MoveToNextPos()
    {
        float curAnimLength = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        if (moveTimer == 0)
        {
            initPos = transform.position;
            destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        }

        moveTimer += Time.deltaTime;

        float t = moveTimer / curAnimLength;

        if (t > 1)
            t = 1;

        transform.position = initPos + ((destPos - initPos) * t);

        if (moveTimer > curAnimLength)
        {
            moveTimer = 0f;
            transform.position = destPos;

            // 몬스터 스폰 (풀숲도 그리드 방식으로 수정이 필요)
            if (_pokemonTile != null)
            {
                if (_pokemonTile.AppearPokemon())
                {
                    State = CreatureState.Fight;
                    _scene.DoNextAction(State);
                    return;
                }
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                Dir = MoveDir.Up;
                SetToNextPos();
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                Dir = MoveDir.Down;
                SetToNextPos();
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                Dir = MoveDir.Left;
                SetToNextPos();
            }
            else if (Input.GetKey(KeyCode.RightArrow))
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
        if (Managers.Map.CanGo(destPos))
        {
            if (Managers.Object.FindCreature(destPos) == null)
            {
                CellPos = destPos;
            }
        }
    }

    protected override void CheckUpdatedFlag()
    {
        if (_updated)
        {
            SendPosInfoPacket();
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