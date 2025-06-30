using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public enum BattleSceneState
{
    NONE = 0,
    INTRO = 1,
    APPEAR_SCRIPTING = 2,
    CHANGING_POKEMON = 3,
    SHOWING_POKEMON = 4,
    SELECTING_ACTION = 5,
    SELECTING_MOVE = 6,
    CANNOT_USE_MOVE = 7,
    ATTACK_INSTRUCTING = 8,
    FIRST_ATTACK_ANIMATION = 9,
    FIRST_ATTACK_FAILED = 10,
    HIT_POKEMON_BLINK = 11,
    CHANGE_POKEMON_HP = 12,
    POKEMON_DIE = 13,
    MY_POKEMON_DIE_SCRIPTING = 14,
    ENEMY_POKEMON_DIE_SCRIPTING = 15,
    GETTING_EXP = 16,
    LEVEL_UP_SCRIPTING = 17,
    UPGRADING_STATUS = 18,
    SHOWING_UPGRADED_STATUS = 19,
    AFTER_USE_ITEM_UPDATE = 20,
    ITEM_USE_SCRIPTING = 21,
}

public class BattleScene : BaseScene
{
    BattleSceneState _sceneState = BattleSceneState.NONE;
    TextMeshProUGUI[] _moveInfoTMPs;
    PlayableDirector _playableDirector;
    PlayerInfo _playerInfo;
    List<Pokemon> _myPokemons;

    Pokemon _myPokemon;
    Pokemon _enemyPokemon;
    Item _usedItem;

    Pokemon _attackPKM;
    BattleArea _attackPKMArea;
    Pokemon _defensePKM;
    BattleArea _defensePKMArea;
    bool _isMyPKMAttack = false;
    bool _isEnemyPKMAttack = false;
    bool _loadingPacket = false;

    int _remainEXPToGet;

    [SerializeField] ScriptBoxUI _scriptBox;
    [SerializeField] GridLayoutSelectBox _actionSelectBox;
    [SerializeField] List<DynamicButton> _actionBtns;
    [SerializeField] GridLayoutSelectBox _moveSelectBox;
    [SerializeField] DynamicButton _moveBtn;
    List<DynamicButton> _moveBtns;
    [SerializeField] GameObject _moveInfoBox;
    [SerializeField] BattleArea _enemyPokemonArea;
    [SerializeField] BattleArea _myPokemonArea;
    [SerializeField] StatusBox _statusBox;

    public BattleSceneState SceneState
    {
        set
        {
            _sceneState = value;

            // 액션 선택 박스
            if (_sceneState == BattleSceneState.SELECTING_ACTION)
            {
                _actionSelectBox.gameObject.SetActive(true);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
            }
            else
            {
                _actionSelectBox.gameObject.SetActive(false);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;
            }

            // 기술 선택 박스
            if (_sceneState == BattleSceneState.SELECTING_MOVE)
            {
                _moveSelectBox.gameObject.SetActive(true);
                _moveSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;

                _moveInfoBox.SetActive(true);
            }
            else
            {
                _moveSelectBox.gameObject.SetActive(false);
                _moveSelectBox.UIState = GridLayoutSelectBoxState.NONE;

                _moveInfoBox.SetActive(false);
            }

            // 레벨 업 스텟 표시 박스
            if (_sceneState == BattleSceneState.UPGRADING_STATUS)
            {
                _statusBox.gameObject.SetActive(true);
            }
            else
            {
                _statusBox.gameObject.SetActive(false);
            }
        }
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Battle;

        _moveBtns = new List<DynamicButton>();
    }

    protected override void Start()
    {
        _playableDirector = GetComponent<PlayableDirector>();
        _moveInfoTMPs = _moveInfoBox.GetComponentsInChildren<TextMeshProUGUI>();

        // 테스트 시 사용.
        if (Managers.Network.Packet == null)
        {
            C_EnterPokemonBattleScene enterBattlePacket = new C_EnterPokemonBattleScene();
            enterBattlePacket.PlayerId = -1;
            enterBattlePacket.LocationNum = 1;

            Managers.Network.Send(enterBattlePacket);
        }
        else
            Managers.Network.SendSavedPacket();
    }

