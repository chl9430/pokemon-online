using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class SaveManager
    {
        public class GameSaveDataCollection
        {
            public List<PlayerInfo> gameSaveDatas { get; set; } = new List<PlayerInfo>();
        }

        public static SaveManager Instance { get; } = new SaveManager();

        readonly string _saveFilePath = "../../../Data/GameSaveData.json";
        object _lock = new object();
        public Dictionary<string, PlayerInfo> _userSaveDataDict;

        void LoadAllGameSaveData()
        {
            if (!File.Exists(_saveFilePath))
            {
                _userSaveDataDict = new Dictionary<string, PlayerInfo>();
                return;
            }

            try
            {
                string jsonContent = File.ReadAllText(_saveFilePath);

                // 1. JSON 문자열을 루트 객체 LoginDataCollection으로 역직렬화
                GameSaveDataCollection loadedCollection = JsonConvert.DeserializeObject<GameSaveDataCollection>(jsonContent);

                if (loadedCollection == null || loadedCollection.gameSaveDatas == null)
                {
                    Console.WriteLine("[WARNING] JSON 파일은 존재하지만 유효한 데이터가 없습니다.");
                    _userSaveDataDict = new Dictionary<string, PlayerInfo>();
                    return;
                }

                // 2. 로드된 리스트를 메모리 내 Dictionary로 변환 (고속 검색)
                List<PlayerInfo> loadedList = loadedCollection.gameSaveDatas;

                lock (_lock)
                {
                    _userSaveDataDict = new Dictionary<string, PlayerInfo>();

                    // Username을 키로 사용하여 딕셔너리로 변환
                    _userSaveDataDict = loadedList
                        .Where(data => data != null && !string.IsNullOrEmpty(data.UserId))
                        .ToDictionary(data => data.UserId, data => data);
                }

                Console.WriteLine($"[SUCCESS] 총 {_userSaveDataDict.Count}개의 사용자 정보가 메모리 Dictionary에 로드되었습니다.");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"[ERROR] JSON 파일 역직렬화 오류: 파일 내용이 올바르지 않습니다. {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[FATAL] 사용자 데이터 로드 중 치명적인 오류 발생: {e.Message}");
            }
        }

        public void CheckGameSaveData(ClientSession session, string id)
        {
            lock (_lock)
            {
                if (_userSaveDataDict == null)
                {
                    LoadAllGameSaveData();
                }

                // 저장된 게임 데이터가 있다면
                if (_userSaveDataDict.ContainsKey(id))
                {
                    S_CheckSaveData checkSaveData = new S_CheckSaveData();
                    checkSaveData.FoundDataId = id;
                    checkSaveData.IsDataFound = true;

                    session.Send(checkSaveData);
                }
                else
                {
                    S_CheckSaveData checkSaveData = new S_CheckSaveData();
                    checkSaveData.FoundDataId = id;
                    checkSaveData.IsDataFound = false;

                    session.Send(checkSaveData);
                }
            }
        }

        public void SaveGameData(ClientSession session, string id, int objId)
        {
            lock (_lock)
            {
                // 데이터 존재 여부를 확인한다. 덮어쓰기
                if (_userSaveDataDict.TryGetValue(id, out PlayerInfo data))
                {
                    data = ObjectManager.Instance.Find(objId).MakePlayerInfo();
                    data.ObjectInfo.PosInfo.State = CreatureState.Idle;

                    _userSaveDataDict[id] = data;

                    GameSaveDataCollection dataCollection = new GameSaveDataCollection
                    {
                        gameSaveDatas = _userSaveDataDict.Values.ToList(),
                    };

                    string jsonString = JsonConvert.SerializeObject(dataCollection, Formatting.Indented);

                    // 6. 🚨 파일에 JSON 문자열 쓰기 (기존 파일 전체 덮어쓰기)
                    File.WriteAllText(_saveFilePath, jsonString);

                    S_SaveGameData savePacket = new S_SaveGameData();
                    savePacket.IsSuccess = true;

                    session.Send(savePacket);
                }
                else
                {
                    GameSaveDataCollection dataCollection = new GameSaveDataCollection
                    {
                        gameSaveDatas = _userSaveDataDict.Values.ToList(),
                    };

                    PlayerInfo playerInfo = ObjectManager.Instance.Find(objId).MakePlayerInfo();
                    playerInfo.ObjectInfo.PosInfo.State = CreatureState.Idle;

                    dataCollection.gameSaveDatas.Add(playerInfo);
                    _userSaveDataDict.Add(playerInfo.UserId, playerInfo);

                    string jsonString = JsonConvert.SerializeObject(dataCollection, Formatting.Indented);

                    try
                    {
                        File.WriteAllText(_saveFilePath, jsonString);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine($"[ERROR] 파일 저장 실패 (IO 오류): {e.Message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[ERROR] 로그인 정보 저장 중 알 수 없는 오류 발생: {e.Message}");
                    }

                    S_SaveGameData savePacket = new S_SaveGameData();
                    savePacket.IsSuccess = true;

                    session.Send(savePacket);
                }
            }
        }

        public PlayerInfo GetGameSaveData(string accountId)
        {
            if (_userSaveDataDict.TryGetValue(accountId, out var data))
            {
                PlayerInfo info = data.Clone();

                return info;
            }
            else
                return null;
        }
    }
}
