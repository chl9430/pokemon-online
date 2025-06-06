using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PokemonSummarySceneState
{
    SLIDE_NOT_MOVING = 0,
    SLIDE_MOVING = 1
}

public class PokemonSummaryUI : Action_UI
{
    float startTime;
    PokemonSummarySceneState sceneState;
    Vector2 oldMinPos;
    Vector2 oldMaxPos;
    Vector2 newMinPos;
    Vector2 newMaxPos;
    RectTransform selectedContent;
    // PokemonSummary _summary;

    [SerializeField] float slideSpeed;
    [SerializeField] RectTransform[] sliderContent;
    [SerializeField] RectTransform _indicator;

    [SerializeField] TextMeshProUGUI dictNum;
    [SerializeField] Image pokemonImg;
    [SerializeField] TextMeshProUGUI nickName;
    [SerializeField] TextMeshProUGUI pokemonName;
    // [SerializeField] Image catchBall;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] Image gender;
    [SerializeField] TextMeshProUGUI owner;
    [SerializeField] TextMeshProUGUI ownerId;
    [SerializeField] Image type1;
    [SerializeField] Image type2;
    [SerializeField] TextMeshProUGUI nature;
    [SerializeField] TextMeshProUGUI metLevel;
    // [SerializeField] TextMeshProUGUI metPlace;

    // [SerializeField] TextMeshProUGUI item;
    // [SerializeField] TextMeshProUGUI ribbon;
    [SerializeField] TextMeshProUGUI hpAndMaxHP;
    [SerializeField] TextMeshProUGUI maxHP;
    [SerializeField] TextMeshProUGUI attack;
    [SerializeField] TextMeshProUGUI defense;
    [SerializeField] TextMeshProUGUI speicalAttack;
    [SerializeField] TextMeshProUGUI speicalDefense;
    [SerializeField] TextMeshProUGUI speed;
    [SerializeField] TextMeshProUGUI totalEXP;
    [SerializeField] TextMeshProUGUI expToNextLevel;

    void Start()
    {
        /*
        {
            PokemonSummary dummySummary = new PokemonSummary();
            PokemonInfo dummyInfo = new PokemonInfo()
            {
                DictionaryNum = 35,
                NickName = "MESSI",
                PokemonName = "Charmander",
                Level = 10,
                Gender = PokemonGender.Male,
                Type1 = PokemonType.Fire,
                Type2 = PokemonType.Water,
                OwnerName = "CHRIS",
                OwnerId = 99999
            };
            PokemonSkill dummySkill = new PokemonSkill()
            {
                Stat = new PokemonStat()
                {
                    Hp = 10,
                    MaxHp = 100,
                    Attack = 50,
                    Defense = 40,
                    SpecialAttack = 70,
                    SpecialDefense = 40,
                    Speed = 60
                },
                RemainLevelExp = 53,
                TotalExp = 100,
            };
            PokemonBattleMove dummyBattleMove = new PokemonBattleMove()
            {
            };

            dummySummary.Info = dummyInfo;
            dummySummary.Skill = dummySkill;
            dummySummary.BattleMove = dummyBattleMove;
            summary = dummySummary;
        }
        */
    }

