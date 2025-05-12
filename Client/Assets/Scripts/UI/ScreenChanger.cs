using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenChanger : MonoBehaviour
{
    public Image fadeImage; // 페이드 이미지를 드래그하여 할당
    public float fadeDuration = 1f;

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
