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
    WILD_POKEMON_APPEAR_SCRIPTING = 200,
    OPPONENT_TRAINER_DISAPPEARING = 201,
    OPPONENT_POKEMON_APPEAR_SCRIPTING = 202,
    OPPONENT_POKEMON_APPEARING = 203,
    OPPONENT_POKEMON_CARD_APPEARING = 204,
    TRAINER_DISAPPEARING = 210,
    MY_POKEMON_APPEAR_SCRIPTING = 220,
    SHOWING_POKEMON = 240,
    SHOWING_UI = 5,
    ATTACK_INSTRUCTING = 9,
    ATTACK_ANIMATION = 10,
    ATTACK_MISSED_SCRIPTING = 11,
    POKEMON_BLINK_ANIMATION = 12,
    CHANGE_POKEMON_HP = 13,
    ASKING_TO_SWITCH_POKEMON = 56,
    EFFECTIVENESS_SCRIPTING = 58,
    POKEMON_DIE = 14,
    POKEMON_CARD_DISAPPEAR_ANIMATION = 141,
    MY_POKEMON_DIE_SCRIPTING = 15,
    GOT_EXP_SCRIPTING = 16,
    GETTING_EXP = 17,
    LEVEL_UP_SCRIPTING = 18,
    SHOWING_LEVEL_UP_STATUS_BOX = 19,
    AFTER_MOVE_SELECTION_SCRIPTING = 21,
    ASKING_TO_LEARN_NEW_MOVE = 22,
    ANSWERING_TO_LEARN_NEW_MOVE = 23,
    ANSWERING_TO_SWITCH_POKEMON = 66,
    NEW_MOVE_NOT_LEARN_SCRIPTING = 24,
    COME_BACK_POKEMON_SCRIPTING = 25,
    COMING_BACK_POKEMON = 26,
    COME_BACK_POKEMON_CARD = 27,
    AFTER_SWITCH_POKEMON_SCRIPTING = 47,
    AFTER_DIE_SWITCH_POKEMON = 76,
    GO_NEXT_POKEMON_SCRIPTING = 77,
    ITEM_USE_SCRIPTING = 87,
    BALL_THROW_ANIMATION = 88,
    POKEMON_SUCKED_ANIMATION = 89,
    POKE_BALL_ANIMATION = 90,
    POKEMON_COME_OUT_ANIMATION = 91,
    CATCH_SCRIPTING = 92,
    TRAINER_DEATED_SCRIPTING = 93,
    OPPONENT_TRAINER_APPEARING = 94,
    AFTER_BATTLE_NPC_SCRIPTING = 95,
    GOT_REWARD_SCRIPTING = 96,
    ESCAPE_SCRIPTING = 100,
    OnlineBattleSurrenderScripting = 997,
    Inactiving = 999
}

public class BattleScene : BaseScene
{
    BattleSceneState _sceneState = BattleSceneState.NONE;
    TextMeshProUGUI[] _moveInfoTMPs;
    PlayableDirector _playableDirector;
    NPCInfo _npcInfo;

    List<string> _script = new List<string>();
    List<Pokemon> _myPokemons = new List<Pokemon>();
    Pokemon _myPokemon;
    Pokemon _enemyPokemon;
    Item _usedItem;

    Pokemon _attackPokemon;
    Pokemon _defensePokemon;
    BattleArea _attackPokemonArea;
    BattleArea _defensePokemonArea;

    Pokemon _expPokemon;
    PokemonMove _newMove;

    [SerializeField] ActionSelectContent _actionSelectContent;
    [SerializeField] OnlineBattleContent _onlineBattleContent;

    [SerializeField] GameObject _moveInfoBox;
    [SerializeField] BattleArea _enemyPokemonArea;
    [SerializeField] BattleArea _myPokemonArea;
    [SerializeField] StatusBox _statusBox;

    public List<Pokemon> Pokemons {  get { return _myPokemons; } }

    public BattleSceneState SceneState
    {
        set
        {
            _sceneState = value;

            if (_sceneState == BattleSceneState.NONE)
            {
                _statusBox.gameObject.SetActive(false);
            }

            if (_sceneState == BattleSceneState.SHOWING_POKEMON)
            {
                _myPokemonArea.FillPokemonInfo(_myPokemon, true);
                _myPokemonArea.PlayPokemonZoneAnim("Zone_RightAppear");
            }

            // 액션 선택 박스
            if (_sceneState == BattleSceneState.SHOWING_LEVEL_UP_STATUS_BOX)
            {
                _statusBox.gameObject.SetActive(true);
                _statusBox.State = StatusBoxState.SHOWING_RATE;
            }
            else
            {
                _statusBox.gameObject.SetActive(false);
                _statusBox.State = StatusBoxState.NONE;
            }

            // 예 아니오 버튼
            if (_sceneState == BattleSceneState.ANSWERING_TO_LEARN_NEW_MOVE || _sceneState == BattleSceneState.ANSWERING_TO_SWITCH_POKEMON)
            {
                List<string> btnNames = new List<string>()
                {
                    "Yes",
                    "No",
                };
                ContentManager.Instance.ScriptBox.CreateSelectBox(btnNames, 1, 400, 100);
            }
        }
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Battle;
    }