    public void FillPokemonSummary(PokemonSummary pokemonSum)
    {
        FillText(dictNum, $"No.{pokemonSum.PokemonInfo.DictionaryNum}");
        FillText(nickName, $"{pokemonSum.PokemonInfo.NickName}");
        FillText(pokemonName, $"/ {pokemonSum.PokemonInfo.PokemonName}");
        FillText(level, $"Lv.{pokemonSum.PokemonInfo.Level}");
        FillText(owner, $"Owner : {pokemonSum.PokemonInfo.OwnerName}");
        FillText(ownerId, $"ID : {pokemonSum.PokemonInfo.OwnerId}");
        FillText(nature, $"{pokemonSum.PokemonInfo.Nature} nature");
        FillText(metLevel, $"met at Lv.{pokemonSum.PokemonInfo.MetLevel}");

        FillImage(pokemonImg, $"Textures/Pokemon/{pokemonSum.PokemonInfo.PokemonName}");
        FillImage(gender, $"Textures/UI/PokemonGender_{pokemonSum.PokemonInfo.Gender}");
        FillImage(type1, $"Textures/UI/{pokemonSum.PokemonInfo.Type1}_Icon");
        FillImage(type2, $"Textures/UI/{pokemonSum.PokemonInfo.Type2}_Icon");

        FillText(hpAndMaxHP, $"{pokemonSum.PokemonStat.Hp} / {pokemonSum.PokemonStat.MaxHp}");
        FillText(attack, $"{pokemonSum.PokemonStat.Attack}");
        FillText(defense, $"{pokemonSum.PokemonStat.Defense}");
        FillText(speicalAttack, $"{pokemonSum.PokemonStat.SpecialAttack}");
        FillText(speicalDefense, $"{pokemonSum.PokemonStat.SpecialDefense}");
        FillText(speed, $"{pokemonSum.PokemonStat.Speed}");
        FillText(totalEXP, $"{pokemonSum.PokemonExpInfo.TotalExp}");
        FillText(expToNextLevel, $"{pokemonSum.PokemonExpInfo.RemainExpToNextLevel}");
    }

    void FillText(TextMeshProUGUI tmp, string text)
    {
        tmp.text = text;
    }

    void FillImage(Image img, string imgPath)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(imgPath);
        //float newWidth = sprite.rect.width * 8;
        //float newHeight = sprite.rect.height * 8;
        //Debug.Log(newWidth);
        //Debug.Log(newHeight);
        img.sprite = sprite;
        img.SetNativeSize();

        //RectTransform rt = img.rectTransform;
        //rt.anchorMin = Vector2.zero;
        //rt.anchorMax = Vector2.one;

        //rt.sizeDelta = new Vector2(newWidth, newHeight);
    }

    void Update()
    {
        switch (sceneState)
        {
            case PokemonSummarySceneState.SLIDE_NOT_MOVING:
                ChooseAction();
                break;
            case PokemonSummarySceneState.SLIDE_MOVING:
                MoveSlideContent();
                break;
        }
    }

    public override void ChooseAction()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (selectedIdx == sliderContent.Length - 1)
                return;

            selectedIdx++;

            SetSelectedSlideContent(-1);

            _indicator.anchorMax = new Vector2(1f / sliderContent.Length * (selectedIdx + 1), 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (selectedIdx == 0)
                return;

            SetSelectedSlideContent(1);

            selectedIdx--;

            _indicator.anchorMax = new Vector2(1f / sliderContent.Length * (selectedIdx + 1), 1);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Managers.Scene.CurrentScene.ScreenChanger.ChangeAndFadeOutScene(Define.Scene.PokemonList);
        }
    }

    void SetSelectedSlideContent(int dir)
    {
        sceneState = PokemonSummarySceneState.SLIDE_MOVING;
        startTime = Time.time;
        oldMinPos = sliderContent[selectedIdx].anchorMin;
        oldMaxPos = sliderContent[selectedIdx].anchorMax;

        float minX = oldMinPos.x + dir;
        float maxX = oldMaxPos.x + dir;

        newMinPos = new Vector2(minX, oldMinPos.y);
        newMaxPos = new Vector2(maxX, oldMaxPos.y);
        selectedContent = sliderContent[selectedIdx];
    }

    void MoveSlideContent()
    {
        float timeElapsed = Time.time - startTime;
        float t = Mathf.Clamp01(timeElapsed * slideSpeed);

        selectedContent.anchorMin = Vector2.Lerp(oldMinPos, newMinPos, t);
        selectedContent.anchorMax = Vector2.Lerp(oldMaxPos, newMaxPos, t);

        if (t >= 1f)
        {
            sceneState = PokemonSummarySceneState.SLIDE_NOT_MOVING;
            selectedContent.anchorMin = newMinPos;
            selectedContent.anchorMax = newMaxPos;
        }
    }
}
