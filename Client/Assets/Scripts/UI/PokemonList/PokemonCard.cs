using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PokemonCard : ImageButton
{
    float startTime;
    Vector2 oldMinPos;
    Vector2 oldMaxPos;
    Vector2 newMinPos;
    Vector2 newMaxPos;
    PokemonListUI _pokemonListUI;
    RectTransform _rt;

    [SerializeField] Image pokemonImg;
    [SerializeField] TextMeshProUGUI pokemonNickname;
    [SerializeField] TextMeshProUGUI pokemonLevel;
    [SerializeField] Image pokemonGenderImg;
    [SerializeField] TextMeshProUGUI pokemonHp;

    public PokemonListUI PokemonListUI
    {
        set
        {
            _pokemonListUI = value;
        }
    }

    void Start()
    {
        _rt = GetComponent<RectTransform>();
    }

    public void ApplyImage(Texture2D img)
    {
        pokemonImg.sprite = Sprite.Create(img, new Rect(0, 0, img.width, img.height), Vector2.one * 0.5f);
        pokemonImg.SetNativeSize();

        //pokemonImg.rectTransform.sizeDelta = new Vector2(img.width * 5, img.height * 5);

        //pokemonImg.rectTransform.anchorMin = Vector2.zero;
        //pokemonImg.rectTransform.anchorMax = Vector2.one;

        //pokemonImg.rectTransform.offsetMin = Vector2.zero;
        //pokemonImg.rectTransform.offsetMax = Vector2.zero;
    }

    public void ApplyPokemonInfo(string nickName, int hp, int maxHp, int level)
    {
        pokemonNickname.text = nickName;
        pokemonHp.text = $"HP : {hp.ToString()} / {maxHp.ToString()}";
        pokemonLevel.text = $"Lv : {level}";
    }

    public void SetOldAndNewPos(int dir)
    {
        _pokemonListUI.SceneState = PokemonListSceneState.SWITCHING_POKEMON;
        startTime = Time.time;
        oldMinPos = _rt.anchorMin;
        oldMaxPos = _rt.anchorMax;

        float minX = oldMinPos.x + (dir * 2);
        float maxX = oldMaxPos.x + (dir * 2);

        newMinPos = new Vector2(minX, oldMinPos.y);
        newMaxPos = new Vector2(maxX, oldMaxPos.y);
    }

    public void MoveCard(float speed)
    {
        float timeElapsed = Time.time - startTime;
        float t = Mathf.Clamp01(timeElapsed * speed);

        _rt.anchorMin = Vector2.Lerp(oldMinPos, newMinPos, t);
        _rt.anchorMax = Vector2.Lerp(oldMaxPos, newMaxPos, t);

        if (t >= 1f)
        {
            _pokemonListUI.SceneState = PokemonListSceneState.FINISING_SWITCHING_POKEMON;
            _rt.anchorMin = newMinPos;
            _rt.anchorMax = newMaxPos;
        }
    }

    public void MoveBackCard(float speed)
    {
        float timeElapsed = Time.time - startTime;
        float t = Mathf.Clamp01(timeElapsed * speed);

        _rt.anchorMin = Vector2.Lerp(oldMinPos, newMinPos, t);
        _rt.anchorMax = Vector2.Lerp(oldMaxPos, newMaxPos, t);

        if (t >= 1f)
        {
            _pokemonListUI.SceneState = PokemonListSceneState.NON_SELECTED;
            _rt.anchorMin = newMinPos;
            _rt.anchorMax = newMaxPos;
        }
    }
}
