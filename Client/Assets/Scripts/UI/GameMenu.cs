using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class GameMenu : MonoBehaviour
{
    [SerializeField] int curMenuNum = 0;
    [SerializeField] int gameMenuCnt;

    List<GameMenuItem> menuItems;

    void Start()
    {
        menuItems = new List<GameMenuItem>();

        curMenuNum = 0;

        foreach (Transform child in gameObject.transform)
        {
            menuItems.Add(child.GetComponent<GameMenuItem>());
            gameMenuCnt++;
        }

        menuItems[curMenuNum].ToggleArrow(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            menuItems[curMenuNum].ToggleArrow(false);
            curMenuNum++;

            if (curMenuNum == gameMenuCnt)
            {
                curMenuNum = 0;
            }

            menuItems[curMenuNum].ToggleArrow(true);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            menuItems[curMenuNum].ToggleArrow(false);
            curMenuNum--;

            if (curMenuNum < 0)
            {
                curMenuNum = gameMenuCnt - 1;
            }

            menuItems[curMenuNum].ToggleArrow(true);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (curMenuNum == 0)
            {
                Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.PokemonList);
            }
        }
    }
}
