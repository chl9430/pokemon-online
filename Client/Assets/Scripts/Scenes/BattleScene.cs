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

enum BattleSceneState
{
    INTRO = 0,
    APPEAR_SCRIPTING = 1,
    CHANGING_POKEMON = 2,
    SHOWING_POKEMON = 3,
    SELECTING_ACTION = 4,
    SELECTING_MOVE = 5,
    CANNOT_USE_MOVE = 11,
    ATTACK_INSTRUCTING = 6,
    FIRST_ATTACK_ANIMATION = 7,
    FIRST_ATTACK_FAILED = 8,
    HIT_POKEMON_BLINK = 9,
    CHANGE_POKEMON_HP = 10,
    POKEMON_DIE = 12,
    MY_POKEMON_DIE_SCRIPTING = 13,
    ENEMY_POKEMON_DIE_SCRIPTING = 14,
    GETTING_EXP = 15,
    LEVEL_UP_SCRIPTING = 16,
    UPGRADING_STATUS = 17,
    SHOWING_UPGRADED_STATUS = 18,
}

public class BattleScene : BaseScene
{
    BattleSceneState _sceneState = BattleSceneState.INTRO;
    List<List<string>> _scripts;
    TextMeshProUGUI[] _moveInfoTMPs;
    PlayableDirector _playableDirector;
    ObjectInfo _playerInfo;

    Pokemon _myPokemon;
    Pokemon _enemyPokemon;
    PokemonMove _selectedMove;

    Pokemon _attackPKM;
    BattleArea _attackPKMArea;
    Pokemon _defensePKM;
    BattleArea _defensePKMArea;
    bool _isMyPKMAttack = false;
    bool _isEnemyPKMAttack = false;
    bool _loadingPacket = false;
    string _selectedAction;

    int _remainEXPToGet;

    [SerializeField] ScriptBoxUI _scriptBox;
    [SerializeField] GridSelectBox _actionSelectBox;
    [SerializeField] GridSelectBox _moveSelectBox;
    [SerializeField] GameObject _moveInfoBox;
    [SerializeField] BattleArea _enemyPokemonArea;
    [SerializeField] BattleArea _myPokemonArea;
    [SerializeField] StatusBox _statusBox;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Battle;
        
        _scripts = new List<List<string>>();

