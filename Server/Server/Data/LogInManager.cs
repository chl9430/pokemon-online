using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class LoginDataCollection
    {
        // JSON 키 "logInDatas"와 일치합니다.
        public List<LogInData> logInDatas { get; set; } = new List<LogInData>();
    }

    public class LogInData
    {
        // C# 표준에 따라 속성(Property) 형태로 정의
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastLoginTime { get; set; }
    }

    public class LogInManager
    {
        public static LogInManager Instance { get; } = new LogInManager();

        Dictionary<string, LogInData> _userDataMap;

        readonly string _saveFilePath = "../../../Data/LogInData.json";

        object _lock = new object();

        void LoadAllUsers()
        {
            if (!File.Exists(_saveFilePath))
            {
                Console.WriteLine("[INFO] 저장 파일이 존재하지 않습니다. 새 딕셔너리를 초기화합니다.");
                _userDataMap = new Dictionary<string, LogInData>();
                return;
            }

            try
            {
                string jsonContent = File.ReadAllText(_saveFilePath);

                // 1. JSON 문자열을 루트 객체 LoginDataCollection으로 역직렬화
                LoginDataCollection loadedCollection = JsonConvert.DeserializeObject<LoginDataCollection>(jsonContent);

                if (loadedCollection == null || loadedCollection.logInDatas == null)
                {
                    Console.WriteLine("[WARNING] JSON 파일은 존재하지만 유효한 데이터가 없습니다.");
                    _userDataMap = new Dictionary<string, LogInData>();
                    return;
                }

                // 2. 로드된 리스트를 메모리 내 Dictionary로 변환 (고속 검색)
                List<LogInData> loadedList = loadedCollection.logInDatas;

                lock (_lock)
                {
                    _userDataMap = new Dictionary<string, LogInData>();

                    // Username을 키로 사용하여 딕셔너리로 변환
                    _userDataMap = loadedList
                        .Where(data => data != null && !string.IsNullOrEmpty(data.Username))
                        .ToDictionary(data => data.Username, data => data);
                }

                Console.WriteLine($"[SUCCESS] 총 {_userDataMap.Count}개의 사용자 정보가 메모리 Dictionary에 로드되었습니다.");
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

        public void CheckLogInInfo(ClientSession session, string id, string pw)
        {
            lock (_lock)
            {
                if (_userDataMap == null)
                {
                    LoadAllUsers();
                }

                // 아이디가 일치한다면
                if (_userDataMap.TryGetValue(id, out LogInData data))
                {
                    // 비밀번호도 맞다면
                    if (data.PasswordHash == HashPassword(pw))
                    {
                        // 로그인 성공, 데이터 이어받기 질문
                        S_LogIn logInPacket = new S_LogIn();
                        logInPacket.LogInResult = LogInResultType.Success;

                        session.Send(logInPacket);
                    }
                    else
                    {
                        // 비밀번호 불일치로 로그인 실패
                        S_LogIn logInPacket = new S_LogIn();
                        logInPacket.LogInResult = LogInResultType.PassswordError;

                        session.Send(logInPacket);
                    }
                }
                else
                {
                    // 존재하지 않는 아이디로 로그인 실패(계정 생성 유도)
                    S_LogIn logInPacket = new S_LogIn();
                    logInPacket.LogInResult = LogInResultType.IdError;

                    session.Send(logInPacket);
                }
            }
        }

        public void CreateNewAccount(ClientSession session, string id, string pw)
        {
            lock (_lock)
            {
                // 아이디 존재 여부를 확인한다.
                if (_userDataMap.ContainsKey(id))
                {
                    S_CreateAccount createAccount = new S_CreateAccount();
                    createAccount.IsSuccess = false;

                    session.Send(createAccount);
                }
                else
                {
                    LoginDataCollection dataCollection = new LoginDataCollection
                    {
                        logInDatas = _userDataMap.Values.ToList()
                    };

                    LogInData dataToSave = new LogInData
                    {
                        Username = id,
                        PasswordHash = HashPassword(pw),
                        LastLoginTime = DateTime.UtcNow
                    };

                    dataCollection.logInDatas.Add(dataToSave);
                    _userDataMap.Add(dataToSave.Username, dataToSave);

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

                    S_CreateAccount createAccount = new S_CreateAccount();
                    createAccount.IsSuccess = true;

                    session.Send(createAccount);
                }
            }
        }

        string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // 비밀번호를 바이트 배열로 변환
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // 바이트 배열을 16진수 문자열로 변환하여 저장
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
