using Google.Protobuf.Protocol;
using UnityEngine;

public class SelectBoxUI : Action_UI
{
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
                Pokemon pokemon = _pokemonListUI.GetSelectedPokemon();

                C_AccessPokemonSummary accessPacket = new C_AccessPokemonSummary();
                accessPacket.PlayerId = Managers.Object.MyPlayer.Id;
                accessPacket.PkmDicNum = pokemon.PokemonSummary.Info.DictionaryNum;

                Managers.Network.SavePacket(accessPacket);

                Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.PokemonSummary);
            }
            else if (selectedIdx == 1)
            {
                ((PokemonListScene)scene).ToggleSelectBoxUI(false);
                _pokemonListUI.SceneState = PokemonListSceneState.CHOOSE_POKEMON_TO_SWITCH;
            }
            else if (selectedIdx == 2)
            {
            }
            else if (selectedIdx == 3)
            {
                ((PokemonListScene)scene).ToggleSelectBoxUI(false);
                _pokemonListUI.SceneState = PokemonListSceneState.NON_SELECTED;
            }
        }
    }
}
