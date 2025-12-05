using Google.Protobuf.Protocol;
using System.Collections;
using TMPro;
using UnityEngine;

public enum StatusBoxState
{
    NONE = 0,
    SHOWING_RATE = 1,
    SHOWING_FINAL_STAT = 2,
}

public class StatusBox : MonoBehaviour
{
    LevelUpStatusDiff _statDiff;
    PokemonStat _upgradedStat;
    StatusBoxState _uiState = StatusBoxState.NONE;

    public StatusBoxState State 
    { 
        set 
        {  
            _uiState = value;

            if (_uiState == StatusBoxState.SHOWING_RATE)
                ShowStatusDiffRate();
            else if (_uiState == StatusBoxState.SHOWING_FINAL_STAT)
                ShowFinalStat();
        } 
    }

    [SerializeField] TextMeshProUGUI maxHPText;
    [SerializeField] TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI defenseText;
    [SerializeField] TextMeshProUGUI specialAttackText;
    [SerializeField] TextMeshProUGUI specialDefenseText;
    [SerializeField] TextMeshProUGUI speedText;

    void Update()
    {
        switch (_uiState)
        {
            case StatusBoxState.SHOWING_RATE:
                {
                    if (Input.GetKeyDown(KeyCode.D))
                    {
                        State = StatusBoxState.SHOWING_FINAL_STAT;
                    }
                }
                break;
            case StatusBoxState.SHOWING_FINAL_STAT:
                {
                    if (Input.GetKeyDown(KeyCode.D))
                    {
                        State = StatusBoxState.NONE;
                        Managers.Scene.CurrentScene.DoNextAction();
                    }
                }
                break;
        }
    }

    public void ShowStatusDiffRate()
    {
        _uiState = StatusBoxState.SHOWING_RATE;

        maxHPText.text = "+" + _statDiff.MaxHP.ToString();
        attackText.text = "+" + _statDiff.Attack.ToString();
        defenseText.text = "+" + _statDiff.Defense.ToString();
        specialAttackText.text = "+" + _statDiff.SpecialAttack.ToString();
        specialDefenseText.text = "+" + _statDiff.SpecialDefense.ToString();
        speedText.text = "+" + _statDiff.Speed.ToString();
    }

    public void ShowFinalStat()
    {
        _uiState = StatusBoxState.SHOWING_FINAL_STAT;

        maxHPText.text = _upgradedStat.MaxHp.ToString();
        attackText.text = _upgradedStat.Attack.ToString();
        defenseText.text = _upgradedStat.Defense.ToString();
        specialAttackText.text = _upgradedStat.SpecialAttack.ToString();
        specialDefenseText.text = _upgradedStat.SpecialDefense.ToString();
        speedText.text = _upgradedStat.Speed.ToString();
    }

    public void SetLevelUpStatusBox(LevelUpStatusDiff diff, PokemonStat stat)
    {
        _statDiff = diff;
        _upgradedStat = stat;
    }
}