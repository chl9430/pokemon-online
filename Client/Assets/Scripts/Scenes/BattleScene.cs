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
    NO_MOVE_SCRIPTING = 44,
    ATTACK_INSTRUCTING = 9,
    FIRST_ATTACK_ANIMATION = 10,
    FIRST_ATTACK_FAILED = 11,
    HIT_POKEMON_BLINK = 12,
    CHANGE_POKEMON_HP = 13,
    ASKING_TO_SWITCH_POKEMON = 56,
    POKEMON_DIE = 14,
    MY_POKEMON_DIE_SCRIPTING = 15,
    NOT_AVAILABLE_BATTLE_SCRIPING = 43,
    GOT_EXP_SCRIPTING = 16,
    GETTING_EXP = 17,
    LEVEL_UP_SCRIPTING = 18,
    UPGRADING_STATUS = 19,
    SHOWING_UPGRADED_STATUS = 20,
    NEW_MOVE_LEARN_SCRIPTING = 21,
    ASKING_TO_LEARN_NEW_MOVE = 22,
    ANSWERING_TO_LEARN_NEW_MOVE = 23,
    ANSWERING_TO_SWITCH_POKEMON = 66,
    NEW_MOVE_NOT_LEARN_SCRIPTING = 24,
    COME_BACK_POKEMON_SCRIPTING = 25,
    COMING_BACK_POKEMON = 26,
    COME_BACK_POKEMON_CARD = 27,
    SWITCH_POKEMON_SCRIPTING = 28,
    SHOWING_SWITCH_POKEMON = 29,
    SHOWING_SWITCH_POKEMON_CARD = 30,
    AFTER_USE_ITEM_UPDATE = 46,
    AFTER_SWITCH_POKEMON = 47,
    AFTER_DIE_SWITCH_POKEMON = 76,
    GO_NEXT_POKEMON_SCRIPTING = 77,
    AFTER_RETURN_BATTLESCENE = 48,
    ITEM_USE_SCRIPTING = 87,
    MOVING_TO_BAG_SCENE = 90,
    MOVING_TO_POKEMON_SCENE = 91,
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
    PokemonMove _newLearnableMove;
    Item _usedItem;
    Pokemon _prevSwitchPokemon;

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

            if (_sceneState == BattleSceneState.SHOWING_POKEMON)
            {
                _myPokemonArea.FillPokemonInfo(_myPokemon, true);
                _myPokemonArea.PlayPokemonZoneAnim("Zone_RightAppear");
            }

            // 액션 선택 박스
            if (_sceneState == BattleSceneState.SELECTING_ACTION)
            {
                _scriptBox.SetScriptWihtoutTyping($"What will {_myPokemon.PokemonInfo.NickName} do?");
                _actionSelectBox.gameObject.SetActive(true);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
            }
            else if (_sceneState == BattleSceneState.MOVING_TO_BAG_SCENE || _sceneState == BattleSceneState.MOVING_TO_POKEMON_SCENE)
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
            if (_sceneState == BattleSceneState.ANSWERING_TO_LEARN_NEW_MOVE || _sceneState == BattleSceneState.ANSWERING_TO_SWITCH_POKEMON)
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
        _myPokemons = new List<Pokemon>();
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
        if (packet is S_UsePokemonMove)
        {
            _loadingPacket = false;

            S_UsePokemonMove useMovePacket = packet as S_UsePokemonMove;
            int remainedPP = useMovePacket.RemainedPP;

            _attackPKM.SelectedMove.CurPP = remainedPP;

            SceneState = BattleSceneState.ATTACK_INSTRUCTING;

            List<string> scripts = new List<string>()
            {
                $"{_attackPKM.PokemonInfo.NickName} used {_attackPKM.SelectedMove.MoveName}!"
            };
            _scriptBox.BeginScriptTyping(scripts, true);

            return;
        }
        else if (packet is S_GetEnemyPokemonExp)
        {
            _loadingPacket = false;

            S_GetEnemyPokemonExp getExpPacket = packet as S_GetEnemyPokemonExp;
            int exp = getExpPacket.Exp;

            _remainEXPToGet = exp;

            SceneState = BattleSceneState.GOT_EXP_SCRIPTING;

            List<string> scripts = new List<string>()
            {
                $"{_attackPKM.PokemonInfo.NickName} got {_remainEXPToGet} exp!"
            };
            _scriptBox.BeginScriptTyping(scripts);

            return;
        }

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
                        IList myPokemonSums = returnBattleScenePacket.MyPokemonSums;

                        // 포켓몬 및 플레이어 데이터 채우기
                        _playerInfo = playerInfo;
                        _enemyPokemon = new Pokemon(enemyPokemonSum);

                        foreach (PokemonSummary sum in myPokemonSums)
                            _myPokemons.Add(new Pokemon(sum));
                        _myPokemon = _myPokemons[0];

                        // 포켓몬 랜더링
                        _enemyPokemonArea.FillPokemonInfo(_enemyPokemon, false);
                        _myPokemonArea.FillPokemonInfo(_myPokemon, true);

                        // UI 생성
                        MakeUI();

                        if (_myPokemons[0].PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                            _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftHide");

                        SceneState = BattleSceneState.AFTER_RETURN_BATTLESCENE;
                    }
                    else if (packet is S_SwitchBattlePokemon)
                    {
                        _enterEffect.PlayEffect("FadeIn");
                        _myPokemonArea.SetActiveTrainer(false);

                        S_SwitchBattlePokemon switchPokemonPacket = packet as S_SwitchBattlePokemon;
                        PlayerInfo playerInfo = switchPokemonPacket.PlayerInfo;
                        PokemonSummary enemyPokemonSum = switchPokemonPacket.EnemyPokemonSum;
                        IList myPokemonSums = switchPokemonPacket.MyPokemonSums;

                        _prevSwitchPokemon = Managers.Scene.Data as Pokemon;

                        // 포켓몬 및 플레이어 데이터 채우기
                        _playerInfo = playerInfo;
                        _enemyPokemon = new Pokemon(enemyPokemonSum);

                        foreach (PokemonSummary sum in myPokemonSums)
                            _myPokemons.Add(new Pokemon(sum));
                        _myPokemon = _myPokemons[0];

                        // 포켓몬 랜더링
                        _enemyPokemonArea.FillPokemonInfo(_enemyPokemon, false);
                        _myPokemonArea.FillPokemonInfo(_prevSwitchPokemon, true);

                        // UI 생성
                        MakeUI();

                        if (_prevSwitchPokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                        {
                            _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftHide");
                            _myPokemonArea.PlayInfoZoneAnim("Zone_RightHide");
                            SceneState = BattleSceneState.AFTER_DIE_SWITCH_POKEMON;
                        }
                        else
                            SceneState = BattleSceneState.AFTER_SWITCH_POKEMON;
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
                        _enemyPokemonArea.FillPokemonInfo(_enemyPokemon, false);
                        _myPokemonArea.FillPokemonInfo(_myPokemon, true);

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
                        IList myPokemonSums = enterBattleScenePacket.PlayerPokemonSums;

                        // 포켓몬 및 플레이어 데이터 채우기
                        _playerInfo = playerInfo;
                        _enemyPokemon = new Pokemon(enemyPokemonSum);

                        foreach (PokemonSummary sum in myPokemonSums)
                            _myPokemons.Add(new Pokemon(sum));
                        _myPokemon = _myPokemons[0];

                        // 포켓몬 랜더링
                        _myPokemonArea.FillTrainerImage(playerInfo.PlayerGender);
                        _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftHide");
                        _myPokemonArea.PlayInfoZoneAnim("Zone_RightHide");

                        _enemyPokemonArea.FillPokemonInfo(_enemyPokemon, false);
                        _enemyPokemonArea.PlayPokemonZoneAnim("Zone_Default");

                        // UI 생성
                        MakeUI();
                    }
                }
                break;
            case BattleSceneState.HIT_POKEMON_BLINK:
                {
                    if (packet is S_ChangePokemonHp)
                    {
                        S_ChangePokemonHp changeHpPacket = packet as S_ChangePokemonHp;
                        SceneState = BattleSceneState.CHANGE_POKEMON_HP;

                        _loadingPacket = false;

                        int remainedHp = changeHpPacket.RemainedHp;

                        _defensePKM.PokemonStat.Hp = remainedHp;

                        _defensePKMArea.ChangePokemonHP(_defensePKM.PokemonStat.Hp);
                    }
                    else if (packet is S_PokemonFaint)
                    {
                        S_PokemonFaint pokemonFaintPacket = packet as S_PokemonFaint;
                        SceneState = BattleSceneState.CHANGE_POKEMON_HP;

                        _loadingPacket = false;

                        int remainedHp = pokemonFaintPacket.RemainedHp;
                        PokemonStatusCondition pokemonStatus = pokemonFaintPacket.PokemonStatus;

                        _defensePKM.PokemonStat.Hp = remainedHp;
                        _defensePKM.SetPokemonStatus(pokemonStatus);

                        _defensePKMArea.ChangePokemonHP(_defensePKM.PokemonStat.Hp);
                    }
                }
                break;
            case BattleSceneState.GOT_EXP_SCRIPTING:
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
                    _myPokemonArea.PlayTrainerZoneAnim("Zone_LeftDisappear");
                    SceneState = BattleSceneState.CHANGING_POKEMON;
                }
                break;
            case BattleSceneState.CHANGING_POKEMON:
                {
                    _myPokemonArea.PlayPokemonZoneAnim("Zone_RightAppear");
                    SceneState = BattleSceneState.SHOWING_POKEMON;
                }
                break;
            case BattleSceneState.SHOWING_POKEMON:
                {
                    _myPokemonArea.PlayInfoZoneAnim("Zone_LeftAppear");
                    SceneState = BattleSceneState.SHOWING_UI;
                }
                break;
            case BattleSceneState.SHOWING_UI:
                {
                    SceneState = BattleSceneState.SELECTING_ACTION;
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
                                    SceneState = BattleSceneState.NO_MOVE_SCRIPTING;

                                    _myPokemon.SetNoPPMove();
                                    SetWildPokemonSelectedMove();

                                    SetAttackAndDefensePokemonArea();

                                    List<string> scripts = new List<string>()
                                    {
                                        $"{_myPokemon.PokemonInfo.NickName} has no move to use!",
                                    };

                                    _scriptBox.BeginScriptTyping(scripts, true);
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
            case BattleSceneState.NO_MOVE_SCRIPTING:
                {
                    if (!_loadingPacket)
                    {
                        // 서버에게 기술 사용 요청
                        C_UsePokemonMove movePacket = new C_UsePokemonMove();
                        movePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                        movePacket.IsMyPokemon = _attackPKM == _myPokemon ? true : false;
                        movePacket.MoveOrder = _attackPKM.GetSelectedMoveIdx();

                        Managers.Network.Send(movePacket);

                        _loadingPacket = true;
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

                                _myPokemon.SelectedMove = selectedMove;
                                SetWildPokemonSelectedMove();

                                if (!_loadingPacket)
                                {
                                    // 서버에게 기술 사용 요청
                                    C_UsePokemonMove movePacket = new C_UsePokemonMove();
                                    movePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    movePacket.IsMyPokemon = _attackPKM == _myPokemon ? true : false;
                                    movePacket.MoveOrder = _attackPKM.GetSelectedMoveIdx();

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

                    GoToTheNextPokemonTurn();
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
                    SceneState = BattleSceneState.MY_POKEMON_DIE_SCRIPTING;

                    List<string> scripts = new List<string>()
                    {
                        $"{_defensePKM.PokemonInfo.NickName} fell down!",
                    };
                    _scriptBox.BeginScriptTyping(scripts);
                }
                break;
            case BattleSceneState.MY_POKEMON_DIE_SCRIPTING:
                {
                    if (_defensePKM == _myPokemon)
                    {
                        bool isAvailable = CheckAvailablePokemonToFight();

                        if (isAvailable)
                        {
                            SceneState = BattleSceneState.ASKING_TO_SWITCH_POKEMON;

                            List<string> scripts = new List<string>()
                            {
                                $"Do you want to change to another Pokemon?",
                            };
                            _scriptBox.BeginScriptTyping(scripts);
                        }
                        else
                        {
                            SceneState = BattleSceneState.NOT_AVAILABLE_BATTLE_SCRIPING;

                            List<string> scripts = new List<string>()
                            {
                                $"There is no more pokemon to fight!",
                                $"......",
                                $"I took the injured Pokemon to the nearby Pokemon Center in a hurry."
                            };
                            _scriptBox.BeginScriptTyping(scripts);
                        }
                    }
                    // 죽은 포켓몬이 야생 포켓몬일 경우
                    else if (_defensePKM != _myPokemon)
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
            case BattleSceneState.GOT_EXP_SCRIPTING:
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
            case BattleSceneState.ASKING_TO_SWITCH_POKEMON:
                {
                    SceneState = BattleSceneState.ANSWERING_TO_SWITCH_POKEMON;
                }
                break;
            case BattleSceneState.ANSWERING_TO_SWITCH_POKEMON:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            string answer = _yesOrNoSelectBox.GetSelectedBtnData() as string;

                            if (answer == "YES")
                            {
                                C_EnterPokemonListScene enterPokemonPacket = new C_EnterPokemonListScene();
                                enterPokemonPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(enterPokemonPacket);

                                _enterEffect.PlayEffect("FadeOut");

                                SceneState = BattleSceneState.MOVING_TO_POKEMON_SCENE;

                                _actionSelectBox.gameObject.SetActive(false);
                                _yesOrNoSelectBox.gameObject.SetActive(true);
                            }
                            else if (answer == "NO")
                            {
                                //SceneState = BattleSceneState.NEW_MOVE_NOT_LEARN_SCRIPTING;
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
            case BattleSceneState.AFTER_SWITCH_POKEMON:
                {
                    SceneState = BattleSceneState.COME_BACK_POKEMON_SCRIPTING;

                    List<string> scripts = new List<string>()
                    {
                        $"That is enough! Come back {_prevSwitchPokemon.PokemonInfo.NickName}!",
                    };
                    _scriptBox.BeginScriptTyping(scripts, false);
                }
                break;
            case BattleSceneState.AFTER_DIE_SWITCH_POKEMON:
                {
                    SceneState = BattleSceneState.GO_NEXT_POKEMON_SCRIPTING;

                    List<string> scripts = new List<string>()
                    {
                        $"Go! {_myPokemon.PokemonInfo.NickName}!",
                    };
                    _scriptBox.BeginScriptTyping(scripts, false);
                }
                break;
            case BattleSceneState.GO_NEXT_POKEMON_SCRIPTING:
                {
                    SceneState = BattleSceneState.SHOWING_POKEMON;
                }
                break;
            case BattleSceneState.AFTER_RETURN_BATTLESCENE:
                {
                    if (_myPokemons[0].PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                    {
                        List<string> scripts = new List<string>()
                        {
                            $"Do you want to change to another Pokemon?",
                        };
                        _scriptBox.BeginScriptTyping(scripts);

                        SceneState = BattleSceneState.ASKING_TO_SWITCH_POKEMON;
                    }
                    else
                    {
                        SceneState = BattleSceneState.SELECTING_ACTION;
                    }
                }
                break;
            case BattleSceneState.COME_BACK_POKEMON_SCRIPTING:
                {
                    _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftDisappear");

                    SceneState = BattleSceneState.COMING_BACK_POKEMON;
                }
                break;
            case BattleSceneState.COMING_BACK_POKEMON:
                {
                    _myPokemonArea.PlayInfoZoneAnim("Zone_RightDisappear");

                    SceneState = BattleSceneState.COME_BACK_POKEMON_CARD;
                }
                break;
            case BattleSceneState.COME_BACK_POKEMON_CARD:
                {
                    _myPokemonArea.FillPokemonInfo(_myPokemon, true);

                    _attackPKM = _myPokemon;
                    _attackPKMArea = _myPokemonArea;

                    _defensePKM = _enemyPokemon;
                    _defensePKMArea = _enemyPokemonArea;

                    SetWildPokemonSelectedMove();

                    List<string> scripts = new List<string>()
                    {
                        $"Go! {_myPokemon.PokemonInfo.NickName}!",
                    };
                    _scriptBox.BeginScriptTyping(scripts, false);

                    SceneState = BattleSceneState.SWITCH_POKEMON_SCRIPTING;
                }
                break;
            case BattleSceneState.SWITCH_POKEMON_SCRIPTING:
                {
                    _myPokemonArea.PlayPokemonZoneAnim("Zone_RightAppear");

                    SceneState = BattleSceneState.SHOWING_SWITCH_POKEMON;
                }
                break;
            case BattleSceneState.SHOWING_SWITCH_POKEMON:
                {
                    _myPokemonArea.PlayInfoZoneAnim("Zone_LeftAppear");

                    SceneState = BattleSceneState.SHOWING_SWITCH_POKEMON_CARD;
                }
                break;
            case BattleSceneState.SHOWING_SWITCH_POKEMON_CARD:
                {
                    GoToTheNextPokemonTurn();
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

    bool CheckAvailablePokemonToFight()
    {
        foreach (Pokemon pokemon in _myPokemons)
        {
            if (pokemon.PokemonInfo.PokemonStatus != PokemonStatusCondition.Fainting)
                return true;
        }

        return false;
    }

    void GoToTheNextPokemonTurn()
    {
        if (_attackPKMArea == _myPokemonArea)
            _isMyPKMAttack = true;
        else
            _isEnemyPKMAttack = true;

        if (!_isMyPKMAttack || !_isEnemyPKMAttack)
        {
            Pokemon prevAttackPKM = _attackPKM;
            BattleArea prevAttackPKMArea = _attackPKMArea;

            _attackPKM = _defensePKM;
            _attackPKMArea = _defensePKMArea;

            _defensePKM = prevAttackPKM;
            _defensePKMArea = prevAttackPKMArea;

            if (!_loadingPacket)
            {
                // 서버에게 기술 사용 요청
                C_UsePokemonMove movePacket = new C_UsePokemonMove();
                movePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                movePacket.IsMyPokemon = _attackPKM == _myPokemon ? true : false;
                movePacket.MoveOrder = _attackPKM.GetSelectedMoveIdx();

                Managers.Network.Send(movePacket);

                _loadingPacket = true;
            }
        }
        else
        {
            SceneState = BattleSceneState.SELECTING_ACTION;

            _isEnemyPKMAttack = false;
            _isMyPKMAttack = false;

            _scriptBox.SetScriptWihtoutTyping($"What will {_myPokemon.PokemonInfo.NickName} do?");
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

        if (stat.Hp <= 0)
        {
            SceneState = BattleSceneState.POKEMON_DIE;

            _defensePKMArea.PlayPokemonZoneAnim("Zone_DownDisappear");

            yield break;
        }

        GoToTheNextPokemonTurn();
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

    void SetWildPokemonSelectedMove()
    {
        List<PokemonMove> enemyPKMAvailableMoves = FindAvailableMove(_enemyPokemon);

        if (enemyPKMAvailableMoves.Count > 0)
            _enemyPokemon.SelectedMove = enemyPKMAvailableMoves[Random.Range(0, enemyPKMAvailableMoves.Count)];
        else
        {
            _enemyPokemon.SetNoPPMove();
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
