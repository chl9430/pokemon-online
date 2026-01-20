using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TrainerContentState
{
    NONE = 0,
    BEFORE_BATTLE_SCRIPTING = 1,
}

public class TrainerContent : ObjectContents
{
    TrainerContentState _state;

    [SerializeField] GameObject _screenEffecter;

    public TrainerContentState State
    {
        set
        {
            _state = value;

            if (_state == TrainerContentState.NONE)
            {
                ContentManager.Instance.gameObject.SetActive(false);
            }
        }
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;

        if (_packet is S_GetTrainerTalk)
        {
            Managers.Object.MyPlayerController.NPC = GetComponent<CreatureController>();
            Managers.Object.MyPlayerController.State = CreatureState.Talk;
            Managers.Object.MyPlayerController.IsLoading = false;

            IList<string> packetScripts = (_packet as S_GetTrainerTalk).Scripts;

            List<string> scripts = new List<string>();
            foreach (string script in packetScripts)
            {
                scripts.Add(script);
            }
            ContentManager.Instance.BeginScriptTyping(scripts);

            State = TrainerContentState.BEFORE_BATTLE_SCRIPTING;
        }
        else if (_packet is S_ReturnGame)
        {
            ContentManager.Instance.FadeInScreenEffect();

            IList<ObjectInfo> players = ((S_ReturnGame)packet).OtherPlayers;

            foreach (ObjectInfo player in players)
            {
                GameObject obj = Managers.Object.FindById(player.ObjectId);

                BaseController bc = obj.GetComponent<BaseController>();

                bc.CellPos = new Vector3Int(player.PosInfo.PosX, player.PosInfo.PosY);
                bc.Dir = player.PosInfo.MoveDir;
                bc.State = CreatureState.Idle;
            }

            FinishContent();
        }
    }

    public override void SetNextAction(object value)
    {
        switch (_state)
        {
            case TrainerContentState.BEFORE_BATTLE_SCRIPTING:
                {
                    if (_packet is S_GetTrainerTalk)
                    {
                        bool canBattle = (_packet as S_GetTrainerTalk).CanBattle;

                        if (canBattle)
                        {
                            if (!_isLoading)
                            {
                                _isLoading = true;

                                C_EnterPokemonBattleScene enterBattlePacket = new C_EnterPokemonBattleScene();
                                enterBattlePacket.PlayerId = Managers.Object.MyPlayerController.Id;

                                Managers.Network.SavePacket(enterBattlePacket);

                                ContentManager.Instance.FadeOutSceneToMove(Define.Scene.Battle, "BattleEffect_FadeOut", enterBattlePacket);

                                State = TrainerContentState.NONE;
                            }
                        }
                        else
                        {
                            FinishContent();

                            if (!_isLoading)
                            {
                                _isLoading = true;

                                C_FinishNpcTalk finishTalk = new C_FinishNpcTalk();
                                finishTalk.PlayerId = Managers.Object.MyPlayerController.Id;

                                Managers.Network.Send(finishTalk);
                            }
                        }
                    }
                }
                break;
        }
    }

    public override void FinishContent()
    {
        State = TrainerContentState.NONE;

        Managers.Scene.CurrentScene.FinishContents(true);

        Managers.Object.MyPlayerController.State = CreatureState.Idle;
        Managers.Object.MyPlayerController.NPC = null;

        ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
    }
}