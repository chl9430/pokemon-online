using Google.Protobuf.Protocol;
using UnityEngine;

public class GameMenuUI : Action_UI
{
    [SerializeField] ArrowButton[] _btns;

    void Start()
    {
        scene = Managers.Scene.CurrentScene;
        _btns[selectedIdx].ToggleArrow(true);
    }

    void Update()
    {
        ChooseAction();
    }

    public override void ChooseAction()
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
                Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.PokemonList);
            }
            else if (selectedIdx == 1)
            {
                ((GameScene)scene).ToggleGameMenu(false);
                Managers.Object.MyPlayer.State = CreatureState.Idle;
            }
        }
    }
}
