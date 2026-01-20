using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public class GameContentManager : MonoBehaviour
{
    public static GameContentManager Instance { get; private set; }

    Define.Scene _nextUIType;
    IMessage _nextPacket;

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

    public void OpenNextUI()
    {
        ContentManager.Instance.ScreenEffecter.SetFadeIn();

        if (_nextUIType == Define.Scene.PokemonList)
        {
            Managers.Scene.CurrentScene.ContentStack.Push(_pokemonListContent);
            Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
        }
        else if (_nextUIType == Define.Scene.Bag)
        {
            Managers.Scene.CurrentScene.ContentStack.Push(_bagContent);
            Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
        }
        else if (_nextUIType == Define.Scene.PokemonSummary)
        {
            Managers.Scene.CurrentScene.ContentStack.Push(_pokemonSummaryContent);
            Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
        }
        else if (_nextUIType == Define.Scene.MoveSelection)
        {
            Managers.Scene.CurrentScene.ContentStack.Push(_moveSelectionContent);
            Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
        }
        else if (_nextUIType == Define.Scene.Evolution)
        {
            Managers.Scene.CurrentScene.ContentStack.Push(_pokemonEvolutionContent);
            Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
        }
    }

    public void OpenGameMenu()
    {
        Managers.Scene.CurrentScene.ContentStack.Push(_gameMenuContent);
        Managers.Scene.CurrentScene.ContentStack.Peek().SetNextAction();
    }

    public void OpenPokemonList(List<Pokemon> pokemons, List<string> actionBtnNames, string effectName)
    {
        _nextUIType = Define.Scene.PokemonList;

        ContentManager.Instance.ScreenEffecter.PlayEffect(effectName);
        ContentManager.Instance.ScreenEffecter.SetMoveSceneType(MoveSceneType.OpenUI);

        _pokemonListContent.SetPokemonSelectArea(pokemons, actionBtnNames);
    }

    public void OpenBag(Dictionary<ItemCategory, List<Item>> items, string effectName)
    {
        _nextUIType = Define.Scene.Bag;

        ContentManager.Instance.ScreenEffecter.PlayEffect(effectName);
        ContentManager.Instance.ScreenEffecter.SetMoveSceneType(MoveSceneType.OpenUI);

        _bagContent.SetBagItems(items);
    }

    public void OpenPokemonSum(Pokemon pokemon, string effectName)
    {
        _nextUIType = Define.Scene.PokemonSummary;

        ContentManager.Instance.ScreenEffecter.PlayEffect(effectName);
        ContentManager.Instance.ScreenEffecter.SetMoveSceneType(MoveSceneType.OpenUI);

        _pokemonSummaryContent.SetPokemonSummary(pokemon);
    }

    public void OpenMoveSelection(Pokemon expPokemon, PokemonMove newMove, string effectName)
    {
        _nextUIType = Define.Scene.MoveSelection;

        ContentManager.Instance.ScreenEffecter.PlayEffect(effectName);
        ContentManager.Instance.ScreenEffecter.SetMoveSceneType(MoveSceneType.OpenUI);

        _moveSelectionContent.SetMoveSelectionScene(expPokemon, newMove);
    }

    public void OpenPokemonEvolution(Pokemon evolvePokemon, string evolutionPokemonName, string effectName)
    {
        _nextUIType = Define.Scene.Evolution;

        ContentManager.Instance.ScreenEffecter.PlayEffect(effectName);
        ContentManager.Instance.ScreenEffecter.SetMoveSceneType(MoveSceneType.OpenUI);

        _pokemonEvolutionContent.SetEvolutionPokemon(evolvePokemon, evolutionPokemonName);
    }

    public void CloseCurUI(string effectName, IMessage nextPacket = null)
    {
        if (nextPacket != null)
            _nextPacket = nextPacket;

        ContentManager.Instance.ScreenEffecter.PlayEffect(effectName);
        ContentManager.Instance.ScreenEffecter.SetMoveSceneType(MoveSceneType.CloseUI);
    }

    public void PopCurUI()
    {
        if (_nextUIType == Define.Scene.PokemonList)
        {
            Managers.Scene.CurrentScene.FinishContents(false);

            if (_nextPacket is C_SwitchPokemon)
            {
                Managers.Scene.CurrentScene.PopUntilSpecificChild<OnlineBattleContent>();

                Managers.Network.Send(_nextPacket);
            }
        }
        else if (_nextUIType == Define.Scene.Bag)
        {
            Managers.Scene.CurrentScene.FinishContents(false);
        }
    }
}