    void MakeUI()
    {
        // 액션 버튼 데이터 채우기
        for (int i = 0; i < _actionBtns.Count; i++)
        {
            _actionBtns[i].BtnData = Util.FindChild<TextMeshProUGUI>(_actionBtns[i].gameObject, "ContentText", true).text;
        }
        _actionSelectBox.SetSelectBoxContent(_actionBtns, 2, 2);

        // 기술 버튼 데이터 채우기
        for (int i = 0; i < _myPokemon.PokemonMoves.Count; i++)
        {
            _moveBtns.Add(GameObject.Instantiate(_moveBtn, _moveSelectBox.transform));
            Util.FindChild<TextMeshProUGUI>(_moveBtns[i].gameObject, "ContentText", true).text = _myPokemon.PokemonMoves[i].MoveName;
            _moveBtns[i].BtnData = _myPokemon.PokemonMoves[i];
        }
        _moveSelectBox.SetSelectBoxContent(_moveBtns, 2, 2);
    }

    public override void UpdateData(IMessage packet)
    {
        switch (_sceneState)
        {
            case BattleSceneState.NONE:
                {
                    if (packet is S_UseItem)
                    {
                        _enterEffect.PlayEffect("FadeIn");

                        S_UseItem useItemPacket = packet as S_UseItem;

                        PlayerInfo playerInfo = useItemPacket.PlayerInfo;
                        PokemonSummary enemyPokemon = useItemPacket.EnemyPokemon;
                        IList myPokmeonSums = useItemPacket.PlayerPokemons;
                        ItemSummary itemSumamary = useItemPacket.UsedItem;

                        // 포켓몬 및 플레이어 데이터 채우기
                        _playerInfo = playerInfo;

                        _myPokemons = new List<Pokemon>();

                        foreach (PokemonSummary pokemonSum in myPokmeonSums)
                        {
                            _myPokemons.Add(new Pokemon(pokemonSum));
                        }

                        // 포켓몬 랜더링
                        _myPokemonArea.FillPokemonInfo(_myPokemon, true);

                        _myPokemonArea.Pokemon = _myPokemons[0];
                        _myPokemon = _myPokemonArea.Pokemon;

                        _enemyPokemonArea.FillPokemonInfo(new Pokemon(enemyPokemon), false);
                        _enemyPokemon = _enemyPokemonArea.Pokemon;

                        // UI 생성
                        MakeUI();

                        SceneState = BattleSceneState.AFTER_USE_ITEM_UPDATE;

                        _usedItem = new Item(itemSumamary);
                    }
                    else if (packet is S_EnterPokemonBattleScene)
                    {
                        _enterEffect.PlayEffect("FadeIn");
                        _playableDirector.Play();

                        S_EnterPokemonBattleScene enterBattleScenePacket = packet as S_EnterPokemonBattleScene;

                        PlayerInfo playerInfo = enterBattleScenePacket.PlayerInfo;
                        PokemonSummary enemyPokemon = enterBattleScenePacket.EnemyPokemon;
                        IList myPokmeonSums = enterBattleScenePacket.PlayerPokemons;

                        // 포켓몬 및 플레이어 데이터 채우기
                        _playerInfo = playerInfo;

                        _myPokemons = new List<Pokemon>();

                        foreach (PokemonSummary pokemonSum in myPokmeonSums)
                        {
                            _myPokemons.Add(new Pokemon(pokemonSum));
                        }

                        // 포켓몬 랜더링
                        _myPokemonArea.FillTrainerImage(playerInfo.PlayerGender);

                        _myPokemonArea.Pokemon = _myPokemons[0];
                        _myPokemon = _myPokemonArea.Pokemon;

                        _enemyPokemonArea.FillPokemonInfo(new Pokemon(enemyPokemon), false);
                        _enemyPokemon = _enemyPokemonArea.Pokemon;

                        // UI 생성
                        MakeUI();
                    }
                }
                break;
            case BattleSceneState.SELECTING_MOVE:
                {
                    S_UsePokemonMove useMovePacket = packet as S_UsePokemonMove;
                    int remainedPP = useMovePacket.RemainedPP;

                    SceneState = BattleSceneState.ATTACK_INSTRUCTING;
                    _loadingPacket = false;

                    _attackPKM.SelectedMove.CurPP = remainedPP;

                    List<string> scripts = new List<string>()
                    {
                        $"{_attackPKM.PokemonInfo.NickName} used {_attackPKM.SelectedMove.MoveName}!"
                    };

                    _scriptBox.BeginScriptTyping(scripts, true);
                }
                break;
            case BattleSceneState.HIT_POKEMON_BLINK:
                {
                    S_ChangePokemonHp changeHpPacket = packet as S_ChangePokemonHp;
                    SceneState = BattleSceneState.CHANGE_POKEMON_HP;

                    _loadingPacket = false;

                    int remainedHp = changeHpPacket.RemainedHp;

                    _defensePKM.PokemonStat.Hp = remainedHp;

                    _defensePKMArea.ChangePokemonHP(_defensePKM.PokemonStat.Hp);
                }
                break;
            case BattleSceneState.POKEMON_DIE:
                {
                    S_GetEnemyPokemonExp getExpPacket = packet as S_GetEnemyPokemonExp;
                    int exp = getExpPacket.Exp;
                    SceneState = BattleSceneState.ENEMY_POKEMON_DIE_SCRIPTING;

                    _loadingPacket = false;

                    _remainEXPToGet = exp;

                    List<string> scripts = new List<string>()
                    {
                        $"{_defensePKM.PokemonInfo.NickName} fell down!",
                        $"{_attackPKM.PokemonInfo.NickName} got {_remainEXPToGet} exp!"
                    };

                    _scriptBox.BeginScriptTyping(scripts);
                }
                break;
            case BattleSceneState.ENEMY_POKEMON_DIE_SCRIPTING:
                {
                    SceneState = BattleSceneState.GETTING_EXP;

                    _loadingPacket = false;

                    S_ChangePokemonExp changeExpPacket = packet as S_ChangePokemonExp;
                    PokemonExpInfo expInfo = changeExpPacket.PokemonExpInfo;

                    _myPokemon.PokemonExpInfo = expInfo;

                    _myPokemonArea.ChangePokemonEXP(_myPokemon.PokemonExpInfo.CurExp);
                }
                break;
            case BattleSceneState.GETTING_EXP:
                {
                    S_ChangePokemonLevel changeLevel = packet as S_ChangePokemonLevel;
                    int pokemonLevel = changeLevel.PokemonLevel;
                    PokemonStat stat = changeLevel.PokemonStat;
                    LevelUpStatusDiff statDiff = changeLevel.StatDiff;
                    int remainLevelExp = changeLevel.PokemonRemainLevelExp;
                    int curExp = changeLevel.PokemonCurExp;

                    SceneState = BattleSceneState.LEVEL_UP_SCRIPTING;

                    _loadingPacket = false;

                    _myPokemon.PokemonInfo.Level = pokemonLevel;
                    _myPokemon.PokemonStat = stat;
                    _myPokemon.PokemonExpInfo.RemainExpToNextLevel = remainLevelExp;
                    _myPokemon.PokemonExpInfo.CurExp = curExp;

                    _myPokemonArea.FillPokemonInfo(_myPokemon, true);
                    _statusBox.SetStatusDiffRate(statDiff);

                    List<string> scripts = new List<string>()
                    {
                        $"{_myPokemon.PokemonInfo.NickName}'s level went up to {_myPokemon.PokemonInfo.Level}."
                    };

                    _scriptBox.BeginScriptTyping(scripts);
                }
                break;
            case BattleSceneState.SHOWING_UPGRADED_STATUS:
                {
                    SceneState = BattleSceneState.GETTING_EXP;

                    _loadingPacket = false;

                    S_ChangePokemonExp changeExpPacket = packet as S_ChangePokemonExp;
                    PokemonExpInfo expInfo = changeExpPacket.PokemonExpInfo;

                    _myPokemon.PokemonExpInfo = expInfo;

                    _myPokemonArea.ChangePokemonEXP(_myPokemon.PokemonExpInfo.CurExp);
                }
                break;
        }
    }

