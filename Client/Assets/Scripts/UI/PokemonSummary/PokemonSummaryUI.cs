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
    [SerializeField] Image _type;
    [SerializeField] Image _type1;
    [SerializeField] Image _type2;
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

    public void FillPokemonBasicInfo(PokemonSummary pokemonSum)
    {
        FillText(dictNum, $"No. {pokemonSum.PokemonInfo.DictionaryNum}");
        FillText(nickName, $"{pokemonSum.PokemonInfo.NickName}");
        FillText(pokemonName, $"/ {pokemonSum.PokemonInfo.PokemonName}");
        FillText(level, $"Lv. {pokemonSum.PokemonInfo.Level}");

        FillImage(pokemonSum);
    }

    public void FillPokemonSummary(PokemonSummary summary)
    {
        FillText(owner, $"{summary.PokemonInfo.OwnerName} (ID : {summary.PokemonInfo.OwnerId})");
        FillText(nature, $"{summary.PokemonInfo.Nature} nature,");
        FillText(metLevel, $"met at Lv. {summary.PokemonInfo.MetLevel}, MOON FIELD.");

        FillTypeImage(summary);

        FillText(hpAndMaxHP, $"{summary.PokemonStat.Hp} / {summary.PokemonStat.MaxHp}");
        FillText(attack, $"{summary.PokemonStat.Attack}");
        FillText(defense, $"{summary.PokemonStat.Defense}");
        FillText(speicalAttack, $"{summary.PokemonStat.SpecialAttack}");
        FillText(speicalDefense, $"{summary.PokemonStat.SpecialDefense}");
        FillText(speed, $"{summary.PokemonStat.Speed}");
        FillText(totalEXP, $"{summary.PokemonExpInfo.TotalExp}");
        FillText(expToNextLevel, $"{summary.PokemonExpInfo.RemainExpToNextLevel}");
    }

    void FillText(TextMeshProUGUI tmp, string text)
    {
        tmp.text = text;
    }

    void FillTypeImage(PokemonSummary summary)
    {
        if (summary.PokemonInfo.Type2 == PokemonType.TypeNone)
        {
            Texture2D typeImg = Managers.Resource.Load<Texture2D>($"Textures/UI/{summary.PokemonInfo.Type1}_Icon");
            _type.sprite = Sprite.Create(typeImg, new Rect(0, 0, typeImg.width, typeImg.height), Vector2.one * 0.5f);
            _type.SetNativeSize();

            _type1.gameObject.SetActive(false);
            _type2.gameObject.SetActive(false);
        }
        else
        {
            Texture2D typeImg = Managers.Resource.Load<Texture2D>($"Textures/UI/{summary.PokemonInfo.Type1}_Icon");
            _type1.sprite = Sprite.Create(typeImg, new Rect(0, 0, typeImg.width, typeImg.height), Vector2.one * 0.5f);
            _type1.SetNativeSize();

            typeImg = Managers.Resource.Load<Texture2D>($"Textures/UI/{summary.PokemonInfo.Type2}_Icon");
            _type2.sprite = Sprite.Create(typeImg, new Rect(0, 0, typeImg.width, typeImg.height), Vector2.one * 0.5f);
            _type2.SetNativeSize();

            _type.gameObject.SetActive(false);
        }
    }

    void FillImage(PokemonSummary summary)
    {
        Texture2D image = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{summary.PokemonInfo.PokemonName}");
        _pokemonImg.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        _pokemonImg.SetNativeSize();

        image = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/PokemonGender_{summary.PokemonInfo.Gender}");
        gender.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.one * 0.5f);
        gender.SetNativeSize();
    }
}
