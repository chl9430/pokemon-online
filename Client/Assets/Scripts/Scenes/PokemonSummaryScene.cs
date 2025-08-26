using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public enum PokemonSummarySceneState
{
    NONE = 0,
    WAITING_INPUT = 1,
    MOVING_TO_POKEMON_SCENE = 2,
    SLIDER_MOVING = 3,
    EXCHANGE_CANCELED_SCRIPTING = 4,
    EXIT_EXCHANGE_SCRIPTING = 5,
    MOVING_TO_GAME_SCENE = 6,
}

public class PokemonSummaryScene : BaseScene
{
    PokemonSummarySceneState _sceneState = PokemonSummarySceneState.NONE;

    [SerializeField] PokemonSummaryUI summaryUI;
    [SerializeField] CategorySlider _slider;
    [SerializeField] List<SliderContent> _sliderContents;
    [SerializeField] RectTransform _indicator;
    [SerializeField] ScriptBoxUI _scriptBox;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.PokemonSummary;
    }

    protected override void Start()
    {
        base.Start();

        if (Managers.Scene.Data is PokemonSummary)
        {
            summaryUI.FillPokemonSummary(Managers.Scene.Data as PokemonSummary);

            _slider.UpdateSliderContents(_sliderContents);

            _enterEffect.PlayEffect("FadeIn");
        }
        else
        {
            PokemonSummary dummySummary = new PokemonSummary();
            PokemonInfo dummyInfo = new PokemonInfo()
            {
                DictionaryNum = 35,
                NickName = "MESSI",
                PokemonName = "Charmander",
                Level = 10,
                Gender = PokemonGender.Male,
                Type1 = PokemonType.Fire,
                Type2 = PokemonType.Water,
                OwnerName = "CHRIS",
                OwnerId = 99999,
                Nature = PokemonNature.Quirky,
                MetLevel = 10,
            };
            PokemonStat dummyStat = new PokemonStat()
            {
                Hp = 10,
                MaxHp = 100,
                Attack = 50,
                Defense = 40,
                SpecialAttack = 70,
                SpecialDefense = 40,
                Speed = 60,
            };
            PokemonExpInfo dummyExpInfo = new PokemonExpInfo()
            {
                CurExp = 1000,
                TotalExp = 10000,
                RemainExpToNextLevel = 5000,
            };

            dummySummary.PokemonInfo = dummyInfo;
            dummySummary.PokemonStat = dummyStat;
            dummySummary.PokemonExpInfo = dummyExpInfo;

            summaryUI.FillPokemonSummary(dummySummary);

            _slider.UpdateSliderContents(_sliderContents);

            _enterEffect.PlayEffect("FadeIn");
        }
    }

    public override void UpdateData(IMessage packet)
    {
        if (packet is S_ExitPokemonExchangeScene)
        {
            S_ExitPokemonExchangeScene exitExchangePacket = packet as S_ExitPokemonExchangeScene;
            PlayerInfo exitPlayerInfo = exitExchangePacket.ExitPlayerInfo;

            _sceneState = PokemonSummarySceneState.EXIT_EXCHANGE_SCRIPTING;
            List<string> scripts = new List<string>()
            {
                $"{exitPlayerInfo.PlayerName} has ended the exchange."
            };
            _scriptBox.BeginScriptTyping(scripts, true);
        }
        else if (packet is S_FinalAnswerToExchange)
        {
            S_FinalAnswerToExchange finalAnswerPacket = packet as S_FinalAnswerToExchange;
            PokemonSummary myPokemonSum = finalAnswerPacket.MyPokemonSum;
            PokemonSummary otherPokemonSum = finalAnswerPacket.OtherPokemonSum;

            if (myPokemonSum == null || otherPokemonSum == null)
            {
                _sceneState = PokemonSummarySceneState.EXCHANGE_CANCELED_SCRIPTING;
                List<string> script = new List<string>()
                {
                    $"The other party canceled the exchange.",
                };
                _scriptBox.BeginScriptTyping(script, true);
            }
        }
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);

        switch (_sceneState)
        {
            case PokemonSummarySceneState.NONE:
                {
                    // ¾À »óÅÂ º¯°æ
                    _sceneState = PokemonSummarySceneState.WAITING_INPUT;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case PokemonSummarySceneState.WAITING_INPUT:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            _enterEffect.PlayEffect("FadeOut");

                            _sceneState = PokemonSummarySceneState.MOVING_TO_POKEMON_SCENE;
                            ActiveUIBySceneState(_sceneState);
                        }
                    }
                    else
                    {
                        int selectedIdx = _slider.CurIdx;

                        _indicator.anchorMax = new Vector2(((float)selectedIdx + 1f) / (float)_sliderContents.Count, 1f);
                    }
                }
                break;
            case PokemonSummarySceneState.MOVING_TO_POKEMON_SCENE:
                {
                    if (Managers.Object.PlayerInfo.ObjectInfo.PosInfo.State == CreatureState.Exchanging)
                    {
                        C_EnterPokemonExchangeScene enterExchangeScene = new C_EnterPokemonExchangeScene();
                        enterExchangeScene.PlayerId = Managers.Object.PlayerInfo.ObjectInfo.ObjectId;

                        Managers.Network.SavePacket(enterExchangeScene);

                        // ¾À º¯°æ
                        Managers.Scene.LoadScene(Define.Scene.PokemonExchange);
                    }
                    else
                    {
                        C_EnterPokemonListScene enterPokemonPacket = new C_EnterPokemonListScene();
                        enterPokemonPacket.PlayerId = Managers.Object.PlayerInfo.ObjectInfo.ObjectId;

                        Managers.Network.SavePacket(enterPokemonPacket);

                        // ¾À º¯°æ
                        Managers.Scene.LoadScene(Define.Scene.PokemonList);
                    }
                }
                break;
            case PokemonSummarySceneState.EXCHANGE_CANCELED_SCRIPTING:
                {
                    _enterEffect.PlayEffect("FadeOut");

                    _sceneState = PokemonSummarySceneState.MOVING_TO_POKEMON_SCENE;
                    ActiveUIBySceneState(_sceneState);
                }
                break;
            case PokemonSummarySceneState.EXIT_EXCHANGE_SCRIPTING:
                {
                    _enterEffect.PlayEffect("FadeOut");

                    _sceneState = PokemonSummarySceneState.MOVING_TO_GAME_SCENE;
                }
                break;
            case PokemonSummarySceneState.MOVING_TO_GAME_SCENE:
                {
                    C_ReturnGame returnGamePacket = new C_ReturnGame();
                    returnGamePacket.PlayerId = Managers.Object.PlayerInfo.ObjectInfo.ObjectId;

                    Managers.Network.SavePacket(returnGamePacket);

                    // ¾À º¯°æ
                    Managers.Scene.LoadScene(Define.Scene.Game);
                }
                break;
        }
    }

    void ActiveUIBySceneState(PokemonSummarySceneState state)
    {
        if (_sceneState == PokemonSummarySceneState.WAITING_INPUT)
        {
            _slider.SliderState = SliderState.WAITING_INPUT;
        }
    }

    public override void Clear()
    {
    }
}
