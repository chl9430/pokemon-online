using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;

public class PokemonSummaryScene : BaseScene
{
    [SerializeField] PokemonSummaryUI summaryUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.PokemonSummary;

        Managers.Network.SendSavedPacket();
    }

    public override void Clear()
    {
    }

    public override void UpdateData(IMessage packet)
    {
        S_AccessPokemonSummary accessPacket = packet as S_AccessPokemonSummary;
        summaryUI.FillPokemonSummary(accessPacket.PkmSummary);
    }
}
