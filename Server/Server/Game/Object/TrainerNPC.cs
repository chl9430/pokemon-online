using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class TrainerNPC : NPC
    {
        BattleNPCDictData _npcDictData;

        string[] _beforeBattleScripts;
        string[] _afterBattleScripts;

        public BattleNPCDictData BattleNPCDictData {  get { return _npcDictData; } }

        public void SetNPCId(int id)
        {
            Id = id;

            if (DataManager.BattleNPCDict.TryGetValue(Id, out _npcDictData))
            {
                _beforeBattleScripts = _npcDictData.beforeBattleScripts;
                _afterBattleScripts = _npcDictData.afterBattleScripts;
            }
        }

        public string[] GetTalk(int npcNumer)
        {
            if (npcNumer < Id)
            {
                return _beforeBattleScripts;
            }
            else
            {
                return _afterBattleScripts;
            }
        }

        public bool CanBattle(int npcNumber)
        {
            if (npcNumber < Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
