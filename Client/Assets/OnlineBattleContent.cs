using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.Playables;

public enum OnlineBattleContentState
{
    None = 0,
    BattleBeginScripting = 2,
    OpponentTrainerDisappear = 3,
    OpponentPokemonAppearScripting = 4,
    OpponentPokemonAppearing = 5,
    OpponentPokemonCardAppearing = 6,
    TrainerDisappear = 7,
    MyPokemonAppearScripting = 8,
    MyPokemonAppearing = 9,
    MyPokemonCardAppearing = 10,
    AttackInstructScripting = 11,
    EffectivenessScripting = 12,
    AttackAnimation = 13,
    PokemonBlinkAnimation = 14,
    ChangingPokemonHP = 15,
    PokemonDieAnimation = 16,
    AttackMissedScriptng = 20,
    PokemonCardDisappearing = 21,
    PokemonDieScripting = 22,
    MovingToPokemonList = 23,
    SwitchingPokemonScripting = 24,
    PrevPokemonDisappearing = 25,
    PrevPokemonCardDisappearing = 26,
    FinishBattleScripting = 27,
    MovingToGameScene = 28,
    Inactiving = 99,
}

public class OnlineBattleContent : ObjectContents
{
    OnlineBattleContentState _state = OnlineBattleContentState.None;
    PlayableDirector _playableDirector;
    OtherPlayerInfo _otherPlayerInfo;
    Pokemon _enemyPokemon;
    Pokemon _myPokemon;

    Pokemon _attackPokemon;
    Pokemon _defensePokemon;
    BattleArea _attackPokemonArea;
    BattleArea _defensePokemonArea;

    [SerializeField] OnlineBattleActionContent _actionSelectContent;

    [SerializeField] BattleArea _enemyPokemonArea;
    [SerializeField] BattleArea _myPokemonArea;

