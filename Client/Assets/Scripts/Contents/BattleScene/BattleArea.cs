using Google.Protobuf.Protocol;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleArea : MonoBehaviour
{
    bool _isMyPokemon;
    Pokemon _pokemon;
    [SerializeField] Image _pokemonImage;
    [SerializeField] Image _pokemonHitImage;
    [SerializeField] TextMeshProUGUI _pokemonNickName;
    [SerializeField] TextMeshProUGUI _pokemonLevel;
    [SerializeField] Image _pokemonGender;
    [SerializeField] GaugeUI _hpGauge;
    [SerializeField] GaugeUI _expGauge;
    [SerializeField] MoveableUI _pokemonUI;

    public Pokemon Pokemon { get { return _pokemon; } set { _pokemon = value; } }

    void LoadComponent()
    {
        if (_pokemonImage == null)
            _pokemonImage = Util.FindChild<Image>(gameObject, "PokemonImage", true);
        if (_pokemonHitImage == null)
            _pokemonHitImage = Util.FindChild<Image>(gameObject, "PokemonHitImage", true);
        if (_pokemonNickName == null)
            _pokemonNickName = Util.FindChild<TextMeshProUGUI>(gameObject, "PokemonNickName", true);
        if (_pokemonLevel == null)
            _pokemonLevel = Util.FindChild<TextMeshProUGUI>(gameObject, "PokemonLevel", true);
        if (_pokemonGender == null)
            _pokemonGender = Util.FindChild<Image>(gameObject, "PokemonGender", true);
        if (_hpGauge == null)
            _hpGauge = Util.FindChild<GaugeUI>(gameObject, "HPGauge", true);
        if (_pokemonUI == null)
            _pokemonUI = Util.FindChild<MoveableUI>(gameObject, "PokemonMoveableUI", true);
    }

    public void FillTrainerImage(PlayerGender gender)
    {
        LoadComponent();

        Texture2D image = Managers.Resource.Load<Texture2D>($"Textures/BattleScene/Trainer_Back_{gender.ToString()}");

        _pokemonImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonImage.SetNativeSize();
    }



    public void FillPokemonInfo(Pokemon pokemon, bool isMyPokemon)
    {
        LoadComponent();

        _isMyPokemon = isMyPokemon;
        _pokemon = pokemon;

        PokemonInfo pokemonInfo = pokemon.PokemonInfo;
        PokemonStat pokemonStat = pokemon.PokemonStat;
        PokemonExpInfo expInfo = pokemon.PokemonExpInfo;

        Texture2D image;

        if (_isMyPokemon)
            image = pokemon.PokemonBackImage;
        else
            image = pokemon.PokemonImage;

        _pokemonImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonImage.SetNativeSize();

        _pokemonNickName.text = pokemonInfo.NickName;

        _pokemonLevel.text = $"Lv.{pokemonInfo.Level.ToString()}";

        image = Managers.Resource.Load<Texture2D>($"Textures/UI/PokemonGender_{pokemonInfo.Gender}");

        _pokemonGender.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonGender.SetNativeSize();

        _hpGauge.SetGauge(pokemonStat.Hp, pokemonStat.MaxHp);

        if (_expGauge != null)
            _expGauge.SetGauge(expInfo.CurExp, expInfo.RemainExpToNextLevel);
    }

    public void ChangePokemonHP(int destHP)
    {
        _hpGauge.ChangeGauge(destHP, 0.01f);
    }

    public void ChangePokemonEXP(int destExp)
    {
        _expGauge.ChangeGauge(destExp, 0.01f);
    }

    public void AttackMovePokemonUI()
    {
        RectTransform rt = _pokemonUI.GetComponent<RectTransform>();

        Vector2 minDestPos;
        Vector2 maxDestPos;

        if (_isMyPokemon)
        {
            minDestPos = new Vector2(rt.anchorMin.x + 0.25f, rt.anchorMin.y);
            maxDestPos = new Vector2(rt.anchorMax.x + 0.25f, rt.anchorMax.y);
        }
        else
        {
            minDestPos = new Vector2(rt.anchorMin.x - 0.25f, rt.anchorMin.y);
            maxDestPos = new Vector2(rt.anchorMax.x - 0.25f, rt.anchorMax.y);
        }

        _pokemonUI.SetOldAndDestPos(minDestPos, maxDestPos, MoveableUIState.MOVE_AND_COMEBACK, 3f);
    }

    public void BlinkPokemonUI()
    {
        _pokemonUI.StartBlink(2, 0.25f);
    }

    public void TriggerPokemonHitImage(Pokemon attackingPKM)
    {
        StartCoroutine(ShowHitEffect(attackingPKM));
    }

    IEnumerator ShowHitEffect(Pokemon attackingPKM)
    {
        Color colorToVisible = _pokemonHitImage.color;
        colorToVisible.a = 255f;
        _pokemonHitImage.color = colorToVisible;

        Texture2D image = attackingPKM.SelectedMove.HitEffectImage;
        _pokemonHitImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonHitImage.SetNativeSize();

        yield return new WaitForSeconds(0.25f);

        Color colorToUnvisible = _pokemonHitImage.color;
        colorToUnvisible.a = 0f;
        _pokemonHitImage.color = colorToUnvisible;
    }

    public void PokemonDie()
    {
        RectTransform rt = _pokemonUI.GetComponent<RectTransform>();

        Vector2 minDestPos;
        Vector2 maxDestPos;

        minDestPos = new Vector2(rt.anchorMin.x, rt.anchorMin.y - 1);
        maxDestPos = new Vector2(rt.anchorMax.x, rt.anchorMax.y - 1);

        _pokemonUI.SetOldAndDestPos(minDestPos, maxDestPos, MoveableUIState.MOVING, 3f);
    }
}
