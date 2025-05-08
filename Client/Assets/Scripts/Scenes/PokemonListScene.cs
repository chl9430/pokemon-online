using UnityEngine;

public class PokemonListScene : BaseScene
{
    [SerializeField] SelectBoxUI _selectBoxUI;

    public SelectBoxUI SelectBoxUI
    {
        get { return _selectBoxUI; }
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.PokemonList;
    }

    public override void Clear()
    {
    }

    public void ToggleSelectBoxUI(bool toggle)
    {
        _selectBoxUI.gameObject.SetActive(toggle);
    }
}
