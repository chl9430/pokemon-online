using UnityEngine;

public class PokemonListScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.PokemonList;

        ScreenChanger.FadeInScene();
    }

    public override void Clear()
    {

    }
}
