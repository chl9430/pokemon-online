using System.Collections;
using UnityEngine;

public class FadeInOutUI : MonoBehaviour
{
    CanvasGroup _canvasGroup;
    BaseScene _scene;

    [SerializeField] float _fadeDuration = 1f;

    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        _scene = Managers.Scene.CurrentScene;
    }

    private IEnumerator ChangeAlphaCoroutine(float alpha)
    {
        float startAlpha = _canvasGroup.alpha;
        float timer = 0f;

        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, alpha, timer / _fadeDuration);
            _canvasGroup.alpha = currentAlpha;
            yield return null; // 다음 프레임까지 대기
        }

        _canvasGroup.alpha = alpha;
        // 최종적으로 알파 값을 0으로 설정 (정확성을 위해)
        // 필요하다면 오브젝트를 비활성화하거나 삭제하는 등의 추가 작업 수행
        _scene.DoNextAction();

        if (alpha == 0)
            gameObject.SetActive(false);
        // Destroy(gameObject);
    }

    public void ChangeUIAlpha(float alpha)
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_scene == null)
            _scene = Managers.Scene.CurrentScene;

        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        StartCoroutine(ChangeAlphaCoroutine(alpha));
    }
}