    protected override void Start()
    {
        base.Start();

        _playableDirector = GetComponent<PlayableDirector>();
        _moveInfoTMPs = _moveInfoBox.GetComponentsInChildren<TextMeshProUGUI>();
    }

    public override void UpdateData(IMessage packet)
    {
        _loadingPacket = false;
        _packet = packet;

        // 어떤 상황에서도 우선적으로 발생해야 하는 콘텐츠
        if (_packet is S_SurrenderTrainerBattle)
        {
            int surrenderPlayerId = ((S_SurrenderTrainerBattle)_packet).SurrenderPlayerId;

            ObjectContents content = _contentStack.Peek();
            content.SetIsActionStop(true);
            content.InactiveContent();

            List<string> scripts;

            if (surrenderPlayerId == Managers.Object.MyPlayerController.Id)
                scripts = new List<string>() { $"You surrendered the battle." };
            else
                scripts = new List<string>() { $"The other side surrendered in battle." };

            ContentManager.Instance.BeginScriptTyping(scripts, false, 1, true);

            SceneState = BattleSceneState.OnlineBattleSurrenderScripting;
        }

        if (_contentStack.Count == 0)
        {
            if (packet is S_EnterPokemonBattleScene)
            {
                ContentManager.Instance.FadeInScreenEffect();

                Managers.Object.MyPlayerController.State = CreatureState.Fight;

                _playableDirector.Play();

                S_EnterPokemonBattleScene enterBattleScenePacket = packet as S_EnterPokemonBattleScene;
                int battlePokemonOrder = enterBattleScenePacket.MyFirstBattlePokemonOrder;
                PokemonSummary enemyPokemonSum = enterBattleScenePacket.EnemyPokemonSum;
                NPCInfo npcInfo = enterBattleScenePacket.NpcInfo;

                if (npcInfo != null)
                {
                    _npcInfo = npcInfo;
                    _enemyPokemonArea.FillOpponentTrainerImage(_npcInfo.NpcType);
                    _enemyPokemonArea.PlayInfoZoneAnim("Zone_LeftHide");
                    _enemyPokemonArea.PlayPokemonZoneAnim("Zone_RightHide");
                }
                else
                {
                    _enemyPokemonArea.PlayPokemonZoneAnim("Zone_Default");
                    _enemyPokemonArea.PlayTrainerZoneAnim("Zone_RightHide");
                }

                // 포켓몬 및 플레이어 데이터 채우기
                _enemyPokemon = new Pokemon(enemyPokemonSum);

                foreach (Pokemon pokemon in Managers.Object.MyPlayerController.MyPokemons)
                {
                    _myPokemons.Add(pokemon);
                }

                // 선두 포켓몬이 기절일 경우를 대비
                Pokemon temp = _myPokemons[0];
                _myPokemons[0] = _myPokemons[battlePokemonOrder];
                _myPokemons[battlePokemonOrder] = temp;

                _myPokemon = _myPokemons[0];

                // 포켓몬 랜더링
                _myPokemonArea.FillTrainerImage(Managers.Object.MyPlayerController.PlayerGender, true);
                _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftHide");
                _myPokemonArea.PlayInfoZoneAnim("Zone_RightHide");

                _enemyPokemonArea.FillPokemonInfo(_enemyPokemon, false);

                // 기술 버튼 데이터 채우기
                _actionSelectContent.CreateButton(_myPokemon);
            }
            else if (packet is S_EnterTrainerBattle)
            {
                int firstBattlePokemonOrder = ((S_EnterTrainerBattle)_packet).FirstBattlePokemonOrder;

                Managers.Object.MyPlayerController.State = CreatureState.Fight;

                foreach (Pokemon pokemon in Managers.Object.MyPlayerController.MyPokemons)
                {
                    _myPokemons.Add(pokemon);
                }

                // 선두 포켓몬이 기절일 경우를 대비
                Pokemon temp = _myPokemons[0];
                _myPokemons[0] = _myPokemons[firstBattlePokemonOrder];
                _myPokemons[firstBattlePokemonOrder] = temp;

                _myPokemon = _myPokemons[0];

                _contentStack.Push(_onlineBattleContent);
                _contentStack.Peek().UpdateData(packet);
            }
            else if (packet is S_ProcessTurn)
            {
                S_ProcessTurn processTurnPacket = packet as S_ProcessTurn;
                PokemonMoveSummary usedMoveSum = processTurnPacket.UsedMoveSum;
                PokemonSummary defensePokemonSum = processTurnPacket.DefensePokemonSum;
                bool isMyPokemon = processTurnPacket.IsMyPokemon;
                int moveOrder = processTurnPacket.UsedMoveOrder;

                if (isMyPokemon)
                {
                    _attackPokemon = _myPokemon;
                    _attackPokemonArea = _myPokemonArea;

                    _defensePokemon = _enemyPokemon;
                    _defensePokemonArea = _enemyPokemonArea;
                }
                else
                {
                    _attackPokemon = _enemyPokemon;
                    _attackPokemonArea = _enemyPokemonArea;

                    _defensePokemon = _myPokemon;
                    _defensePokemonArea = _myPokemonArea;
                }

                if (moveOrder != -1)
                    _attackPokemon.PokemonMoves[moveOrder].UpdatePokemonMoveSummary(usedMoveSum);

                _defensePokemon.UpdatePokemonSummary(defensePokemonSum);

                RefillScriptBox(new string[] { $"{_attackPokemon.PokemonInfo.NickName} used {usedMoveSum.MoveName}!" }, true);
                SceneState = BattleSceneState.ATTACK_INSTRUCTING;
            }
            else if (packet is S_CheckAvailableBattlePokemon)
            {
                S_CheckAvailableBattlePokemon checkBattlePokemonPacket = packet as S_CheckAvailableBattlePokemon;
                bool canFight = checkBattlePokemonPacket.CanFight;

                if (canFight)
                {
                    if (Managers.Object.MyPlayerController.NPC != null)
                    {
                        List<string> actionBtnNames = new List<string>()
                        {
                            "Send Out",
                            "Summary",
                            "Cancel"
                        };
                        GameContentManager.Instance.OpenPokemonList(_myPokemons, actionBtnNames, "FadeOut");
                    }
                    else
                    {
                        RefillScriptBox(new string[] { $"Do you want to change to another Pokemon?" });
                        SceneState = BattleSceneState.ASKING_TO_SWITCH_POKEMON;
                    }
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
                string expPokemonName = getExpPacket.ExpPokemonName;
                int exp = getExpPacket.Exp;

                RefillScriptBox(new string[] { $"{expPokemonName} got {exp} exp!" });
                SceneState = BattleSceneState.GOT_EXP_SCRIPTING;
                return;
            }
            else if (packet is S_CheckAndApplyRemainedExp)
            {
                S_CheckAndApplyRemainedExp expPacket = packet as S_CheckAndApplyRemainedExp;
                PokemonSummary expPokemonSum = expPacket.ExpPokemonSum;
                PokemonMoveSummary newMoveSum = expPacket.NewMoveSum;
                LevelUpStatusDiff statDiff = expPacket.StatDiff;
                int finalExp = expPacket.FinalExp;
                int expPokemonOrder = expPacket.ExpPokemonOrder;
                bool isExpFinish = expPacket.IsExpFinish;

                _expPokemon = _myPokemons[expPokemonOrder];

                if (newMoveSum != null)
                    _newMove = new PokemonMove(newMoveSum);

                if (expPokemonOrder == 0)
                {
                    _myPokemonArea.ChangePokemonEXP(_expPokemon.PokemonExpInfo.CurExp + finalExp);

                    _expPokemon.UpdatePokemonSummary(expPokemonSum);

                    SceneState = BattleSceneState.GETTING_EXP;
                }
                else
                {
                    _expPokemon.UpdatePokemonSummary(expPokemonSum);

                    if (statDiff != null)
                    {
                        _statusBox.SetLevelUpStatusBox(statDiff, _expPokemon.PokemonStat);
                        RefillScriptBox(new string[] { $"{_expPokemon.PokemonInfo.NickName}'s level went up to {_expPokemon.PokemonInfo.Level}." });
                        SceneState = BattleSceneState.LEVEL_UP_SCRIPTING;
                    }
                    else
                    {
                        _expPokemon = null;

                        if (isExpFinish)
                        {
                            if (_npcInfo != null)
                            {
                                SendRequestDataPacket(RequestType.SendOpponentNextPokemon);
                            }
                            else
                            {
                                SendRequestDataPacket(RequestType.CheckPokemonEvolution);
                            }
                        }
                        else
                        {
                            SendRequestDataPacket(RequestType.GetEnemyPokemonExp);
                        }
                    }
                }

                return;
            }
            else if (packet is S_ForgetAndLearnNewMove)
            {
                S_ForgetAndLearnNewMove learnNewMovePacket = packet as S_ForgetAndLearnNewMove;
                string prevMoveName = learnNewMovePacket.PrevMoveName;

                if (prevMoveName == "")
                {
                    RefillScriptBox(new string[] {
                        $"{_expPokemon.PokemonInfo.NickName} did not learn the move {_newMove.MoveName}."
                    });
                }
                else
                {
                    RefillScriptBox(new string[] {
                        $"1, 2, and... ... ... Poof!",
                        $"{_expPokemon.PokemonInfo.NickName} forgot how to use {prevMoveName}.",
                        "And...",
                        $"{_expPokemon.PokemonInfo.NickName} learned {_newMove.MoveName}!",
                    });
                }

                _actionSelectContent.CreateButton(_myPokemon);

                _newMove = null;

                _sceneState = BattleSceneState.AFTER_MOVE_SELECTION_SCRIPTING;
            }
            else if (packet is S_SwitchBattlePokemon)
            {
                ContentManager.Instance.FadeInScreenEffect();

                S_SwitchBattlePokemon switchPokemonPacket = packet as S_SwitchBattlePokemon;

                PokemonSummary prevPokemonSum = switchPokemonPacket.PrevPokemonSum;

                _myPokemon = _myPokemons[0];

                // 기술 버튼 데이터 채우기
                _actionSelectContent.CreateButton(_myPokemon);

                if (prevPokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                {
                    // 포켓몬 랜더링
                    _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftHide");
                    _myPokemonArea.PlayInfoZoneAnim("Zone_RightHide");

                    RefillScriptBox(new string[] { $"Go! {_myPokemon.PokemonInfo.NickName}!" });
                    SceneState = BattleSceneState.AFTER_SWITCH_POKEMON_SCRIPTING;
                }
                else
                {
                    RefillScriptBox(new string[] { $"That is enough! Come back {prevPokemonSum.PokemonInfo.NickName}!" });
                    SceneState = BattleSceneState.AFTER_SWITCH_POKEMON_SCRIPTING;
                }
            }
            else if (packet is S_SendOpponentNextPokemon)
            {
                PokemonSummary pokemonSum = ((S_SendOpponentNextPokemon)packet).OpponentPokemonSum;

                if (pokemonSum != null)
                {
                    _enemyPokemonArea.PlayPokemonZoneAnim("Zone_RightHide");
                    _enemyPokemon = new Pokemon(pokemonSum);
                    _enemyPokemonArea.FillPokemonInfo(_enemyPokemon, false);

                    RefillScriptBox(new string[] { $"{_npcInfo.NpcName} sent out {pokemonSum.PokemonInfo.NickName}!" });
                    SceneState = BattleSceneState.OPPONENT_POKEMON_APPEAR_SCRIPTING;
                }
                else
                {
                    if (!_loadingPacket)
                    {
                        _loadingPacket = true;

                        C_GetRewardInfo getRewardPacket = new C_GetRewardInfo();
                        getRewardPacket.PlayerId = Managers.Object.MyPlayerController.Id;

                        Managers.Network.Send(getRewardPacket);
                    }
                }
            }
            else if (packet is S_GetRewardInfo)
            {
                S_GetRewardInfo getRewardPacket = packet as S_GetRewardInfo;
                int reward = getRewardPacket.Money;
                IList scripts = getRewardPacket.AfterBattleScripts;

                RefillScriptBox(new string[] { $"Player defeated {_npcInfo.NpcType} {_npcInfo.NpcName}!" });
                SceneState = BattleSceneState.TRAINER_DEATED_SCRIPTING;
            }
            else if (packet is S_CheckPokemonEvolution)
            {
                S_CheckPokemonEvolution checkEvolutionPacket = packet as S_CheckPokemonEvolution;
                int evolvePokemonIdx = checkEvolutionPacket.EvolvePokemonIdx;
                string evolutionPokemonName = checkEvolutionPacket.EvolutionPokemonName;

                if (evolvePokemonIdx != -1)
                {
                    GameContentManager.Instance.OpenPokemonEvolution(Managers.Object.MyPlayerController.MyPokemons[evolvePokemonIdx], evolutionPokemonName, "FadeOut");
                }
                else
                {
                    C_ReturnGame returnGamePacket = new C_ReturnGame();
                    returnGamePacket.PlayerId = Managers.Object.MyPlayerController.Id;

                    ContentManager.Instance.FadeOutCurSceneToUnload(Define.Scene.Battle, "FadeOut", returnGamePacket);
                }
            }
            else if (packet is S_UseItemInListScene)
            {
                ItemSummary itemSumamary = ((S_UseItemInListScene)_packet).ItemSum;

                RefillScriptBox(new string[] { $"{Managers.Object.MyPlayerController.PlayerName} used {itemSumamary.ItemName}!" }, true);
                _sceneState = BattleSceneState.ITEM_USE_SCRIPTING;
            }
            if (packet is S_EscapeFromWildPokemon)
            {
                S_EscapeFromWildPokemon escapePacket = packet as S_EscapeFromWildPokemon;
                bool canEscape = escapePacket.CanEscape;

                if (canEscape)
                {
                    List<string> scripts = new List<string>()
                    {
                        $"Got away safely!"
                    };
                    ContentManager.Instance.BeginScriptTyping(scripts);
                }
                else
                {
                    List<string> scripts = new List<string>()
                    {
                        $"Can't escape!"
                    };
                    ContentManager.Instance.BeginScriptTyping(scripts);
                }

                SceneState = BattleSceneState.ESCAPE_SCRIPTING;
            }
        }
        else
        {
            _contentStack.Peek().UpdateData(packet);
            return;
        }
    }

    public void UpdateMyPokemonInfo(Pokemon pokemon)
    {
        _myPokemonArea.FillPokemonInfo(pokemon, true);
    }

    public override void DoNextAction(object value = null)
    {
        if (_contentStack.Count > 0)
        {
            _contentStack.Peek().SetNextAction(value);
            return;
        }

        switch (_sceneState)
        {
            case BattleSceneState.AFTER_SWITCH_POKEMON_SCRIPTING:
                {
                    if (_packet is S_SwitchBattlePokemon)
                    {
                        S_SwitchBattlePokemon switchPokemonPacket = _packet as S_SwitchBattlePokemon;
                        PokemonSummary prevPokemonSum = switchPokemonPacket.PrevPokemonSum;

                        if (prevPokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                        {
                            _myPokemonArea.PlayPokemonZoneAnim("Zone_RightAppear");
                            SceneState = BattleSceneState.SHOWING_POKEMON;
                        }
                        else
                        {
                            _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftDisappear");
                            SceneState = BattleSceneState.COMING_BACK_POKEMON;
                        }
                    }
                }
                break;
            case BattleSceneState.NONE:
                {
                    if (_npcInfo != null)
                    {
                        RefillScriptBox(new string[] { $"{_npcInfo.NpcType} {_npcInfo.NpcName} would like to battle!" });
                        SceneState = BattleSceneState.WILD_POKEMON_APPEAR_SCRIPTING;
                    }
                    else
                    {
                        RefillScriptBox(new string[] { $"Wild {_enemyPokemon.PokemonInfo.PokemonName} appeard!" });
                        SceneState = BattleSceneState.WILD_POKEMON_APPEAR_SCRIPTING;
                    }
                }
                break;
            case BattleSceneState.WILD_POKEMON_APPEAR_SCRIPTING:
                {
                    if (_npcInfo != null)
                    {
                        _enemyPokemonArea.PlayTrainerZoneAnim("Zone_RightDisappear");
                        SceneState = BattleSceneState.OPPONENT_TRAINER_DISAPPEARING;
                    }
                    else
                    {
                        _myPokemonArea.PlayTrainerZoneAnim("Zone_LeftDisappear");
                        SceneState = BattleSceneState.TRAINER_DISAPPEARING;
                    }
                }
                break;
            case BattleSceneState.OPPONENT_TRAINER_DISAPPEARING:
                {
                    RefillScriptBox(new string[] { $"{_npcInfo.NpcName} sent out {_enemyPokemon.PokemonInfo.NickName}!" });
                    SceneState = BattleSceneState.OPPONENT_POKEMON_APPEAR_SCRIPTING;
                }
                break;
            case BattleSceneState.OPPONENT_POKEMON_APPEAR_SCRIPTING:
                {
                    _enemyPokemonArea.PlayPokemonZoneAnim("Zone_LeftAppear");
                    SceneState = BattleSceneState.OPPONENT_POKEMON_APPEARING;    
                }
                break;
            case BattleSceneState.OPPONENT_POKEMON_APPEARING:
                {
                    _enemyPokemonArea.PlayInfoZoneAnim("Zone_RightAppear");
                    SceneState = BattleSceneState.OPPONENT_POKEMON_CARD_APPEARING;
                }
                break;
            case BattleSceneState.OPPONENT_POKEMON_CARD_APPEARING:
                {
                    if (_packet is S_SendOpponentNextPokemon)
                    {
                        ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                        _contentStack.Push(_actionSelectContent);
                        _contentStack.Peek().SetNextAction();
                    }
                    else
                    {
                        _myPokemonArea.PlayTrainerZoneAnim("Zone_LeftDisappear");
                        SceneState = BattleSceneState.TRAINER_DISAPPEARING;
                    }
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
                            ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                            _contentStack.Push(_actionSelectContent);
                            _contentStack.Peek().SetNextAction();
                        }
                        else
                        {
                            SendProcessTurnPacket(0);
                        }
                    }
                    else
                    {
                        ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                        _contentStack.Push(_actionSelectContent);
                        _contentStack.Peek().SetNextAction();
                    }
                }
                break;
            case BattleSceneState.ATTACK_INSTRUCTING:
                {
                    if (_packet is S_ProcessTurn)
                    {
                        S_ProcessTurn processTurn = _packet as S_ProcessTurn;
                        int usedMoveOrder = processTurn.UsedMoveOrder;
                        bool isHit = processTurn.IsHit;
                        float typeEffectiveness = processTurn.TypeEffectiveness;

                        if (isHit)
                        {
                            if (typeEffectiveness == 0f)
                            {
                                RefillScriptBox(new string[] { $"It doesn't affect {_defensePokemon.PokemonInfo.NickName}..." }, true);
                                SceneState = BattleSceneState.EFFECTIVENESS_SCRIPTING;
                            }
                            else
                            {
                                SceneState = BattleSceneState.ATTACK_ANIMATION;
                                PokemonMove usedMove = usedMoveOrder != -1 ? _attackPokemon.PokemonMoves[usedMoveOrder] : _attackPokemon.NoPPMove;

                                if (_attackPokemonArea == _myPokemonArea)
                                    _attackPokemonArea.PlayBattlePokemonAnim("BattlePokemon_RightAttack");
                                else
                                    _attackPokemonArea.PlayBattlePokemonAnim("BattlePokemon_LeftAttack");

                                _defensePokemonArea.TriggerPokemonHitImage(usedMove);
                            }
                        }
                        else
                        {
                            RefillScriptBox(new string[] { $"{_attackPokemon.PokemonInfo.NickName}'s attack is off the mark!" }, true);
                            SceneState = BattleSceneState.ATTACK_MISSED_SCRIPTING;
                        }
                    }
                }
                break;
            case BattleSceneState.ATTACK_MISSED_SCRIPTING:
                {
                    if (_packet is S_ProcessTurn)
                    {
                        S_ProcessTurn processTurnPacket = _packet as S_ProcessTurn;
                        bool isTurnFinish = processTurnPacket.IsTurnFinish;

                        if (isTurnFinish)
                        {
                            ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                            _contentStack.Push(_actionSelectContent);
                            _contentStack.Peek().SetNextAction();
                        }
                        else
                        {
                            SendProcessTurnPacket(_actionSelectContent.GetSelectMoveOrder());
                        }
                    }
                }
                break;
            case BattleSceneState.ATTACK_ANIMATION:
                {
                    SceneState = BattleSceneState.POKEMON_BLINK_ANIMATION;
                    _defensePokemonArea.PlayBattlePokemonAnim("BattlePokemon_Hit");
                }
                break;
            case BattleSceneState.POKEMON_BLINK_ANIMATION:
                {
                    _defensePokemonArea.ChangePokemonHP(_defensePokemon.PokemonStat.Hp);
                    SceneState = BattleSceneState.CHANGE_POKEMON_HP;
                }
                break;
            case BattleSceneState.CHANGE_POKEMON_HP:
                {
                    if (_packet is S_ProcessTurn)
                    {
                        S_ProcessTurn processTurnPacket = _packet as S_ProcessTurn;
                        float typeEffectiveness = processTurnPacket.TypeEffectiveness;
                        bool isCriticalHit = processTurnPacket.IsCriticalHit;

                        List<string> scripts = null;
                        scripts = CreateEffectivenessScripts(isCriticalHit, typeEffectiveness);

                        if (scripts.Count > 0)
                        {
                            ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);
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
                        bool isMyPokemon = processTurnPacket.IsMyPokemon;
                        bool isTurnFinish = processTurnPacket.IsTurnFinish;

                        if (isMyPokemon)
                            _enemyPokemonArea.PlayInfoZoneAnim("Zone_LeftDisappear");
                        else
                            _myPokemonArea.PlayInfoZoneAnim("Zone_RightDisappear");

                        SceneState = BattleSceneState.POKEMON_CARD_DISAPPEAR_ANIMATION;
                    }
                }
                break;
            case BattleSceneState.POKEMON_CARD_DISAPPEAR_ANIMATION:
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
                        LevelUpStatusDiff statDiff = expPacket.StatDiff;
                        bool isExpFinish = expPacket.IsExpFinish;

                        if (statDiff != null)
                        {
                            _statusBox.SetLevelUpStatusBox(statDiff, _expPokemon.PokemonStat);
                            _myPokemonArea.FillPokemonInfo(_myPokemon, true);

                            RefillScriptBox(new string[] { $"{_myPokemon.PokemonInfo.NickName}'s level went up to {_myPokemon.PokemonInfo.Level}." });
                            SceneState = BattleSceneState.LEVEL_UP_SCRIPTING;
                        }
                        else
                        {
                            _expPokemon = null;

                            if (isExpFinish)
                            {
                                if (_npcInfo != null)
                                {
                                    SendRequestDataPacket(RequestType.SendOpponentNextPokemon);
                                }
                                else
                                {
                                    SendRequestDataPacket(RequestType.CheckPokemonEvolution);
                                }
                            }
                            else
                            {
                                SendRequestDataPacket(RequestType.GetEnemyPokemonExp);
                            }
                        }
                    }
                }
                break;
            case BattleSceneState.LEVEL_UP_SCRIPTING:
                {
                    SceneState = BattleSceneState.SHOWING_LEVEL_UP_STATUS_BOX;
                }
                break;
            case BattleSceneState.SHOWING_LEVEL_UP_STATUS_BOX:
                {
                    if (_newMove != null)
                    {
                        if (_expPokemon.PokemonMoves.Count < 4)
                        {
                            _expPokemon.PokemonMoves.Add(_newMove);
                            _actionSelectContent.CreateMoveButtons();

                            RefillScriptBox(new string[] { $"{_expPokemon.PokemonInfo.NickName} learned {_newMove.MoveName}!" });

                            _newMove = null;

                            SceneState = BattleSceneState.AFTER_MOVE_SELECTION_SCRIPTING;
                        }
                        else
                        {
                            RefillScriptBox(new string[] {
                                    $"{_expPokemon.PokemonInfo.NickName} wants to learn the move {_newMove.MoveName}.",
                                    $"However, {_expPokemon.PokemonInfo.NickName} already knows four moves.",
                                    $"Sould a move be deleted and replaced with {_newMove.MoveName}?"
                                });

                            SceneState = BattleSceneState.ASKING_TO_LEARN_NEW_MOVE;
                        }
                    }
                    else
                    {
                        SendRequestDataPacket(RequestType.CheckAndApplyRemainedExp);
                    }
                }
                break;
            case BattleSceneState.AFTER_MOVE_SELECTION_SCRIPTING:
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
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                GameContentManager.Instance.OpenMoveSelection(_expPokemon, _newMove, "FadeOut");
                            }
                            else if (selectBox.GetSelectedBtnData() as string == "No")
                            {
                                RefillScriptBox(new string[] {
                                    $"{_expPokemon.PokemonInfo.NickName} did not learn the move {_newMove.MoveName}."
                                });
                                _newMove = null;
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
                            GridLayoutSelectBox selectBox = ContentManager.Instance.ScriptBox.ScriptSelectBox;

                            if (selectBox.GetSelectedBtnData() as string == "Yes")
                            {
                                List<string> actionBtnNames = new List<string>()
                                {
                                    "Send Out",
                                    "Summary",
                                    "Cancel"
                                };
                                GameContentManager.Instance.OpenPokemonList(_myPokemons, actionBtnNames, "FadeOut");
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
            case BattleSceneState.ITEM_USE_SCRIPTING:
                {
                    if (_packet is S_UseItemInListScene)
                    {
                        ItemSummary itemSummary = ((S_UseItemInListScene)_packet).ItemSum;
                        int usedItemOrder = ((S_UseItemInListScene)_packet).UsedItemOrder;

                        // 아이템도 사용처리
                        Item usedItem = GameContentManager.Instance.BagContent.GetSelectedItem();
                        usedItem.UpdateItemSummary(itemSummary);

                        if (usedItem.ItemCnt == 0)
                            Managers.Object.MyPlayerController.Items[(ItemCategory)usedItem.ItemCategory].RemoveAt(usedItemOrder);

                        // 포켓볼 준비
                        _myPokemonArea.CreateAndThrowBall(usedItem);
                        _sceneState = BattleSceneState.BALL_THROW_ANIMATION;
                    }
                }
                break;
            case BattleSceneState.BALL_THROW_ANIMATION:
                {
                    _enemyPokemonArea.PlayBattlePokemonAnim("BattlePokemon_SuckedInToBall");
                    _sceneState = BattleSceneState.POKEMON_SUCKED_ANIMATION;
                }
                break;
            case BattleSceneState.POKEMON_SUCKED_ANIMATION:
                {
                    if (_packet is S_UseItemInListScene)
                    {
                        ItemUseResult itemUseResult = ((S_UseItemInListScene)_packet).ItemUseResult;

                        switch (itemUseResult.SpecialInfoCase)
                        {
                            case ItemUseResult.SpecialInfoOneofCase.PokeBallUseResult:
                                {
                                    PokeBallUseResult useResult = itemUseResult.PokeBallUseResult;
                                    bool didCatch = useResult.DidCatch;

                                    if (didCatch)
                                    {
                                        _myPokemonArea.PlaySuccessCatchBallAnim();
                                    }
                                    else
                                    {
                                        int ran = Random.Range(1, 4);
                                        _myPokemonArea.PlayFailCatchBallAnim(ran);
                                    }

                                    _sceneState = BattleSceneState.POKE_BALL_ANIMATION;
                                }
                                break;
                        }
                    }
                }
                break;
            case BattleSceneState.POKE_BALL_ANIMATION:
                {
                    if (_packet is S_UseItemInListScene)
                    {
                        ItemUseResult itemUseResult = ((S_UseItemInListScene)_packet).ItemUseResult;

                        switch (itemUseResult.SpecialInfoCase)
                        {
                            case ItemUseResult.SpecialInfoOneofCase.PokeBallUseResult:
                                {
                                    PokeBallUseResult useResult = itemUseResult.PokeBallUseResult;
                                    bool didCatch = useResult.DidCatch;

                                    if (didCatch)
                                    {
                                        Managers.Object.MyPlayerController.AddPokemon(_enemyPokemon);
                                        RefillScriptBox(new string[] { $"Gotcha! {_enemyPokemon} was caught!" }, true);
                                        SceneState = BattleSceneState.CATCH_SCRIPTING;
                                    }
                                    else
                                    {
                                        _enemyPokemonArea.PlayBattlePokemonAnim("BattlePokemon_ComeOutFromBall");
                                        _sceneState = BattleSceneState.POKEMON_COME_OUT_ANIMATION;
                                    }
                                }
                                break;
                        }
                    }
                }
                break;
            case BattleSceneState.POKEMON_COME_OUT_ANIMATION:
                {
                    RefillScriptBox(new string[] { "Aww! It appeared to be caught!" }, true);
                    SceneState = BattleSceneState.CATCH_SCRIPTING;
                }
                break;
            case BattleSceneState.CATCH_SCRIPTING:
                {
                    if (_packet is S_UseItemInListScene)
                    {
                        ItemUseResult itemUseResult = ((S_UseItemInListScene)_packet).ItemUseResult;

                        switch (itemUseResult.SpecialInfoCase)
                        {
                            case ItemUseResult.SpecialInfoOneofCase.PokeBallUseResult:
                                {
                                    PokeBallUseResult useResult = itemUseResult.PokeBallUseResult;
                                    bool didCatch = useResult.DidCatch;

                                    if (didCatch)
                                    {
                                        C_ReturnGame returnGamePacket = new C_ReturnGame();
                                        returnGamePacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                        ContentManager.Instance.FadeOutCurSceneToUnload(Define.Scene.Battle, "FadeOut", returnGamePacket);
                                    }
                                    else
                                    {
                                        SendProcessTurnPacket(_actionSelectContent.GetSelectMoveOrder());
                                    }
                                }
                                break;
                        }
                    }
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
            case BattleSceneState.TRAINER_DEATED_SCRIPTING:
                {
                    _enemyPokemonArea.PlayTrainerZoneAnim("Zone_LeftAppear");
                    SceneState = BattleSceneState.OPPONENT_TRAINER_APPEARING;
                }
                break;
            case BattleSceneState.OPPONENT_TRAINER_APPEARING:
                {
                    if (_packet is S_GetRewardInfo)
                    {
                        IList scripts = ((S_GetRewardInfo)_packet).AfterBattleScripts;
                        _script.Clear();

                        for (int i = 0; i < scripts.Count; i++)
                            _script.Add(scripts[i] as string);

                        ContentManager.Instance.ScriptBox.BeginScriptTyping(_script);
                        SceneState = BattleSceneState.AFTER_BATTLE_NPC_SCRIPTING;
                    }
                }
                break;
            case BattleSceneState.AFTER_BATTLE_NPC_SCRIPTING:
                {
                    if (_packet is S_GetRewardInfo)
                    {
                        int rewardMoney = ((S_GetRewardInfo)_packet).Money;
                        Managers.Object.MyPlayerController.ChangeMoney(Managers.Object.MyPlayerController.Money + rewardMoney);

                        RefillScriptBox(new string[] { $"{Managers.Object.MyPlayerController.PlayerName} got ${rewardMoney} for winning!" });
                        SceneState = BattleSceneState.GOT_REWARD_SCRIPTING;
                    }
                }
                break;
            case BattleSceneState.GOT_REWARD_SCRIPTING:
                {
                    SendRequestDataPacket(RequestType.CheckPokemonEvolution);
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
                            returnGamePacket.PlayerId = Managers.Object.MyPlayerController.Id;

                            ContentManager.Instance.FadeOutCurSceneToUnload(Define.Scene.Battle, "FadeOut", returnGamePacket);
                        }
                        else
                        {
                            if (_myPokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                            {
                                List<string> actionBtnNames = new List<string>()
                                {
                                    "Send Out",
                                    "Summary",
                                    "Cancel"
                                };
                                GameContentManager.Instance.OpenPokemonList(_myPokemons, actionBtnNames, "FadeOut");
                            }
                            else
                            {
                                SendProcessTurnPacket(_actionSelectContent.GetSelectMoveOrder());
                            }
                        }
                    }
                }
                break;
        }
    }

