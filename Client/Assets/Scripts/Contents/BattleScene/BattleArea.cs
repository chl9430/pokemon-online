using Google.Protobuf.Protocol;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleArea : MonoBehaviour
{
    [SerializeField] Animator _trainerAnim;
    [SerializeField] Animator _pokemonZoneAnim;
    [SerializeField] Animator _battlePokemonAnim;
    [SerializeField] Animator _pokemonInfoCardAnim;

    [SerializeField] Image _trainerImage;
    [SerializeField] Image _battlePokemonImage;
    [SerializeField] Image _battlePokemonHitImage;
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

    public void FillPokemonInfo(Pokemon pokemon, bool isMyPokemon)
    {
        PokemonInfo pokemonInfo = pokemon.PokemonInfo;
        PokemonStat pokemonStat = pokemon.PokemonStat;
        PokemonExpInfo expInfo = pokemon.PokemonExpInfo;

        Texture2D image;

        if (isMyPokemon)
            image = pokemon.PokemonBackImage;
        else
            image = pokemon.PokemonImage;

        _battlePokemonImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _battlePokemonImage.SetNativeSize();

        _pokemonNickName.text = pokemonInfo.NickName;

        _pokemonLevel.text = $"Lv.{pokemonInfo.Level.ToString()}";

        image = pokemon.PokemonGenderImage;

        _pokemonGender.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonGender.SetNativeSize();

        _hpGauge.SetGauge(pokemonStat.Hp, pokemonStat.MaxHp);

        if (_expGauge != null)
            _expGauge.SetGauge(expInfo.CurExp, expInfo.CurExp + expInfo.RemainExpToNextLevel);
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

    public void PlayTrainerZoneAnim(string name)
    {
        _trainerAnim.Play(name);
    }

    public void PlayPokemonZoneAnim(string name)
    {
        _pokemonZoneAnim.Play(name);
    }

    public void PlayBattlePokemonAnim(string name)
    {
        _battlePokemonAnim.Play(name);
    }

    public void PlayInfoZoneAnim(string name)
    {
        _pokemonInfoCardAnim.Play(name);
    }

    public IEnumerator BlinkPokemonHitEffect(Texture2D texture)
    {
        Color colorToVisible = _battlePokemonHitImage.color;
        colorToVisible.a = 255f;
        _battlePokemonHitImage.color = colorToVisible;

        _battlePokemonHitImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        _battlePokemonHitImage.SetNativeSize();

        yield return new WaitForSeconds(0.25f);

        Color colorToUnvisible = _battlePokemonHitImage.color;
        colorToUnvisible.a = 0f;
        _battlePokemonHitImage.color = colorToUnvisible;
    }

    public void TriggerPokemonHitImage(Pokemon attackingPKM)
    {
        Texture2D texture = attackingPKM.SelectedMove.HitEffectImage;
        StartCoroutine(BlinkPokemonHitEffect(texture));
    }

}