        Managers.Network.SendSavedPacket();
    }

    public override void UpdateData(IMessage packet)
    {
        switch (_sceneState)
        {
            case BattleSceneState.INTRO:
                {
                    S_AccessPokemonSummary accessPacket = packet as S_AccessPokemonSummary;
                    ObjectInfo info = accessPacket.PlayerInfo;

                    _playerInfo = info;

                    _myPokemonArea.FillTrainerImage(info.Gender);
                    _myPokemonArea.Pokemon = Managers.Object._pokemons[0];
                    _myPokemon = _myPokemonArea.Pokemon;

                    _enemyPokemonArea.FillPokemonInfo(new Pokemon(accessPacket.PkmSummary), false);
                    _enemyPokemon = _enemyPokemonArea.Pokemon;
                }
                break;
            case BattleSceneState.SELECTING_MOVE:
                {
                    S_UsePokemonMove useMovePacket = packet as S_UsePokemonMove;
                    int remainedPP = useMovePacket.RemainedPP;

                    _sceneState = BattleSceneState.ATTACK_INSTRUCTING;
                    _loadingPacket = false;

                    SetSelectedMove(_selectedMove);

                    _myPokemon.SelectedMove.PP = remainedPP;
                    _enemyPokemon.SelectedMove.PP--;

                    ActiveUIBySceneState(_sceneState);

                    SetAttackAndDefensePokemonArea(ref _myPokemonArea, ref _enemyPokemonArea);

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
                    _sceneState = BattleSceneState.CHANGE_POKEMON_HP;

                    _loadingPacket = false;

                    int remainedHp = changeHpPacket.RemainedHP;

                    _defensePKM.PokemonStat.Hp = remainedHp;

                    _defensePKMArea.ChangePokemonHP(_defensePKM.PokemonStat.Hp);
                }
                break;
            case BattleSceneState.POKEMON_DIE:
                {
                    S_GetEnemyPokemonExp getExpPacket = packet as S_GetEnemyPokemonExp;
                    int exp = getExpPacket.Exp;
                    _sceneState = BattleSceneState.ENEMY_POKEMON_DIE_SCRIPTING;

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
                    _sceneState = BattleSceneState.GETTING_EXP;

                    _loadingPacket = false;

                    S_ChangePokemonExp changeExpPacket = packet as S_ChangePokemonExp;
                    int myPokemonTotalExp = changeExpPacket.PokemonTotalExp;
                    int myPokemonRemainLevelExp = changeExpPacket.PokemonRemainLevelExp;
                    int myPokemonCurExp = changeExpPacket.PokemonCurExp;

                    _myPokemon.PokemonSkill.TotalExp = myPokemonTotalExp;
                    _myPokemon.PokemonSkill.RemainLevelExp = myPokemonRemainLevelExp;
                    _myPokemon.PokemonSkill.CurExp = myPokemonCurExp;

                    _myPokemonArea.ChangePokemonEXP(_myPokemon.PokemonSkill.CurExp);
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

                    _sceneState = BattleSceneState.LEVEL_UP_SCRIPTING;

                    _loadingPacket = false;

                    _myPokemon.PokemonInfo.Level = pokemonLevel;
                    _myPokemon.PokemonStat = stat;
                    _myPokemon.PokemonSkill.RemainLevelExp = remainLevelExp;
                    _myPokemon.PokemonSkill.CurExp = curExp;

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
                    _sceneState = BattleSceneState.GETTING_EXP;

                    _loadingPacket = false;

                    S_ChangePokemonExp changeExpPacket = packet as S_ChangePokemonExp;
                    int myPokemonTotalExp = changeExpPacket.PokemonTotalExp;
                    int myPokemonRemainLevelExp = changeExpPacket.PokemonRemainLevelExp;
                    int myPokemonCurExp = changeExpPacket.PokemonCurExp;

                    _myPokemon.PokemonSkill.TotalExp = myPokemonTotalExp;
                    _myPokemon.PokemonSkill.RemainLevelExp = myPokemonRemainLevelExp;
                    _myPokemon.PokemonSkill.CurExp = myPokemonCurExp;

                    _myPokemonArea.ChangePokemonEXP(_myPokemon.PokemonSkill.CurExp);

                    ActiveUIBySceneState(_sceneState);
                }
                break;
        }
    }

    protected override void Start()
    {
        _playableDirector = GetComponent<PlayableDirector>();
        _moveInfoTMPs = _moveInfoBox.GetComponentsInChildren<TextMeshProUGUI>();

        {
            List<string> btnNames = new List<string>();
            btnNames.Add("Fight");
            btnNames.Add("Bag");
            btnNames.Add("Pokemon");
            btnNames.Add("Run");

            List<object> nameObjs = new List<object>();

            foreach (string name in btnNames)
                nameObjs.Add(name);

            _actionSelectBox.SetButtonNames(btnNames);
            _actionSelectBox.SetButtonDatas(nameObjs);
        }

        {
            // PokemonSummary dummySummary = new PokemonSummary();
            //PokemonInfo dummyInfo = new PokemonInfo()
            //{
            //    DictionaryNum = 35,
            //    NickName = "MESSI",
            //    PokemonName = "Charmander",
            //    Level = 1,
            //    Gender = PokemonGender.Male,
            //    Type1 = PokemonType.Fire,
            //    Type2 = PokemonType.Water,
            //    OwnerName = "CHRIS",
            //    OwnerId = 99999
            //};
            //PokemonSkill dummySkill = new PokemonSkill()
            //{
            //    Stat = new PokemonStat(),
            //};
            //PokemonBattleMove dummyBattleMove = new PokemonBattleMove()
            //{
            //};

            //dummySummary.Info = dummyInfo;
            //dummySummary.Skill = dummySkill;

            //List<PokemonMove> moves = new List<PokemonMove>()
            //    {
            //        new(20, 10, 100, "Ember", PokemonType.Fire, MoveCategory.Special),
            //        new(15, 20, 100, "Ember2", PokemonType.Fire, MoveCategory.Special),
            //        new(10, 30, 100, "Ember3", PokemonType.Fire, MoveCategory.Special),
            //        new(5, 100, 100, "Ember4", PokemonType.Fire, MoveCategory.Special),
            //    };

            //Pokemon pokemon = new Pokemon(dummySummary);
            //pokemon.Moves = moves;
            //pokemon.PokemonBaseStat = new PokemonBaseStat()
            //{
            //    MaxHP = 35,
            //    Attack = 55,
            //    Defense = 30,
            //    SpecialAttack = 50,
            //    SpecialDefense = 40,
            //    Speed = 90
            //};
            //pokemon.SetStat();
            //pokemon.PokemonStat.Hp = pokemon.PokemonStat.MaxHp;

            //List<string> names = new List<string>();
            //names.Add(pokemon.Moves[0].MoveName);
            //names.Add(pokemon.Moves[1].MoveName);
            //names.Add(pokemon.Moves[2].MoveName);
            //names.Add(pokemon.Moves[3].MoveName);

            //List<object> movesObjs = new List<object>();

            //foreach (PokemonMove move in moves)
            //    movesObjs.Add(move);

            //_moveSelectBox.SetButtonNames(names);
            //_moveSelectBox.SetButtonDatas(movesObjs);

            //_myPokemonArea.FillPokemonInfo(pokemon, true);
        }

        {
            //PokemonSummary dummySummary = new PokemonSummary();
            //PokemonInfo dummyInfo = new PokemonInfo()
            //{
            //    DictionaryNum = 35,
            //    NickName = "Squirtle",
            //    PokemonName = "Squirtle",
            //    Level = 1,
            //    Gender = PokemonGender.Female,
            //    Type1 = PokemonType.Water,
            //    Type2 = PokemonType.Normal,
            //    OwnerName = "NONE",
            //    OwnerId = 0
            //};
            //PokemonSkill dummySkill = new PokemonSkill()
            //{
            //    Stat = new PokemonStat(),
            //};
            //PokemonBattleMove dummyBattleMove = new PokemonBattleMove()
            //{
            //};

            //dummySummary.Info = dummyInfo;
            //dummySummary.Skill = dummySkill;

            //List<PokemonMove> _moves = new List<PokemonMove>()
            //    {
            //        new(20, 10, 100, "Bubble", PokemonType.Water, MoveCategory.Special),
            //        new(15, 20, 100, "Bubble2", PokemonType.Water, MoveCategory.Special),
            //        new(10, 30, 100, "Bubble3", PokemonType.Water, MoveCategory.Special),
            //        new(5, 40, 100, "Bubble4", PokemonType.Water, MoveCategory.Special),
            //    };

            //Pokemon pokemon = new Pokemon(dummySummary);
            //pokemon.Moves = _moves;
            //pokemon.PokemonBaseStat = new PokemonBaseStat()
            //{
            //    MaxHP = 35,
            //    Attack = 55,
            //    Defense = 30,
            //    SpecialAttack = 50,
            //    SpecialDefense = 40,
            //    Speed = 90
            //};
            //pokemon.SetStat();
            //pokemon.PokemonStat.Hp = pokemon.PokemonStat.MaxHp;

            //_enemyPokemonArea.FillPokemonInfo(pokemon, false);
        }
    }

    public void TriggerTimelineAction()
    {
        switch (_sceneState)
        {
            case BattleSceneState.INTRO:
                {
                    _playableDirector.Pause();

                    List<string> scripts = new List<string>()
                    {
                        $"Wild {_enemyPokemon.PokemonInfo.PokemonName} appeard!",
                        $"Go! {_myPokemon.PokemonInfo.NickName}!",
                    };

                    _scriptBox.BeginScriptTyping(scripts);
                    _sceneState = BattleSceneState.APPEAR_SCRIPTING;
                }
                break;
            case BattleSceneState.CHANGING_POKEMON:
                {
                    _myPokemonArea.FillPokemonInfo(Managers.Object._pokemons[0], true);
                    _myPokemon = _myPokemonArea.Pokemon;

                    List<string> names = new List<string>();

                    foreach (PokemonMove move in _myPokemon.Moves)
                        names.Add(move.MoveName);

                    List<object> movesObjs = new List<object>();

                    foreach (PokemonMove move in _myPokemon.Moves)
                        movesObjs.Add(move);

                    _moveSelectBox.SetButtonNames(names);
                    _moveSelectBox.SetButtonDatas(movesObjs);

                    _sceneState = BattleSceneState.SHOWING_POKEMON;
                }
                break;
            case BattleSceneState.SHOWING_POKEMON:
                {
                    _sceneState = BattleSceneState.SELECTING_ACTION;

                    ActiveUIBySceneState(_sceneState);

                    _scriptBox.SetScriptWihtoutTyping($"What will {_myPokemon.PokemonInfo.NickName} do?");

                }
                break;
        }
    }

    public override void DoNextAction(object value = null)
    {
        switch (_sceneState)
        {
            case BattleSceneState.APPEAR_SCRIPTING:
                {
                    _playableDirector.Resume();
                    _sceneState = BattleSceneState.CHANGING_POKEMON;
                }
                break;
            case BattleSceneState.CANNOT_USE_MOVE:
                {
                    _sceneState = BattleSceneState.SELECTING_MOVE;

                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case BattleSceneState.SELECTING_ACTION:
                {
                    if (value as string == "Select")
                    {
                        if (_selectedAction == "Fight")
                        {
                            // 내 포켓몬에 사용 가능한 기술이 있는 지 확인한다.
                            List<PokemonMove> myPKMAvailableMoves = FindAvailableMove(_myPokemon);

                            if (myPKMAvailableMoves.Count > 0)
                            {
                                _sceneState = BattleSceneState.SELECTING_MOVE;

                                ActiveUIBySceneState(_sceneState);
                            }
                            else
                            {
                                _sceneState = BattleSceneState.ATTACK_INSTRUCTING;

                                ActiveUIBySceneState(_sceneState);

                                SetSelectedMove(new PokemonMove(9999, 50, 100, "Struggle", PokemonType.Normal, MoveCategory.Physical));

                                SetAttackAndDefensePokemonArea(ref _myPokemonArea, ref _enemyPokemonArea);

                                List<string> scripts = new List<string>()
                                {
                                    $"{_myPokemon.PokemonInfo.NickName} has no move to use!",
                                    $"{_attackPKM.PokemonInfo.NickName} used {_attackPKM.SelectedMove.MoveName}!"
                                };

                                _scriptBox.BeginScriptTyping(scripts, true);

                                _enemyPokemon.SelectedMove.PP--;
                            }
                        }
                    }
                    else
                    {
                        _selectedAction = value as string;
                    }
                }
                break;
            case BattleSceneState.SELECTING_MOVE:
                {
                    if (value as string == "Select")
                    {
                        if (_selectedMove.PP == 0)
                        {
                            _sceneState = BattleSceneState.CANNOT_USE_MOVE;
                            ActiveUIBySceneState(_sceneState);

                            List<string> scripts = new List<string>()
                            {
                                $"Cannot use this move!"
                            };

                            _scriptBox.BeginScriptTyping(scripts);
                        }
                        else
                        {
                            if (!_loadingPacket)
                            {
                                // 서버에게 기술 사용 요청
                                C_UsePokemonMove movePacket = new C_UsePokemonMove();
                                movePacket.PlayerId = _playerInfo.ObjectId;
                                movePacket.PokemonOrder = Managers.Object._pokemons.FindIndex(pokemon => pokemon == _myPokemon);
                                movePacket.MoveOrder = _myPokemon.Moves.FindIndex(move => move == _selectedMove);
                                movePacket.UsedPP = 1;

                                Managers.Network.Send(movePacket);

                                _loadingPacket = true;
                            }
                        }
                    }
                    else if (value as string == "Back")
                    {
                        _sceneState = BattleSceneState.SELECTING_ACTION;

                        ActiveUIBySceneState(_sceneState);
                    }
                    else
                    {
                        _selectedMove = value as PokemonMove;
                        _moveInfoTMPs[1].text = $"{_selectedMove.PP.ToString()} / {_selectedMove.MaxPP.ToString()}";
                        _moveInfoTMPs[2].text = $"TYPE / {_selectedMove.MoveType.ToString()}";
                    }
                }
                break;
            case BattleSceneState.ATTACK_INSTRUCTING:
                {
                    if (_attackPKM.IsHitByAcc())
                    {
                        _sceneState = BattleSceneState.FIRST_ATTACK_ANIMATION;

                        _attackPKMArea.AttackMovePokemonUI();
                        _defensePKMArea.TriggerPokemonHitImage(_attackPKM);
                    }
                    else
                    {
                        _sceneState = BattleSceneState.FIRST_ATTACK_FAILED;

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
                    PokemonStat stat = _defensePKM.PokemonSummary.Skill.Stat;

                    if (_attackPKMArea == _myPokemonArea)
                        _isMyPKMAttack = true;
                    else
                        _isEnemyPKMAttack = true;

                    if (!_isMyPKMAttack || !_isEnemyPKMAttack)
                    {
                        _sceneState = BattleSceneState.ATTACK_INSTRUCTING;

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
                        _sceneState = BattleSceneState.SELECTING_ACTION;

                        _isEnemyPKMAttack = false;
                        _isMyPKMAttack = false;

                        ActiveUIBySceneState(_sceneState);
                        _scriptBox.SetScriptWihtoutTyping($"What will {_myPokemon.PokemonInfo.NickName} do?");
                    }
                }
                break;
            case BattleSceneState.FIRST_ATTACK_ANIMATION:
                {
                    _sceneState = BattleSceneState.HIT_POKEMON_BLINK;

                    _defensePKMArea.BlinkPokemonUI();
                }
                break;
            case BattleSceneState.HIT_POKEMON_BLINK:
                {
                    if (!_loadingPacket)
                    {
                        C_ChangePokemonHp changeHpPacket = new C_ChangePokemonHp();
                        changeHpPacket.MoveCategory = _attackPKM.SelectedMove.MoveCategory;
                        changeHpPacket.AttackPKMInfo = _attackPKM.PokemonInfo;
                        changeHpPacket.DefensePKMInfo = _defensePKM.PokemonInfo;
                        changeHpPacket.AttackPKMStat = _attackPKM.PokemonStat;
                        changeHpPacket.DefensePKMStat = _defensePKM.PokemonStat;
                        changeHpPacket.MovePower = _attackPKM.SelectedMove.MovePower;
                        changeHpPacket.PlayerId = _playerInfo.ObjectId;
                        changeHpPacket.PokemonOrder = Managers.Object._pokemons.FindIndex(pokemon => pokemon == _defensePKM);

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
                        _sceneState = BattleSceneState.MY_POKEMON_DIE_SCRIPTING;

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
                                getExpPacket.PlayerId = _playerInfo.ObjectId;
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
                            changeExpPacket.PlayerId = _playerInfo.ObjectId;
                            changeExpPacket.PokemonOrder = Managers.Object._pokemons.FindIndex(pokemon => pokemon == _myPokemon);
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
                    if (_myPokemon.PokemonSkill.RemainLevelExp == 0)
                    {
                        if (!_loadingPacket)
                        {
                            C_ChangePokemonLevel changeLevelPacket = new C_ChangePokemonLevel();
                            changeLevelPacket.PlayerId = _playerInfo.ObjectId;
                            changeLevelPacket.PokemonOrder = Managers.Object._pokemons.FindIndex(pokemon => pokemon == _myPokemon);

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
                    _sceneState = BattleSceneState.UPGRADING_STATUS;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case BattleSceneState.UPGRADING_STATUS:
                {
                    _sceneState = BattleSceneState.SHOWING_UPGRADED_STATUS;
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
                            changeExpPacket.PlayerId = _playerInfo.ObjectId;
                            changeExpPacket.PokemonOrder = Managers.Object._pokemons.FindIndex(pokemon => pokemon == _myPokemon);
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
        }
    }

    int SetAndApplyFinalEXP()
    {
        int finalEXP = 0;

        if (_remainEXPToGet > _myPokemon.PokemonSkill.RemainLevelExp)
            finalEXP = _myPokemon.PokemonSkill.RemainLevelExp;
        else
            finalEXP = _remainEXPToGet;

        _remainEXPToGet -= finalEXP;

        return finalEXP;
    }

    IEnumerator ActionAfterChangeHP()
    {
        yield return new WaitForSeconds(1f);

        PokemonStat stat = _defensePKM.PokemonSummary.Skill.Stat;

        if (_attackPKMArea == _myPokemonArea)
            _isMyPKMAttack = true;
        else
            _isEnemyPKMAttack = true;

        if (stat.Hp <= 0)
        {
            _sceneState = BattleSceneState.POKEMON_DIE;

            _defensePKMArea.PokemonDie();
        }
        else if (!_isMyPKMAttack || !_isEnemyPKMAttack)
        {
            _sceneState = BattleSceneState.ATTACK_INSTRUCTING;

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
            _sceneState = BattleSceneState.SELECTING_ACTION;

            _isEnemyPKMAttack = false;
            _isMyPKMAttack = false;

            ActiveUIBySceneState(_sceneState);
            _scriptBox.SetScriptWihtoutTyping($"What will {_myPokemon.PokemonInfo.NickName} do?");
        }
    }

    List<PokemonMove> FindAvailableMove(Pokemon pokemon)
    {
        List<PokemonMove> availableMoves = new List<PokemonMove>();

        for (int i = 0; i < pokemon.Moves.Count; i++)
        {
            PokemonMove move = pokemon.Moves[i];

            if (move.PP == 0)
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
            _enemyPokemon.SelectedMove = new PokemonMove(9999, 50, 100, "Struggle", PokemonType.Normal, MoveCategory.Physical);
    }

    void ActiveUIBySceneState(BattleSceneState state)
    {
        if (state == BattleSceneState.SELECTING_ACTION)
        {
            _actionSelectBox.ChangeUIState(GridSelectBoxState.SELECTING, true);
            _moveSelectBox.ChangeUIState(GridSelectBoxState.NONE, false);
            _moveInfoBox.SetActive(false);
        }
        else if (state == BattleSceneState.SELECTING_MOVE)
        {
            _actionSelectBox.ChangeUIState(GridSelectBoxState.NONE, false);
            _moveSelectBox.ChangeUIState(GridSelectBoxState.SELECTING, true);
            _moveInfoBox.SetActive(true);
        }
        else if (state == BattleSceneState.UPGRADING_STATUS)
        {
            _statusBox.gameObject.SetActive(true);
        }
        else
        {
            _actionSelectBox.ChangeUIState(GridSelectBoxState.NONE, false);
            _moveSelectBox.ChangeUIState(GridSelectBoxState.NONE, false);
            _moveInfoBox.SetActive(false);
            _statusBox.gameObject.SetActive(false);
        }
    }

    void SetAttackAndDefensePokemonArea(ref BattleArea firstAttackPKMArea, ref BattleArea secondAttackPKMArea)
    {
        Pokemon firstPokemon = firstAttackPKMArea.Pokemon;
        Pokemon secondPokemon = secondAttackPKMArea.Pokemon;

        int firstPkmSpeed = firstPokemon.PokemonSummary.Skill.Stat.Speed;
        int secondPkmSpeed = secondPokemon.PokemonSummary.Skill.Stat.Speed;

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
