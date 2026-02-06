using ProtoBuf;

namespace GameClient.Data
{
    [ProtoContract]
    public class SpriteMoveData
    {
        /// <summary>
        /// ID nhân vật
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tọa độ bắt đầu X
        /// </summary>
        [ProtoMember(2)]
        public int FromX { get; set; }

        /// <summary>
        /// Tọa độ bắt đầu Y
        /// </summary>
        [ProtoMember(3)]
        public int FromY { get; set; }

        /// <summary>
        /// Tọa độ đích X
        /// </summary>
        [ProtoMember(4)]
        public int ToX { get; set; }

        /// <summary>
        /// Tọa độ đích Y
        /// </summary>
        [ProtoMember(5)]
        public int ToY { get; set; }

        /// <summary>
        /// Chuỗi mã hóa đoạn đường di chuyển
        /// </summary>
        [ProtoMember(6)]
        public string PathString = "";
    }
}
