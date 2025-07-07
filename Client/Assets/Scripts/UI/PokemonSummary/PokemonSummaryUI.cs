using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PokemonSummaryUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dictNum;
    [SerializeField] Image _pokemonImg;
    [SerializeField] TextMeshProUGUI nickName;
    [SerializeField] TextMeshProUGUI pokemonName;
    // [SerializeField] Image catchBall;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] Image gender;
    [SerializeField] TextMeshProUGUI owner;
    [SerializeField] Image type;
    [SerializeField] Image type1;
    [SerializeField] Image type2;
    [SerializeField] TextMeshProUGUI nature;
    [SerializeField] TextMeshProUGUI metLevel;
    // [SerializeField] TextMeshProUGUI metPlace;

    // [SerializeField] TextMeshProUGUI item;
    // [SerializeField] TextMeshProUGUI ribbon;
    [SerializeField] TextMeshProUGUI hpAndMaxHP;
    [SerializeField] TextMeshProUGUI attack;
    [SerializeField] TextMeshProUGUI defense;
    [SerializeField] TextMeshProUGUI speicalAttack;
    [SerializeField] TextMeshProUGUI speicalDefense;
    [SerializeField] TextMeshProUGUI speed;
    [SerializeField] TextMeshProUGUI totalEXP;
    [SerializeField] TextMeshProUGUI expToNextLevel;

    public void FillPokemonSummary(Pokemon pokemon)
    {
        FillText(dictNum, $"No. {pokemon.PokemonInfo.DictionaryNum}");
        FillText(nickName, $"{pokemon.PokemonInfo.NickName}");
        FillText(pokemonName, $"/ {pokemon.PokemonInfo.PokemonName}");
        FillText(level, $"Lv. {pokemon.PokemonInfo.Level}");
        FillText(owner, $"{pokemon.PokemonInfo.OwnerName} (ID : {pokemon.PokemonInfo.OwnerId})");
        FillText(nature, $"{pokemon.PokemonInfo.Nature} nature,");
        FillText(metLevel, $"met at Lv. {pokemon.PokemonInfo.MetLevel}, MOON FIELD.");

        FillImage(pokemon);
        FillImage(pokemon);
        FillImage(pokemon);
        FillImage(pokemon);

        FillTypeImage(pokemon);

        FillText(hpAndMaxHP, $"{pokemon.PokemonStat.Hp} / {pokemon.PokemonStat.MaxHp}");
        FillText(attack, $"{pokemon.PokemonStat.Attack}");
        FillText(defense, $"{pokemon.PokemonStat.Defense}");
        FillText(speicalAttack, $"{pokemon.PokemonStat.SpecialAttack}");
        FillText(speicalDefense, $"{pokemon.PokemonStat.SpecialDefense}");
        FillText(speed, $"{pokemon.PokemonStat.Speed}");
        FillText(totalEXP, $"{pokemon.PokemonExpInfo.TotalExp}");
        FillText(expToNextLevel, $"{pokemon.PokemonExpInfo.RemainExpToNextLevel}");
    }

    void FillText(TextMeshProUGUI tmp, string text)
    {
        tmp.text = text;
    }

    void FillImage(Pokemon pokemon)
    {
        Texture2D image = pokemon.PokemonImage;

        _pokemonImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonImg.SetNativeSize();
    }

    void FillTypeImage(Pokemon pokemon)
    {
        if (pokemon.PokemonInfo.Type2 == PokemonType.TypeNone)
        {
            Texture2D typeImg = Managers.Resource.Load<Texture2D>($"Textures/UI/{pokemon.PokemonInfo.Type1}_Icon");
            type.sprite = Sprite.Create(typeImg, new Rect(0, 0, typeImg.width, typeImg.height), Vector2.one * 0.5f);
            type.SetNativeSize();

            type1.gameObject.SetActive(false);
            type2.gameObject.SetActive(false);
        }
        else
        {
            Texture2D typeImg = Managers.Resource.Load<Texture2D>($"Textures/UI/{pokemon.PokemonInfo.Type1}_Icon");
            type1.sprite = Sprite.Create(typeImg, new Rect(0, 0, typeImg.width, typeImg.height), Vector2.one * 0.5f);
            type1.SetNativeSize();

            typeImg = Managers.Resource.Load<Texture2D>($"Textures/UI/{pokemon.PokemonInfo.Type2}_Icon");
            type2.sprite = Sprite.Create(typeImg, new Rect(0, 0, typeImg.width, typeImg.height), Vector2.one * 0.5f);
            type2.SetNativeSize();

            type.gameObject.SetActive(false);
        }
    }
}
