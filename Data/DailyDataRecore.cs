using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Data
{
    /// <summary>
    ///  Các sự kiện đánh dấu theo ngày
    /// </summary>
    ///
    [ProtoContract]
    public class DailyDataRecore
    {
        /// <summary>
        /// ID của ngày tính theo DAYOFYEAR
        /// </summary>
        ///
        [ProtoMember(1)]
        public int DayID { get; set; }

        /// <summary>
        /// Danh sách các sự kiện cần đánh dấu
        /// Ví dụ : VẬT PHẨM A ĐÃ ĂN MẤY LẦN
        /// </summary>
        ///
        [ProtoMember(2)]
        public Dictionary<int, int> EventRecoding { get; set; } = new Dictionary<int, int>();
    }

    /// <summary>
    /// Các sự kiện đánh dấu theo tuần
    /// </summary>
    ///
    [ProtoContract]
    public class WeekDataRecore
    {
        /// <summary>
        /// ID của ngày tính theo DAYOFYEAR
        /// </summary>
        ///
        [ProtoMember(1)]
        public int WeekID { get; set; }

        /// <summary>
        /// Danh sách các sự kiện cần đánh dấu
        /// Ví dụ : VẬT PHẨM A ĐÃ ĂN MẤY LẦN
        /// </summary>
        ///
        [ProtoMember(2)]
        public Dictionary<int, int> EventRecoding { get; set; } = new Dictionary<int, int>();
    }

    /// <summary>
    /// Các sự kiện đánh dấu theo tuần
    /// </summary>
    ///
    [ProtoContract]
    public class ForeverRecore
    {
        ///
        [ProtoMember(1)]
        public Dictionary<int, int> EventRecoding { get; set; } = new Dictionary<int, int>();
    }


}
