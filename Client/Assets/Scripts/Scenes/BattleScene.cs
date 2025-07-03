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
    SHOWING_UI = 5,
    SELECTING_ACTION = 6,
    SELECTING_MOVE = 7,
    CANNOT_USE_MOVE = 8,
    ATTACK_INSTRUCTING = 9,
    FIRST_ATTACK_ANIMATION = 10,
    FIRST_ATTACK_FAILED = 11,
    HIT_POKEMON_BLINK = 12,
    CHANGE_POKEMON_HP = 13,
    POKEMON_DIE = 14,
    MY_POKEMON_DIE_SCRIPTING = 15,
    ENEMY_POKEMON_DIE_SCRIPTING = 16,
    GETTING_EXP = 17,
    LEVEL_UP_SCRIPTING = 18,
    UPGRADING_STATUS = 19,
    SHOWING_UPGRADED_STATUS = 20,
    NEW_MOVE_LEARN_SCRIPTING = 21,
    ASKING_TO_LEARN_NEW_MOVE = 22,
    ANSWERING_TO_LEARN_NEW_MOVE = 23,
    NEW_MOVE_NOT_LEARN_SCRIPTING = 24,
    SWITCH_POKEMON_SCRIPTING = 25,
    AFTER_USE_ITEM_UPDATE = 26,
    ITEM_USE_SCRIPTING = 27,
    MOVING_TO_BAG_SCENE = 90,
    MOVING_TO_POKEMON_SCENE = 91,
}

public class BattleScene : BaseScene
{
    BattleSceneState _sceneState = BattleSceneState.NONE;
    TextMeshProUGUI[] _moveInfoTMPs;
    PlayableDirector _playableDirector;
    PlayerInfo _playerInfo;

