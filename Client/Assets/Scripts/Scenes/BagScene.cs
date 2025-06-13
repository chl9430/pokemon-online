using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BagSceneState
{
    NONE = 0,
    WAITING_INPUT = 1,
}

public class BagScene : BaseScene
{
    BagSceneState _sceneState = BagSceneState.NONE;

    [SerializeField] CategorySlider categorySlider;
    [SerializeField] List<Image> sliderIndicatorImgs;


    protected override void Init()
    {
        base.Init();

        _sceneState = BagSceneState.WAITING_INPUT;

        SceneType = Define.Scene.Bag;

        // 테스트 시 사용.
        if (Managers.Network.Packet == null)
        {
            C_EnterPlayerBagScene enterBagScenePacket = new C_EnterPlayerBagScene();
            enterBagScenePacket.PlayerId = -1;
            enterBagScenePacket.ItemCategory = ItemCategory.PokeBall;

            Managers.Network.Send(enterBagScenePacket);
        }
        else
            Managers.Network.SendSavedPacket();
    }

    protected override void Start()
    {
        base.Start();

        categorySlider.WaitUserInputForSlider(true);
    }

    public override void UpdateData(IMessage packet)
    {
    }

    public override void DoNextAction(object value = null)
    {
        switch (_sceneState)
        {
            case BagSceneState.WAITING_INPUT:
                {
                    int selectedIdx = (int)value;
                    Debug.Log(selectedIdx);

                    categorySlider.WaitUserInputForSlider(true);

                    for (int i = 0; i < sliderIndicatorImgs.Count; i++)
                    {
                        Image img = sliderIndicatorImgs[i];
                        RectTransform rt = sliderIndicatorImgs[i].GetComponent<RectTransform>();

                        if (i == selectedIdx)
                        {
                            rt.sizeDelta = new Vector2(30, 30);
                            img.color = Color.red;
                        }
                        else
                        {
                            rt.sizeDelta = new Vector2(15, 15);
                            img.color = Color.white;
                        }
                    }
                }
                break;
        }
    }

    public override void Clear()
    {
        throw new System.NotImplementedException();
    }
}
