using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PokemonAppearanceTile : MonoBehaviour
{
    BaseScene _scene;

    [SerializeField] int _appearanceRate;
    [SerializeField] int _LocationNum;
    [SerializeField] ScreenEffecter _pokemonAppearEffect;

    MyPlayerController _myPlayer;

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
        _myPlayer = Managers.Object.MyPlayer;
    }

    public bool AppearPokemon()
    {
        if (_myPlayer == null)
            _myPlayer = Managers.Object.MyPlayer;

        int ran = Random.Range(0, 100);

        if (ran < _appearanceRate)
        {
            ScreenEffecter effecter = Instantiate(_pokemonAppearEffect, _scene.ScreenEffecterZone);
            effecter.PlayEffect("PokemonAppear");
            _scene.ScreenEffecter = effecter;

            C_EnterPokemonBattleScene c_enterBattleScenePacket = new C_EnterPokemonBattleScene();

            c_enterBattleScenePacket.PlayerId = Managers.Object.MyPlayer.Id;
            c_enterBattleScenePacket.LocationNum = _LocationNum;

            Managers.Network.SavePacket(c_enterBattleScenePacket);

            return true;
        }

        return false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            MyPlayerController myPlayer = collision.gameObject.GetComponent<MyPlayerController>();

            myPlayer.PokemonTile = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            MyPlayerController myPlayer = collision.gameObject.GetComponent<MyPlayerController>();

            myPlayer.PokemonTile = null;
        }
    }
}
