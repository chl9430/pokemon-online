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

    [SerializeField] GameMenuContent _gameMenuContent;
    [SerializeField] PokemonListContent _pokemonListContent;
    [SerializeField] BagContent _bagContent;

    [SerializeField] PokemonSummaryContent _pokemonSummaryContent;
    [SerializeField] MoveSelectionContent _moveSelectionContent;
    [SerializeField] PokemonEvolutionContent _pokemonEvolutionContent;
    [SerializeField] ScriptBoxUI _scriptBox;
    [SerializeField] ScreenEffecter _screenEffecter;

    public PokemonListContent PokemonListContent { get { return  _pokemonListContent; } }
    public BagContent BagContent { get { return _bagContent; } }
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

    public void LoadUIPrefabs()
    {
        _gameMenuContent = Managers.Resource.Instantiate("UI/GameScene/GameMenu", _canvas.transform).GetComponent<GameMenuContent>();
        _pokemonListContent = Managers.Resource.Instantiate("UI/GameScene/PokemonSelectArea", _canvas.transform).GetComponent<PokemonListContent>();
        _bagContent = Managers.Resource.Instantiate("UI/GameScene/Bag", _canvas.transform).GetComponent<BagContent>();
        _pokemonSummaryContent = Managers.Resource.Instantiate("UI/GameScene/PokemonSummary", _canvas.transform).GetComponent<PokemonSummaryContent>();
        _pokemonEvolutionContent = Managers.Resource.Instantiate("UI/GameScene/PokemonEvolution", _canvas.transform).GetComponent<PokemonEvolutionContent>();
        _moveSelectionContent = Managers.Resource.Instantiate("UI/GameScene/MoveSelection", _canvas.transform).GetComponent<MoveSelectionContent>();
    }

    public void SetGameMenu()
    {
        // 메뉴 버튼 데이터 채우기
        _gameMenuContent.SetMenuButtons();
    }

    //public void SetBag(Dictionary<ItemCategory, List<Item>> items)
    //{
    //    // 가방 리스트 데이터 채우기
    //    _bagContent.SetBagItems(items);
    //}

    public void OpenGameMenu()
    {
        Managers.Scene.CurrentScene.ContentStack.Push(_gameMenuContent);
        Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
    }

    public void OpenPokemonList(List<Pokemon> pokemons, List<string> actionBtnNames)
    {
        Managers.Scene.CurrentScene.ContentStack.Push(_pokemonListContent);
        _pokemonListContent.SetPokemonSelectArea(pokemons, actionBtnNames);
        Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
    }

    public void OpenBag(Dictionary<ItemCategory, List<Item>> items)
    {
        Managers.Scene.CurrentScene.ContentStack.Push(_bagContent);
        _bagContent.SetBagItems(items);
        Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
    }

    public void OpenPokemonSum(Pokemon pokemon)
    {
        _pokemonSummaryContent.SetPokemonSummary(pokemon);

        Managers.Scene.CurrentScene.ContentStack.Push(_pokemonSummaryContent);
        Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction(pokemon);
    }

    public void OpenMoveSelection(Pokemon expPokemon, PokemonMove newMove)
    {
        _moveSelectionContent.SetMoveSelectionScene(expPokemon, newMove);

        Managers.Scene.CurrentScene.ContentStack.Push(_moveSelectionContent);
        Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
    }

    public void OpenPokemonEvolution(Pokemon evolvePokemon, string evolutionPokemonName)
    {
        _pokemonEvolutionContent.SetEvolutionPokemon(evolvePokemon, evolutionPokemonName);

        Managers.Scene.CurrentScene.ContentStack.Push(_pokemonEvolutionContent);
        Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
    }

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
