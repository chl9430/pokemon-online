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
}

public class PokemonSummaryScene : BaseScene
{
    PokemonSummarySceneState _sceneState = PokemonSummarySceneState.NONE;

    [SerializeField] PokemonSummaryUI summaryUI;
    [SerializeField] CategorySlider _slider;
    [SerializeField] List<SliderContent> _sliderContents;
    [SerializeField] RectTransform _indicator;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.PokemonSummary;
    }

    protected override void Start()
    {
        base.Start();

        Pokemon pokemon = Managers.Scene.Data as Pokemon;

        // 테스트 시 사용
        if (pokemon == null)
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

            pokemon = new Pokemon(dummySummary);
        }

        summaryUI.FillPokemonSummary(pokemon);

        _slider.UpdateSliderContents(_sliderContents);

        _enterEffect.PlayEffect("FadeIn");
    }

    public override void DoNextAction(object value = null)
    {
        Debug.Log(value);

        switch (_sceneState)
        {
            case PokemonSummarySceneState.NONE:
                {
                    // 씬 상태 변경
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
                    C_EnterPokemonListScene enterPokemonPacket = new C_EnterPokemonListScene();
                    enterPokemonPacket.PlayerId = Managers.Object.PlayerInfo.ObjectInfo.ObjectId;

                    Managers.Network.SavePacket(enterPokemonPacket);

                    // 씬 변경
                    Managers.Scene.LoadScene(Define.Scene.PokemonList);
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
