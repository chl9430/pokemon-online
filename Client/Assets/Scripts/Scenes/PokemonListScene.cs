using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public class PokemonListScene : BaseScene
{
    int selectedIdx;
    PokemonList _pokemonList;
    List<PokemonCard> _cards;
    PriorityQueue<Pokemon> _pokemons;

    [SerializeField]
    Transform mainCardZone;
    [SerializeField]
    Transform[] subCardZone;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.PokemonList;

        ScreenChanger.FadeInScene();
    }

    void Start()
    {
        //_pokemonList = Managers.Object.MyPlayer.GetComponent<PokemonList>();
        //_cards = new List<PokemonCard>();
        //_pokemons = _pokemonList.Pokemons;

        {
            _pokemons = new PriorityQueue<Pokemon>();

            Pokemon pokemon1 = new Pokemon("MESSI", 5, 1, 0, gameObject, new PokemonFinalStatInfo()
            {
                PokemonName = "Pikachu",
                MaxHp = 10,
                Attack = 5,
                Defense = 5,
                SpecialAttack = 7,
                SpecialDefense = 6,
                Speed = 8,
            });

            Pokemon pokemon2 = new Pokemon("VILLA", 5, 1, 0, gameObject, new PokemonFinalStatInfo()
            {
                PokemonName = "Squirtle",
                MaxHp = 15,
                Attack = 5,
                Defense = 7,
                SpecialAttack = 9,
                SpecialDefense = 6,
                Speed = 6,
            });

            Pokemon pokemon3 = new Pokemon("PEDRO", 5, 1, 0, gameObject, new PokemonFinalStatInfo()
            {
                PokemonName = "Charmander",
                MaxHp = 12,
                Attack = 6,
                Defense = 5,
                SpecialAttack = 9,
                SpecialDefense = 6,
                Speed = 10,
            });

            _pokemons.Push(pokemon1);
            _pokemons.Push(pokemon2);
            _pokemons.Push(pokemon3);
        }

        int queueIdx = 0;

        foreach (Pokemon pokemon in _pokemons)
        {
            PokemonCard card;

            if (queueIdx == 0)
            {
                card = Managers.Resource.Instantiate("UI/MainPokemonCard").GetComponent<PokemonCard>();
                card.transform.SetParent(mainCardZone);
            }
            else
            {
                card = Managers.Resource.Instantiate("UI/SubPokemonCard").GetComponent<PokemonCard>();
                card.transform.SetParent(subCardZone[queueIdx - 1]);
            }

            Texture2D image = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{pokemon.FinalStatInfo.PokemonName}");

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            card.ApplyImage(image);
            card.ApplyPokemonInfo(pokemon.NickName, pokemon.Hp, pokemon.FinalStatInfo.MaxHp, pokemon.Level);

            queueIdx++;
        }
    }

    public override void Clear()
    {

    }
}
