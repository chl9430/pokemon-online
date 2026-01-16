using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContentManager : MonoBehaviour
{
    public static ContentManager Instance { get; private set; }

    Canvas _canvas;
    ScreenEffecter _screenEffecter;

    [SerializeField] ScriptBoxUI _scriptBox;

    public ScriptBoxUI ScriptBox { get { return _scriptBox; } }
    public ScreenEffecter ScreenEffecter { get { return _screenEffecter; } }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _canvas = Util.FindChild<Canvas>(gameObject);

        _screenEffecter = Managers.Resource.Instantiate("UI/Fading", _canvas.transform).GetComponent<ScreenEffecter>();
    }

    //public void SetBag(Dictionary<ItemCategory, List<Item>> items)
    //{
    //    // 가방 리스트 데이터 채우기
    //    _bagContent.SetBagItems(items);
    //}

    //public void OpenPokemonList(List<Pokemon> pokemons, List<string> actionBtnNames)
    //{
    //    Managers.Scene.CurrentScene.ContentStack.Push(_pokemonListContent);
    //    _pokemonListContent.SetPokemonSelectArea(pokemons, actionBtnNames);
    //    Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
    //}

    //public void OpenBag(Dictionary<ItemCategory, List<Item>> items)
    //{
    //    Managers.Scene.CurrentScene.ContentStack.Push(_bagContent);
    //    _bagContent.SetBagItems(items);
    //    Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
    //}

    //public void OpenPokemonSum(Pokemon pokemon)
    //{
    //    _pokemonSummaryContent.SetPokemonSummary(pokemon);

    //    Managers.Scene.CurrentScene.ContentStack.Push(_pokemonSummaryContent);
    //    Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction(pokemon);
    //}

    //public void OpenMoveSelection(Pokemon expPokemon, PokemonMove newMove)
    //{
    //    _moveSelectionContent.SetMoveSelectionScene(expPokemon, newMove);

    //    Managers.Scene.CurrentScene.ContentStack.Push(_moveSelectionContent);
    //    Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
    //}

    //public void OpenPokemonEvolution(Pokemon evolvePokemon, string evolutionPokemonName)
    //{
    //    _pokemonEvolutionContent.SetEvolutionPokemon(evolvePokemon, evolutionPokemonName);

    //    Managers.Scene.CurrentScene.ContentStack.Push(_pokemonEvolutionContent);
    //    Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
    //}

    public void BeginScriptTyping(List<string> scripts, bool autoSkip = false, float autoSkipTime = 1, bool isStatic = false)
    {
        _scriptBox.BeginScriptTyping(scripts, autoSkip, autoSkipTime, isStatic);
    }

    public void PlayScreenEffecter(string animName)
    {
        if (animName == "FadeOut")
        {
            EnrollScreenEffecter("UI/Fading");
        }
        else if (animName == "PokemonAppear")
        {
            EnrollScreenEffecter("UI/GameScene/PokemonAppearEffect");
        }

        _screenEffecter.PlayEffect(animName);
    }

    void EnrollScreenEffecter(string effecterPath)
    {
        ScreenEffecter newEffecter = Managers.Resource.Instantiate(effecterPath, _canvas.transform).GetComponent<ScreenEffecter>();

        if (_screenEffecter != null)
            Destroy(_screenEffecter.gameObject);

        _screenEffecter = newEffecter;
    }
}
