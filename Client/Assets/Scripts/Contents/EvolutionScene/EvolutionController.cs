using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public enum EvolutionControllerState
{
    NONE = 0,
    SELECTING_EVOLUTION = 1,
}

public class EvolutionController : MonoBehaviour
{
    Sprite _prevPokemonImage;
    Sprite _evolvePokemonImage;
    Image _pokemonImage;
    Animator _anim;
    EvolutionControllerState _controllerState;

    public EvolutionControllerState ControllerState {  set  { _controllerState = value; } }

    void Update()
    {
        switch (_controllerState)
        {
            case EvolutionControllerState.SELECTING_EVOLUTION:
                {
                    EvolutionInput();
                }
                break;
        }
    }

    void EvolutionInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Managers.Scene.CurrentScene.DoNextAction(InputSelectBoxEvent.BACK);
        }
    }

    public void SetPrevPokemonImage(string prevPokemonName)
    {
        if (_pokemonImage == null)
            _pokemonImage = Util.FindChild<Image>(gameObject);

        Texture2D texture = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{prevPokemonName}");
        _prevPokemonImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

        _pokemonImage.sprite = _prevPokemonImage;
        _pokemonImage.SetNativeSize();
    }

    public void SetEvolvePokemonImage(string evolvePokemonName)
    {
        Texture2D texture = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{evolvePokemonName}");
        _evolvePokemonImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
    }

    public void ChangeEvolvePokemonImage()
    {
        _pokemonImage.sprite = _evolvePokemonImage;
        _pokemonImage.SetNativeSize();
    }

    public void ChangePrevPokemonImage()
    {
        _pokemonImage.sprite = _prevPokemonImage;
        _pokemonImage.SetNativeSize();
    }

    public void PokemonEvolution()
    {
        if (_anim == null)
            _anim = GetComponent<Animator>();

        _anim.Play("PokemonEvolution_Evolving");
    }

    public void EvolutionFadeOut()
    {
        _anim.Play("PokemonEvolution_WhiteFadeOut");
    }

    public void EvolutionFadeIn()
    {
        _anim.Play("PokemonEvolution_WhiteFadeIn");
    }

    public void BroadcastToScene()
    {
        Managers.Scene.CurrentScene.DoNextAction();
    }
}