    public override void DoNextStaticAction(object value = null)
    {
        base.DoNextStaticAction(value);

        switch (_sceneState)
        {
            case BattleSceneState.OnlineBattleSurrenderScripting:
                {
                    PopAllContents();

                    C_ReturnGame returnGamePacket = new C_ReturnGame();
                    returnGamePacket.PlayerId = Managers.Object.MyPlayerController.Id;

                    ContentManager.Instance.FadeOutCurSceneToUnload(Define.Scene.Battle, "FadeOut", returnGamePacket);
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
            c_RequestDataPacket.PlayerId = Managers.Object.MyPlayerController.Id;
            c_RequestDataPacket.RequestType = requestType;

            Managers.Network.Send(c_RequestDataPacket);

            _loadingPacket = true;
        }
    }

    public void SwitchPokemon(int from, int to)
    {
        Pokemon pokemon = _myPokemons[from];
        _myPokemons[from] = _myPokemons[to];
        _myPokemons[to] = pokemon;
    }

    void SendProcessTurnPacket(int moveOrder)
    {
        if (!_loadingPacket)
        {
            C_ProcessTurn processTurnPacket = new C_ProcessTurn();
            processTurnPacket.PlayerId = Managers.Object.MyPlayerController.Id;
            processTurnPacket.MoveOrder = moveOrder;

            Managers.Network.Send(processTurnPacket);

            _loadingPacket = true;
        }
    }

    void RefillScriptBox(string[] scripts, bool autoSkip = false, float autoSkipTime = 1f, bool isStatic = false)
    {
        _script.Clear();

        for (int i = 0; i < scripts.Length; i++)
            _script.Add(scripts[i]);
        ContentManager.Instance.ScriptBox.BeginScriptTyping(_script, autoSkip, autoSkipTime, isStatic);
    }

    IEnumerator ActionAfterChangeHP()
    {
        yield return new WaitForSeconds(1f);

        if (_packet is S_ProcessTurn)
        {
            S_ProcessTurn processTurnPacket = _packet as S_ProcessTurn;
            bool isMyPokemon = processTurnPacket.IsMyPokemon;
            bool isTurnFinish = processTurnPacket.IsTurnFinish;

            if (_defensePokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
            {
                _defensePokemonArea.PlayPokemonZoneAnim("Zone_DownDisappear");

                SceneState = BattleSceneState.POKEMON_DIE;
            }
            else
            {
                if (isTurnFinish)
                {
                    ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                    _contentStack.Push(_actionSelectContent);
                    _contentStack.Peek().SetNextAction();
                }
                else
                {
                    if (!isMyPokemon)
                    {
                        // 내 포켓몬에 사용 가능한 기술이 있는 지 확인한다.
                        List<PokemonMove> myAvailableMoves = _actionSelectContent.FindAvailableMove();

                        if (myAvailableMoves.Count > 0)
                        {
                            SendProcessTurnPacket(_actionSelectContent.GetSelectMoveOrder());
                        }
                        else
                        {
                            SendProcessTurnPacket(-1);
                        }
                    }
                    else
                    {
                        SendProcessTurnPacket(_actionSelectContent.GetSelectMoveOrder());
                    }
                }
            }
        }
    }

    public override void Clear()
    {
    }
}