    public override void DoNextAction(object value = null)
    {
        switch (_sceneState)
        {
            case BattleSceneState.NONE:
                {
                    SceneState = BattleSceneState.INTRO;
                }
                break;
            case BattleSceneState.INTRO:
                {
                    _playableDirector.Pause();

                    List<string> scripts = new List<string>()
                    {
                        $"Wild {_enemyPokemon.PokemonInfo.PokemonName} appeard!",
                        $"Go! {_myPokemon.PokemonInfo.NickName}!",
                    };

                    _scriptBox.BeginScriptTyping(scripts);
                    SceneState = BattleSceneState.APPEAR_SCRIPTING;
                }
                break;
            case BattleSceneState.APPEAR_SCRIPTING:
                {
                    _playableDirector.Resume();
                    SceneState = BattleSceneState.CHANGING_POKEMON;
                }
                break;
            case BattleSceneState.CHANGING_POKEMON:
                {
                    _myPokemonArea.FillPokemonInfo(_myPokemon, true);

                    SceneState = BattleSceneState.SHOWING_POKEMON;
                }
                break;
            case BattleSceneState.SHOWING_POKEMON:
                {
                    SceneState = BattleSceneState.SELECTING_ACTION;

                    _scriptBox.SetScriptWihtoutTyping($"What will {_myPokemon.PokemonInfo.NickName} do?");
                }
                break;
            case BattleSceneState.CANNOT_USE_MOVE:
                {
                    SceneState = BattleSceneState.SELECTING_MOVE;
                }
                break;
            case BattleSceneState.SELECTING_ACTION:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            string selectedAction = _actionSelectBox.GetSelectedBtnData() as string;

                            if (selectedAction == "Fight")
                            {
                                // 내 포켓몬에 사용 가능한 기술이 있는 지 확인한다.
                                List<PokemonMove> myPKMAvailableMoves = FindAvailableMove(_myPokemon);

                                if (myPKMAvailableMoves.Count > 0)
                                {
                                    SceneState = BattleSceneState.SELECTING_MOVE;
                                }
                                else
                                {
                                    SceneState = BattleSceneState.ATTACK_INSTRUCTING;

                                    PokemonMoveSummary struggleMove = new PokemonMoveSummary()
                                    {
                                        MaxPP = 9999,
                                        MovePower = 50,
                                        MoveAccuracy = 100,
                                        MoveName = "Struggle",
                                        MoveType = PokemonType.Normal,
                                        MoveCategory = MoveCategory.Physical,
                                    };

                                    SetSelectedMove(new PokemonMove(struggleMove));

                                    SetAttackAndDefensePokemonArea(ref _myPokemonArea, ref _enemyPokemonArea);

                                    List<string> scripts = new List<string>()
                                    {
                                        $"{_myPokemon.PokemonInfo.NickName} has no move to use!",
                                        $"{_attackPKM.PokemonInfo.NickName} used {_attackPKM.SelectedMove.MoveName}!"
                                    };

                                    _scriptBox.BeginScriptTyping(scripts, true);

                                    _enemyPokemon.SelectedMove.CurPP--;
                                }
                            }
                            else if (selectedAction == "Bag")
                            {
                                C_EnterPlayerBagScene enterBagPacket = new C_EnterPlayerBagScene();
                                enterBagPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(enterBagPacket);

                                // Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.Bag);
                            }
                        }
                    }
                }
                break;
            case BattleSceneState.SELECTING_MOVE:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            PokemonMove selectedMove = _moveSelectBox.GetSelectedBtnData() as PokemonMove;

                            if (selectedMove.CurPP == 0)
                            {
                                SceneState = BattleSceneState.CANNOT_USE_MOVE;

                                List<string> scripts = new List<string>()
                                {
                                    $"Cannot use this move!"
                                };

                                _scriptBox.BeginScriptTyping(scripts);
                            }
                            else
                            {
                                SetAttackAndDefensePokemonArea(ref _myPokemonArea, ref _enemyPokemonArea);
                                SetSelectedMove(selectedMove);

                                if (!_loadingPacket)
                                {
                                    // 서버에게 기술 사용 요청
                                    C_UsePokemonMove movePacket = new C_UsePokemonMove();
                                    movePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                    if (_attackPKM == _myPokemon)
                                        movePacket.PokemonOrder = _myPokemons.FindIndex(pokemon => pokemon == _myPokemon);
                                    else
                                        movePacket.PokemonOrder = -1;

                                    movePacket.MoveOrder = _attackPKM.PokemonMoves.FindIndex(move => move == selectedMove);
                                    movePacket.UsedPP = 1;

                                    Managers.Network.Send(movePacket);

                                    _loadingPacket = true;
                                }
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            SceneState = BattleSceneState.SELECTING_ACTION;
                        }
                    }
                    else
                    {
                        PokemonMove selectedMove = _moveSelectBox.GetSelectedBtnData() as PokemonMove;

                        _moveInfoTMPs[1].text = $"{selectedMove.CurPP.ToString()} / {selectedMove.MaxPP.ToString()}";
                        _moveInfoTMPs[2].text = $"TYPE / {selectedMove.MoveType.ToString()}";
                    }
                }
                break;
            case BattleSceneState.ATTACK_INSTRUCTING:
                {
                    if (_attackPKM.IsHitByAcc())
                    {
                        SceneState = BattleSceneState.FIRST_ATTACK_ANIMATION;

                        _attackPKMArea.AttackMovePokemonUI();
                        _defensePKMArea.TriggerPokemonHitImage(_attackPKM);
                    }
                    else
                    {
                        SceneState = BattleSceneState.FIRST_ATTACK_FAILED;

                        List<string> script = new List<string>()
                        {
                            $"{_attackPKM.PokemonInfo.NickName}'s attack is off the mark!"
                        };

                        _scriptBox.BeginScriptTyping(script, true);
                    }
                }
                break;
            case BattleSceneState.FIRST_ATTACK_FAILED:
                {
                    PokemonStat stat = _defensePKM.PokemonStat;

                    if (_attackPKMArea == _myPokemonArea)
                        _isMyPKMAttack = true;
                    else
                        _isEnemyPKMAttack = true;

                    if (!_isMyPKMAttack || !_isEnemyPKMAttack)
                    {
                        SceneState = BattleSceneState.ATTACK_INSTRUCTING;

                        Pokemon prevAttackPKM = _attackPKM;
                        BattleArea prevAttackPKMArea = _attackPKMArea;

                        _attackPKM = _defensePKM;
                        _attackPKMArea = _defensePKMArea;

                        _defensePKM = prevAttackPKM;
                        _defensePKMArea = prevAttackPKMArea;

                        List<string> scripts = new List<string>()
                        {
                            $"{_attackPKM.PokemonInfo.NickName} used {_attackPKM.SelectedMove.MoveName}!"
                        };

                        _scriptBox.BeginScriptTyping(scripts, true);
                    }
                    else
                    {
                        SceneState = BattleSceneState.SELECTING_ACTION;

                        _isEnemyPKMAttack = false;
                        _isMyPKMAttack = false;

                        _scriptBox.SetScriptWihtoutTyping($"What will {_myPokemon.PokemonInfo.NickName} do?");
                    }
                }
                break;
            case BattleSceneState.FIRST_ATTACK_ANIMATION:
                {
                    SceneState = BattleSceneState.HIT_POKEMON_BLINK;

                    _defensePKMArea.BlinkPokemonUI();
                }
                break;
            case BattleSceneState.HIT_POKEMON_BLINK:
                {
                    if (!_loadingPacket)
                    {
                        C_ChangePokemonHp changeHpPacket = new C_ChangePokemonHp();
                        changeHpPacket.MoveCategory = _attackPKM.SelectedMove.MoveCategory;
                        changeHpPacket.MovePower = _attackPKM.SelectedMove.MovePower;
                        changeHpPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                        changeHpPacket.PokemonOrder = _myPokemons.FindIndex(pokemon => pokemon == _defensePKM);

                        Managers.Network.Send(changeHpPacket);

                        _loadingPacket = true;
                    }
                }
                break;
            case BattleSceneState.CHANGE_POKEMON_HP:
                {
                    StartCoroutine(ActionAfterChangeHP());
                }
                break;
            case BattleSceneState.POKEMON_DIE:
                {
                    // 죽은 포켓몬이 내 포켓몬일 경우
                    if (_defensePKM == _myPokemon)
                    {
                        SceneState = BattleSceneState.MY_POKEMON_DIE_SCRIPTING;

                        List<string> scripts = new List<string>()
                        {
                            $"{_defensePKM.PokemonInfo.NickName} fell down!"
                        };

                        scripts.Add($"There is no more pokemon to fight!");
                        scripts.Add($"......");
                        scripts.Add($"I took the injured Pokemon to the nearby Pokemon Center in a hurry.");

                        _scriptBox.BeginScriptTyping(scripts);
                    }
                    else
                    {
                        if (_myPokemon.PokemonInfo.Level != 100)
                        {
                            if (!_loadingPacket)
                            {
                                C_GetEnemyPokemonExp getExpPacket = new C_GetEnemyPokemonExp();
                                getExpPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                getExpPacket.EnemyPokemonInfo = _enemyPokemon.PokemonInfo;

                                Managers.Network.Send(getExpPacket);

                                _loadingPacket = true;
                            }
                        }
                    }
                }
                break;
            case BattleSceneState.ENEMY_POKEMON_DIE_SCRIPTING:
                {
                    if (_remainEXPToGet != 0)
                    {
                        if (!_loadingPacket)
                        {
                            int finalExp = SetAndApplyFinalEXP();

                            // 서버에도 포켓몬 경험치 획득 요청
                            C_ChangePokemonExp changeExpPacket = new C_ChangePokemonExp();
                            changeExpPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                            changeExpPacket.PokemonOrder = _myPokemons.FindIndex(pokemon => pokemon == _myPokemon);
                            changeExpPacket.Exp = finalExp;

                            Managers.Network.Send(changeExpPacket);

                            _loadingPacket = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Battle Finsh");
                    }
                }
                break;
            case BattleSceneState.GETTING_EXP:
                {
                    if (_myPokemon.PokemonExpInfo.RemainExpToNextLevel == 0)
                    {
                        if (!_loadingPacket)
                        {
                            C_ChangePokemonLevel changeLevelPacket = new C_ChangePokemonLevel();
                            changeLevelPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                            changeLevelPacket.PokemonOrder = _myPokemons.FindIndex(pokemon => pokemon == _myPokemon);

                            Managers.Network.Send(changeLevelPacket);

                            _loadingPacket = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Battle Finsh");
                    }
                }
                break;
            case BattleSceneState.LEVEL_UP_SCRIPTING:
                {
                    SceneState = BattleSceneState.UPGRADING_STATUS;
                }
                break;
            case BattleSceneState.UPGRADING_STATUS:
                {
                    SceneState = BattleSceneState.SHOWING_UPGRADED_STATUS;
                    _statusBox.ShowFinalStat(_myPokemon.PokemonStat);
                }
                break;
            case BattleSceneState.SHOWING_UPGRADED_STATUS:
                {
                    if (_remainEXPToGet != 0)
                    {
                        if (!_loadingPacket)
                        {
                            int finalExp = SetAndApplyFinalEXP();

                            // 서버에도 포켓몬 경험치 획득 요청
                            C_ChangePokemonExp changeExpPacket = new C_ChangePokemonExp();
                            changeExpPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                            changeExpPacket.PokemonOrder = _myPokemons.FindIndex(pokemon => pokemon == _myPokemon);
                            changeExpPacket.Exp = finalExp;

                            Managers.Network.Send(changeExpPacket);

                            _loadingPacket = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Battle Finsh");
                    }
                }
                break;
            case BattleSceneState.AFTER_USE_ITEM_UPDATE:
                {
                    List<string> scripts = new List<string>()
                    {
                        $"{_playerInfo.PlayerName} used {_usedItem.ItemName}!"
                    };

                    _scriptBox.BeginScriptTyping(scripts, true);

                    _sceneState = BattleSceneState.ITEM_USE_SCRIPTING;
                }
                break;
        }
    }

    int SetAndApplyFinalEXP()
    {
        int finalEXP = 0;

        if (_remainEXPToGet > _myPokemon.PokemonExpInfo.RemainExpToNextLevel)
            finalEXP = _myPokemon.PokemonExpInfo.RemainExpToNextLevel;
        else
            finalEXP = _remainEXPToGet;

        _remainEXPToGet -= finalEXP;

        return finalEXP;
    }

    IEnumerator ActionAfterChangeHP()
    {
        yield return new WaitForSeconds(1f);

        PokemonStat stat = _defensePKM.PokemonStat;

        if (_attackPKMArea == _myPokemonArea)
            _isMyPKMAttack = true;
        else
            _isEnemyPKMAttack = true;

        if (stat.Hp <= 0)
        {
            SceneState = BattleSceneState.POKEMON_DIE;

            _defensePKMArea.PokemonDie();
        }
        else if (!_isMyPKMAttack || !_isEnemyPKMAttack)
        {
            SceneState = BattleSceneState.ATTACK_INSTRUCTING;

            Pokemon prevAttackPKM = _attackPKM;
            BattleArea prevAttackPKMArea = _attackPKMArea;

            _attackPKM = _defensePKM;
            _attackPKMArea = _defensePKMArea;

            _defensePKM = prevAttackPKM;
            _defensePKMArea = prevAttackPKMArea;

            List<string> scripts = new List<string>()
            {
                $"{_attackPKM.PokemonInfo.NickName} used {_attackPKM.SelectedMove.MoveName}!"
            };

            _scriptBox.BeginScriptTyping(scripts, true);
        }
        else
        {
            SceneState = BattleSceneState.SELECTING_ACTION;

            _isEnemyPKMAttack = false;
            _isMyPKMAttack = false;

            _scriptBox.SetScriptWihtoutTyping($"What will {_myPokemon.PokemonInfo.NickName} do?");
        }
    }

    List<PokemonMove> FindAvailableMove(Pokemon pokemon)
    {
        List<PokemonMove> availableMoves = new List<PokemonMove>();

        for (int i = 0; i < pokemon.PokemonMoves.Count; i++)
        {
            PokemonMove move = pokemon.PokemonMoves[i];

            if (move.CurPP == 0)
                continue;
            else
                availableMoves.Add(move);
        }

        return availableMoves;
    }

    void SetSelectedMove(PokemonMove myPKMMove)
    {
        _myPokemon.SelectedMove = myPKMMove;

        List<PokemonMove> enemyPKMAvailableMoves = FindAvailableMove(_enemyPokemon);

        if (enemyPKMAvailableMoves.Count > 0)
            _enemyPokemon.SelectedMove = enemyPKMAvailableMoves[Random.Range(0, enemyPKMAvailableMoves.Count)];
        else
        {
            PokemonMoveSummary struggleMove = new PokemonMoveSummary()
            {
                MaxPP = 9999,
                MovePower = 50,
                MoveAccuracy = 100,
                MoveName = "Struggle",
                MoveType = PokemonType.Normal,
                MoveCategory = MoveCategory.Physical,
            };

            _enemyPokemon.SelectedMove = new PokemonMove(struggleMove);
        }
    }

    void SetAttackAndDefensePokemonArea(ref BattleArea firstAttackPKMArea, ref BattleArea secondAttackPKMArea)
    {
        Pokemon firstPokemon = firstAttackPKMArea.Pokemon;
        Pokemon secondPokemon = secondAttackPKMArea.Pokemon;

        int firstPkmSpeed = firstPokemon.PokemonStat.Speed;
        int secondPkmSpeed = secondPokemon.PokemonStat.Speed;

        if (firstPkmSpeed > secondPkmSpeed)
        {
            _attackPKM = firstPokemon;
            _attackPKMArea = firstAttackPKMArea;

            _defensePKM = secondPokemon;
            _defensePKMArea = secondAttackPKMArea;
        }
        else if (firstPkmSpeed < secondPkmSpeed)
        {
            _attackPKM = secondPokemon;
            _attackPKMArea = secondAttackPKMArea;

            _defensePKM = firstPokemon;
            _defensePKMArea = firstAttackPKMArea;
        }
        else
        {
            int ran = Random.Range(0, 100);

            if (ran >= 50)
            {
                _attackPKM = firstPokemon;
                _attackPKMArea = firstAttackPKMArea;

                _defensePKM = secondPokemon;
                _defensePKMArea = secondAttackPKMArea;
            }
            else
            {
                _attackPKM = secondPokemon;
                _attackPKMArea = secondAttackPKMArea;

                _defensePKM = firstPokemon;
                _defensePKMArea = firstAttackPKMArea;
            }
        }
    }

    public override void Clear()
    {
    }
}
