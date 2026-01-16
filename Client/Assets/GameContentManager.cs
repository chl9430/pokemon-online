using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public class GameContentManager : MonoBehaviour
{
    public static GameContentManager Instance { get; private set; }

    [SerializeField] GameMenuContent _gameMenuContent;
    [SerializeField] PokemonListContent _pokemonListContent;
    [SerializeField] BagContent _bagContent;

    [SerializeField] PokemonSummaryContent _pokemonSummaryContent;
    [SerializeField] MoveSelectionContent _moveSelectionContent;
    [SerializeField] PokemonEvolutionContent _pokemonEvolutionContent;

    public PokemonListContent PokemonListContent { get { return _pokemonListContent; } }
    public BagContent BagContent { get { return _bagContent; } }

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

    public void SetGameMenu()
    {
        // 메뉴 버튼 데이터 채우기
        _gameMenuContent.SetMenuButtons();
    }

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
}
