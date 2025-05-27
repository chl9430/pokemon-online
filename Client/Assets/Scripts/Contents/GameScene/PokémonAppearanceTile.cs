using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum AppearPokemon
{
    PIKACHU = 0,
    ASB = 1,
    WER = 2,
}

public class PokemonAppearanceTile : MonoBehaviour
{
    [SerializeField] int _appearanceRate;
    [SerializeField] List<AppearPokemon> _appearPkms;
    [SerializeField] GameObject _pkmAppearEffect;

    Tilemap _tilemap;
    MyPlayerController _myPlayer;
    ScreenChanger _screenChanger;

    void Start()
    {
        _tilemap = GetComponent<Tilemap>();
        _myPlayer = FindFirstObjectByType<MyPlayerController>();
    }

    public bool AppearPokemon()
    {
        int ran = Random.Range(0, 100);

        if (ran < _appearanceRate)
        {
            _myPlayer.State = CreatureState.Fight;

            int pkmRan = Random.Range(0, _appearPkms.Count);

            Debug.Log($"{_appearPkms[pkmRan]} 발생!");

            _screenChanger = Instantiate(_pkmAppearEffect).GetComponent<ScreenChanger>();

            Managers.Scene.CurrentScene.AttachToTheUI(_screenChanger.gameObject);

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
