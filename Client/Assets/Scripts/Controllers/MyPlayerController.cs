using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MyPlayerController : PlayerController
{
    public PokemonAppearanceTile pkmAppearTile;

    float moveTimerLimit = 0.15f;

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
                ToggleMenu(true);
                break;
            case CreatureState.Walk:
                MoveToNextPos();
                break;
            case CreatureState.WatchMenu:
                ToggleMenu(false);
                break;
            case CreatureState.Fight:

                break;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            C_AddPokemon addPacket = new C_AddPokemon()
            {
                PokemonName = "Pikachu",
                NickName = "PIKAO",
                Level = 5,
                Gender = PokemonGender.Male,
                OwnerName = "CHRIS",
                OwnerId = Id,
                Nature = PokemonNature.Serious,
                Hp = 1,
            };
            C_AddPokemon addPacket2 = new C_AddPokemon()
            {
                PokemonName = "Charmander",
                NickName = "CHAO",
                Level = 1,
                Gender = PokemonGender.Female,
                OwnerName = "CHRIS",
                OwnerId = Id,
                Nature = PokemonNature.Timid,
                Hp = 21,
            };
            C_AddPokemon addPacket3 = new C_AddPokemon()
            {
                PokemonName = "Squirtle",
                NickName = "SKIRT",
                Level = 3,
                Gender = PokemonGender.Male,
                OwnerName = "CHRIS",
                OwnerId = Id,
                Nature = PokemonNature.Bashful,
                Hp = 3,
            };

            Managers.Network.Send(addPacket);
            Managers.Network.Send(addPacket2);
            Managers.Network.Send(addPacket3);
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
        else if (Input.GetKeyDown(KeyCode.S))
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
        else if (Input.GetKeyDown(KeyCode.A))
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
        else if (Input.GetKeyDown(KeyCode.D))
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

        if (Input.GetKey(KeyCode.W))
        {
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveTimer += Time.deltaTime;

            if (moveTimer > moveTimerLimit)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
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
            BaseScene scene = Managers.Scene.CurrentScene;

            if (toggle)
            {
                ((GameScene)scene).ToggleGameMenu(toggle);
                State = CreatureState.WatchMenu;
            }
            else
            {
                ((GameScene)scene).ToggleGameMenu(toggle);
                State = CreatureState.Idle;
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

            // 몬스터 스폰
            if (pkmAppearTile != null)
            {
                if (pkmAppearTile.AppearPokemon())
                    return;
            }

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
                SendPosInfoPacket();
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

        CheckUpdatedFlag();
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