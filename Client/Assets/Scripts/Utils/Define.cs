using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Intro,
        Game,
        PokemonList,
        PokemonSummary,
        Battle,
        Bag
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Drag,
    }

    public enum InputSelectBoxEvent
    {
        NONE,
        SELECT,
        BACK,
    }
}
