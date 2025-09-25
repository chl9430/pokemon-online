using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MyPlayerController : PlayerController
{
    bool _isLoading = false;
    float moveTimerLimit = 0.15f;
    BaseScene _scene;
    IMessage _packet;

    public bool IsLoading { set {  _isLoading = value; } }
    public IMessage Packet { set  { _packet = value; } }

    void Awake()
    {
        _isLoading = true;
    }

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
                // 문 검사
                if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                {
                    MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                    if (destDir == MoveDir.Up)
                    {
                        _isLoading = true;

                        ((GameScene)_scene).SaveEnterScenePacket();
                    }
                }
                else
                {
                    State = CreatureState.Walk;
                    SetToNextPos();
                }
            }
            else
            {
                Dir = MoveDir.Up;

                // 문 검사
                if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                {
                    MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                    if (destDir == MoveDir.Up)
                    {
                        State = CreatureState.Idle;
                        _isLoading = true;

                        ((GameScene)_scene).SaveEnterScenePacket();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (Dir == MoveDir.Down)
            {
                // 문 검사
                if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                {
                    MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                    if (destDir == MoveDir.Down)
                    {
                        _isLoading = true;

                        ((GameScene)_scene).SaveEnterScenePacket();
                    }
                }
                else
                {
                    State = CreatureState.Walk;
                    SetToNextPos();
                }
            }
            else
            {
                Dir = MoveDir.Down;

                // 문 검사
                if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                {
                    MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                    if (destDir == MoveDir.Down)
                    {
                        State = CreatureState.Idle;
                        _isLoading = true;

                        ((GameScene)_scene).SaveEnterScenePacket();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Dir == MoveDir.Left)
            {
                // 문 검사
                if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                {
                    MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                    if (destDir == MoveDir.Left)
                    {
                        _isLoading = true;

                        ((GameScene)_scene).SaveEnterScenePacket();
                    }
                }
                else
                {
                    State = CreatureState.Walk;
                    SetToNextPos();
                }
            }
            else
            {
                Dir = MoveDir.Left;

                // 문 검사
                if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                {
                    MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                    if (destDir == MoveDir.Left)
                    {
                        State = CreatureState.Idle;
                        _isLoading = true;

                        ((GameScene)_scene).SaveEnterScenePacket();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Dir == MoveDir.Right)
            {
                // 문 검사
                if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                {
                    MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                    if (destDir == MoveDir.Right)
                    {
                        _isLoading = true;

                        ((GameScene)_scene).SaveEnterScenePacket();
                    }
                }
                else
                {
                    State = CreatureState.Walk;
                    SetToNextPos();
                }
            }
            else
            {
                Dir = MoveDir.Right;

                // 문 검사
                if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                {
                    MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                    if (destDir == MoveDir.Right)
                    {
                        State = CreatureState.Idle;
                        _isLoading = true;

                        ((GameScene)_scene).SaveEnterScenePacket();
                    }
                }
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

        // 한칸 이동이 끝났다면
        if (moveTimer > curAnimLength)
        {
            moveTimer = 0f;
            transform.position = destPos;

            if (((GameScene)_scene).DidMeetWildPokemon() == false)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    if (Dir == MoveDir.Up)
                    {
                        // 문 검사
                        if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                        {
                            MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                            if (destDir == MoveDir.Up)
                            {
                                State = CreatureState.Idle;
                                _isLoading = true;

                                ((GameScene)_scene).SaveEnterScenePacket();
                            }
                            else
                                SetToNextPos();
                        }
                        else
                        {
                            State = CreatureState.Walk;
                            SetToNextPos();
                        }
                    }
                    else
                    {
                        Dir = MoveDir.Up;

                        // 문 검사
                        if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                        {
                            MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                            if (destDir == MoveDir.Up)
                            {
                                State = CreatureState.Idle;
                                _isLoading = true;

                                ((GameScene)_scene).SaveEnterScenePacket();
                            }
                            else
                                SetToNextPos();
                        }
                        else
                        {
                            State = CreatureState.Walk;
                            SetToNextPos();
                        }
                    }
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    if (Dir == MoveDir.Down)
                    {
                        // 문 검사
                        if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                        {
                            MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                            if (destDir == MoveDir.Down)
                            {
                                State = CreatureState.Idle;
                                _isLoading = true;

                                ((GameScene)_scene).SaveEnterScenePacket();
                            }
                            else
                                SetToNextPos();
                        }
                        else
                        {
                            State = CreatureState.Walk;
                            SetToNextPos();
                        }
                    }
                    else
                    {
                        Dir = MoveDir.Down;

                        // 문 검사
                        if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                        {
                            MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                            if (destDir == MoveDir.Down)
                            {
                                State = CreatureState.Idle;
                                _isLoading = true;

                                ((GameScene)_scene).SaveEnterScenePacket();
                            }
                            else
                                SetToNextPos();
                        }
                        else
                        {
                            State = CreatureState.Walk;
                            SetToNextPos();
                        }
                    }
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    if (Dir == MoveDir.Left)
                    {
                        // 문 검사
                        if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                        {
                            MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                            if (destDir == MoveDir.Left)
                            {
                                State = CreatureState.Idle;
                                _isLoading = true;

                                ((GameScene)_scene).SaveEnterScenePacket();
                            }
                            else
                                SetToNextPos();
                        }
                        else
                        {
                            State = CreatureState.Walk;
                            SetToNextPos();
                        }
                    }
                    else
                    {
                        Dir = MoveDir.Left;

                        // 문 검사
                        if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                        {
                            MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                            if (destDir == MoveDir.Left)
                            {
                                State = CreatureState.Idle;
                                _isLoading = true;

                                ((GameScene)_scene).SaveEnterScenePacket();
                            }
                            else
                                SetToNextPos();
                        }
                        else
                        {
                            State = CreatureState.Walk;
                            SetToNextPos();
                        }
                    }
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    if (Dir == MoveDir.Right)
                    {
                        // 문 검사
                        if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                        {
                            MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                            if (destDir == MoveDir.Right)
                            {
                                State = CreatureState.Idle;
                                _isLoading = true;

                                ((GameScene)_scene).SaveEnterScenePacket();
                            }
                            else
                                SetToNextPos();
                        }
                        else
                        {
                            State = CreatureState.Walk;
                            SetToNextPos();
                        }
                    }
                    else
                    {
                        Dir = MoveDir.Right;

                        // 문 검사
                        if (Managers.Map.IsDoor(CellPos) && _packet is S_GetDoorDestDir)
                        {
                            MoveDir destDir = ((S_GetDoorDestDir)_packet).DestDir;

                            if (destDir == MoveDir.Right)
                            {
                                State = CreatureState.Idle;
                                _isLoading = true;

                                ((GameScene)_scene).SaveEnterScenePacket();
                            }
                            else
                                SetToNextPos();
                        }
                        else
                        {
                            State = CreatureState.Walk;
                            SetToNextPos();
                        }
                    }
                }
                else
                {
                    State = CreatureState.Idle;
                }
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