using Google.Protobuf.Protocol;
using System.Globalization;

namespace Server
{
    public class Pokemon : GameObject
    {
        PokemonInfo _pokemonInfo;
        PokemonStat _pokemonStat;
        PokemonExpInfo _expInfo;
        List<PokemonMove> _pokemonMoves;
        PokemonSummaryDictData _summaryDictData;
        PokemonMoveDictData _moveDictData;

        public PokemonInfo PokemonInfo
        {
            get { return _pokemonInfo; }
        }

        public PokemonStat PokemonStat
        {
            get { return _pokemonStat; }
        }

        public PokemonExpInfo ExpInfo
        {
            get { return _expInfo; }
        }

        public List<PokemonMove> PokemonMoves
        {
            get { return _pokemonMoves; }
        }

        public Pokemon(string pokemonName, string pokemonNickName, int level, string ownerName, int ownerId)
        {
            _pokemonInfo = new PokemonInfo();
            _pokemonStat = new PokemonStat();
            _expInfo = new PokemonExpInfo();
            _pokemonMoves = new List<PokemonMove>();

            Random random = new Random();

            if (DataManager.PokemonSummaryDict.TryGetValue(pokemonName, out _summaryDictData))
            {
                GenderRatioData[] genderRatioDatas = _summaryDictData.genderRatio;
                GenderRatioData foundGenderData = genderRatioDatas[0];

                float ran = (float)(random.NextDouble() * 100.0f);

                float totalRateCnt = 0;

                for (int i = 0; i < genderRatioDatas.Length; i++)
                {
                    float rateCnt = 0;
                    float genderRatio = genderRatioDatas[i].ratio;

                    bool found = false;

                    while (rateCnt < genderRatio)
                    {
                        if (totalRateCnt != ran)
                        {
                            totalRateCnt += 0.1f;
                            rateCnt += 0.1f;
                        }
                        else
                        {
                            foundGenderData = genderRatioDatas[i];
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        break;
                }

                // 성별
                PokemonGender foundGender = (PokemonGender)Enum.Parse(typeof(PokemonGender), foundGenderData.gender);

                // 성격
                PokemonNature[] allNatures = (PokemonNature[])Enum.GetValues(typeof(PokemonNature));
                int randomNatureIndex = random.Next(0, allNatures.Length);
                PokemonNature randomNature = allNatures[randomNatureIndex];

                // 기본정보
                _pokemonInfo.DictionaryNum = _summaryDictData.dictionaryNum;
                _pokemonInfo.NickName = pokemonNickName;
                _pokemonInfo.PokemonName = pokemonName;
                _pokemonInfo.Level = level;
                _pokemonInfo.Gender = foundGender;
                _pokemonInfo.OwnerName = ownerName;
                _pokemonInfo.OwnerId = ownerId;
                _pokemonInfo.Type1 = (PokemonType)Enum.Parse(typeof(PokemonType), _summaryDictData.type1);
                _pokemonInfo.Type2 = (PokemonType)Enum.Parse(typeof(PokemonType), _summaryDictData.type2);
                _pokemonInfo.Nature = randomNature;
                _pokemonInfo.MetLevel = level;

                // 스텟
                UpdateStat();

                // 경험치
                int prevLevelTotalExp = 0;
                int nextLevelTotalExp = 0;

                if (level > 1)
                    prevLevelTotalExp = (int)Math.Pow(level - 1, 3);

                if (level < 100)
                    nextLevelTotalExp = (int)Math.Pow(level, 3);

                _expInfo.CurExp = 0;
                _expInfo.TotalExp = prevLevelTotalExp;
                _expInfo.RemainExpToNextLevel = nextLevelTotalExp - prevLevelTotalExp;

                // 기술
                LearnableMoveData[] learnMoveDatas = _summaryDictData.learnableMoves;

                string[] moveNames = new string[4];

                int foundIdx = FindLastLearnableMoveIndex(learnMoveDatas, level);
                int moveIdxToFill = 0;
                int fillCnt = 0;

                for (int i = moveNames.Length - 1; i >= 0; i--)
                {
                    int idx = foundIdx - i;

                    if (idx < 0)
                        idx += (moveNames.Length - 1 - foundIdx);

                    if (fillCnt <= foundIdx)
                        moveNames[moveIdxToFill] = learnMoveDatas[idx].moveName;
                    else
                        moveNames[moveIdxToFill] = "";

                    fillCnt++;
                    moveIdxToFill++;
                }

                for (int i = 0; i < moveNames.Length; i++)
                {
                    if (DataManager.PokemonMoveDict.TryGetValue(moveNames[i], out _moveDictData))
                    {
                        PokemonMove move = new PokemonMove()
                        {
                            MoveName = _moveDictData.moveName,
                            MovePower = _moveDictData.movePower,
                            MoveAccuracy = _moveDictData.moveAccuracy,
                            CurPP = _moveDictData.maxPP,
                            MaxPP = _moveDictData.maxPP,
                            MoveType = _moveDictData.moveType,
                            MoveCategory = _moveDictData.moveCategory,
                        };

                        _pokemonMoves.Add(move);
                    }
                    else
                    {
                        Console.WriteLine("Cannot find Pokemon Move Data!");
                    }
                }
            }
            else
            {
                Console.WriteLine("Cannot find Pokemon Base Stat!");
            }
        }

        int FindLastLearnableMoveIndex(LearnableMoveData[] moveDatas, int pokemonLevel)
        {
            int low = 0;
            int high = moveDatas.Length - 1;
            int resultIndex = -1;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;

                if (moveDatas[mid].learnLevel <= pokemonLevel)
                {
                    resultIndex = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            return resultIndex;
        }

        public void GetDamage(int damage)
        {
            _pokemonStat.Hp -= damage;

            if (_pokemonStat.Hp < 0)
                _pokemonStat.Hp = 0;
        }

        public void GetExp(int exp)
        {
            if (_expInfo.RemainExpToNextLevel >= exp)
            {
                _expInfo.CurExp += exp;
                _expInfo.TotalExp += exp;
                _expInfo.RemainExpToNextLevel -= exp;
            }
            else
            {
                return;
            }
        }

        public LevelUpStatusDiff LevelUp()
        {
            if (_pokemonInfo.Level < 100)
            {
                PokemonStat prevStat = new PokemonStat()
                {
                    MaxHp = _pokemonStat.MaxHp,
                    Attack = _pokemonStat.Attack,
                    Defense = _pokemonStat.Defense,
                    SpecialAttack = _pokemonStat.SpecialAttack,
                    SpecialDefense = _pokemonStat.SpecialDefense,
                    Speed = _pokemonStat.Speed,
                };

                _pokemonInfo.Level++;
                _expInfo.CurExp = 0;
                _expInfo.RemainExpToNextLevel = ((int)Math.Pow(_pokemonInfo.Level, 3)) - ((int)Math.Pow(_pokemonInfo.Level - 1, 3));

                UpdateStat();

                LevelUpStatusDiff levelUpDiff = new LevelUpStatusDiff() 
                {
                    MaxHP = _pokemonStat.MaxHp - prevStat.MaxHp,
                    Attack = _pokemonStat.Attack - prevStat.Attack,
                    Defense = _pokemonStat.Defense - prevStat.Defense,
                    SpecialAttack = _pokemonStat.SpecialAttack - prevStat.SpecialAttack,
                    SpecialDefense = _pokemonStat.SpecialDefense - prevStat.SpecialDefense,
                    Speed = _pokemonStat.Speed - prevStat.Speed,
                };

                _pokemonStat.Hp += levelUpDiff.MaxHP;

                return levelUpDiff;
            }
            else
            {
                return null;
            }
        }

        public void UpdateStat()
        {
            _pokemonStat.MaxHp = CalPokemonStat(true, _summaryDictData.maxHp, _pokemonInfo.Level);
            _pokemonStat.Attack = CalPokemonStat(false, _summaryDictData.attack, _pokemonInfo.Level);
            _pokemonStat.Defense = CalPokemonStat(false, _summaryDictData.defense, _pokemonInfo.Level);
            _pokemonStat.SpecialAttack = CalPokemonStat(false, _summaryDictData.specialAttack, _pokemonInfo.Level);
            _pokemonStat.SpecialDefense = CalPokemonStat(false, _summaryDictData.specialDefense, _pokemonInfo.Level);
            _pokemonStat.Speed = CalPokemonStat(false, _summaryDictData.speed, _pokemonInfo.Level);
        }

        public int CalPokemonStat(bool isHP, int baseStat, int level)
        {
            int stat = 0;

            if (isHP)
                stat = (int)(((((float)baseStat) * 2f) * ((float)level) / 100f) + 10f + ((float)level));
            else
                stat = (int)(((((float)baseStat) * 2f) * ((float)level) / 100f) + 5f);

            return stat;
        }
    }
}
