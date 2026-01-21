using Google.Protobuf;
using Google.Protobuf.Protocol;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MyPlayerController : PlayerController
{
    bool _isLoading = false;
    float moveTimerLimit = 0.15f;
    int _money;
    List<Pokemon> _myPokemons = new List<Pokemon>();
    Dictionary<ItemCategory, List<Item>> _items = new Dictionary<ItemCategory, List<Item>>();
    CreatureController _npc;

    public bool IsLoading { set { _isLoading = value; } }
    public int Money { get { return _money; } }
    public List<Pokemon> MyPokemons { set { _myPokemons = value; } get { return _myPokemons; } }
    public Dictionary<ItemCategory, List<Item>> Items { set { _items = value; } get { return _items; } }
    public CreatureController NPC { get  { return _npc; } set { _npc = value; } }

    public void SetMyPlayerInfo(PlayerInfo playerInfo)
    {
        name = $"{playerInfo.PlayerName}_{playerInfo.ObjectInfo.ObjectId}";

        _name = playerInfo.PlayerName;
        _gender = playerInfo.PlayerGender;

        _money = playerInfo.Money;

        if (playerInfo.NpcInfo != null)
        {
            CreatureController npc = Managers.Object.FindById(playerInfo.NpcInfo.ObjectInfo.ObjectId).GetComponent<CreatureController>();
            _npc = npc;
        }

        // 포켓몬 채우기
        foreach (PokemonSummary pokemonSum in playerInfo.PokemonSums)
            _myPokemons.Add(new Pokemon(pokemonSum));

        // 아이템 채우기
        foreach (var pair in playerInfo.Inventory)
        {
            ItemCategory itemCategory = (ItemCategory)pair.Key;
            _items[itemCategory] = new List<Item>();

            List<ItemSummary> categoryItems = new List<ItemSummary>(pair.Value.CategoryItemSums);

            foreach (ItemSummary itemSum in categoryItems)
            {
                _items[itemCategory].Add(new Item(itemSum));
            }
        }
    }

    public void AddPokemon(Pokemon pokemon)
    {
        _myPokemons.Add(pokemon);
    }

    public void SwitchPokemon(int from, int to)
    {
        Pokemon pokemon = _myPokemons[from];
        _myPokemons[from] = _myPokemons[to];
        _myPokemons[to] = pokemon;
    }

    public void ChangeMoney(int mmoney)
    {
        _money = mmoney;
    }

    public void AddItem(IMessage packet, Item newItem)
    {
        S_BuyItem buyPacket = packet as S_BuyItem;
        bool createNewIdx = buyPacket.CreateNewIdx;
        int newItemCnt = buyPacket.NewItemCnt;
        int foundItemIdx = buyPacket.FoundItemIdx;
        int foundItemCnt = buyPacket.FoundItemCnt;

        ItemCategory itemCategory = newItem.ItemCategory;

        List<Item> categoryItems = _items[itemCategory];

        if (foundItemIdx != -1)
        {
            if (createNewIdx)
            {
                categoryItems[foundItemIdx].ItemCnt = foundItemCnt;

                newItem.ItemCnt = newItemCnt;
                categoryItems.Add(newItem);
            }
            else
            {
                categoryItems[foundItemIdx].ItemCnt = foundItemCnt;
            }
        }
        else
        {
            newItem.ItemCnt = newItemCnt;
            categoryItems.Add(newItem);
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Init()
    {
        base.Init();
        Application.wantsToQuit += OnApplicationWantsToQuit;
    }

    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    protected override void UpdateController()
    {
        base.UpdateController();

        switch (State)
        {
            case CreatureState.Idle:
                ChangeDir();
                ChangeToWalk();
                ToggleMenu();
                BeginTalk();
                break;
        }

        CheckUpdatedFlag();
    }

    void ChangeDir()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            CheckDir(MoveDir.Up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CheckDir(MoveDir.Down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CheckDir(MoveDir.Left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CheckDir(MoveDir.Right);
        }
    }

    void CheckDir(MoveDir moveDir)
    {
        if (Dir == moveDir)
        {
            State = CreatureState.Walk;
            SetToNextPos();
        }
        else
        {
            Dir = moveDir;

            if (State == CreatureState.Idle)
                return;
            else if (State == CreatureState.Walk)
            {
                State = CreatureState.Walk;
                SetToNextPos();
            }
        }
    }

    void ChangeToWalk()
    {
        if (State == CreatureState.Walk)
            return;

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

        if (Input.GetKey(KeyCode.UpArrow))
        {
            Dir = MoveDir.Up;
            moveTimer += Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            Dir = MoveDir.Down;
            moveTimer += Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            Dir = MoveDir.Left;
            moveTimer += Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Dir = MoveDir.Right;
            moveTimer += Time.deltaTime;
        }
        else
        {
            moveTimer = 0;
            return;
        }

        if (Managers.Map.IsDoor(destPos))
        {
            State = CreatureState.NoneState;

            ((GameScene)Managers.Scene.CurrentScene).SaveEnterScenePacket();
        }
        else
        {
            if (moveTimer > moveTimerLimit)
            {
                moveTimer = 0;
                State = CreatureState.Walk;
                SetToNextPos();
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
        if (Managers.Map.CanGo(destPos) && Managers.Object.FindCreature(destPos) == null)
        {
            if (Managers.Map.IsDoor(destPos))
            {
                State = CreatureState.NoneState;

                ((GameScene)Managers.Scene.CurrentScene).SaveEnterScenePacket();
            }
            else
                CellPos = destPos;
        }
    }

    void ToggleMenu()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            State = CreatureState.WatchMenu;

            GameContentManager.Instance.OpenGameMenu();
        }
    }

    void BeginTalk()
    {
        if (Input.GetKeyDown(KeyCode.D))
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

            GameObject obj = Managers.Object.FindCreature(destPos);

            if (obj != null)
            {
                Managers.Scene.CurrentScene.ContentStack.Push(obj.GetComponent<ObjectContents>());

                if (!_isLoading)
                {
                    _isLoading = true;

                    C_RequestDataById c_RequestDataPacket = new C_RequestDataById();
                    c_RequestDataPacket.PlayerId = Id;
                    c_RequestDataPacket.RequestType = RequestType.CheckObjectInMap;

                    Managers.Network.Send(c_RequestDataPacket);
                }
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

            if (((GameScene)Managers.Scene.CurrentScene).DidMeetWildPokemon() == false)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    CheckDir(MoveDir.Up);
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    CheckDir(MoveDir.Down);
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    CheckDir(MoveDir.Left);
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    CheckDir(MoveDir.Right);
                }
                else
                {
                    State = CreatureState.Idle;
                }
            }
            else
                State = CreatureState.Fight;
        }
    }

    protected override void CheckUpdatedFlag()
    {
        base.CheckUpdatedFlag();
    }

    bool OnApplicationWantsToQuit()
    {
        C_ExitGame exitPacket = new C_ExitGame();
        exitPacket.ObjectId = Id;
        Managers.Network.Send(exitPacket);

        return true;
    }
}