using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PokemonMoveCard : MonoBehaviour
{
    [SerializeField] Image _moveTypeImage;
    [SerializeField] TextMeshProUGUI _moveName;
    [SerializeField] TextMeshProUGUI _movePP;

    public TextMeshProUGUI MoveNameText { get { return _moveName; } }

    public void FillMoveCard(PokemonMoveSummary moveSum)
    {
        Texture2D texture = Managers.Resource.Load<Texture2D>($"Textures/UI/{moveSum.MoveType}_Icon");
        _moveTypeImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        _moveTypeImage.SetNativeSize();

        _moveName.text = moveSum.MoveName;
        _movePP.text = $"PP {moveSum.CurPP} / {moveSum.MaxPP}";
    }
}
