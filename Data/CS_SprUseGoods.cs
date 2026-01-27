using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Data
{
    /// <summary>
    /// Gói tin gửi từ Client về Server thông báo đối tượng sử dụng vật phẩm
    /// </summary>
    [ProtoContract]
    public class CS_SprUseGoods
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Danh sách DbID vật phẩm
        /// </summary>
        [ProtoMember(2)]
        public List<int> DbIds { get; set; }
    }
}
