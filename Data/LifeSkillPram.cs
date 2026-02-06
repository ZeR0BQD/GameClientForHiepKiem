using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Data
{
    /// <summary>
    /// Thông tin kỹ năng sống
    /// </summary>
    [ProtoContract]
    public class LifeSkillPram
    {
        /// <summary>
        /// ID kỹ năng
        /// </summary>
        [ProtoMember(1)]
        public int LifeSkillID { get; set; }

        /// <summary>
        /// Cấp độ kỹ năng
        /// </summary>
        [ProtoMember(2)]
        public int LifeSkillLevel { get; set; }

        /// <summary>
        /// Kinh nghiệm kỹ năng
        /// </summary>
        [ProtoMember(3)]
        public int LifeSkillExp { get; set; }

    }
}