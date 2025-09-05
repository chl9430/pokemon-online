using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PokemonCardState
{
    NONE = 0,
    MOVING = 1,
    MOVING_BACK = 2,
}

public class PokemonCard : MonoBehaviour
{
    int _dir;
    float _speed;
    float startTime;
    Vector2 oldMinPos;
    Vector2 oldMaxPos;
    Vector2 newMinPos;
    Vector2 newMaxPos;
    PokemonCardState _state = PokemonCardState.NONE;
    RectTransform _rt;
    DynamicButton _dynamicButton;
    BaseScene _scene;
    PokemonListSelectArea _pokemonSelectingZone;

    [SerializeField] Image _pokemonImg;
    [SerializeField] Image _pokemonStatusImg;
    [SerializeField] TextMeshProUGUI _pokemonNickname;
    [SerializeField] TextMeshProUGUI _pokemonLevel;
    [SerializeField] Image _pokemonGenderImg;
    [SerializeField] TextMeshProUGUI _pokemonHp;

    void Start()
    {
        _rt = GetComponent<RectTransform>();
        _dynamicButton = GetComponent<DynamicButton>();
        _scene = Managers.Scene.CurrentScene;
        _pokemonSelectingZone = GetComponentInParent<PokemonListSelectArea>();
    }

    void Update()
    {
        switch (_state)
        {
            case PokemonCardState.MOVING:
                    MoveCard();
                break;
            case PokemonCardState.MOVING_BACK:
                    MoveBackCard();
                break;
        }
    }

    public void FillPokemonCard(PokemonSummary pokemonSum)
    {
        Texture2D image = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{pokemonSum.PokemonInfo.PokemonName}_Icon");

        _pokemonImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonImg.SetNativeSize();

        image = Managers.Resource.Load<Texture2D>($"Textures/UI/{pokemonSum.PokemonInfo.PokemonStatus}_Icon");

        if (image == null)
        {
            Color color = _pokemonStatusImg.color;
            color.a = 0f;

            _pokemonStatusImg.color = color;
        }
        else
        {
            Color color = _pokemonStatusImg.color;
            color.a = 1f;

            _pokemonStatusImg.color = color;

            _pokemonStatusImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
            _pokemonImg.SetNativeSize();
        }

        image = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/PokemonGender_{pokemonSum.PokemonInfo.Gender}");

        _pokemonGenderImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonImg.SetNativeSize();

        _pokemonNickname.text = pokemonSum.PokemonInfo.NickName;
        _pokemonHp.text = $"HP : {pokemonSum.PokemonStat.Hp.ToString()} / {pokemonSum.PokemonStat.MaxHp.ToString()}";
        _pokemonLevel.text = $"Lv.{pokemonSum.PokemonInfo.Level}";
    }

    public void SetDirection(int dir, float speed)
    {
        startTime = Time.time;
        _dir = dir;
        _speed = speed;
        oldMinPos = _rt.anchorMin;
        oldMaxPos = _rt.anchorMax;

        float minX = oldMinPos.x + (_dir * 2);
        float maxX = oldMaxPos.x + (_dir * 2);

        newMinPos = new Vector2(minX, oldMinPos.y);
        newMaxPos = new Vector2(maxX, oldMaxPos.y);

        _state = PokemonCardState.MOVING;
    }

    public void MoveCard()
    {
        float timeElapsed = Time.time - startTime;
        float t = Mathf.Clamp01(timeElapsed * _speed);

        _rt.anchorMin = Vector2.Lerp(oldMinPos, newMinPos, t);
        _rt.anchorMax = Vector2.Lerp(oldMaxPos, newMaxPos, t);

        if (t >= 1f)
        {
            _rt.anchorMin = newMinPos;
            _rt.anchorMax = newMaxPos;

            FillPokemonCard(_dynamicButton.BtnData as PokemonSummary);

            SetDirection(_dir * -1, _speed);
            _state = PokemonCardState.MOVING_BACK;
        }
    }

    public void MoveBackCard()
    {
        float timeElapsed = Time.time - startTime;
        float t = Mathf.Clamp01(timeElapsed * _speed);

        _rt.anchorMin = Vector2.Lerp(oldMinPos, newMinPos, t);
        _rt.anchorMax = Vector2.Lerp(oldMaxPos, newMaxPos, t);

        if (t >= 1f)
        {
            _rt.anchorMin = newMinPos;
            _rt.anchorMax = newMaxPos;

            _state = PokemonCardState.NONE;
            _pokemonSelectingZone.CountContentMoving();
        }
    }
}
