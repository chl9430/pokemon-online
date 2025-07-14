using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

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
    ASKING_TO_SWITCH_POKEMON = 56,
    EFFECTIVENESS_SCRIPTING = 58,
    POKEMON_DIE = 14,
    MY_POKEMON_DIE_SCRIPTING = 15,
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
    MOVING_TO_GAME_SCENE = 92,
    ESCAPE_SCRIPTING = 100
}

public class BattleScene : BaseScene
{
    IMessage _packet;
    BattleSceneState _sceneState = BattleSceneState.NONE;
    TextMeshProUGUI[] _moveInfoTMPs;
    PlayableDirector _playableDirector;
    PlayerInfo _playerInfo;

    List<string> _script;
    List<Pokemon> _myPokemons;
    Pokemon _myPokemon;
    Pokemon _enemyPokemon;
    PokemonMove _newLearnableMove;
    Item _usedItem;
    Pokemon _prevSwitchPokemon;

    Pokemon _attackPokemon;
    BattleArea _attackPKMArea;
    Pokemon _defensePokemon;
    BattleArea _defensePKMArea;
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

        _script = new List<string>();
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
        if (packet is S_SetBattlePokemonMove)
        {
            _loadingPacket = false;

            _packet = packet;

            S_SetBattlePokemonMove setMovePacket = packet as S_SetBattlePokemonMove;
            int myMoveOrder = setMovePacket.MyMoveOrder;
            int enemyMoveOrder = setMovePacket.EnemyMoveOrder;

            _myPokemon.SetSelectedMove(myMoveOrder);
            _enemyPokemon.SetSelectedMove(enemyMoveOrder);

            SendUseMovePacket();
        }
        else if (packet is S_UsePokemonMove)
        {
            _loadingPacket = false;

            _packet = packet;

            S_UsePokemonMove useMovePacket = packet as S_UsePokemonMove;
            bool isMyPokemon = useMovePacket.IsMyPokemon;
            int remainedPP = useMovePacket.RemainedPP;

            if (isMyPokemon)
            {
                _attackPokemon = _myPokemon;
                _attackPKMArea = _myPokemonArea;
                _defensePokemon = _enemyPokemon;
                _defensePKMArea = _enemyPokemonArea;
            }
            else
            {
                _attackPokemon = _enemyPokemon;
                _attackPKMArea = _enemyPokemonArea;
                _defensePokemon = _myPokemon;
                _defensePKMArea = _myPokemonArea;
            }

            _attackPokemon.SelectedMove.CurPP = remainedPP;

            RefillScriptBox(new string[] { $"{_attackPokemon.PokemonInfo.NickName} used {_attackPokemon.SelectedMove.MoveName}!" }, true);

            if (_attackPokemon == _myPokemon)
            {
                if (_myPokemon.SelectedMove == _myPokemon.NoPPMove)
                {
                    RefillScriptBox(new string[] { 
                        $"{_myPokemon.PokemonInfo.NickName} has no move to use!", 
                        $"{_myPokemon.PokemonInfo.NickName} used {_myPokemon.SelectedMove.MoveName}!" 
                    }, true);
                }
            }

            SceneState = BattleSceneState.ATTACK_INSTRUCTING;

            return;
        }
        else if (packet is S_GetEnemyPokemonExp)
        {
            _loadingPacket = false;

            S_GetEnemyPokemonExp getExpPacket = packet as S_GetEnemyPokemonExp;
            int exp = getExpPacket.Exp;

            _remainEXPToGet = exp;

            SceneState = BattleSceneState.GOT_EXP_SCRIPTING;

            RefillScriptBox(new string[] { $"{_myPokemon.PokemonInfo.NickName} got {_remainEXPToGet} exp!" });

            return;
        }
        else if (packet is S_CheckAndApplyRemainedExp)
        {
            _loadingPacket = false;

            _packet = packet;

            S_CheckAndApplyRemainedExp expPacket = packet as S_CheckAndApplyRemainedExp;
            int finalExp = expPacket.FinalExp;

            _myPokemon.PokemonExpInfo.CurExp += finalExp;

            _myPokemonArea.ChangePokemonEXP(_myPokemon.PokemonExpInfo.CurExp);
            SceneState = BattleSceneState.GETTING_EXP;

            return;
        }
        else if (packet is S_EscapeFromWildPokemon)
        {
            _loadingPacket = false;

            _packet = packet;

            S_EscapeFromWildPokemon escapePacket = packet as S_EscapeFromWildPokemon;
            bool canEscape = escapePacket.CanEscape;

            if (canEscape)
                RefillScriptBox(new string[] { $"Got away safely!" });
            else
                RefillScriptBox(new string[] { $"Can't escape!" });

            SceneState = BattleSceneState.ESCAPE_SCRIPTING;

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
                    RefillScriptBox(new string[] { $"Wild {_enemyPokemon.PokemonInfo.PokemonName} appeard!", $"Go! {_myPokemon.PokemonInfo.NickName}!" });

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
                                    SendSetBattlePokemonMovePacket(-1);
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
                            else if (selectedAction == "Run")
                            {
                                if (!_loadingPacket)
                                {
                                    C_RequestDataById requestPacket = new C_RequestDataById();
                                    requestPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
                                    requestPacket.RequestType = RequestType.EscapeFromWildPokemon;

                                    Managers.Network.Send(requestPacket);

                                    _loadingPacket = true;
                                }
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

                                RefillScriptBox(new string[] { $"Cannot use this move!" });
                            }
                            else
                            {
                                SendSetBattlePokemonMovePacket(_moveSelectBox.GetSelectedIdx());
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
                    if (_packet is S_UsePokemonMove)
                    {
                        S_UsePokemonMove s_UseMovePacket = _packet as S_UsePokemonMove;
                        bool isHit = s_UseMovePacket.IsHit;
                        float typeEffectiveness = s_UseMovePacket.TypeEffectiveness;

                        if (isHit)
                        {
                            if (typeEffectiveness == 0f)
                            {
                                RefillScriptBox(new string[] { $"It doesn't affect {_defensePokemon.PokemonInfo.NickName}..." }, true);
                                SceneState = BattleSceneState.EFFECTIVENESS_SCRIPTING;
                            }
                            else
                            {
                                SceneState = BattleSceneState.FIRST_ATTACK_ANIMATION;

                                if (_attackPokemon == _myPokemon)
                                    _attackPKMArea.PlayBattlePokemonAnim("BattlePokemon_RightAttack");
                                else
                                    _attackPKMArea.PlayBattlePokemonAnim("BattlePokemon_LeftAttack");

                                _defensePKMArea.TriggerPokemonHitImage(_attackPokemon);
                            }
                        }
                        else
                        {
                            RefillScriptBox(new string[] { $"{_attackPokemon.PokemonInfo.NickName}'s attack is off the mark!" }, true);
                            SceneState = BattleSceneState.FIRST_ATTACK_FAILED;
                        }
                    }
                }
                break;
            case BattleSceneState.FIRST_ATTACK_FAILED:
                {
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
                    if (_packet is S_UsePokemonMove)
                    {
                        S_UsePokemonMove s_UseMovePacket = _packet as S_UsePokemonMove;
                        int remainedHp = s_UseMovePacket.RemainedHp;
                        PokemonStatusCondition status = s_UseMovePacket.PokemonStatus;

                        _defensePokemon.PokemonStat.Hp = remainedHp;
                        _defensePokemon.PokemonInfo.PokemonStatus = status;

                        _defensePKMArea.ChangePokemonHP(_defensePokemon.PokemonStat.Hp);
                        SceneState = BattleSceneState.CHANGE_POKEMON_HP;
                    }
                }
                break;
            case BattleSceneState.CHANGE_POKEMON_HP:
                {
                    if (_packet is S_UsePokemonMove)
                    {
                        S_UsePokemonMove s_UseMovePacket = _packet as S_UsePokemonMove;
                        float typeEffectiveness = s_UseMovePacket.TypeEffectiveness;
                        bool isCriticalHit = s_UseMovePacket.IsCriticalHit;

                        List<string> scripts = new List<string>();

                        if (isCriticalHit)
                            scripts.Add("A critical hit!");

                        if (typeEffectiveness < 1f)
                        {
                            scripts.Add("It's not very effective...");
                            _scriptBox.BeginScriptTyping(scripts, true);
                            SceneState = BattleSceneState.EFFECTIVENESS_SCRIPTING;
                        }
                        else if (typeEffectiveness > 1f)
                        {
                            scripts.Add("It's super effective!");
                            _scriptBox.BeginScriptTyping(scripts, true);
                            SceneState = BattleSceneState.EFFECTIVENESS_SCRIPTING;
                        }
                        else if (typeEffectiveness == 1f)
                        {
                            if (scripts.Count > 0)
                            {
                                _scriptBox.BeginScriptTyping(scripts, true);
                                SceneState = BattleSceneState.EFFECTIVENESS_SCRIPTING;
                            }
                            else
                            {
                                StartCoroutine(ActionAfterChangeHP());
                            }
                        }
                    }
                }
                break;
            case BattleSceneState.EFFECTIVENESS_SCRIPTING:
                {
                    StartCoroutine(ActionAfterChangeHP());
                }
                break;
            case BattleSceneState.POKEMON_DIE:
                {
                    List<string> scripts = new List<string>()
                    {
                        $"{_defensePokemon.PokemonInfo.NickName} fell down!"
                    };

                    if (_defensePokemon == _myPokemon)
                    {
                        bool isAvailableToFight = CheckAvailablePokemonToFight();

                        if (isAvailableToFight)
                        {
                            scripts.Add($"Do you want to change to another Pokemon?");
                            SceneState = BattleSceneState.ASKING_TO_SWITCH_POKEMON;
                            _scriptBox.BeginScriptTyping(scripts);

                            return;
                        }
                        else
                        {
                            scripts.Add($"There is no more pokemon to fight!");
                            scripts.Add($"......");
                            scripts.Add($"I took the injured Pokemon to the nearby Pokemon Center in a hurry.");
                        }
                    }

                    _scriptBox.BeginScriptTyping(scripts);

                    SceneState = BattleSceneState.MY_POKEMON_DIE_SCRIPTING;
                }
                break;
            case BattleSceneState.MY_POKEMON_DIE_SCRIPTING:
                {
                    if (_defensePokemon == _myPokemon)
                    {
                        Debug.Log("Battle Finish");
                    }
                    // 죽은 포켓몬이 야생 포켓몬일 경우
                    else
                    {
                        if (_myPokemon.PokemonInfo.Level != 100)
                        {
                            SendRequestDataPacket(RequestType.GetEnemyPokemonExp);
                        }
                    }
                }
                break;
            case BattleSceneState.ASKING_TO_SWITCH_POKEMON:
                {
                    SceneState = BattleSceneState.ANSWERING_TO_SWITCH_POKEMON;
                }
                break;
            case BattleSceneState.GOT_EXP_SCRIPTING:
                {
                    SendRequestDataPacket(RequestType.CheckAndApplyRemainedExp);
                }
                break;
            case BattleSceneState.GETTING_EXP:
                {
                    if (_packet is S_CheckAndApplyRemainedExp)
                    {
                        S_CheckAndApplyRemainedExp expPacket = _packet as S_CheckAndApplyRemainedExp;
                        int pokemonLevel = expPacket.PokemonLevel;
                        PokemonStat pokemonStat = expPacket.PokemonStat;
                        LevelUpStatusDiff statDiff = expPacket.StatDiff;
                        PokemonExpInfo expInfo = expPacket.ExpInfo;

                        _myPokemon.PokemonExpInfo = expInfo;

                        if (pokemonStat != null)
                        {
                            _myPokemon.PokemonInfo.Level = pokemonLevel;
                            _myPokemon.PokemonStat = pokemonStat;

                            _myPokemonArea.FillPokemonInfo(_myPokemon, true);
                            _statusBox.SetStatusDiffRate(statDiff);

                            RefillScriptBox(new string[] { $"{_myPokemon.PokemonInfo.NickName}'s level went up to {_myPokemon.PokemonInfo.Level}." });
                            SceneState = BattleSceneState.LEVEL_UP_SCRIPTING;
                        }
                        else
                        {
                            C_ReturnGame returnGamePacket = new C_ReturnGame();
                            returnGamePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                            Managers.Network.SavePacket(returnGamePacket);

                            _enterEffect.PlayEffect("FadeOut");

                            SceneState = BattleSceneState.MOVING_TO_GAME_SCENE;
                        }
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
                    if (_packet is S_CheckAndApplyRemainedExp)
                    {
                        S_CheckAndApplyRemainedExp expPacket = _packet as S_CheckAndApplyRemainedExp;
                        PokemonMoveSummary newMoveSum = expPacket.NewMoveSum;

                        if (newMoveSum != null)
                        {
                            if (_myPokemon.PokemonMoves.Count < 4)
                            {
                                PokemonMove move = new PokemonMove(newMoveSum);
                                _myPokemon.PokemonMoves.Add(move);

                                RefillScriptBox(new string[] { $"{_myPokemon.PokemonInfo.NickName} learned {move.MoveName}!" });

                                SceneState = BattleSceneState.NEW_MOVE_LEARN_SCRIPTING;
                            }
                            else
                            {
                                _newLearnableMove = new PokemonMove(newMoveSum);

                                RefillScriptBox(new string[] { $"{_myPokemon.PokemonInfo.NickName} wants to learn the move {_newLearnableMove.MoveName}.", $"However, {_myPokemon.PokemonInfo.NickName} already knows four moves.", $"Sould a move be deleted and replaced with {_newLearnableMove.MoveName}?" });

                                SceneState = BattleSceneState.ASKING_TO_LEARN_NEW_MOVE;
                            }
                        }
                        else
                        {
                            SendRequestDataPacket(RequestType.CheckAndApplyRemainedExp);
                        }
                    }
                }
                break;
            case BattleSceneState.NEW_MOVE_LEARN_SCRIPTING:
                {
                    SendRequestDataPacket(RequestType.CheckAndApplyRemainedExp);
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
                                RefillScriptBox(new string[] { $"{_myPokemon.PokemonInfo.NickName} did not learn the move {_newLearnableMove.MoveName}." });

                                SceneState = BattleSceneState.NEW_MOVE_NOT_LEARN_SCRIPTING;
                            }
                        }
                    }
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
                                SendRequestDataPacket(RequestType.EscapeFromWildPokemon);
                            }
                        }
                    }
                }
                break;
            case BattleSceneState.NEW_MOVE_NOT_LEARN_SCRIPTING:
                {
                    SendRequestDataPacket(RequestType.CheckAndApplyRemainedExp);
                }
                break;
            case BattleSceneState.AFTER_USE_ITEM_UPDATE:
                {
                    RefillScriptBox(new string[] { $"{_playerInfo.PlayerName} used {_usedItem.ItemName}!" }, true);

                    _sceneState = BattleSceneState.ITEM_USE_SCRIPTING;
                }
                break;
            case BattleSceneState.AFTER_SWITCH_POKEMON:
                {
                    SceneState = BattleSceneState.COME_BACK_POKEMON_SCRIPTING;

                    RefillScriptBox(new string[] { $"That is enough! Come back {_prevSwitchPokemon.PokemonInfo.NickName}!" });
                }
                break;
            case BattleSceneState.AFTER_DIE_SWITCH_POKEMON:
                {
                    SceneState = BattleSceneState.GO_NEXT_POKEMON_SCRIPTING;

                    RefillScriptBox(new string[] { $"Go! {_myPokemon.PokemonInfo.NickName}!" });
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
                        RefillScriptBox(new string[] { $"Do you want to change to another Pokemon?" });

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

                    RefillScriptBox(new string[] { $"Go! {_myPokemon.PokemonInfo.NickName}!" }, false);

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
                    SendSetBattlePokemonMovePacket(-1);
                }
                break;
            case BattleSceneState.ESCAPE_SCRIPTING:
                {
                    if (_packet is S_EscapeFromWildPokemon)
                    {
                        S_EscapeFromWildPokemon escapePacket = _packet as S_EscapeFromWildPokemon;
                        bool canEscape = escapePacket.CanEscape;

                        if (canEscape)
                        {
                            C_ReturnGame returnGamePacket = new C_ReturnGame();
                            returnGamePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                            Managers.Network.SavePacket(returnGamePacket);

                            _enterEffect.PlayEffect("FadeOut");

                            SceneState = BattleSceneState.MOVING_TO_GAME_SCENE;
                        }
                        else
                        {
                            if (_myPokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                            {
                                C_EnterPokemonListScene enterPokemonPacket = new C_EnterPokemonListScene();
                                enterPokemonPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(enterPokemonPacket);

                                _enterEffect.PlayEffect("FadeOut");

                                SceneState = BattleSceneState.MOVING_TO_POKEMON_SCENE;

                                _actionSelectBox.gameObject.SetActive(false);
                                _yesOrNoSelectBox.gameObject.SetActive(true);
                            }
                            else
                                SendSetBattlePokemonMovePacket(-1);
                        }
                    }
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
            case BattleSceneState.MOVING_TO_GAME_SCENE:
                {
                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.Game);
                }
                break;
        }
    }

    void SendRequestDataPacket(RequestType requestType)
    {
        if (!_loadingPacket)
        {
            C_RequestDataById c_RequestDataPacket = new C_RequestDataById();
            c_RequestDataPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
            c_RequestDataPacket.RequestType = requestType;

            Managers.Network.Send(c_RequestDataPacket);

            _loadingPacket = true;
        }
    }

    void SendSetBattlePokemonMovePacket(int moveOrder)
    {
        if (!_loadingPacket)
        {
            C_SetBattlePokemonMove setMovePacket = new C_SetBattlePokemonMove();
            setMovePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
            setMovePacket.MoveOrder = moveOrder;

            Managers.Network.Send(setMovePacket);

            _loadingPacket = true;
        }
    }

    void SendUseMovePacket()
    {
        if (!_loadingPacket)
        {
            // 서버에게 기술 사용 요청
            C_UsePokemonMove movePacket = new C_UsePokemonMove();
            movePacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

            Managers.Network.Send(movePacket);

            _loadingPacket = true;
        }
    }

    void RefillScriptBox(string[] scripts, bool autoSkip = false, float autoSkipTime = 1f)
    {
        _script.Clear();

        for (int i = 0; i < scripts.Length; i++)
            _script.Add(scripts[i]);
        _scriptBox.BeginScriptTyping(_script, autoSkip, autoSkipTime);
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
        if (_packet is S_UsePokemonMove)
        {
            S_UsePokemonMove s_UseMovePacket = _packet as S_UsePokemonMove;
            bool isTurnFinish = s_UseMovePacket.IsTurnFinish;

            if (isTurnFinish)
            {
                SceneState = BattleSceneState.SELECTING_ACTION;
            }
            else
            {
                SendUseMovePacket();
            }
        }
    }

    IEnumerator ActionAfterChangeHP()
    {
        yield return new WaitForSeconds(1f);

        if (_packet is S_UsePokemonMove)
        {
            S_UsePokemonMove s_UseMovePacket = _packet as S_UsePokemonMove;
            PokemonStatusCondition statusCondition = s_UseMovePacket.PokemonStatus;

            if (statusCondition == PokemonStatusCondition.Fainting)
            {
                SceneState = BattleSceneState.POKEMON_DIE;

                _defensePKMArea.PlayPokemonZoneAnim("Zone_DownDisappear");

                yield break;
            }

            GoToTheNextPokemonTurn();
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

    public override void Clear()
    {
    }
}
