using Google.Protobuf.Protocol;
using System.Globalization;

namespace Server
{
    public class Pokemon
    {
        PokemonInfo _pokemonInfo;
        PokemonStat _pokemonStat;
        PokemonExpInfo _expInfo;
        List<PokemonMove> _pokemonMoves;
        PokemonMove _selectedMove;
        PokemonMove _noPPMove;
        PokemonMove _newLearnableMove;
        PokemonSummaryDictData _summaryDictData;

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

        public PokemonMove SelectedMove
        {
            set { _selectedMove = value; }
            get { return _selectedMove; }
        }

        public Pokemon(string pokemonName, string pokemonNickName, int level, string ownerName, int ownerId, int remainHp = -1)
        {
            _pokemonInfo = new PokemonInfo();
            _pokemonStat = new PokemonStat();
            _expInfo = new PokemonExpInfo();
            _pokemonMoves = new List<PokemonMove>();
            _noPPMove = new PokemonMove(1, 30, 100, "Struggle", PokemonType.Normal, MoveCategory.Physical);

            Random random = new Random();

            if (DataManager.PokemonSummaryDict.TryGetValue(pokemonName, out _summaryDictData))
            {
                GenderRatioData[] genderRatioDatas = _summaryDictData.genderRatio;
                GenderRatioData foundGenderData = genderRatioDatas[0];

                float ran = (float)Math.Round(random.NextDouble() * 100.0f, 1);

                int totalRateCnt = 0;

                for (int i = 0; i < genderRatioDatas.Length; i++)
                {
                    float rateCnt = 0;
                    float genderRatio = genderRatioDatas[i].ratio;

                    bool found = false;

                    while (rateCnt < genderRatio)
                    {
                        if (((float)totalRateCnt / 10f) != ran)
                        {
                            totalRateCnt += 1;
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

                int hp = remainHp;

                if (hp > _pokemonStat.MaxHp)
                    hp = _pokemonStat.MaxHp;

                _pokemonStat.Hp = hp == -1 ? _pokemonStat.MaxHp : hp;

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
                    if (moveNames[i] == "")
                    {
                        continue;
                    }

                    if (DataManager.PokemonMoveDict.TryGetValue(moveNames[i], out PokemonMoveDictData moveDictData))
                    {
                        PokemonMove move = new PokemonMove(moveDictData.maxPP, moveDictData.movePower, moveDictData.moveAccuracy, moveDictData.moveName, moveDictData.moveType, moveDictData.moveCategory);

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

        public int GetSelectedMoveIdx()
        {
            return _pokemonMoves.IndexOf(_selectedMove);
        }

        public bool DidSelectedMoveHit()
        {
            _selectedMove.CurPP--;

            Random random = new Random();
            int ran = random.Next(1, 101);

            if (ran > _selectedMove.MoveAccuracy)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SetSelectedMove(int moveOrder)
        {
            _selectedMove = _pokemonMoves[moveOrder];
        }

        public void SetNoPPMoveToSelectedMove()
        {
            _selectedMove = _noPPMove;
        }

        public void GetDamaged(int damage)
        {
            _pokemonStat.Hp -= damage;

            if (_pokemonStat.Hp <= 0)
            {
                _pokemonStat.Hp = 0;
                _pokemonInfo.PokemonStatus = PokemonStatusCondition.Fainting;
            }
        }

        public void GetExp(int exp, S_CheckAndApplyRemainedExp expPacket)
        {
            if (_expInfo.RemainExpToNextLevel >= exp)
            {
                _expInfo.CurExp += exp;
                _expInfo.TotalExp += exp;
                _expInfo.RemainExpToNextLevel -= exp;

                if (_expInfo.RemainExpToNextLevel == 0)
                {
                    expPacket.StatDiff = LevelUp();
                    expPacket.NewMoveSum = CheckNewLearnableMove();

                    expPacket.ExpInfo = _expInfo;
                    expPacket.PokemonLevel = _pokemonInfo.Level;
                    expPacket.PokemonStat = _pokemonStat;
                }
                else
                {
                    expPacket.ExpInfo = _expInfo;
                }
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

        public PokemonMoveSummary CheckNewLearnableMove()
        {
            LearnableMoveData[] moveDatas = _summaryDictData.learnableMoves;
            string foundMoveName = "";
            PokemonMoveSummary moveSum = null;

            for (int i = 0; i < moveDatas.Length; i++)
            {
                if (_pokemonInfo.Level > moveDatas[i].learnLevel)
                    continue;
                else if (_pokemonInfo.Level == moveDatas[i].learnLevel)
                    foundMoveName = moveDatas[i].moveName;
                else if (_pokemonInfo.Level < moveDatas[i].learnLevel)
                    break;
            }

            if (foundMoveName != "")
            {
                if (DataManager.PokemonMoveDict.TryGetValue(foundMoveName, out PokemonMoveDictData moveDictData))
                {
                    PokemonMove move = new PokemonMove(moveDictData.maxPP, moveDictData.movePower, moveDictData.moveAccuracy, moveDictData.moveName, moveDictData.moveType, moveDictData.moveCategory);

                    if (_pokemonMoves.Count < 4)
                    {
                        _pokemonMoves.Add(move);
                    }
                    else if (_pokemonMoves.Count == 4)
                        _newLearnableMove = move;

                    moveSum = new PokemonMoveSummary();
                    moveSum = move.MakePokemonMoveSummary();
                }
                else
                {
                    Console.WriteLine("Cannot find Pokemon Move Data!");
                }
            }

            return moveSum;
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

        public PokemonSummary MakePokemonSummary()
        {
            PokemonSummary pokemonSum = new PokemonSummary();
            pokemonSum.PokemonInfo = _pokemonInfo;
            pokemonSum.PokemonStat = _pokemonStat;
            pokemonSum.PokemonExpInfo = _expInfo;

            foreach (PokemonMove move in _pokemonMoves)
            {
                pokemonSum.PokemonMoves.Add(move.MakePokemonMoveSummary());
            }
            pokemonSum.NoPPMove = _noPPMove.MakePokemonMoveSummary();

            return pokemonSum;
        }
    }
}
