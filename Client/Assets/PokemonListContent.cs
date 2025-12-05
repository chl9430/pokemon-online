using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PokemonListContentState
{
    None = 0,
    Choosing_Pokemon = 1,
    Choosing_Action = 2,
    Choosing_Pokemon_To_Switch = 3,
    Switching_Pokemon = 4,
    Item_Cannot_Use_Scripting = 5,
    After_Item_Use_Scripting = 6,
    MovingToBattleScene = 7,
    Inactiving = 99,
}

public class PokemonListContent : ObjectContents
{
    int _selectedPokemonIdx;
    int _selectedSwitchPokemonIdx;
    PokemonListContentState _state = PokemonListContentState.None;
    List<Pokemon> _pokemons;

    [SerializeField] PokemonListSelectArea _pokemonSelectArea;
    [SerializeField] GridLayoutSelectBox _actionSelectBox;
    [SerializeField] TextMeshProUGUI _instructorTmp;

    public PokemonListContentState State
    {
        set
        {
            _state = value;

            if (_state == PokemonListContentState.Choosing_Pokemon)
            {
                gameObject.SetActive(true);

                _instructorTmp.text = "Choose a Pokemon.";
                ContentManager.Instance.ScriptBox.gameObject.SetActive(false);
                _pokemonSelectArea.State = PokemonSelectAreaState.SELECTING_POKEMON;

                _actionSelectBox.gameObject.SetActive(false);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;
            }
            else if (_state == PokemonListContentState.Choosing_Action)
            {
                _instructorTmp.text = "Do what with this Pokemon?";
                _pokemonSelectArea.State = PokemonSelectAreaState.NONE;
                _actionSelectBox.gameObject.SetActive(true);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.SELECTING;
            }
            else if (_state == PokemonListContentState.Choosing_Pokemon_To_Switch)
            {
                _instructorTmp.text = "Move to where?";
                _actionSelectBox.gameObject.SetActive(false);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;
                _pokemonSelectArea.State = PokemonSelectAreaState.SELECTING_TO_SWITCH_POKEMON;
            }
            else if (_state == PokemonListContentState.Switching_Pokemon)
            {
                _pokemonSelectArea.State = PokemonSelectAreaState.NONE;
            }
            else if (_state == PokemonListContentState.Item_Cannot_Use_Scripting)
            {
                _actionSelectBox.gameObject.SetActive(false);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;

                ContentManager.Instance.ScriptBox.gameObject.SetActive(true);
            }
            else if (_state == PokemonListContentState.After_Item_Use_Scripting)
            {
                _actionSelectBox.gameObject.SetActive(false);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;

                ContentManager.Instance.ScriptBox.gameObject.SetActive(true);
            }
            else if (_state == PokemonListContentState.MovingToBattleScene)
            {
                _actionSelectBox.gameObject.SetActive(false);
                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;
            }
            else if (_state == PokemonListContentState.Inactiving)
            {
                gameObject.SetActive(false);

                _actionSelectBox.UIState = GridLayoutSelectBoxState.NONE;
            }
            else if (_state == PokemonListContentState.None)
            {
                gameObject.SetActive(false);

                _pokemonSelectArea.State = PokemonSelectAreaState.NONE;
            }
        }
    }

    public override void UpdateData(IMessage packet)
    {
        _packet = packet;
        _isLoading = false;

        if (_packet is S_UseItemInListScene)
        {
            ItemUseResult itemUseResult = ((S_UseItemInListScene)_packet).ItemUseResult;
            ItemSummary itemSum = ((S_UseItemInListScene)_packet).ItemSum;
            PokemonSummary targetPokemonSum = ((S_UseItemInListScene)_packet).TargetPokemonSum;

            switch (itemUseResult.SpecialInfoCase)
            {
                case ItemUseResult.SpecialInfoOneofCase.HpRecoveryItemUseResult:
                    {
                        DynamicButton dynamicBtn = _pokemonSelectArea.GetSelectedBtn();
                        dynamicBtn.GetComponent<PokemonCard>().UpdatePokemonHP(targetPokemonSum.PokemonStat.Hp, targetPokemonSum.PokemonStat.MaxHp);

                        Pokemon targetPokemon = _pokemonSelectArea.GetSelectedBtnData() as Pokemon;
                        targetPokemon.UpdatePokemonSummary(targetPokemonSum);

                        List<string> scripts = new List<string>()
                        {
                            $"{targetPokemon.PokemonInfo.NickName} recovered by {itemUseResult.HpRecoveryItemUseResult.RealRecoveryAmt}."
                        };
                        ContentManager.Instance.BeginScriptTyping(scripts);

                        State = PokemonListContentState.After_Item_Use_Scripting;
                    }
                    break;
            }
        }
    }

