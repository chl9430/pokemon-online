using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PokemonCard : ImageButton
{
    [SerializeField] Image pokemonImg;
    [SerializeField] TextMeshProUGUI pokemonNickname;
    [SerializeField] TextMeshProUGUI pokemonLevel;
    [SerializeField] Image pokemonGenderImg;
    [SerializeField] TextMeshProUGUI pokemonHp;

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
}
