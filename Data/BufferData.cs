using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Data
{
    /// <summary>
    /// Cầu hình Thực thể Buff
    /// </summary>
    [ProtoContract]
    public class BufferData
    {
        /// <summary>
        /// BuffID
        /// </summary>
        [ProtoMember(1)]
        public int BufferID { get; set; } = 0;

        /// <summary>
        /// Thời gian bắt đầu
        /// </summary>
        [ProtoMember(2)]
        public long StartTime { get; set; } = 0;

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        [ProtoMember(3)]
        public long BufferSecs { get; set; } = 0;

        /// <summary>
        /// Cấp độ Buff
        /// </summary>
        [ProtoMember(4)]
        public long BufferVal { get; set; } = 0;

        /// <summary>
        /// ProDict tùy chọn (có từ vật phẩm)
        /// </summary>
        [ProtoMember(5)]
        public string CustomProperty { get; set; } = "";
    }
}
