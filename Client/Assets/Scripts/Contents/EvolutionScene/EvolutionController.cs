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
    BaseScene _scene;
    Sprite _prevPokemonImage;
    Sprite _evolvePokemonImage;
    Image _pokemonImage;
    Animator _anim;
    EvolutionControllerState _controllerState;

    public EvolutionControllerState ControllerState {  set  { _controllerState = value; } }

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
        _pokemonImage = GetComponent<Image>();
        _anim = GetComponent<Animator>();
    }

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
            _scene.DoNextAction(InputSelectBoxEvent.BACK);
        }
    }

    public void PlayEvolutionAnim(string animName)
    {
        _anim.Play(animName);
    }

    public void SetPokemonImages(string prevPokemonName, string evolvePokemonName)
    {
        Texture2D texture = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{prevPokemonName}");
        _prevPokemonImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

        texture = Managers.Resource.Load<Texture2D>($"Textures/Pokemon/{evolvePokemonName}");
        _evolvePokemonImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

        _pokemonImage.sprite = _prevPokemonImage;
        _pokemonImage.SetNativeSize();
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

    public void BroadcastToScene()
    {
        _scene.DoNextAction();
    }
}
