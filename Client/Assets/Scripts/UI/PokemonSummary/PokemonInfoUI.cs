using UnityEngine;

public enum PokemonSummarySceneState
{
    SLIDE_NOT_MOVING = 0,
    SLIDE_MOVING = 1
}

public class PokemonInfoUI : Action_UI
{
    public int selectedIdx;
    float startTime;
    PokemonSummarySceneState sceneState;
    Vector2 oldMinPos;
    Vector2 oldMaxPos;
    Vector2 newMinPos;
    Vector2 newMaxPos;
    RectTransform selectedContent;

    [SerializeField] float slideSpeed;
    [SerializeField] RectTransform[] sliderContent;

    void Update()
    {
        switch (sceneState)
        {
            case PokemonSummarySceneState.SLIDE_NOT_MOVING:
                ChooseAction();
                break;
            case PokemonSummarySceneState.SLIDE_MOVING:
                MoveSlideContent();
                break;
        }
    }

    public override void ChooseAction()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (selectedIdx == sliderContent.Length - 1)
                return;

            selectedIdx++;

            SetSelectedSlideContent(-1);

            //indicator.anchorMax = new Vector2(1f / sliderContent.Length * (curItemNum + 1), 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (selectedIdx == 0)
                return;

            SetSelectedSlideContent(1);

            selectedIdx--;

            //indicator.anchorMax = new Vector2(1f / sliderContent.Length * (curItemNum + 1), 1);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.PokemonList);
        }
    }

    void SetSelectedSlideContent(int dir)
    {
        sceneState = PokemonSummarySceneState.SLIDE_MOVING;
        startTime = Time.time;
        oldMinPos = sliderContent[selectedIdx].anchorMin;
        oldMaxPos = sliderContent[selectedIdx].anchorMax;

        float minX = oldMinPos.x + dir;
        float maxX = oldMaxPos.x + dir;

        newMinPos = new Vector2(minX, oldMinPos.y);
        newMaxPos = new Vector2(maxX, oldMaxPos.y);
        selectedContent = sliderContent[selectedIdx];
    }

    void MoveSlideContent()
    {
        float timeElapsed = Time.time - startTime;
        float t = Mathf.Clamp01(timeElapsed * slideSpeed);

        selectedContent.anchorMin = Vector2.Lerp(oldMinPos, newMinPos, t);
        selectedContent.anchorMax = Vector2.Lerp(oldMaxPos, newMaxPos, t);

        if (t >= 1f)
        {
            sceneState = PokemonSummarySceneState.SLIDE_NOT_MOVING;
            selectedContent.anchorMin = newMinPos;
            selectedContent.anchorMax = newMaxPos;
        }
    }
}
