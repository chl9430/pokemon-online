using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Google.Protobuf.Protocol;

public class PokeBall : MonoBehaviour
{
    float _throwDuration = 1.0f;
    float _rotationAmount = 1080f;
    float _jumpHeight = 300f;
    BaseScene _scene;
    RectTransform _rt;
    Image _img;

    [SerializeField] Color finalColor = Color.black;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _scene = Managers.Scene.CurrentScene;
        _img = GetComponent<Image>();
    }

    public void ThrowTheBall(Vector3 targetPos, float throwDuration = 1.0f, float rotationAmt = 1080f, float jumpHeight = 300f)
    {
        _throwDuration = throwDuration;
        _rotationAmount = rotationAmt;
        _jumpHeight = jumpHeight;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt.parent.GetComponent<RectTransform>(),
            targetPos,
            null,
            out Vector2 localUIPosition);

        ThrowObject(localUIPosition);
    }

    public void ThrowObject(Vector2 localUIPosition)
    {
        // 1. DOJumpAnchorPos로 포물선 이동
        _rt.DOJumpAnchorPos(localUIPosition, _jumpHeight, 1, _throwDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            _scene.DoNextAction();
        });

        // 2. DORotate로 회전 효과 추가
        _rt.DORotate(new Vector3(0, 0, _rotationAmount), _throwDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear);
    }

    public void SetBallImage(string ballName)
    {
        Texture2D img = Managers.Resource.Load<Texture2D>($"Textures/Item/PokeBall/{ballName}_Battle");

        _img.sprite = Sprite.Create(img, new Rect(0, 0, img.width, img.height), Vector2.one * 0.5f);
        _img.SetNativeSize();
    }

    public void FailBallShake(int shakeCnt)
    {
        Sequence mySequence = DOTween.Sequence();

        for (int i = 0; i < shakeCnt; i++)
        {
            // 좌측으로 흔들기
            mySequence.Append(_rt.DORotate(new Vector3(0, 0, 15f), 0.5f / 2f).SetEase(Ease.InOutSine));

            // 우측으로 흔들기
            mySequence.Append(_rt.DORotate(new Vector3(0, 0, -15f), 0.5f).SetEase(Ease.InOutSine));

            // 원래 위치로 복귀
            mySequence.Append(_rt.DORotate(Vector3.zero, 0.5f / 2).SetEase(Ease.InOutSine));

            // 텀을 준다.
            mySequence.AppendInterval(0.5f);
        }

        // 시퀀스 완료 시 특정 동작 실행
        mySequence.OnComplete(() => {
            _scene.DoNextAction();
            Destroy(gameObject);
        });
    }

    public void SuccessBallShake()
    {
        Sequence mySequence = DOTween.Sequence();

        for (int i = 0; i < 3; i++)
        {
            // 좌측으로 흔들기
            mySequence.Append(_rt.DORotate(new Vector3(0, 0, 15f), 0.5f / 2f).SetEase(Ease.InOutSine));

            // 우측으로 흔들기
            mySequence.Append(_rt.DORotate(new Vector3(0, 0, -15f), 0.5f).SetEase(Ease.InOutSine));

            // 원래 위치로 복귀
            mySequence.Append(_rt.DORotate(Vector3.zero, 0.5f / 2).SetEase(Ease.InOutSine));

            // 텀을 준다.
            mySequence.AppendInterval(0.5f);
        }

        // 볼 색상을 검은색으로 변경
        mySequence.Append(_img.DOColor(finalColor, 0.5f)
            .SetEase(Ease.Linear));

        // 시퀀스 완료 시 특정 동작 실행
        mySequence.OnComplete(() => {
            _scene.DoNextAction();
        });
    }
}
