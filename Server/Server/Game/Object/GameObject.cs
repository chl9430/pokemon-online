using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class GameObject
    {
        public GameObjectType ObjectType
        {
            get
            {
                return Info.ObjectType;
            }
        }

        public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }

        public GameRoom Room { get; set; }
        public ObjectInfo Info { get; set; } = new ObjectInfo();
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();
        string _name;

        public string Name { get { return _name; } set { _name = value; } }


        public GameObject()
        {
            Info.PosInfo = PosInfo;
        }

        public virtual void Update()
        {

        }

        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            }

            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
            }
        }
    }
}
