using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum PokemonListSceneState
{
    NON_SELECTED = 0,
    SELECTED = 1
}

public class PokemonListUI : Action_UI
{
    int selectedIdx;
    PokemonListSceneState sceneState;
    BaseScene scene;

    List<ImageButton> _btns;
    List<Pokemon> _pokemons;
    Pokemon _selectedPokemon;

    [SerializeField] Transform mainPokemonCardZone;
    [SerializeField] Transform[] subPokemonCardZone;
    [SerializeField] ImageButton cancelBtn;

    public PokemonListSceneState SceneState
    {
        set
        {
            sceneState = value;
        }
    }

    void Start()
    {
        scene = Managers.Scene.CurrentScene;
        _btns = new List<ImageButton>();

        {
            _pokemons = new List<Pokemon>();

            PokemonSummary summary = new PokemonSummary();
            PokemonInfo info = new PokemonInfo()
            {
                DictionaryNum = 35,
                NickName = "MESSI",
                PokemonName = "Charmander",
                Level = 10,
                Gender = PokemonGender.Male,
            };
            PokemonSkill skill = new PokemonSkill()
            {
                Stat = new PokemonStat()
                {
                    Hp = 10,
                    MaxHp = 100,
                    Attack = 50,
                    Defense = 40,
                    SpecialAttack = 70,
                    SpecialDefense = 40,
                    Speed = 60
                }
            };
            PokemonBattleMove battleMove = new PokemonBattleMove()
            {
            };

            summary.Info = info;
            summary.Skill = skill;
            summary.BattleMove = battleMove;

            Pokemon pokemon = new Pokemon(summary);

            PokemonSummary summary1 = new PokemonSummary();
            PokemonInfo info1 = new PokemonInfo()
            {
                DictionaryNum = 35,
                NickName = "PEDRO",
                PokemonName = "Pikachu",
                Level = 10,
                Gender = PokemonGender.Male,
            };
            PokemonSkill skill1 = new PokemonSkill()
            {
                Stat = new PokemonStat()
                {
                    Hp = 10,
                    MaxHp = 100,
                    Attack = 50,
                    Defense = 40,
                    SpecialAttack = 70,
                    SpecialDefense = 40,
                    Speed = 60
                }
            };
            PokemonBattleMove battleMove1 = new PokemonBattleMove()
            {
            };

            summary1.Info = info1;
            summary1.Skill = skill1;
            summary1.BattleMove = battleMove1;

            Pokemon pokemon1 = new Pokemon(summary1);

            PokemonSummary summary2 = new PokemonSummary();
            PokemonInfo info2 = new PokemonInfo()
            {
                DictionaryNum = 35,
                NickName = "VILLA",
                PokemonName = "Squirtle",
                Level = 10,
                Gender = PokemonGender.Male,
            };
            PokemonSkill skill2 = new PokemonSkill()
            {
                Stat = new PokemonStat()
                {
                    Hp = 10,
                    MaxHp = 100,
                    Attack = 50,
                    Defense = 40,
                    SpecialAttack = 70,
                    SpecialDefense = 40,
                    Speed = 60
                }
            };
            PokemonBattleMove battleMove2 = new PokemonBattleMove()
            {
            };

            summary2.Info = info2;
            summary2.Skill = skill2;
            summary2.BattleMove = battleMove2;

            Pokemon pokemon2 = new Pokemon(summary2);

            _pokemons.Add(pokemon);
            _pokemons.Add(pokemon1);
            _pokemons.Add(pokemon2);
        }

        FillButtonList();

        _btns[selectedIdx].ToggleSelected(true);
    }

    void Update()
    {
        switch (sceneState)
        {
            case PokemonListSceneState.NON_SELECTED:
                ChooseAction();
                break;
            case PokemonListSceneState.SELECTED:
                ((PokemonListScene)scene).PokemonListSelectMenu.ChooseAction();
                break;
        }
    }

    void FillButtonList()
    {
        int queueIdx = 0;

        foreach (Pokemon pokemon in _pokemons)
        {
            PokemonCard card;

            if (queueIdx == 0)
            {
                card = Managers.Resource.Instantiate("UI/PokemonList/MainPokemonCard").GetComponent<PokemonCard>();
                card.transform.SetParent(mainPokemonCardZone);
            }
            else
            {
                card = Managers.Resource.Instantiate("UI/PokemonList/SubPokemonCard").GetComponent<PokemonCard>();
                card.transform.SetParent(subPokemonCardZone[queueIdx - 1]);
            }

            Texture2D image = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{pokemon.PokemonSummary.Info.PokemonName}");

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector2.one;

            card.ApplyImage(image);
            card.ApplyPokemonInfo(pokemon.PokemonSummary.Info.NickName,
                pokemon.PokemonSummary.Skill.Stat.Hp,
                pokemon.PokemonSummary.Skill.Stat.MaxHp,
                pokemon.PokemonSummary.Info.Level);

            _btns.Add(card);

            queueIdx++;
        }

        _btns.Add(cancelBtn);
    }

    public override void ChooseAction()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _btns[selectedIdx].ToggleSelected(false);
            selectedIdx++;

            if (selectedIdx == _btns.Count)
            {
                selectedIdx = _btns.Count - 1;
            }
            _btns[selectedIdx].ToggleSelected(true);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _btns[selectedIdx].ToggleSelected(false);
            selectedIdx--;

            if (selectedIdx < 0)
            {
                selectedIdx = 0;
            }
            _btns[selectedIdx].ToggleSelected(true);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            PokemonListScene pokemonList = scene as PokemonListScene;

            if (selectedIdx != _btns.Count - 1)
            {
                _selectedPokemon = _pokemons[selectedIdx];
                sceneState = PokemonListSceneState.SELECTED;
                ((PokemonListScene)scene).TogglePokemonListSelectMenu(true);
            }
            else
            {
                Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.Game);
            }
        }
    }
}
