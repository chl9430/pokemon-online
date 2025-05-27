using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

enum BattleSceneState
{
    INTRO = 0,
    APPEAR_SCRIPTING = 1,
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

    Pokemon _myPokemon;
    Pokemon _enemyPokemon;
    PokemonMove _selectedMove;

    Pokemon _attackPKM;
    BattleArea _attackPKMArea;
    Pokemon _defensePKM;
    BattleArea _defensePKMArea;
    bool _isMyPKMAttack = false;
    bool _isEnemyPKMAttack = false;

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
    }

    void Start()
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
            PokemonSummary dummySummary = new PokemonSummary();
            PokemonInfo dummyInfo = new PokemonInfo()
            {
                DictionaryNum = 35,
                NickName = "MESSI",
                PokemonName = "Charmander",
                Level = 1,
                Gender = PokemonGender.Male,
                Type1 = PokemonType.Fire,
                Type2 = PokemonType.Water,
                OwnerName = "CHRIS",
                OwnerId = 99999
            };
            PokemonSkill dummySkill = new PokemonSkill()
            {
                Stat = new PokemonStat(),
            };
            PokemonBattleMove dummyBattleMove = new PokemonBattleMove()
            {
            };

            dummySummary.Info = dummyInfo;
            dummySummary.Skill = dummySkill;
            dummySummary.BattleMove = dummyBattleMove;

            List<PokemonMove> moves = new List<PokemonMove>()
                {
                    new(20, 10, 100, "Ember", PokemonType.Fire, MoveCategory.Special),
                    new(15, 20, 100, "Ember2", PokemonType.Fire, MoveCategory.Special),
                    new(10, 30, 100, "Ember3", PokemonType.Fire, MoveCategory.Special),
                    new(5, 100, 100, "Ember4", PokemonType.Fire, MoveCategory.Special),
                };

            Pokemon pokemon = new Pokemon(dummySummary);
            pokemon.Moves = moves;
            pokemon.PokemonBaseStat = new PokemonBaseStat()
            {
                MaxHP = 35,
                Attack = 55,
                Defense = 30,
                SpecialAttack = 50,
                SpecialDefense = 40,
                Speed = 90
            };
            pokemon.SetStat();
            pokemon.PokemonStat.Hp = pokemon.PokemonStat.MaxHp;

            List<string> names = new List<string>();
            names.Add(pokemon.Moves[0].MoveName);
            names.Add(pokemon.Moves[1].MoveName);
            names.Add(pokemon.Moves[2].MoveName);
            names.Add(pokemon.Moves[3].MoveName);

            List<object> movesObjs = new List<object>();

            foreach (PokemonMove move in moves)
                movesObjs.Add(move);

            _moveSelectBox.SetButtonNames(names);
            _moveSelectBox.SetButtonDatas(movesObjs);

            _myPokemonArea.FillPokemonInfo(pokemon, true);
        }

        {
            PokemonSummary dummySummary = new PokemonSummary();
            PokemonInfo dummyInfo = new PokemonInfo()
            {
                DictionaryNum = 35,
                NickName = "Squirtle",
                PokemonName = "Squirtle",
                Level = 1,
                Gender = PokemonGender.Female,
                Type1 = PokemonType.Water,
                Type2 = PokemonType.Normal,
                OwnerName = "NONE",
                OwnerId = 0
            };
            PokemonSkill dummySkill = new PokemonSkill()
            {
                Stat = new PokemonStat(),
            };
            PokemonBattleMove dummyBattleMove = new PokemonBattleMove()
            {
            };

            dummySummary.Info = dummyInfo;
            dummySummary.Skill = dummySkill;
            dummySummary.BattleMove = dummyBattleMove;

            List<PokemonMove> _moves = new List<PokemonMove>()
                {
                    new(20, 10, 100, "Bubble", PokemonType.Water, MoveCategory.Special),
                    new(15, 20, 100, "Bubble2", PokemonType.Water, MoveCategory.Special),
                    new(10, 30, 100, "Bubble3", PokemonType.Water, MoveCategory.Special),
                    new(5, 40, 100, "Bubble4", PokemonType.Water, MoveCategory.Special),
                };

            Pokemon pokemon = new Pokemon(dummySummary);
            pokemon.Moves = _moves;
            pokemon.PokemonBaseStat = new PokemonBaseStat()
            {
                MaxHP = 35,
                Attack = 55,
                Defense = 30,
                SpecialAttack = 50,
                SpecialDefense = 40,
                Speed = 90
            };
            pokemon.SetStat();
            pokemon.PokemonStat.Hp = pokemon.PokemonStat.MaxHp;

            _enemyPokemonArea.FillPokemonInfo(pokemon, false);
        }

        _myPokemon = _myPokemonArea.Pokemon;
        _enemyPokemon = _enemyPokemonArea.Pokemon;
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
                    _sceneState = BattleSceneState.SHOWING_POKEMON;
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

                                _myPokemon.SelectedMove.PP--;
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

                            List<string> scripts = new List<string>()
                            {
                                $"Cannot use this move!"
                            };

                            _scriptBox.BeginScriptTyping(scripts);
                        }
                        else
                        {
                            _sceneState = BattleSceneState.ATTACK_INSTRUCTING;

                            SetSelectedMove(_selectedMove);

                            SetAttackAndDefensePokemonArea(ref _myPokemonArea, ref _enemyPokemonArea);

                            List<string> scripts = new List<string>()
                            {
                                $"{_attackPKM.PokemonInfo.NickName} used {_attackPKM.SelectedMove.MoveName}!"
                            };

                            _scriptBox.BeginScriptTyping(scripts, true);

                            _myPokemon.SelectedMove.PP--;
                            _enemyPokemon.SelectedMove.PP--;
                        }

                        ActiveUIBySceneState(_sceneState);
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

                        _scriptBox.BeginScriptTyping(script);

                        if (_attackPKMArea == _myPokemonArea)
                            _isMyPKMAttack = true;
                        else
                            _isEnemyPKMAttack = true;
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
                    _sceneState = BattleSceneState.CHANGE_POKEMON_HP;

                    PokemonStat attackPKMStat = _attackPKM.PokemonSummary.Skill.Stat;
                    PokemonStat DefensePKMstat = _defensePKM.PokemonSummary.Skill.Stat;

                    int finalDamage = 0;

                    if (_attackPKM.SelectedMove.MoveCategory == MoveCategory.Physical)
                        finalDamage = (int)((
                        ((((float)_attackPKM.PokemonInfo.Level) * 2f / 5f) + 2f) 
                        * ((float)_attackPKM.SelectedMove.MovePower) 
                        * ((float)attackPKMStat.Attack) / ((float)DefensePKMstat.Defense)
                        ) / 50f + 2f);
                    else if (_attackPKM.SelectedMove.MoveCategory == MoveCategory.Special)
                        finalDamage = (int)((
                        ((((float)_attackPKM.PokemonInfo.Level) * 2f / 5f) + 2f)
                        * ((float)_attackPKM.SelectedMove.MovePower)
                        * ((float)attackPKMStat.SpecialAttack) / ((float)DefensePKMstat.SpecialDefense)
                        ) / 50f + 2f);

                    _defensePKM.GetDamaged(finalDamage, _attackPKM.SelectedMove.MoveCategory);
                    _defensePKMArea.ChangePokemonHP(DefensePKMstat.Hp);
                }
                break;
            case BattleSceneState.CHANGE_POKEMON_HP:
                {
                    StartCoroutine(ActionAfterChangeHP());
                }
                break;
            case BattleSceneState.POKEMON_DIE:
                {
                    List<string> scripts = new List<string>()
                    {
                        $"{_defensePKM.PokemonInfo.NickName} fell down!"
                    };

                    // 죽은 포켓몬이 내 포켓몬일 경우
                    if (_defensePKM == _myPokemon)
                    {
                        _sceneState = BattleSceneState.MY_POKEMON_DIE_SCRIPTING;

                        scripts.Add($"There is no more pokemon to fight!");
                        scripts.Add($"......");
                        scripts.Add($"I took the injured Pokemon to the nearby Pokemon Center in a hurry.");
                    }
                    else
                    {
                        _sceneState = BattleSceneState.ENEMY_POKEMON_DIE_SCRIPTING;

                        if (_myPokemon.PokemonInfo.Level != 100)
                        {
                            _remainEXPToGet = 1000;

                            scripts.Add($"{_myPokemon.PokemonInfo.NickName} got {_remainEXPToGet} exp!");
                        }
                    }

                    _scriptBox.BeginScriptTyping(scripts);
                }
                break;
            case BattleSceneState.ENEMY_POKEMON_DIE_SCRIPTING:
                {
                    if (_remainEXPToGet != 0)
                    {
                        _sceneState = BattleSceneState.GETTING_EXP;

                        SetAndApplyFinalEXP();
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
                        _sceneState = BattleSceneState.LEVEL_UP_SCRIPTING;

                        LevelUpStatusRate rate = _myPokemon.PokemonLevelUp();
                        _myPokemonArea.FillPokemonInfo(_myPokemon, true);
                        _statusBox.SetStatusDiffRate(rate);

                        List<string> scripts = new List<string>()
                        {
                            $"{_myPokemon.PokemonInfo.NickName}'s level went up to {_myPokemon.PokemonInfo.Level}."
                        };

                        _scriptBox.BeginScriptTyping(scripts);
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
                        _sceneState = BattleSceneState.GETTING_EXP;

                        SetAndApplyFinalEXP();
                    }
                    else
                    {
                        Debug.Log("Battle Finsh");
                    }

                    ActiveUIBySceneState(_sceneState);
                }
                break;
        }
    }

    void SetAndApplyFinalEXP()
    {
        int finalEXP = 0;

        if (_remainEXPToGet > _myPokemon.PokemonSkill.RemainLevelExp)
            finalEXP = _myPokemon.PokemonSkill.RemainLevelExp;
        else
            finalEXP = _remainEXPToGet;

        _myPokemonArea.ChangePokemonEXP(finalEXP);
        _remainEXPToGet -= finalEXP;

        _myPokemon.GetEXP(finalEXP);
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
