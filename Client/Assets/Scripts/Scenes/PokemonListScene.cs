using UnityEngine;

public class PokemonListScene : BaseScene
{
    [SerializeField] PokemonListSelectMenu _pokemonListSelectMenu;

    public PokemonListSelectMenu PokemonListSelectMenu
    {
        get { return _pokemonListSelectMenu; }
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.PokemonList;
    }

    public override void Clear()
    {
    }

    public void TogglePokemonListSelectMenu(bool toggle)
    {
        _pokemonListSelectMenu.gameObject.SetActive(toggle);
    }
}
