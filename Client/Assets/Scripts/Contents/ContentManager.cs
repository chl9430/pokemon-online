using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContentManager : MonoBehaviour
{
    public static ContentManager Instance { get; private set; }

    Canvas _canvas;
    Define.Scene _nextSceneType;
    IMessage _nextScenePacket;

    [SerializeField] ScriptBoxUI _scriptBox;
    [SerializeField] ScreenEffecter _screenEffecter;

    public ScriptBoxUI ScriptBox { get { return _scriptBox; } }
    public ScreenEffecter ScreenEffecter { get { return _screenEffecter; } }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _canvas = Util.FindChild<Canvas>(gameObject);
    }

    public void BeginScriptTyping(List<string> scripts, bool autoSkip = false, float autoSkipTime = 1, bool isStatic = false)
    {
        _scriptBox.BeginScriptTyping(scripts, autoSkip, autoSkipTime, isStatic);
    }

    public void FadeOutSceneToMove(Define.Scene nextSceneType, string effectName, IMessage nextScenePacket = null)
    {
        _nextSceneType = nextSceneType;

        if (nextScenePacket != null)
            _nextScenePacket = nextScenePacket;

        _screenEffecter.PlayEffect(effectName);
        _screenEffecter.SetMoveSceneType(MoveSceneType.MovingNewScene);
    }

    public void FadeOutCurSceneToUnload(Define.Scene curSceneType, string effectName, IMessage nextScenePacket = null)
    {
        _nextSceneType = curSceneType;

        if (nextScenePacket != null)
            _nextScenePacket = nextScenePacket;

        _screenEffecter.PlayEffect(effectName);
        _screenEffecter.SetMoveSceneType(MoveSceneType.UnloadCurScene);
    }

    public void MoveToAnotherScene()
    {
        if (_nextSceneType == Define.Scene.Intro)
        {
            Managers.Scene.AsyncLoadScene(Define.Scene.Intro, () => { });
        }
        else if (_nextSceneType == Define.Scene.Game)
        {
            if (_nextScenePacket is C_LoadGameData)
            {
                Managers.Scene.AsyncLoadScene(Define.Scene.Game, () =>
                {
                    Managers.Scene.CurrentScene = GameObject.FindFirstObjectByType<GameScene>();

                    Managers.Network.Send(_nextScenePacket);
                });
            }
            else if (_nextScenePacket is C_EnterRoom)
            {
                Managers.Scene.AsyncLoadScene(Define.Scene.Game, () =>
                {
                    Managers.Scene.CurrentScene = GameObject.FindFirstObjectByType<GameScene>();

                    Managers.Network.Send(_nextScenePacket);
                });
            }
        }
        else if (_nextSceneType == Define.Scene.Battle)
        {
            if (_nextScenePacket is C_EnterPokemonBattleScene)
            {
                Managers.Scene.AsyncLoadScene(Define.Scene.Battle, () =>
                {
                    Managers.Scene.CurrentScene = GameObject.FindFirstObjectByType<BattleScene>();

                    Managers.Network.Send(_nextScenePacket);
                }, LoadSceneMode.Additive);
            }
        }
        else if (_nextSceneType == Define.Scene.PokemonExchange)
        {
            if (_nextScenePacket is C_EnterPokemonExchangeScene)
            {
                Managers.Scene.AsyncLoadScene(Define.Scene.PokemonExchange, () =>
                {
                    Managers.Scene.CurrentScene = GameObject.FindFirstObjectByType<PokemonExchangeScene>();

                    Managers.Network.Send(_nextScenePacket);
                }, LoadSceneMode.Additive);
            }
        }
    }

    public void UnloadCurScene()
    {
        if (_nextSceneType == Define.Scene.Battle)
        {
            if (_nextScenePacket is C_ReturnGame)
            {
                Managers.Scene.AsyncUnLoadScene(Define.Scene.Battle, () =>
                {
                    Managers.Scene.CurrentScene = GameObject.FindFirstObjectByType<GameScene>();

                    Managers.Network.Send(_nextScenePacket);
                });
            }
        }
    }

    public void FadeInScreenEffect()
    {
        _screenEffecter.SetFadeIn();
    }
}
