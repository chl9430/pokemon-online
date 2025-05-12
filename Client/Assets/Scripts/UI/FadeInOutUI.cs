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
            yield return null; // ���� �����ӱ��� ���
        }

        _canvasGroup.alpha = alpha;
        // ���������� ���� ���� 0���� ���� (��Ȯ���� ����)
        // �ʿ��ϴٸ� ������Ʈ�� ��Ȱ��ȭ�ϰų� �����ϴ� ���� �߰� �۾� ����
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
