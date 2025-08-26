using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExchangePokemonCard : MonoBehaviour
{
    [SerializeField] Image _pokemonIconImage;
    [SerializeField] Image _pokemonGenderImage;
    [SerializeField] TextMeshProUGUI _pokemonNickName;
    [SerializeField] TextMeshProUGUI _pokemonLevel;

    public void FillPokemonCard(PokemonSummary pokemonSum)
    {
        _pokemonNickName.text = pokemonSum.PokemonInfo.NickName;
        _pokemonLevel.text = "Lv. " + pokemonSum.PokemonInfo.Level.ToString();

        Texture2D image = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{pokemonSum.PokemonInfo.PokemonName}_Icon");
        _pokemonIconImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonIconImage.SetNativeSize();

        image = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/PokemonGender_{pokemonSum.PokemonInfo.Gender}");
        _pokemonGenderImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonGenderImage.SetNativeSize();
    }
}
