using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonListSelectMenu : MonoBehaviour
{
    int selectedIdx;
    BaseScene scene;

    [SerializeField] PokemonListUI _pokemonListUI;
    [SerializeField] ArrowButton[] _btns;

    void Start()
    {
        scene = Managers.Scene.CurrentScene;
        _btns[selectedIdx].ToggleArrow(true);
    }

    public void ChooseAction()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _btns[selectedIdx].ToggleArrow(false);
            selectedIdx++;

            if (selectedIdx == _btns.Length)
            {
                selectedIdx = _btns.Length - 1;
            }
            _btns[selectedIdx].ToggleArrow(true);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _btns[selectedIdx].ToggleArrow(false);
            selectedIdx--;

            if (selectedIdx < 0)
            {
                selectedIdx = 0;
            }
            _btns[selectedIdx].ToggleArrow(true);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (selectedIdx == 0)
            {
                Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.PokemonSummary);
            }
            else if (selectedIdx == 1)
            { 
            }
            else if (selectedIdx == 2)
            {
            }
            else if (selectedIdx == 3)
            {
                ((PokemonListScene)scene).TogglePokemonListSelectMenu(false);
                _pokemonListUI.SceneState = PokemonListSceneState.NON_SELECTED;
            }
        }
    }
}
