using UnityEngine;
using UnityEngine.UI;

public class PokemonCard : MonoBehaviour
{
    [SerializeField] Image pokemonImg;
    [SerializeField] Text pokemonNickname;
    [SerializeField] Text pokemonHp;
    [SerializeField] Text pokemonLevel;

    public void ApplyImage(Texture2D img)
    {
        pokemonImg.sprite = Sprite.Create(img, new Rect(0, 0, img.width, img.height), Vector2.one * 0.5f);
        pokemonImg.SetNativeSize();
    }

    public void ApplyPokemonInfo(string nickName, int hp, int maxHp, int level)
    {
        pokemonNickname.text = nickName;
        pokemonHp.text = $"HP : {hp.ToString()} / {maxHp.ToString()}";
        pokemonLevel.text = $"Lv : {level}";
    }
}
