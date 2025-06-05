using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PokemonAppearanceTile : MonoBehaviour
{
    [SerializeField] int _appearanceRate;
    [SerializeField] int _LocationNum;
    [SerializeField] GameObject _pkmAppearEffect;

    Tilemap _tilemap;
    MyPlayerController _myPlayer;
    ScreenChanger _screenChanger;

    void Start()
    {
        _tilemap = GetComponent<Tilemap>();
        _myPlayer = Managers.Object.MyPlayer;
    }

    public bool AppearPokemon()
    {
        if (_myPlayer == null)
            _myPlayer = Managers.Object.MyPlayer;

        int ran = Random.Range(0, 100);

        if (ran < _appearanceRate)
        {
            _myPlayer.State = CreatureState.Fight;

            _screenChanger = Instantiate(_pkmAppearEffect).GetComponent<ScreenChanger>();

            Managers.Scene.CurrentScene.AttachToTheUI(_screenChanger.gameObject);

            C_MeetWildPokemon c_MeetPacket = new C_MeetWildPokemon();

            c_MeetPacket.PlayerInfo = Managers.Object.MyPlayer.MakeObjectInfo();
            c_MeetPacket.LocationNum = _LocationNum;

            BaseScene scene = Managers.Scene.CurrentScene;

            scene.RegisterPacket(c_MeetPacket);

            return true;
        }

        return false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            MyPlayerController myPlayer = collision.gameObject.GetComponent<MyPlayerController>();

            myPlayer.pkmAppearTile = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            MyPlayerController myPlayer = collision.gameObject.GetComponent<MyPlayerController>();

            myPlayer.pkmAppearTile = null;
        }
    }
}