    Pokemon _myPokemon;
    Pokemon _enemyPokemon;
    PokemonMove _newLearnableMove;
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
    List<DynamicButton> _moveBtns;
    [SerializeField] DynamicButton _moveBtn;
    [SerializeField] GameObject _moveInfoBox;
    [SerializeField] BattleArea _enemyPokemonArea;
    [SerializeField] BattleArea _myPokemonArea;
    [SerializeField] StatusBox _statusBox;
    [SerializeField] GridLayoutSelectBox _yesOrNoSelectBox;
    [SerializeField] List<DynamicButton> _yesOrNoBtns;

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
            else if (_sceneState == BattleSceneState.MOVING_TO_BAG_SCENE)
            {
                _actionSelectBox.gameObject.SetActive(true);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;
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
            else if (_sceneState == BattleSceneState.SHOWING_UPGRADED_STATUS)
            {
                _statusBox.gameObject.SetActive(true);
            }
            else
            {
                _statusBox.gameObject.SetActive(false);
            }

            // 예 아니오 버튼
            if (_sceneState == BattleSceneState.ANSWERING_TO_LEARN_NEW_MOVE)
            {
                _yesOrNoSelectBox.gameObject.SetActive(true);
                _yesOrNoSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
            }
            else
            {
                _yesOrNoSelectBox.gameObject.SetActive(false);
                _yesOrNoSelectBox.UIState = GridLayoutSelectBoxState.NONE;
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

        // 예, 아니오 버튼 데이터 채우기
        for (int i = 0; i < _yesOrNoBtns.Count; i++)
        {
            _yesOrNoBtns[i].BtnData = Util.FindChild<TextMeshProUGUI>(_yesOrNoBtns[i].gameObject, "ContentText", true).text;
        }
        _yesOrNoSelectBox.SetSelectBoxContent(_yesOrNoBtns, 2, 2);
    }

    public override void UpdateData(IMessage packet)
    {
        switch (_sceneState)
        {
            case BattleSceneState.NONE:
                {
                    if (packet is S_ReturnPokemonBattleScene)
                    {
                        _enterEffect.PlayEffect("FadeIn");
                        _myPokemonArea.SetActiveTrainer(false);

                        S_ReturnPokemonBattleScene returnBattleScenePacket = packet as S_ReturnPokemonBattleScene;

                        PlayerInfo playerInfo = returnBattleScenePacket.PlayerInfo;
                        PokemonSummary enemyPokemonSum = returnBattleScenePacket.EnemyPokemonSum;
                        PokemonSummary myPokemonSum = returnBattleScenePacket.PlayerPokemonSum;

                        // 포켓몬 및 플레이어 데이터 채우기
                        _playerInfo = playerInfo;
                        _enemyPokemon = new Pokemon(enemyPokemonSum);
                        _myPokemon = new Pokemon(myPokemonSum);

                        // 포켓몬 랜더링
                        _enemyPokemonArea.MakeBattlePokemon(_enemyPokemon, false);
                        _myPokemonArea.MakeBattlePokemon(_myPokemon, true);

                        // UI 생성
                        MakeUI();

                        SceneState = BattleSceneState.SELECTING_ACTION;
                    }
                    else if (packet is S_SwitchBattlePokemon)
                    {
                        _enterEffect.PlayEffect("FadeIn");
                        _myPokemonArea.SetActiveTrainer(false);

                        S_SwitchBattlePokemon switchPokemonPacket = packet as S_SwitchBattlePokemon;
                        PlayerInfo playerInfo = switchPokemonPacket.PlayerInfo;
                        PokemonSummary enemyPokemonSum = switchPokemonPacket.EnemyPokemonSum;
                        PokemonSummary myPokemonSum = switchPokemonPacket.MyPokemonSum;

                        // 포켓몬 및 플레이어 데이터 채우기
                        _playerInfo = playerInfo;
                        _myPokemon = new Pokemon(myPokemonSum);
                        _enemyPokemon = new Pokemon(enemyPokemonSum);

                        // 포켓몬 랜더링
                        _enemyPokemonArea.MakeBattlePokemon(_enemyPokemon, false);

                        // UI 생성
                        MakeUI();

                        SceneState = BattleSceneState.SWITCH_POKEMON_SCRIPTING;

                        List<string> scripts = new List<string>()
                        {
                            $"Go {_myPokemon.PokemonInfo.NickName}!"
                        };
                        _scriptBox.BeginScriptTyping(scripts, true);
                    }
                    else if (packet is S_UseItem)
                    {
                        _enterEffect.PlayEffect("FadeIn");
                        _myPokemonArea.SetActiveTrainer(false);

                        S_UseItem useItemPacket = packet as S_UseItem;
                        PlayerInfo playerInfo = useItemPacket.PlayerInfo;
                        PokemonSummary enemyPokemonSum = useItemPacket.EnemyPokemonSum;
                        PokemonSummary myPokemonSum = useItemPacket.PlayerPokemonSum;
                        ItemSummary itemSumamary = useItemPacket.UsedItem;

                        // 포켓몬 및 플레이어 데이터 채우기
                        _playerInfo = playerInfo;
                        _enemyPokemon = new Pokemon(enemyPokemonSum);
                        _myPokemon = new Pokemon(myPokemonSum);
                        _usedItem = new Item(itemSumamary);

                        // 포켓몬 랜더링
                        _enemyPokemonArea.MakeBattlePokemon(_enemyPokemon, false);
                        _myPokemonArea.MakeBattlePokemon(_myPokemon, true);

                        // UI 생성
                        MakeUI();

                        SceneState = BattleSceneState.AFTER_USE_ITEM_UPDATE;
                    }
                    else if (packet is S_EnterPokemonBattleScene)
                    {
                        _enterEffect.PlayEffect("FadeIn");
                        _playableDirector.Play();

                        S_EnterPokemonBattleScene enterBattleScenePacket = packet as S_EnterPokemonBattleScene;
                        PlayerInfo playerInfo = enterBattleScenePacket.PlayerInfo;
                        PokemonSummary enemyPokemonSum = enterBattleScenePacket.EnemyPokemonSum;
                        PokemonSummary myPokemonSum = enterBattleScenePacket.PlayerPokemonSum;

                        // 포켓몬 및 플레이어 데이터 채우기
                        _playerInfo = playerInfo;
                        _enemyPokemon = new Pokemon(enemyPokemonSum);
                        _myPokemon = new Pokemon(myPokemonSum);

                        // 포켓몬 랜더링
                        _myPokemonArea.FillTrainerImage(playerInfo.PlayerGender);

                        _enemyPokemonArea.MakeBattlePokemon(_enemyPokemon, false);
                        _enemyPokemonArea.PlayBattlePokemonAnim("BattlePokemon_Default");

                        // UI 생성
                        MakeUI();
                    }
                }
                break;
            case BattleSceneState.SELECTING_MOVE:
                {
                    _loadingPacket = false;

                    S_UsePokemonMove useMovePacket = packet as S_UsePokemonMove;
                    int myRemainedPP = useMovePacket.MyRemainedPP;
                    int enemyRemainedPP = useMovePacket.EnemyRemainedPP;

                    _myPokemon.SelectedMove.CurPP = myRemainedPP;
                    _enemyPokemon.SelectedMove.CurPP = enemyRemainedPP;

                    List<string> scripts = new List<string>()
                    {
                        $"{_attackPKM.PokemonInfo.NickName} used {_attackPKM.SelectedMove.MoveName}!"
                    };
                    _scriptBox.BeginScriptTyping(scripts, true);

                    SceneState = BattleSceneState.ATTACK_INSTRUCTING;
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
                    PokemonExpInfo expInfo = changeLevel.PokemonExp;
                    LevelUpStatusDiff statDiff = changeLevel.StatDiff;

                    _loadingPacket = false;

                    _myPokemon.PokemonInfo.Level = pokemonLevel;
                    _myPokemon.PokemonStat = stat;
                    _myPokemon.PokemonExpInfo = expInfo;

                    _myPokemonArea.FillPokemonInfo(_myPokemon, true);
                    _statusBox.SetStatusDiffRate(statDiff);

                    SceneState = BattleSceneState.LEVEL_UP_SCRIPTING;

                    List<string> scripts = new List<string>()
                    {
                        $"{_myPokemon.PokemonInfo.NickName}'s level went up to {_myPokemon.PokemonInfo.Level}."
                    };

                    _scriptBox.BeginScriptTyping(scripts);
                }
                break;
            case BattleSceneState.SHOWING_UPGRADED_STATUS:
                {
                    _loadingPacket = false;

                    S_CheckNewLearnableMove checkMovePacket = packet as S_CheckNewLearnableMove;
                    PokemonMoveSummary newLearnableMove = checkMovePacket.NewMoveSum;

                    if (newLearnableMove != null)
                    {
                        if (_myPokemon.PokemonMoves.Count < 4)
                        {
                            PokemonMove move = new PokemonMove(newLearnableMove);
                            _myPokemon.PokemonMoves.Add(move);

                            List<string> scripts = new List<string>()
                            {
                                $"{_myPokemon.PokemonInfo.NickName} learned {move.MoveName}!"
                            };
                            _scriptBox.BeginScriptTyping(scripts);

                            SceneState = BattleSceneState.NEW_MOVE_LEARN_SCRIPTING;
                        }
                        else
                        {
                            _newLearnableMove = new PokemonMove(checkMovePacket.NewMoveSum);

                            List<string> scripts = new List<string>()
                            {
                                $"{_myPokemon.PokemonInfo.NickName} wants to learn the move {_newLearnableMove.MoveName}.",
                                $"However, {_myPokemon.PokemonInfo.NickName} already knows four moves.",
                                $"Sould a move be deleted and replaced with {_newLearnableMove.MoveName}?",
                            };
                            _scriptBox.BeginScriptTyping(scripts);

                            SceneState = BattleSceneState.ASKING_TO_LEARN_NEW_MOVE;
                        }
                    }
                    else
                    {
                        SceneState = BattleSceneState.NEW_MOVE_LEARN_SCRIPTING;
                        DoNextAction();
                    }
                }
                break;
            case BattleSceneState.NEW_MOVE_LEARN_SCRIPTING:
                {
                    SceneState = BattleSceneState.GETTING_EXP;

                    _loadingPacket = false;

                    S_ChangePokemonExp changeExpPacket = packet as S_ChangePokemonExp;
                    PokemonExpInfo expInfo = changeExpPacket.PokemonExpInfo;

                    _myPokemon.PokemonExpInfo = expInfo;

                    _myPokemonArea.ChangePokemonEXP(_myPokemon.PokemonExpInfo.CurExp);
                }
                break;
            case BattleSceneState.NEW_MOVE_NOT_LEARN_SCRIPTING:
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
                    _myPokemonArea.PlayTrainerAnim("Trainer_LeftAppear");
                    SceneState = BattleSceneState.CHANGING_POKEMON;
                }
                break;
            case BattleSceneState.CHANGING_POKEMON:
                {
                    _myPokemonArea.MakeBattlePokemon(_myPokemon, true);
                    _myPokemonArea.PlayBattlePokemonAnim("BattlePokemon_RightAppear");
                    SceneState = BattleSceneState.SHOWING_POKEMON;
                }
                break;
            case BattleSceneState.SHOWING_POKEMON:
                {
                    _playableDirector.Resume();

                    SceneState = BattleSceneState.SHOWING_UI;
                }
                break;
            case BattleSceneState.SHOWING_UI:
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

                                    SetAttackAndDefensePokemonArea();

                                    List<string> scripts = new List<string>()
                                    {
                                        $"{_myPokemon.PokemonInfo.NickName} has no move to use!",
                                        $"{_attackPKM.PokemonInfo.NickName} used {_attackPKM.SelectedMove.MoveName}!"
                                    };

                                    _scriptBox.BeginScriptTyping(scripts, true);

                                    _enemyPokemon.SelectedMove.CurPP--;
                                }
                            }
                            else if (selectedAction == "Pokemon")
                            {
                                C_EnterPokemonListScene enterPokemonPacket = new C_EnterPokemonListScene();
                                enterPokemonPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(enterPokemonPacket);

                                _enterEffect.PlayEffect("FadeOut");

                                SceneState = BattleSceneState.MOVING_TO_POKEMON_SCENE;
                            }
                            else if (selectedAction == "Bag")
                            {
                                C_EnterPlayerBagScene enterBagPacket = new C_EnterPlayerBagScene();
                                enterBagPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(enterBagPacket);

                                _enterEffect.PlayEffect("FadeOut");

                                SceneState = BattleSceneState.MOVING_TO_BAG_SCENE;
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
                                SetAttackAndDefensePokemonArea();
                                SetSelectedMove(selectedMove);

                                if (!_loadingPacket)
                                {
                                    // 서버에게 기술 사용 요청
                                    C_UsePokemonMove movePacket = new C_UsePokemonMove();
                                    movePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    movePacket.MyMoveOrder = _myPokemon.GetSelectedMoveIdx();
                                    movePacket.EnemyMoveOrder = _enemyPokemon.GetSelectedMoveIdx();

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

                        if (_attackPKM == _myPokemon)
                            _attackPKMArea.PlayBattlePokemonAnim("BattlePokemon_RightAttack");
                        else
                            _attackPKMArea.PlayBattlePokemonAnim("BattlePokemon_LeftAttack");

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

                    _defensePKMArea.PlayBattlePokemonAnim("BattlePokemon_Hit");
                }
                break;
            case BattleSceneState.HIT_POKEMON_BLINK:
                {
                    if (!_loadingPacket)
                    {
                        C_ChangePokemonHp changeHpPacket = new C_ChangePokemonHp();
                        changeHpPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                        changeHpPacket.IsMyPokemon = _attackPKM == _myPokemon ? false : true;

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
                            $"{_defensePKM.PokemonInfo.NickName} fell down!",
                            $"There is no more pokemon to fight!",
                            $"......",
                            $"I took the injured Pokemon to the nearby Pokemon Center in a hurry."
                        };
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
                    // 배울 수 있는 기술이 있는 지 확인
                    if (!_loadingPacket)
                    {
                        C_CheckNewLearnableMove changeMovePacket = new C_CheckNewLearnableMove();
                        changeMovePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                        Managers.Network.Send(changeMovePacket);

                        _loadingPacket = true;
                    }
                }
                break;
            case BattleSceneState.NEW_MOVE_LEARN_SCRIPTING:
                {
                    if (_remainEXPToGet != 0)
                    {
                        if (!_loadingPacket)
                        {
                            int finalExp = SetAndApplyFinalEXP();

                            // 서버에도 포켓몬 경험치 획득 요청
                            C_ChangePokemonExp changeExpPacket = new C_ChangePokemonExp();
                            changeExpPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
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
            case BattleSceneState.ASKING_TO_LEARN_NEW_MOVE:
                {
                    SceneState = BattleSceneState.ANSWERING_TO_LEARN_NEW_MOVE;
                }
                break;
            case BattleSceneState.ANSWERING_TO_LEARN_NEW_MOVE:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            string answer = _yesOrNoSelectBox.GetSelectedBtnData() as string;

                            if (answer == "YES")
                            {
                                Debug.Log("Yes!");
                            }
                            else if (answer == "NO")
                            {
                                List<string> scripts = new List<string>()
                                {
                                    $"{_myPokemon.PokemonInfo.NickName} did not learn the move {_newLearnableMove.MoveName}.",
                                };
                                _scriptBox.BeginScriptTyping(scripts);

                                SceneState = BattleSceneState.NEW_MOVE_NOT_LEARN_SCRIPTING;
                            }
                        }
                    }
                }
                break;
            case BattleSceneState.NEW_MOVE_NOT_LEARN_SCRIPTING:
                {
                    if (_remainEXPToGet != 0)
                    {
                        if (!_loadingPacket)
                        {
                            int finalExp = SetAndApplyFinalEXP();

                            // 서버에도 포켓몬 경험치 획득 요청
                            C_ChangePokemonExp changeExpPacket = new C_ChangePokemonExp();
                            changeExpPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
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
            case BattleSceneState.SWITCH_POKEMON_SCRIPTING:
                {
                    _myPokemonArea.MakeBattlePokemon(_myPokemon, true);
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
            case BattleSceneState.MOVING_TO_BAG_SCENE:
                {
                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.Bag);
                }
                break;
            case BattleSceneState.MOVING_TO_POKEMON_SCENE:
                {
                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.PokemonList);
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

            _defensePKMArea.PlayBattlePokemonAnim("BattlePokemon_Die");
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

    void SetAttackAndDefensePokemonArea()
    {
        int myPokemonSpeed = _myPokemon.PokemonStat.Speed;
        int enemyPokemonSpeed = _enemyPokemon.PokemonStat.Speed;

        if (myPokemonSpeed > enemyPokemonSpeed)
        {
            _attackPKM = _myPokemon;
            _attackPKMArea = _myPokemonArea;

            _defensePKM = _enemyPokemon;
            _defensePKMArea = _enemyPokemonArea;
        }
        else if (myPokemonSpeed < enemyPokemonSpeed)
        {
            _attackPKM = _enemyPokemon;
            _attackPKMArea = _enemyPokemonArea;

            _defensePKM = _myPokemon;
            _defensePKMArea = _myPokemonArea;
        }
        else
        {
            int ran = Random.Range(0, 100);

            if (ran >= 50)
            {
                _attackPKM = _myPokemon;
                _attackPKMArea = _myPokemonArea;

                _defensePKM = _enemyPokemon;
                _defensePKMArea = _enemyPokemonArea;
            }
            else
            {
                _attackPKM = _enemyPokemon;
                _attackPKMArea = _enemyPokemonArea;

                _defensePKM = _myPokemon;
                _defensePKMArea = _myPokemonArea;
            }
        }
    }

    public override void Clear()
    {
    }
}
