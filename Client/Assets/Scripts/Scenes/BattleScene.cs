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
    WILD_POKEMON_APPEAR_SCRIPTING = 200,
    TRAINER_DISAPPEARING = 210,
    MY_POKEMON_APPEAR_SCRIPTING = 220,
    SHOWING_POKEMON = 240,
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
    AFTER_USE_ITEM_UPDATE = 46,
    AFTER_SWITCH_POKEMON = 47,
    AFTER_DIE_SWITCH_POKEMON = 76,
    GO_NEXT_POKEMON_SCRIPTING = 77,
    ITEM_USE_SCRIPTING = 87,
    MOVING_SCENE = 1000,
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
    Pokemon _myPokemon;
    Pokemon _enemyPokemon;
    Item _usedItem;

    bool _loadingPacket = false;

    [SerializeField] ScriptBoxUI _scriptBox;
    [SerializeField] GridLayoutSelectBox _actionSelectBox;
    [SerializeField] GridLayoutSelectBox _moveSelectBox;
    [SerializeField] GameObject _moveInfoBox;
    [SerializeField] BattleArea _enemyPokemonArea;
    [SerializeField] BattleArea _myPokemonArea;
    [SerializeField] StatusBox _statusBox;

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
            else if (_sceneState == BattleSceneState.MOVING_SCENE)
            {
                _enterEffect.PlayEffect("FadeOut");
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
            if (_sceneState == BattleSceneState.UPGRADING_STATUS || _sceneState == BattleSceneState.SHOWING_UPGRADED_STATUS)
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
                List<string> btnNames = new List<string>()
                {
                    "Yes",
                    "No",
                };
                _scriptBox.CreateSelectBox(btnNames, btnNames.Count, 1, 400, 100);
            }
            else
            {
                _scriptBox.HideSelectBox();
            }
        }
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Battle;

        _script = new List<string>();
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

    void MakeUI(PokemonSummary pokemonSum)
    {
        // 액션 버튼 데이터 채우기
        List<string> btnNames = new List<string>()
        {
            "Fight",
            "Bag",
            "Pokemon",
            "Run"
        };
        _actionSelectBox.CreateButtons(btnNames, 2, 400, 100);

        // 기술 버튼 데이터 채우기
        List<string> moveNames = new List<string>();
        foreach (PokemonMoveSummary moveSum in pokemonSum.PokemonMoves)
            moveNames.Add(moveSum.MoveName);

        List<object> moves = new List<object>();
        foreach (PokemonMove move in _myPokemon.PokemonMoves)
            moves.Add(move);
        _moveSelectBox.CreateButtons(moveNames, 2, 600, 100, moves);
    }

    public override void UpdateData(IMessage packet)
    {
        _loadingPacket = false;

        _packet = packet;
        
        if (packet is S_ProcessTurn)
        {
            S_ProcessTurn processTurnPacket = packet as S_ProcessTurn;
            PokemonMoveSummary usedMoveSum = processTurnPacket.UsedMoveSum;
            bool canUseMove = processTurnPacket.CanUseMove;
            bool isMyPokemon = processTurnPacket.IsMyPokemon;
            int moveOrder = processTurnPacket.UsedMoveOrder;

            if (canUseMove)
            {
                Pokemon attackPokemon = isMyPokemon ? _myPokemon : _enemyPokemon;

                if (isMyPokemon && moveOrder != -1)
                {
                    (_moveSelectBox.GetSelectedBtn().BtnData as PokemonMove).CurPP = usedMoveSum.CurPP;
                }

                RefillScriptBox(new string[] { $"{attackPokemon.PokemonInfo.NickName} used {usedMoveSum.MoveName}!" }, true);
                SceneState = BattleSceneState.ATTACK_INSTRUCTING;
            }
            else
            {
                RefillScriptBox(new string[] { $"Cannot use this move!" });
                SceneState = BattleSceneState.CANNOT_USE_MOVE;
            }
        }
        else if (packet is S_CheckAvailableBattlePokemon)
        {
            S_CheckAvailableBattlePokemon checkBattlePokemonPacket = packet as S_CheckAvailableBattlePokemon;
            bool canFight = checkBattlePokemonPacket.CanFight;

            if (canFight)
            {
                RefillScriptBox(
                    new string[] { $"Do you want to change to another Pokemon?" }
                    );
                SceneState = BattleSceneState.ASKING_TO_SWITCH_POKEMON;
            }
            else
            {
                RefillScriptBox(
                    new string[] { 
                        "There is no more pokemon to fight!",
                        "......",
                        "I took the injured Pokemon to the nearby Pokemon Center in a hurry."
                    });
            }
        }
        else if (packet is S_GetEnemyPokemonExp)
        {
            S_GetEnemyPokemonExp getExpPacket = packet as S_GetEnemyPokemonExp;
            PokemonSummary pokemonSum = getExpPacket.GotExpPokemonSum;
            int exp = getExpPacket.Exp;

            RefillScriptBox(new string[] { $"{pokemonSum.PokemonInfo.NickName} got {exp} exp!" });
            SceneState = BattleSceneState.GOT_EXP_SCRIPTING;
            return;
        }
        else if (packet is S_CheckAndApplyRemainedExp)
        {
            S_CheckAndApplyRemainedExp expPacket = packet as S_CheckAndApplyRemainedExp;
            PokemonSummary expPokemonSum = expPacket.ExpPokemonSum;
            int finalExp = expPacket.FinalExp;
            bool isMainPokemon = expPacket.IsMainPokemon;

            if (isMainPokemon)
            {
                _myPokemon.PokemonExpInfo.CurExp += finalExp;
                _myPokemonArea.ChangePokemonEXP(_myPokemon.PokemonExpInfo.CurExp);
                SceneState = BattleSceneState.GETTING_EXP;
            }
            else
            {
                LevelUpStatusDiff statDiff = expPacket.StatDiff;

                if (statDiff != null)
                {
                    _statusBox.SetStatusDiffRate(statDiff);
                    RefillScriptBox(new string[] { $"{expPokemonSum.PokemonInfo.NickName}'s level went up to {expPokemonSum.PokemonInfo.Level}." });
                    SceneState = BattleSceneState.LEVEL_UP_SCRIPTING;
                }
                else
                {
                    SendRequestDataPacket(RequestType.CheckExpPokemon);
                }
            }

            return;
        }
        else if (packet is S_SwitchBattlePokemon)
        {
            _enterEffect.PlayEffect("FadeIn");
            _myPokemonArea.SetActiveTrainer(false);

            S_SwitchBattlePokemon switchPokemonPacket = packet as S_SwitchBattlePokemon;
            PlayerInfo playerInfo = switchPokemonPacket.PlayerInfo;
            PokemonSummary enemyPokemonSum = switchPokemonPacket.EnemyPokemonSum;
            IList myPokemonSums = switchPokemonPacket.MyPokemonSums;
            PokemonSummary prevPokemonSum = switchPokemonPacket.PrevPokemonSum;

            // 포켓몬 및 플레이어 데이터 채우기
            _playerInfo = playerInfo;
            _enemyPokemon = new Pokemon(enemyPokemonSum);

            _myPokemon = new Pokemon(myPokemonSums[0] as PokemonSummary);

            // 포켓몬 랜더링
            _enemyPokemonArea.FillPokemonInfo(_enemyPokemon, false);
            _myPokemonArea.FillPokemonInfo(new Pokemon(prevPokemonSum), true);

            // UI 생성
            MakeUI(myPokemonSums[0] as PokemonSummary);

            if (prevPokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
            {
                _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftHide");
                _myPokemonArea.PlayInfoZoneAnim("Zone_RightHide");
                SceneState = BattleSceneState.AFTER_DIE_SWITCH_POKEMON;
            }
            else
            {
                SceneState = BattleSceneState.NONE;
            }
        }
        else if (packet is S_EscapeFromWildPokemon)
        {
            S_EscapeFromWildPokemon escapePacket = packet as S_EscapeFromWildPokemon;
            bool canEscape = escapePacket.CanEscape;

            if (canEscape)
                RefillScriptBox(new string[] { $"Got away safely!" });
            else
                RefillScriptBox(new string[] { $"Can't escape!" });

            SceneState = BattleSceneState.ESCAPE_SCRIPTING;

            return;
        }
        else if (packet is S_CheckExpPokemon)
        {
            S_CheckExpPokemon checkExpPokemonPacket = packet as S_CheckExpPokemon;
            bool isThereMoreExpPokemon = checkExpPokemonPacket.IsThereMoreExpPokemon;

            if (isThereMoreExpPokemon)
                SendRequestDataPacket(RequestType.GetEnemyPokemonExp);
            else
                SendRequestDataPacket(RequestType.CheckPokemonEvolution);
        }
        else if (packet is S_CheckPokemonEvolution)
        {
            S_CheckPokemonEvolution checkEvolutionPacket = packet as S_CheckPokemonEvolution;
            bool goToEvolutionScene = checkEvolutionPacket.GoToEvolutionScene;

            IMessage moveScenePacket;
            if (goToEvolutionScene)
            {
                moveScenePacket = new C_EnterPokemonEvolutionScene();
                (moveScenePacket as C_EnterPokemonEvolutionScene).PlayerId = _playerInfo.ObjectInfo.ObjectId;
            }
            else
            {
                moveScenePacket = new C_ReturnGame();
                (moveScenePacket as C_ReturnGame).PlayerId = _playerInfo.ObjectInfo.ObjectId;
            }

            Managers.Network.SavePacket(moveScenePacket);
            SceneState = BattleSceneState.MOVING_SCENE;
            _actionSelectBox.gameObject.SetActive(false);
        }
        else if (packet is S_ReturnPokemonBattleScene)
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

            _myPokemon = new Pokemon(myPokemonSums[0] as PokemonSummary);

            // 포켓몬 랜더링
            _enemyPokemonArea.FillPokemonInfo(_enemyPokemon, false);
            _myPokemonArea.FillPokemonInfo(_myPokemon, true);

            // UI 생성
            MakeUI(myPokemonSums[0] as PokemonSummary);

            SceneState = BattleSceneState.NONE;
        }

        switch (_sceneState)
        {
            case BattleSceneState.NONE:
                {
                    if (packet is S_UseItem)
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
                        MakeUI(myPokemonSum);

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

                        _myPokemon = new Pokemon(myPokemonSums[0] as PokemonSummary);

                        // 포켓몬 랜더링
                        _myPokemonArea.FillTrainerImage(playerInfo.PlayerGender);
                        _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftHide");
                        _myPokemonArea.PlayInfoZoneAnim("Zone_RightHide");

                        _enemyPokemonArea.FillPokemonInfo(_enemyPokemon, false);
                        _enemyPokemonArea.PlayPokemonZoneAnim("Zone_Default");

                        // UI 생성
                        MakeUI(myPokemonSums[0] as PokemonSummary);
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
                    if (_packet is S_SwitchBattlePokemon)
                    {
                        PokemonSummary prevPokemonSum = (_packet as S_SwitchBattlePokemon).PrevPokemonSum;

                        RefillScriptBox(new string[] { $"That is enough! Come back {prevPokemonSum.PokemonInfo.NickName}!" });
                        SceneState = BattleSceneState.COME_BACK_POKEMON_SCRIPTING;
                    }
                    else if (_packet is S_ReturnPokemonBattleScene)
                    {
                        SceneState = BattleSceneState.SELECTING_ACTION;
                    }
                    else
                    {
                        SceneState = BattleSceneState.INTRO;
                    }
                }
                break;
            case BattleSceneState.INTRO:
                {
                    RefillScriptBox(new string[] { $"Wild {_enemyPokemon.PokemonInfo.PokemonName} appeard!" });
                    SceneState = BattleSceneState.WILD_POKEMON_APPEAR_SCRIPTING;
                }
                break;
            case BattleSceneState.WILD_POKEMON_APPEAR_SCRIPTING:
                {
                    _myPokemonArea.PlayTrainerZoneAnim("Zone_LeftDisappear");
                    SceneState = BattleSceneState.TRAINER_DISAPPEARING;
                }
                break;
            case BattleSceneState.TRAINER_DISAPPEARING:
                {
                    RefillScriptBox(new string[] { $"Go! {_myPokemon.PokemonInfo.NickName}!" });
                    SceneState = BattleSceneState.MY_POKEMON_APPEAR_SCRIPTING;
                }
                break;
            case BattleSceneState.MY_POKEMON_APPEAR_SCRIPTING:
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
                    if (_packet is S_SwitchBattlePokemon)
                    {
                        S_SwitchBattlePokemon switchPokemonPacket = _packet as S_SwitchBattlePokemon;
                        PokemonSummary prevPokemonSum = switchPokemonPacket.PrevPokemonSum;

                        if (prevPokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                        {
                            SceneState = BattleSceneState.SELECTING_ACTION;
                        }
                        else
                        {
                            SendProcessTurnPacket(_moveSelectBox.GetSelectedIdx());
                        }
                    }
                    else
                    {
                        SceneState = BattleSceneState.SELECTING_ACTION;
                    }
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
                                    SendProcessTurnPacket(-1);
                                }
                            }
                            else if (selectedAction == "Pokemon")
                            {
                                C_EnterPokemonListScene enterPokemonPacket = new C_EnterPokemonListScene();
                                enterPokemonPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(enterPokemonPacket);

                                SceneState = BattleSceneState.MOVING_SCENE;
                            }
                            else if (selectedAction == "Bag")
                            {
                                C_EnterPlayerBagScene enterBagPacket = new C_EnterPlayerBagScene();
                                enterBagPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(enterBagPacket);

                                SceneState = BattleSceneState.MOVING_SCENE;
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
                            SendProcessTurnPacket(_moveSelectBox.GetSelectedIdx());
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
                    if (_packet is S_ProcessTurn)
                    {
                        S_ProcessTurn processTurn = _packet as S_ProcessTurn;
                        bool isMyPokemon = processTurn.IsMyPokemon;
                        int usedMoveOrder = processTurn.UsedMoveOrder;
                        bool isHit = processTurn.IsHit;
                        float typeEffectiveness = processTurn.TypeEffectiveness;

                        if (isMyPokemon)
                        {
                            if (isHit)
                            {
                                if (typeEffectiveness == 0f)
                                {
                                    RefillScriptBox(new string[] { $"It doesn't affect {_enemyPokemon.PokemonInfo.NickName}..." }, true);
                                    SceneState = BattleSceneState.EFFECTIVENESS_SCRIPTING;
                                }
                                else
                                {
                                    SceneState = BattleSceneState.FIRST_ATTACK_ANIMATION;
                                    PokemonMove usedMove = usedMoveOrder != -1 ? _myPokemon.PokemonMoves[usedMoveOrder] : _myPokemon.NoPPMove;

                                    _myPokemonArea.PlayBattlePokemonAnim("BattlePokemon_RightAttack");
                                    _enemyPokemonArea.TriggerPokemonHitImage(usedMove);
                                }
                            }
                            else
                            {
                                RefillScriptBox(new string[] { $"{_myPokemon.PokemonInfo.NickName}'s attack is off the mark!" }, true);
                                SceneState = BattleSceneState.FIRST_ATTACK_FAILED;
                            }
                        }
                        else
                        {

                            PokemonMove usedMove = usedMoveOrder != -1 ? _enemyPokemon.PokemonMoves[usedMoveOrder] : _enemyPokemon.NoPPMove;

                            _enemyPokemonArea.PlayBattlePokemonAnim("BattlePokemon_LeftAttack");
                            _myPokemonArea.TriggerPokemonHitImage(usedMove);
                            SceneState = BattleSceneState.FIRST_ATTACK_ANIMATION;
                        }
                    }
                }
                break;
            case BattleSceneState.FIRST_ATTACK_FAILED:
                {
                    if (_packet is S_ProcessTurn)
                    {
                        S_ProcessTurn processTurnPacket = _packet as S_ProcessTurn;
                        bool isTurnFinish = processTurnPacket.IsTurnFinish;

                        if (isTurnFinish)
                        {
                            SceneState = BattleSceneState.SELECTING_ACTION;
                        }
                        else
                        {
                            SendProcessTurnPacket(_moveSelectBox.GetSelectedIdx());
                        }
                    }
                }
                break;
            case BattleSceneState.FIRST_ATTACK_ANIMATION:
                {
                    if (_packet is S_ProcessTurn)
                    {
                        S_ProcessTurn processTurnPacket = _packet as S_ProcessTurn;
                        bool isMyPokemon = processTurnPacket.IsMyPokemon;

                        BattleArea defensePokemonArea = isMyPokemon ? _enemyPokemonArea : _myPokemonArea;

                        SceneState = BattleSceneState.HIT_POKEMON_BLINK;
                        defensePokemonArea.PlayBattlePokemonAnim("BattlePokemon_Hit");
                    }
                }
                break;
            case BattleSceneState.HIT_POKEMON_BLINK:
                {
                    if (_packet is S_ProcessTurn)
                    {
                        S_ProcessTurn processTurnPacket = _packet as S_ProcessTurn;
                        PokemonSummary defensePokemonSum = processTurnPacket.DefensePokemonSum;
                        bool isMyPokemon = processTurnPacket.IsMyPokemon;

                        BattleArea defensePokemonArea = isMyPokemon ? _enemyPokemonArea : _myPokemonArea;

                        defensePokemonArea.ChangePokemonHP(defensePokemonSum.PokemonStat.Hp);
                        SceneState = BattleSceneState.CHANGE_POKEMON_HP;
                    }
                }
                break;
            case BattleSceneState.CHANGE_POKEMON_HP:
                {
                    if (_packet is S_ProcessTurn)
                    {
                        S_ProcessTurn processTurnPacket = _packet as S_ProcessTurn;
                        PokemonSummary defensePokemonSum = processTurnPacket.DefensePokemonSum;
                        float typeEffectiveness = processTurnPacket.TypeEffectiveness;
                        bool isCriticalHit = processTurnPacket.IsCriticalHit;

                        List<string> scripts = null;
                        scripts = CreateEffectivenessScripts(isCriticalHit, typeEffectiveness);

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
                break;
            case BattleSceneState.EFFECTIVENESS_SCRIPTING:
                {
                    StartCoroutine(ActionAfterChangeHP());
                }
                break;
            case BattleSceneState.POKEMON_DIE:
                {
                    if (_packet is S_ProcessTurn)
                    {
                        S_ProcessTurn processTurnPacket = _packet as S_ProcessTurn;
                        PokemonSummary defensePokemonSum = processTurnPacket.DefensePokemonSum;

                        RefillScriptBox(new string[] { $"{defensePokemonSum.PokemonInfo.NickName} fell down!" });
                        SceneState = BattleSceneState.MY_POKEMON_DIE_SCRIPTING;
                    }
                }
                break;
            case BattleSceneState.MY_POKEMON_DIE_SCRIPTING:
                {
                    if (_packet is S_ProcessTurn)
                    {
                        S_ProcessTurn processTurnPacket = _packet as S_ProcessTurn;
                        PokemonSummary defensePokemonSum = processTurnPacket.DefensePokemonSum;
                        bool isMyPokemon = processTurnPacket.IsMyPokemon;

                        if (isMyPokemon)
                        {
                            if (_myPokemon.PokemonInfo.Level != 100)
                            {
                                SendRequestDataPacket(RequestType.GetEnemyPokemonExp);
                            }
                        }
                        else
                        {
                            SendRequestDataPacket(RequestType.CheckAvailableBattlePokemon);
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
                        PokemonSummary expPokemonSum = expPacket.ExpPokemonSum;
                        LevelUpStatusDiff statDiff = expPacket.StatDiff;

                        _myPokemon.PokemonExpInfo = expPokemonSum.PokemonExpInfo;

                        if (statDiff != null)
                        {
                            _myPokemon.PokemonInfo.Level = expPokemonSum.PokemonInfo.Level;
                            _myPokemon.PokemonStat = expPokemonSum.PokemonStat;

                            _myPokemonArea.FillPokemonInfo(_myPokemon, true);
                            _statusBox.SetStatusDiffRate(statDiff);

                            RefillScriptBox(new string[] { $"{_myPokemon.PokemonInfo.NickName}'s level went up to {_myPokemon.PokemonInfo.Level}." });
                            SceneState = BattleSceneState.LEVEL_UP_SCRIPTING;
                        }
                        else
                        {
                            SendRequestDataPacket(RequestType.CheckExpPokemon);
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
                    if (_packet is S_CheckAndApplyRemainedExp)
                    {
                        PokemonSummary expPokemonSum = (_packet as S_CheckAndApplyRemainedExp).ExpPokemonSum;
                        bool isMainPokemon = (_packet as S_CheckAndApplyRemainedExp).IsMainPokemon;

                        if (isMainPokemon)
                        {
                            _statusBox.ShowFinalStat(_myPokemon.PokemonStat);
                            SceneState = BattleSceneState.SHOWING_UPGRADED_STATUS;
                        }
                        else
                        {
                            _statusBox.ShowFinalStat(expPokemonSum.PokemonStat);
                            SceneState = BattleSceneState.SHOWING_UPGRADED_STATUS;
                        }
                    }
                }
                break;
            case BattleSceneState.SHOWING_UPGRADED_STATUS:
                {
                    if (_packet is S_CheckAndApplyRemainedExp)
                    {
                        S_CheckAndApplyRemainedExp expPacket = _packet as S_CheckAndApplyRemainedExp;
                        PokemonSummary expPokemonSum = expPacket.ExpPokemonSum;
                        PokemonMoveSummary newMoveSum = expPacket.NewMoveSum;
                        bool isMainPokemon = expPacket.IsMainPokemon;

                        if (newMoveSum != null)
                        {
                            if (expPokemonSum.PokemonMoves.Count < 4)
                            {
                                if (isMainPokemon)
                                {
                                    PokemonMove move = new PokemonMove(newMoveSum);
                                    _myPokemon.PokemonMoves.Add(move);
                                }

                                RefillScriptBox(new string[] { $"{expPokemonSum.PokemonInfo.NickName} learned {newMoveSum.MoveName}!" });

                                SceneState = BattleSceneState.NEW_MOVE_LEARN_SCRIPTING;
                            }
                            else
                            {
                                RefillScriptBox(new string[] { 
                                    $"{expPokemonSum.PokemonInfo.NickName} wants to learn the move {newMoveSum.MoveName}.", 
                                    $"However, {expPokemonSum.PokemonInfo.NickName} already knows four moves.", 
                                    $"Sould a move be deleted and replaced with {newMoveSum.MoveName}?" 
                                });

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
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                Debug.Log("Yes!");
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                if (_packet is S_CheckAndApplyRemainedExp)
                                {
                                    S_CheckAndApplyRemainedExp expPacket = _packet as S_CheckAndApplyRemainedExp;
                                    PokemonSummary expPokemonSum = expPacket.ExpPokemonSum;
                                    PokemonMoveSummary newMoveSum = expPacket.NewMoveSum;

                                    RefillScriptBox(new string[] { 
                                        $"{expPokemonSum.PokemonInfo.NickName} did not learn the move {newMoveSum.MoveName}." 
                                    });
                                    SceneState = BattleSceneState.NEW_MOVE_NOT_LEARN_SCRIPTING;
                                }
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
                            GridLayoutSelectBox selectBox = _scriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                C_EnterPokemonListScene enterPokemonPacket = new C_EnterPokemonListScene();
                                enterPokemonPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(enterPokemonPacket);

                                SceneState = BattleSceneState.MOVING_SCENE;
                                _actionSelectBox.gameObject.SetActive(false);
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
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
            case BattleSceneState.AFTER_DIE_SWITCH_POKEMON:
                {
                    RefillScriptBox(new string[] { $"Go! {_myPokemon.PokemonInfo.NickName}!" });
                    SceneState = BattleSceneState.MY_POKEMON_APPEAR_SCRIPTING;
                }
                break;
            case BattleSceneState.GO_NEXT_POKEMON_SCRIPTING:
                {
                    SceneState = BattleSceneState.SHOWING_POKEMON;
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
                    RefillScriptBox(new string[] { $"Go! {_myPokemon.PokemonInfo.NickName}!" });
                    SceneState = BattleSceneState.MY_POKEMON_APPEAR_SCRIPTING;
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

                            SceneState = BattleSceneState.MOVING_SCENE;
                        }
                        else
                        {
                            if (_myPokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                            {
                                C_EnterPokemonListScene enterPokemonPacket = new C_EnterPokemonListScene();
                                enterPokemonPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;

                                Managers.Network.SavePacket(enterPokemonPacket);

                                SceneState = BattleSceneState.MOVING_SCENE;
                                _actionSelectBox.gameObject.SetActive(false);
                            }
                            else
                            {
                                SendProcessTurnPacket(_moveSelectBox.GetSelectedIdx());
                            }
                        }
                    }
                }
                break;
            case BattleSceneState.MOVING_SCENE:
                {
                    if (Managers.Network.Packet is C_EnterPlayerBagScene)
                        Managers.Scene.LoadScene(Define.Scene.Bag);
                    else if (Managers.Network.Packet is C_EnterPokemonListScene)
                        Managers.Scene.LoadScene(Define.Scene.PokemonList);
                    else if (Managers.Network.Packet is C_ReturnGame)
                        Managers.Scene.LoadScene(Define.Scene.Game);
                    else if (Managers.Network.Packet is C_EnterPokemonEvolutionScene)
                        Managers.Scene.LoadScene(Define.Scene.Evolution);
                }
                break;
        }
    }

    List<string> CreateEffectivenessScripts(bool isCriticalHit, float typeEffectiveness)
    {
        List<string> scripts = new List<string>();

        if (isCriticalHit)
            scripts.Add("A critical hit!");

        if (typeEffectiveness < 1f)
        {
            scripts.Add("It's not very effective...");
        }
        else if (typeEffectiveness > 1f)
        {
            scripts.Add("It's super effective!");
        }

        return scripts;
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

    void SendProcessTurnPacket(int moveOrder)
    {
        if (!_loadingPacket)
        {
            C_ProcessTurn processTurnPacket = new C_ProcessTurn();
            processTurnPacket.PlayerId = _playerInfo.ObjectInfo.ObjectId;
            processTurnPacket.MoveOrder = moveOrder;

            Managers.Network.Send(processTurnPacket);

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

    IEnumerator ActionAfterChangeHP()
    {
        yield return new WaitForSeconds(1f);

        if (_packet is S_ProcessTurn)
        {
            S_ProcessTurn processTurnPacket = _packet as S_ProcessTurn;
            PokemonSummary defensePokemonSum = processTurnPacket.DefensePokemonSum;
            bool isMyPokemon = processTurnPacket.IsMyPokemon;
            bool isTurnFinish = processTurnPacket.IsTurnFinish;

            BattleArea defensePokemonArea = isMyPokemon ? _enemyPokemonArea : _myPokemonArea;

            if (defensePokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
            {
                defensePokemonArea.PlayPokemonZoneAnim("Zone_DownDisappear");

                SceneState = BattleSceneState.POKEMON_DIE;
            }
            else
            {
                if (isTurnFinish)
                {
                    SceneState = BattleSceneState.SELECTING_ACTION;
                }
                else
                {
                    if (!isMyPokemon)
                    {
                        // 내 포켓몬에 사용 가능한 기술이 있는 지 확인한다.
                        List<PokemonMove> myPKMAvailableMoves = FindAvailableMove(_myPokemon);

                        if (myPKMAvailableMoves.Count > 0)
                        {
                            SendProcessTurnPacket(_moveSelectBox.GetSelectedIdx());
                        }
                        else
                        {
                            SendProcessTurnPacket(-1);
                        }
                    }
                    else
                    {
                        SendProcessTurnPacket(_moveSelectBox.GetSelectedIdx());
                    }
                }
            }
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
