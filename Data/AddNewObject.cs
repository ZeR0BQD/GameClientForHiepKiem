using ProtoBuf;
using System.Collections.Generic;

namespace GameClient.Data
{
    /// <summary>
    /// Nhóm các đối tượng xuất hiện xung quanh người chơi
    /// </summary>
    [ProtoContract]
    public class AddNewObjects
    {
        /// <summary>
        /// Danh sách quái
        /// </summary>
        // [ProtoMember(1)]
        // public List<MonsterData> Monsters { get; set; }

        /// <summary>
        /// Danh sách người chơi
        /// </summary>
        [ProtoMember(2)]
        public List<RoleDataMini> Players { get; set; } = new List<RoleDataMini>();

        /// <summary>
        /// Danh sách NPC
        /// </summary>
        // [ProtoMember(8)]
        // public List<NPCRole> NPCs { get; set; }


    }
}
