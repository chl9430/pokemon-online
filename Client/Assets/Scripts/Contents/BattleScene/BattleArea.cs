using Google.Protobuf.Protocol;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleArea : MonoBehaviour
{
    bool _isMyPokemon;
    GameObject _battlePokemon;
    Image _pokemonImage;
    Image _pokemonHitImage;
    Animator _pokemonAnim;

    [SerializeField] Animator _trainerAnim;
    [SerializeField] Image _trainerImage;
    [SerializeField] Transform _battlePokemonZone;
    [SerializeField] TextMeshProUGUI _pokemonNickName;
    [SerializeField] TextMeshProUGUI _pokemonLevel;
    [SerializeField] Image _pokemonGender;
    [SerializeField] GaugeUI _hpGauge;
    [SerializeField] GaugeUI _expGauge;

    public void FillTrainerImage(PlayerGender gender)
    {
        Texture2D image = Managers.Resource.Load<Texture2D>($"Textures/BattleScene/Trainer_Back_{gender.ToString()}");

        _trainerImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _trainerImage.SetNativeSize();
    }

    public void MakeBattlePokemon(Pokemon pokemon, bool isMyPokemon)
    {
        _battlePokemon = Managers.Resource.Instantiate("UI/BattleScene/BattlePokemon", _battlePokemonZone);

        _pokemonAnim = _battlePokemon.GetComponent<Animator>();
        _pokemonImage = Util.FindChild<Image>(_battlePokemon, "Image", false);
        _pokemonHitImage = Util.FindChild<Image>(_battlePokemon, "HitImage", true);

        FillPokemonInfo(pokemon, isMyPokemon);
    }

    public void FillPokemonInfo(Pokemon pokemon, bool isMyPokemon)
    {
        _isMyPokemon = isMyPokemon;

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

        image = pokemon.PokemonGenderImage;

        _pokemonGender.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonGender.SetNativeSize();

        _hpGauge.SetGauge(pokemonStat.Hp, pokemonStat.MaxHp);

        if (_expGauge != null)
            _expGauge.SetGauge(expInfo.CurExp, expInfo.RemainExpToNextLevel);
    }

    public void SetActiveTrainer(bool isActive)
    {
        _trainerAnim.gameObject.SetActive(isActive);
    }

    public void ChangePokemonHP(int destHP)
    {
        _hpGauge.ChangeGauge(destHP, 0.01f);
    }

    public void ChangePokemonEXP(int destExp)
    {
        _expGauge.ChangeGauge(destExp, 0.01f);
    }

    public void PlayTrainerAnim(string name)
    {
        _trainerAnim.Play(name);
    }

    public void PlayBattlePokemonAnim(string name)
    {
        _pokemonAnim.Play(name);
    }

    public IEnumerator BlinkPokemonHitEffect(Texture2D texture)
    {
        Color colorToVisible = _pokemonHitImage.color;
        colorToVisible.a = 255f;
        _pokemonHitImage.color = colorToVisible;

        _pokemonHitImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        _pokemonHitImage.SetNativeSize();

        yield return new WaitForSeconds(0.25f);

        Color colorToUnvisible = _pokemonHitImage.color;
        colorToUnvisible.a = 0f;
        _pokemonHitImage.color = colorToUnvisible;
    }

    public void TriggerPokemonHitImage(Pokemon attackingPKM)
    {
        Texture2D texture = attackingPKM.SelectedMove.HitEffectImage;
        StartCoroutine(BlinkPokemonHitEffect(texture));
    }

}