    public OnlineBattleContentState State
    {
        set
        {
            _state = value;
        }
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;

        if (_packet is S_EnterTrainerBattle)
        {
            ContentManager.Instance.PlayScreenEffecter("PokemonAppear_FadeIn");

            IList<OtherPlayerInfo> otherPlayerInfos = ((S_EnterTrainerBattle)packet).OtherPlayerInfos;
            IList<PokemonSummary> otherPokemonSums = ((S_EnterTrainerBattle)packet).OtherPokemonSums;

            if (_playableDirector == null)
                _playableDirector = GetComponent<PlayableDirector>();

            _playableDirector.Play();

            _enemyPokemonArea.FillTrainerImage(otherPlayerInfos[0].PlayerGender, false);
            _enemyPokemonArea.PlayInfoZoneAnim("Zone_LeftHide");
            _enemyPokemonArea.PlayPokemonZoneAnim("Zone_RightHide");

            // 포켓몬 및 플레이어 데이터 채우기
            _otherPlayerInfo = otherPlayerInfos[0];
            _enemyPokemon = new Pokemon(otherPokemonSums[0]);
            _myPokemon = ((BattleScene)Managers.Scene.CurrentScene).Pokemons[0];

            // 포켓몬 랜더링
            _myPokemonArea.FillTrainerImage(Managers.Object.MyPlayerController.PlayerGender, true);
            _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftHide");
            _myPokemonArea.PlayInfoZoneAnim("Zone_RightHide");
        }
        else if (_packet is S_SendAction)
        {
            if (ContentManager.Instance.ScreenEffecter != null)
                ContentManager.Instance.PlayScreenEffecter("FadeIn_NonBroading");

            S_SendAction actionPacket = ((S_SendAction)_packet);
            int turnPlayerId = actionPacket.TurnPlayerId;

            switch (actionPacket.SpecialInfoCase)
            {
                case S_SendAction.SpecialInfoOneofCase.UseMoveResult:
                    {
                        UseMoveResult result = actionPacket.UseMoveResult;
                        PokemonMoveSummary usedMoveSum = result.UsedMoveSum;
                        PokemonSummary defensePokemonSum = result.DefensePokemonSum;
                        int moveOrder = result.UsedMoveOrder;

                        if (Managers.Object.MyPlayerController.Id == turnPlayerId)
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

                        List<string> scripts = new List<string>()
                        {
                            $"{_attackPokemon.PokemonInfo.NickName} used {usedMoveSum.MoveName}!"
                        };
                        ContentManager.Instance.BeginScriptTyping(scripts, true);
                        State = OnlineBattleContentState.AttackInstructScripting;
                    }
                    break;
                case S_SendAction.SpecialInfoOneofCase.SwitchPokemonResult:
                    {
                        SwitchBattlePokemonResult result = actionPacket.SwitchPokemonResult;
                        PokemonSummary prevPokemonSum = result.PrevPokemonSum;
                        PokemonSummary newPokemonSum = result.NewPokemonSum;

                        if (Managers.Object.MyPlayerController.Id == turnPlayerId) // 내가 교체할때
                        {
                            _myPokemon = ((BattleScene)Managers.Scene.CurrentScene).Pokemons[0];

                            if (prevPokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                            {
                                // 포켓몬 랜더링
                                _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftHide");
                                _myPokemonArea.PlayInfoZoneAnim("Zone_RightHide");

                                List<string> scripts = new List<string>()
                                {
                                    $"Go! {_myPokemon.PokemonInfo.NickName}!"
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts, true);
                                State = OnlineBattleContentState.MyPokemonAppearScripting;
                            }
                            else
                            {
                                List<string> scripts = new List<string>()
                                {
                                    $"That is enough! Come back {prevPokemonSum.PokemonInfo.NickName}!"
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts, true);
                                State = OnlineBattleContentState.SwitchingPokemonScripting;
                            }
                        }
                        else // 상대방이 교체할때
                        {
                            _enemyPokemon = new Pokemon(newPokemonSum);

                            if (prevPokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                            {
                                // 포켓몬 랜더링
                                _enemyPokemonArea.PlayPokemonZoneAnim("Zone_RightHide");
                                _enemyPokemonArea.PlayInfoZoneAnim("Zone_LeftHide");

                                List<string> scripts = new List<string>()
                                {
                                    $"{_otherPlayerInfo.PlayerName} sent out {_enemyPokemon.PokemonInfo.NickName}!"
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts, true);
                                State = OnlineBattleContentState.OpponentPokemonAppearScripting;
                            }
                            else
                            {
                                List<string> scripts = new List<string>()
                                {
                                    $"{_otherPlayerInfo.PlayerName} put {prevPokemonSum.PokemonInfo.NickName} in."
                                };
                                ContentManager.Instance.BeginScriptTyping(scripts, true);
                                State = OnlineBattleContentState.SwitchingPokemonScripting;
                            }
                        }
                    }
                    break;
                case S_SendAction.SpecialInfoOneofCase.FinishBattleResult:
                    {
                        FinishBattleResult result = actionPacket.FinishBattleResult;
                        int winPlayerId = result.WinPlayerId;
                        int losePlayerId = result.LosePlayerId;

                        if (Managers.Object.MyPlayerController.Id == winPlayerId)
                        {
                            List<string> scripts = new List<string>()
                            {
                                $"Won the Pokemon battle with {_otherPlayerInfo.PlayerName}."
                            };
                            ContentManager.Instance.BeginScriptTyping(scripts);
                            State = OnlineBattleContentState.FinishBattleScripting;
                        }
                        else
                        {
                            List<string> scripts = new List<string>()
                            {
                                $"Lost the Pokemon battle with {_otherPlayerInfo.PlayerName}."
                            };
                            ContentManager.Instance.BeginScriptTyping(scripts);
                            State = OnlineBattleContentState.FinishBattleScripting;
                        }
                    }
                    break;
            }
        }
        else if (_packet is S_CheckAvailablePokemon)
        {
            bool canFight = ((S_CheckAvailablePokemon)_packet).CanFight;

            if (canFight)
            {
                ContentManager.Instance.PlayScreenEffecter("FadeOut");

                State = OnlineBattleContentState.MovingToPokemonList;
            }
        }
    }

    public override void SetNextAction(object value = null)
    {
        if (_isActionStop)
            return;

        switch (_state)
        {
            case OnlineBattleContentState.None:
                {
                    _isLoading = false;
                    _isActionStop = false;

                    List<string> scripts = new List<string>()
                    {
                        $"{_otherPlayerInfo.PlayerName} would like to battle!"
                    };
                    ContentManager.Instance.BeginScriptTyping(scripts);
                    State = OnlineBattleContentState.BattleBeginScripting;
                }
                break;
            case OnlineBattleContentState.BattleBeginScripting:
                {
                    _enemyPokemonArea.PlayTrainerZoneAnim("Zone_RightDisappear");
                    State = OnlineBattleContentState.OpponentTrainerDisappear;
                }
                break;
            case OnlineBattleContentState.OpponentTrainerDisappear:
                {
                    List<string> scripts = new List<string>()
                    {
                        $"{_otherPlayerInfo.PlayerName} sent out {_enemyPokemon.PokemonInfo.NickName}!"
                    };
                    ContentManager.Instance.BeginScriptTyping(scripts);
                    State = OnlineBattleContentState.OpponentPokemonAppearScripting;
                }
                break;
            case OnlineBattleContentState.OpponentPokemonAppearScripting:
                {
                    _enemyPokemonArea.FillPokemonInfo(_enemyPokemon, false);

                    _enemyPokemonArea.PlayPokemonZoneAnim("Zone_LeftAppear");
                    State = OnlineBattleContentState.OpponentPokemonAppearing;
                }
                break;
            case OnlineBattleContentState.OpponentPokemonAppearing:
                {
                    _enemyPokemonArea.PlayInfoZoneAnim("Zone_RightAppear");
                    State = OnlineBattleContentState.OpponentPokemonCardAppearing;
                }
                break;
            case OnlineBattleContentState.OpponentPokemonCardAppearing:
                {
                    if (_packet is S_SendAction)
                    {
                        S_SendAction actionPacket = ((S_SendAction)_packet);
                        bool isTurnFinish = actionPacket.IsTurnFinish;
                        SwitchBattlePokemonResult result = actionPacket.SwitchPokemonResult;
                        PokemonSummary prevPokemonSum = result.PrevPokemonSum;

                        if (prevPokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                        {
                            ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                            Managers.Scene.CurrentScene.ContentStack.Push(_actionSelectContent);
                            Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction(_myPokemon);

                            State = OnlineBattleContentState.Inactiving;
                        }
                        else
                        {
                            if (isTurnFinish)
                            {
                                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                                Managers.Scene.CurrentScene.ContentStack.Push(_actionSelectContent);
                                Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction(_myPokemon);

                                State = OnlineBattleContentState.Inactiving;
                            }
                            else
                            {
                                if (!_isLoading)
                                {
                                    _isLoading = false;

                                    C_RequestNextBattleAction requestActionPacket = new C_RequestNextBattleAction();
                                    requestActionPacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                    Managers.Network.Send(requestActionPacket);
                                }

                                ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Waiting for the other side...");
                            }
                        }
                    }
                    else
                    {
                        _myPokemonArea.PlayTrainerZoneAnim("Zone_LeftDisappear");
                        State = OnlineBattleContentState.TrainerDisappear;
                    }
                }
                break;
            case OnlineBattleContentState.TrainerDisappear:
                {
                    List<string> scripts = new List<string>()
                    {
                        $"Go! {_myPokemon.PokemonInfo.NickName}!"
                    };
                    ContentManager.Instance.BeginScriptTyping(scripts);
                    State = OnlineBattleContentState.MyPokemonAppearScripting;
                }
                break;
            case OnlineBattleContentState.MyPokemonAppearScripting:
                {
                    _myPokemonArea.FillPokemonInfo(_myPokemon, true);

                    _myPokemonArea.PlayPokemonZoneAnim("Zone_RightAppear");
                    State = OnlineBattleContentState.MyPokemonAppearing;
                }
                break;
            case OnlineBattleContentState.MyPokemonAppearing:
                {
                    _myPokemonArea.PlayInfoZoneAnim("Zone_LeftAppear");
                    State = OnlineBattleContentState.MyPokemonCardAppearing;
                }
                break;
            case OnlineBattleContentState.MyPokemonCardAppearing:
                {
                    if (_packet is S_SendAction)
                    {
                        S_SendAction actionPacket = ((S_SendAction)_packet);
                        bool isTurnFinish = actionPacket.IsTurnFinish;
                        SwitchBattlePokemonResult result = actionPacket.SwitchPokemonResult;
                        PokemonSummary prevPokemonSum = result.PrevPokemonSum;

                        if (prevPokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                        {
                            ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                            Managers.Scene.CurrentScene.ContentStack.Push(_actionSelectContent);
                            Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction(_myPokemon);

                            State = OnlineBattleContentState.Inactiving;
                        }
                        else
                        {
                            if (isTurnFinish)
                            {
                                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                                Managers.Scene.CurrentScene.ContentStack.Push(_actionSelectContent);
                                Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction(_myPokemon);

                                State = OnlineBattleContentState.Inactiving;
                            }
                            else
                            {
                                if (!_isLoading)
                                {
                                    _isLoading = false;

                                    C_RequestNextBattleAction requestActionPacket = new C_RequestNextBattleAction();
                                    requestActionPacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                    Managers.Network.Send(requestActionPacket);
                                }

                                ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Waiting for the other side...");
                            }
                        }
                    }
                    else
                    {
                        ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                        Managers.Scene.CurrentScene.ContentStack.Push(_actionSelectContent);
                        Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction(_myPokemon);

                        State = OnlineBattleContentState.Inactiving;
                    }
                }
                break;
            case OnlineBattleContentState.AttackInstructScripting:
                {
                    if (_packet is S_SendAction)
                    {
                        S_SendAction actionPacket = ((S_SendAction)_packet);
                        switch (actionPacket.SpecialInfoCase)
                        {
                            case S_SendAction.SpecialInfoOneofCase.UseMoveResult:
                                {
                                    UseMoveResult result = actionPacket.UseMoveResult;
                                    int usedMoveOrder = result.UsedMoveOrder;
                                    bool isHit = result.IsHit;
                                    float typeEffectiveness = result.TypeEffectiveness;

                                    if (isHit)
                                    {
                                        if (typeEffectiveness == 0f)
                                        {
                                            List<string> scripts = new List<string>()
                                            {
                                                $"It doesn't affect {_defensePokemon.PokemonInfo.NickName}..."
                                            };
                                            ContentManager.Instance.BeginScriptTyping(scripts, true);
                                            State = OnlineBattleContentState.EffectivenessScripting;
                                        }
                                        else
                                        {
                                            PokemonMove usedMove = usedMoveOrder != -1 ? _attackPokemon.PokemonMoves[usedMoveOrder] : _attackPokemon.NoPPMove;

                                            if (_attackPokemonArea == _myPokemonArea)
                                                _attackPokemonArea.PlayBattlePokemonAnim("BattlePokemon_RightAttack");
                                            else
                                                _attackPokemonArea.PlayBattlePokemonAnim("BattlePokemon_LeftAttack");

                                            _defensePokemonArea.TriggerPokemonHitImage(usedMove);
                                            State = OnlineBattleContentState.AttackAnimation;
                                        }
                                    }
                                    else
                                    {
                                        List<string> scripts = new List<string>()
                                        {
                                            $"{_attackPokemon.PokemonInfo.NickName}'s attack is off the mark!"
                                        };
                                        ContentManager.Instance.BeginScriptTyping(scripts, true);
                                        State = OnlineBattleContentState.AttackMissedScriptng;
                                    }
                                }
                                break;
                        }
                    }
                }
                break;
            case OnlineBattleContentState.EffectivenessScripting:
                {
                    StartCoroutine(ActionAfterChangeHP());
                }
                break;
            case OnlineBattleContentState.AttackAnimation:
                {
                    State = OnlineBattleContentState.PokemonBlinkAnimation;
                    _defensePokemonArea.PlayBattlePokemonAnim("BattlePokemon_Hit");
                }
                break;
            case OnlineBattleContentState.PokemonBlinkAnimation:
                {
                    _defensePokemonArea.ChangePokemonHP(_defensePokemon.PokemonStat.Hp);
                    State = OnlineBattleContentState.ChangingPokemonHP;
                }
                break;
            case OnlineBattleContentState.ChangingPokemonHP:
                {
                    if (_packet is S_SendAction)
                    {
                        S_SendAction actionPacket = ((S_SendAction)_packet);
                        switch (actionPacket.SpecialInfoCase)
                        {
                            case S_SendAction.SpecialInfoOneofCase.UseMoveResult:
                                {
                                    UseMoveResult result = actionPacket.UseMoveResult;
                                    float typeEffectiveness = result.TypeEffectiveness;
                                    bool isCriticalHit = result.IsCriticalHit;

                                    List<string> scripts = null;
                                    scripts = CreateEffectivenessScripts(isCriticalHit, typeEffectiveness);

                                    if (scripts.Count > 0)
                                    {
                                        ContentManager.Instance.ScriptBox.BeginScriptTyping(scripts, true);
                                        State = OnlineBattleContentState.EffectivenessScripting;
                                    }
                                    else
                                    {
                                        StartCoroutine(ActionAfterChangeHP());
                                    }
                                }
                                break;
                        }
                    }
                }
                break;
            case OnlineBattleContentState.AttackMissedScriptng:
                {
                    if (_packet is S_SendAction)
                    {
                        S_SendAction actionPacket = ((S_SendAction)_packet);
                        bool isTurnFinish = actionPacket.IsTurnFinish;
                        switch (actionPacket.SpecialInfoCase)
                        {
                            case S_SendAction.SpecialInfoOneofCase.UseMoveResult:
                                {
                                    UseMoveResult result = actionPacket.UseMoveResult;

                                    if (isTurnFinish)
                                    {
                                        ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                                        Managers.Scene.CurrentScene.ContentStack.Push(_actionSelectContent);
                                        Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction(_myPokemon);

                                        State = OnlineBattleContentState.Inactiving;
                                    }
                                    else
                                    {
                                        if (!_isLoading)
                                        {
                                            _isLoading = false;

                                            C_RequestNextBattleAction requestActionPacket = new C_RequestNextBattleAction();
                                            requestActionPacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                            Managers.Network.Send(requestActionPacket);
                                        }

                                        ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Waiting for the other side...");
                                    }
                                }
                                break;
                        }
                    }
                }
                break;
            case OnlineBattleContentState.PokemonDieAnimation:
                {
                    if (_packet is S_SendAction)
                    {
                        S_SendAction actionPacket = ((S_SendAction)_packet);
                        int turnPlayerId = actionPacket.TurnPlayerId;

                        switch (actionPacket.SpecialInfoCase)
                        {
                            case S_SendAction.SpecialInfoOneofCase.UseMoveResult:
                                {
                                    UseMoveResult result = actionPacket.UseMoveResult;

                                    if (Managers.Object.MyPlayerController.Id == turnPlayerId)
                                        _enemyPokemonArea.PlayInfoZoneAnim("Zone_LeftDisappear");
                                    else
                                        _myPokemonArea.PlayInfoZoneAnim("Zone_RightDisappear");

                                    State = OnlineBattleContentState.PokemonCardDisappearing;
                                }
                                break;
                        }
                    }
                }
                break;
            case OnlineBattleContentState.PokemonCardDisappearing:
                {
                    List<string> scripts = new List<string>()
                    {
                        $"{_defensePokemon.PokemonInfo.NickName} fell down!"
                    };
                    ContentManager.Instance.BeginScriptTyping(scripts);

                    State = OnlineBattleContentState.PokemonDieScripting;
                }
                break;
            case OnlineBattleContentState.PokemonDieScripting:
                {
                    if (_packet is S_SendAction)
                    {
                        S_SendAction actionPacket = ((S_SendAction)_packet);
                        int turnPlayerId = actionPacket.TurnPlayerId;

                        switch (actionPacket.SpecialInfoCase)
                        {
                            case S_SendAction.SpecialInfoOneofCase.UseMoveResult:
                                {
                                    UseMoveResult result = actionPacket.UseMoveResult;

                                    // 상대방의 포켓몬이 기절했다면
                                    if (Managers.Object.MyPlayerController.Id == turnPlayerId)
                                    {
                                        if (!_isLoading)
                                        {
                                            _isLoading = false;

                                            C_RequestNextBattleAction requestActionPacket = new C_RequestNextBattleAction();
                                            requestActionPacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                            Managers.Network.Send(requestActionPacket);
                                        }

                                        ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Waiting for the other side...");
                                    }
                                    else
                                    {
                                        if (!_isLoading)
                                        {
                                            _isLoading = true;

                                            C_CheckAvailablePokemon checkPokemonPacket = new C_CheckAvailablePokemon();
                                            checkPokemonPacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                            Managers.Network.Send(checkPokemonPacket);
                                        }
                                        ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Waiting for the other side...");
                                    }
                                }
                                break;
                        }
                    }
                }
                break;
            case OnlineBattleContentState.MovingToPokemonList:
                {
                    List<string> btnNames = new List<string>()
                    {
                        "Send Out",
                        "Summary",
                        "Cancel"
                    };
                    GameContentManager.Instance.OpenPokemonList(((BattleScene)Managers.Scene.CurrentScene).Pokemons, btnNames);

                    State = OnlineBattleContentState.Inactiving;
                }
                break;
            case OnlineBattleContentState.SwitchingPokemonScripting:
                {
                    if (_packet is S_SendAction)
                    {
                        S_SendAction actionPacket = ((S_SendAction)_packet);
                        int turnPlayerId = actionPacket.TurnPlayerId;

                        if (Managers.Object.MyPlayerController.Id == turnPlayerId)
                        {
                            _myPokemonArea.PlayPokemonZoneAnim("Zone_LeftDisappear");
                            State = OnlineBattleContentState.PrevPokemonDisappearing;
                        }
                        else
                        {
                            _enemyPokemonArea.PlayPokemonZoneAnim("Zone_RightDisappear");
                            State = OnlineBattleContentState.PrevPokemonDisappearing;
                        }
                    }
                }
                break;
            case OnlineBattleContentState.PrevPokemonDisappearing:
                {
                    if (_packet is S_SendAction)
                    {
                        S_SendAction actionPacket = ((S_SendAction)_packet);
                        int turnPlayerId = actionPacket.TurnPlayerId;

                        if (Managers.Object.MyPlayerController.Id == turnPlayerId)
                        {
                            _myPokemonArea.PlayInfoZoneAnim("Zone_RightDisappear");
                            State = OnlineBattleContentState.PrevPokemonCardDisappearing;
                        }
                        else
                        {
                            _enemyPokemonArea.PlayInfoZoneAnim("Zone_LeftDisappear");
                            State = OnlineBattleContentState.PrevPokemonCardDisappearing;
                        }
                    }
                }
                break;
            case OnlineBattleContentState.PrevPokemonCardDisappearing:
                {
                    if (_packet is S_SendAction)
                    {
                        S_SendAction actionPacket = ((S_SendAction)_packet);
                        int turnPlayerId = actionPacket.TurnPlayerId;

                        if (Managers.Object.MyPlayerController.Id == turnPlayerId)
                        {
                            List<string> scripts = new List<string>()
                            {
                                $"Go! {_myPokemon.PokemonInfo.NickName}!"
                            };
                            ContentManager.Instance.BeginScriptTyping(scripts);
                            State = OnlineBattleContentState.MyPokemonAppearScripting;
                        }
                        else
                        {
                            List<string> scripts = new List<string>()
                            {
                                $"{_otherPlayerInfo.PlayerName} sent out {_enemyPokemon.PokemonInfo.NickName}!"
                            };
                            ContentManager.Instance.BeginScriptTyping(scripts);
                            State = OnlineBattleContentState.OpponentPokemonAppearScripting;
                        }
                    }
                }
                break;
            case OnlineBattleContentState.FinishBattleScripting:
                {
                    ContentManager.Instance.PlayScreenEffecter("FadeOut");

                    State = OnlineBattleContentState.MovingToGameScene;
                }
                break;
            case OnlineBattleContentState.MovingToGameScene:
                {
                    C_ReturnGame returnGamePacket = new C_ReturnGame();
                    returnGamePacket.PlayerId = Managers.Object.MyPlayerController.Id;

                    Managers.Scene.AsyncUnLoadScene(Define.Scene.Battle, () =>
                    {
                        ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
                        Managers.Scene.CurrentScene = GameObject.FindFirstObjectByType<GameScene>();
                        Managers.Network.Send(returnGamePacket);
                    });
                }
                break;
            case OnlineBattleContentState.Inactiving:
                {
                }
                break;
        }
    }

    IEnumerator ActionAfterChangeHP()
    {
        yield return new WaitForSeconds(1f);

        if (_packet is S_SendAction)
        {
            S_SendAction actionPacket = ((S_SendAction)_packet);
            bool isTurnFinish = actionPacket.IsTurnFinish;

            switch (actionPacket.SpecialInfoCase)
            {
                case S_SendAction.SpecialInfoOneofCase.UseMoveResult:
                    {
                        UseMoveResult result = actionPacket.UseMoveResult;

                        if (_defensePokemon.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                        {
                            _defensePokemonArea.PlayPokemonZoneAnim("Zone_DownDisappear");

                            State = OnlineBattleContentState.PokemonDieAnimation;
                        }
                        else
                        {
                            if (isTurnFinish)
                            {
                                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);

                                Managers.Scene.CurrentScene.ContentStack.Push(_actionSelectContent);
                                Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction(_myPokemon);

                                State = OnlineBattleContentState.Inactiving;
                            }
                            else
                            {
                                if (!_isLoading)
                                {
                                    _isLoading = false;

                                    C_RequestNextBattleAction requestActionPacket = new C_RequestNextBattleAction();
                                    requestActionPacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                    Managers.Network.Send(requestActionPacket);
                                }

                                ContentManager.Instance.ScriptBox.SetScriptWihtoutTyping("Waiting for the other side...");
                            }
                        }
                    }
                    break;
            }
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

    public override void InactiveContent()
    {
        base.InactiveContent();

        State = OnlineBattleContentState.Inactiving;
    }

    public override void FinishContent()
    {
        State = OnlineBattleContentState.None;

        Managers.Scene.CurrentScene.FinishContents();
    }
}