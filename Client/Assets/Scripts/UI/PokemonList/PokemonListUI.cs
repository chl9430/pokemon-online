using Google.Protobuf.Protocol;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum PokemonListSceneState
{
    NON_SELECTED = 0,
    SELECTED = 1,
    CHOOSE_POKEMON_TO_SWITCH = 2,
    SWITCHING_POKEMON = 3,
    FINISING_SWITCHING_POKEMON = 4,
    FINISING_MOVING_POKEMON = 5,
}

public class PokemonListUI : Action_UI
{
    public int switchPokemonIdx;
    PokemonListSceneState sceneState;

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
        /*
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
        */

        _pokemons = Managers.Object._pokemons;

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
                ((PokemonListScene)scene).SelectBoxUI.ChooseAction();
                break;
            case PokemonListSceneState.CHOOSE_POKEMON_TO_SWITCH:
                ChoosePokemonToSwitch();
                break;
            case PokemonListSceneState.SWITCHING_POKEMON:
                ((PokemonCard)_btns[switchPokemonIdx]).MoveCard(3f);
                ((PokemonCard)_btns[selectedIdx]).MoveCard(3f);
                break;
            case PokemonListSceneState.FINISING_SWITCHING_POKEMON:
                SwitchPokemon();
                break;
            case PokemonListSceneState.FINISING_MOVING_POKEMON:
                ((PokemonCard)_btns[switchPokemonIdx]).MoveBackCard(3f);
                ((PokemonCard)_btns[selectedIdx]).MoveBackCard(3f);
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

            card.PokemonListUI = this;

            Texture2D image = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{pokemon.PokemonSummary.Info.PokemonName}_Icon");

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
                switchPokemonIdx = selectedIdx;
                sceneState = PokemonListSceneState.SELECTED;
                ((PokemonListScene)scene).ToggleSelectBoxUI(true);
            }
            else
            {
                Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.Game);
            }
        }
    }

    public void ChoosePokemonToSwitch()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _btns[selectedIdx].ToggleSelected(false);
            selectedIdx++;

            if (selectedIdx == _btns.Count - 1)
            {
                selectedIdx = _btns.Count - 2;
            }
            _btns[selectedIdx].ToggleSelected(true);
            _btns[switchPokemonIdx].ToggleSelected(true);
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
            _btns[switchPokemonIdx].ToggleSelected(true);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (selectedIdx == switchPokemonIdx)
            {
                sceneState = PokemonListSceneState.NON_SELECTED;
                return;
            }

            C_SwitchPokemon switchPacket = new C_SwitchPokemon();
            switchPacket.OwnerId = Managers.Object.myPlayerObjInfo.ObjectId;
            switchPacket.PokemonFromIdx = switchPokemonIdx;
            switchPacket.PokemonToIdx = selectedIdx;

            Managers.Network.Send(switchPacket);

            int switchPokemonCardDir = 1;
            int selectedPokemonCardDir = 1;

            if (switchPokemonIdx == 0)
                switchPokemonCardDir = -1;
            if (selectedIdx == 0)
                selectedPokemonCardDir = -1;

            ((PokemonCard)_btns[switchPokemonIdx]).SetOldAndNewPos(switchPokemonCardDir);
            ((PokemonCard)_btns[selectedIdx]).SetOldAndNewPos(selectedPokemonCardDir);
            sceneState = PokemonListSceneState.SWITCHING_POKEMON;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            sceneState = PokemonListSceneState.NON_SELECTED;
            ((PokemonCard)_btns[switchPokemonIdx]).ToggleSelected(false);
            ((PokemonCard)_btns[selectedIdx]).ToggleSelected(true);
        }
    }

    void SwitchPokemon()
    {
        Texture2D fromImg = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{_pokemons[switchPokemonIdx].PokemonSummary.Info.PokemonName}");
        Texture2D Toimg = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{_pokemons[selectedIdx].PokemonSummary.Info.PokemonName}");

        ((PokemonCard)_btns[switchPokemonIdx]).ApplyImage(Toimg);
        ((PokemonCard)_btns[switchPokemonIdx]).ApplyPokemonInfo(_pokemons[selectedIdx].PokemonSummary.Info.NickName,
                _pokemons[selectedIdx].PokemonSummary.Skill.Stat.Hp,
                _pokemons[selectedIdx].PokemonSummary.Skill.Stat.MaxHp,
                _pokemons[selectedIdx].PokemonSummary.Info.Level);

        ((PokemonCard)_btns[selectedIdx]).ApplyImage(fromImg);
        ((PokemonCard)_btns[selectedIdx]).ApplyPokemonInfo(_pokemons[switchPokemonIdx].PokemonSummary.Info.NickName,
                _pokemons[switchPokemonIdx].PokemonSummary.Skill.Stat.Hp,
                _pokemons[switchPokemonIdx].PokemonSummary.Skill.Stat.MaxHp,
                _pokemons[switchPokemonIdx].PokemonSummary.Info.Level);

        int switchPokemonCardDir = -1;
        int selectedPokemonCardDir = -1;

        if (switchPokemonIdx == 0)
            switchPokemonCardDir = 1;
        if (selectedIdx == 0)
            selectedPokemonCardDir = 1;

        ((PokemonCard)_btns[switchPokemonIdx]).SetOldAndNewPos(switchPokemonCardDir);
        ((PokemonCard)_btns[selectedIdx]).SetOldAndNewPos(selectedPokemonCardDir);

        sceneState = PokemonListSceneState.FINISING_MOVING_POKEMON;
        ((PokemonCard)_btns[switchPokemonIdx]).ToggleSelected(false);

        // 리스트 수정
        Pokemon pokemon = _pokemons[switchPokemonIdx];

        _pokemons[switchPokemonIdx] = _pokemons[selectedIdx];
        _pokemons[selectedIdx] = pokemon;
    }

    public Pokemon GetSelectedPokemon()
    {
        return _pokemons[selectedIdx];
    }
}
