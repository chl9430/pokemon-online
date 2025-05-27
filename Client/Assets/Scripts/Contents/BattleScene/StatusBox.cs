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
    BaseScene _scene;
    StatusBoxState _uiState = StatusBoxState.NONE;

    [SerializeField] TextMeshProUGUI maxHPText;
    [SerializeField] TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI defenseText;
    [SerializeField] TextMeshProUGUI specialAttackText;
    [SerializeField] TextMeshProUGUI specialDefenseText;
    [SerializeField] TextMeshProUGUI speedText;

    void Start()
    {
        _scene = Managers.Scene.CurrentScene;
    }

    void Update()
    {
        switch (_uiState)
        {
            case StatusBoxState.SHOWING_RATE:
                {
                    if (Input.GetKeyDown(KeyCode.D))
                    {
                        _scene.DoNextAction();
                    }
                }
                break;
            case StatusBoxState.SHOWING_FINAL_STAT:
                {
                    if (Input.GetKeyDown(KeyCode.D))
                    {
                        _scene.DoNextAction();
                    }
                }
                break;
        }
    }

    public void SetStatusDiffRate(LevelUpStatusRate rate)
    {
        _uiState = StatusBoxState.SHOWING_RATE;

        maxHPText.text = "+" + rate.MaxHP.ToString();
        attackText.text = "+" + rate.Attack.ToString();
        defenseText.text = "+" + rate.Defense.ToString();
        specialAttackText.text = "+" + rate.SpecialAttack.ToString();
        specialDefenseText.text = "+" + rate.SpecialDefense.ToString();
        speedText.text = "+" + rate.Speed.ToString();
    }

    public void ShowFinalStat(PokemonStat stat)
    {
        _uiState = StatusBoxState.SHOWING_FINAL_STAT;

        maxHPText.text = stat.MaxHp.ToString();
        attackText.text = stat.Attack.ToString();
        defenseText.text = stat.Defense.ToString();
        specialAttackText.text = stat.SpecialAttack.ToString();
        specialDefenseText.text = stat.SpecialDefense.ToString();
        speedText.text = stat.Speed.ToString();
    }
}