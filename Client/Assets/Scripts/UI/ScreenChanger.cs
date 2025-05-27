using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenChanger : MonoBehaviour
{
    BaseScene _scene;
    PlayableDirector _director;

    public Image fadeImage;
    public float fadeDuration = 1f;

    void Start()
    {
        _director = GetComponent<PlayableDirector>();
        
        if (_director != null)
            _director.stopped += OnTimelineStopped;

        _scene = Managers.Scene.CurrentScene;
    }

    public void OnTimelineStopped(PlayableDirector aDirector)
    {
        Debug.Log("배틀 씬으로 이동");
        _scene.DoNextActionWithTimeline();
    }

    public void FadeInScene()
    {
        StartCoroutine(FadeIn());
    }

    public void ChangeAndFadeOutScene(Define.Scene type)
    {
        StartCoroutine(FadeOut(type));
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0;
        fadeImage.color = color;
        Managers.Scene.CurrentScene.AfterFadeInAction();
    }

    private IEnumerator FadeOut(Define.Scene type)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        Managers.Scene.LoadScene(type);
    }
}