    public override void SetNextAction(object value = null)
    {
        if (_isActionStop)
            return;

        switch (_state)
        {
            case PokemonListContentState.None:
                {
                    State = PokemonListContentState.Choosing_Pokemon;

                    ContentManager.Instance.PlayScreenEffecter("FadeIn_NonBroading");

                    _pokemonSelectArea.ResetSelectedPokemon(true);

                    gameObject.SetActive(true);
                }
                break;
            case PokemonListContentState.Choosing_Pokemon:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            if (Managers.Scene.CurrentScene is BattleScene)
                            {
                                if (_pokemonSelectArea.IsFirstPokemonFainting())
                                {
                                    return;
                                }
                                else
                                {
                                    FinishContent();
                                }
                            }
                            else
                            {
                                FinishContent();
                            }
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (_pokemonSelectArea.GetSelectedBtnData() as string == "Cancel")
                            {
                                if (Managers.Scene.CurrentScene is BattleScene)
                                {
                                    if (_pokemonSelectArea.IsFirstPokemonFainting())
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        FinishContent();
                                    }
                                }
                                else
                                {
                                    FinishContent();
                                }
                            }
                            else
                            {
                                State = PokemonListContentState.Choosing_Action;
                            }
                        }
                    }
                    else
                    {
                        _selectedPokemonIdx = (int)value;
                    }
                }
                break;
            case PokemonListContentState.Choosing_Action:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = PokemonListContentState.Choosing_Pokemon;
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            string selectedAction = _actionSelectBox.GetSelectedBtnData() as string;

                            if (selectedAction == "Summary")
                            {
                                ContentManager.Instance.OpenPokemonSum(_pokemonSelectArea.GetSelectedBtnData() as Pokemon);

                                State = PokemonListContentState.Inactiving;
                            }
                            else if (selectedAction == "Switch")
                            {
                                State = PokemonListContentState.Choosing_Pokemon_To_Switch;
                            }
                            else if (selectedAction == "Send Out")
                            {
                                Pokemon pokemonSum = _pokemonSelectArea.GetSelectedBtnData() as Pokemon;
                                int selectedIdx = _pokemonSelectArea.GetSelectedIndex();

                                if (selectedIdx == 0 || pokemonSum.PokemonInfo.PokemonStatus == PokemonStatusCondition.Fainting)
                                {
                                    return;
                                }

                                ContentManager.Instance.PlayScreenEffecter("FadeOut");

                                State = PokemonListContentState.MovingToBattleScene;
                            }
                            else if (selectedAction == "Use")
                            {
                                Pokemon selectedPokemon = _pokemonSelectArea.GetSelectedBtnData() as Pokemon;

                                if (selectedPokemon.PokemonStat.Hp == selectedPokemon.PokemonStat.MaxHp)
                                {
                                    List<string> scripts = new List<string>()
                                    {
                                        $"Cannot use it to {selectedPokemon.PokemonInfo.NickName}!"
                                    };
                                    ContentManager.Instance.BeginScriptTyping(scripts);
                                    State = PokemonListContentState.Item_Cannot_Use_Scripting;
                                }
                                else
                                {
                                    if (!_isLoading)
                                    {
                                        _isLoading = true;

                                        C_UseItemInListScene useItemPacket = new C_UseItemInListScene();
                                        useItemPacket.PlayerId = Managers.Object.MyPlayerController.Id;
                                        useItemPacket.TargetPokemonOrder = _pokemonSelectArea.GetSelectedIndex();
                                        useItemPacket.UsedItemCategory = ContentManager.Instance.BagContent.GetSelectedItemCategory();
                                        useItemPacket.UsedItemOrder = ContentManager.Instance.BagContent.GetSelectedItemOrder();

                                        Managers.Network.Send(useItemPacket);
                                    }
                                }
                            }
                            else if (selectedAction == "Cancel")
                            {
                                State = PokemonListContentState.Choosing_Pokemon;
                            }
                        }
                    }
                }
                break;
            case PokemonListContentState.Choosing_Pokemon_To_Switch:
                {
                    if (value is Define.InputSelectBoxEvent)
                    {
                        Define.InputSelectBoxEvent inputEvent = (Define.InputSelectBoxEvent)value;

                        if (inputEvent == Define.InputSelectBoxEvent.BACK)
                        {
                            State = PokemonListContentState.Choosing_Pokemon;
                        }
                        else if (inputEvent == Define.InputSelectBoxEvent.SELECT)
                        {
                            if (_pokemonSelectArea.GetSelectedBtnData() as string == "Cancel")
                            {
                                FinishContent();
                            }
                            else
                            {
                                C_SwitchPokemon switchPacket = new C_SwitchPokemon();
                                switchPacket.OwnerId = Managers.Object.MyPlayerController.Id;
                                switchPacket.PokemonFromIdx = _selectedPokemonIdx;
                                switchPacket.PokemonToIdx = _selectedSwitchPokemonIdx;

                                Managers.Network.Send(switchPacket);

                                State = PokemonListContentState.Switching_Pokemon;

                                _pokemonSelectArea.MoveAndSwitchPokemonCard();
                            }
                        }
                    }
                    else
                    {
                        _selectedSwitchPokemonIdx = (int)value;
                    }
                }
                break;
            case PokemonListContentState.Switching_Pokemon:
                {
                    _pokemonSelectArea.ResetSelectedPokemon(false);

                    State = PokemonListContentState.Choosing_Pokemon;
                }
                break;
            case PokemonListContentState.Item_Cannot_Use_Scripting:
                {
                    State = PokemonListContentState.Choosing_Pokemon;
                }
                break;
            case PokemonListContentState.After_Item_Use_Scripting:
                {
                    if (_packet is S_UseItemInListScene)
                    {
                        ItemSummary usedItemSum = ((S_UseItemInListScene)_packet).ItemSum;

                        // 아이템도 사용처리
                        Item usedItem = ContentManager.Instance.BagContent.GetSelectedItem();
                        usedItem.UpdateItemSummary(usedItemSum);

                        if (usedItem.ItemCnt == 0)
                            ContentManager.Instance.BagContent.RemoveSelectedItem();
                        else
                            ContentManager.Instance.BagContent.UpdateSelectedItem();

                        if (Managers.Scene.CurrentScene is BattleScene)
                        {
                            Managers.Scene.CurrentScene.PopAllContents();
                        }
                        else if (Managers.Scene.CurrentScene is GameScene)
                        {
                            FinishContent();
                        }
                        
                        if (Managers.Scene.CurrentScene is BattleScene)
                        {
                            int selectedIdx = _pokemonSelectArea.GetSelectedIndex();

                            if (selectedIdx == 0)
                            {
                                ((BattleScene)Managers.Scene.CurrentScene).UpdateMyPokemonInfo(_pokemonSelectArea.GetSelectedBtnData() as Pokemon);
                            }
                        }

                        C_ProcessTurn processTurnPacket = new C_ProcessTurn();
                        processTurnPacket.PlayerId = Managers.Object.MyPlayerController.Id;

                        Managers.Network.Send(processTurnPacket);
                    }
                }
                break;
            case PokemonListContentState.MovingToBattleScene:
                {
                    int selectedIdx = _pokemonSelectArea.GetSelectedIndex();

                    Managers.Scene.CurrentScene.PopUntilSpecificChild<OnlineBattleContent>();

                    ((BattleScene)Managers.Scene.CurrentScene).SwitchPokemon(0, selectedIdx);

                    C_SwitchPokemon switchPacket = new C_SwitchPokemon();
                    switchPacket.OwnerId = Managers.Object.MyPlayerController.Id;
                    switchPacket.PokemonFromIdx = 0;
                    switchPacket.PokemonToIdx = selectedIdx;

                    Managers.Network.Send(switchPacket);
                }
                break;
            case PokemonListContentState.Inactiving:
                {
                    State = PokemonListContentState.Choosing_Pokemon;
                }
                break;
        }
    }

    public override void FinishContent()
    {
        State = PokemonListContentState.None;

        Managers.Scene.CurrentScene.FinishContents();
    }

    public void SetPokemonSelectArea(List<Pokemon> myPokemons, List<string> btnNames)
    {
        _pokemons = myPokemons;

        // 포켓몬 리스트 ui
        List<object> datas = new List<object>();
        for (int i = 0; i < _pokemons.Count; i++)
        {
            datas.Add(_pokemons[i]);
        }
        _pokemonSelectArea.CreateButton(_pokemons);

        _actionSelectBox.CreateButtons(btnNames, 1, 400, 100);
    }
}
