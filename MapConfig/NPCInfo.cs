using System;

namespace GameClient.MapConfig
{
    /// <summary>
    /// Thong tin NPC trong map
    /// </summary>
    public class NPCInfo
    {
        /// <summary>
        /// Ma NPC
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Ten NPC
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Toa do X
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Toa do Y
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Huong quay (0-7)
        /// </summary>
        public int Dir { get; set; }

        /// <summary>
        /// ID Script xu ly
        /// </summary>
        public int ScriptID { get; set; }

        /// <summary>
        /// Chuc danh
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Ten hien thi tren minimap
        /// </summary>
        public string? MinimapName { get; set; }

        /// <summary>
        /// Co hien thi tren minimap khong
        /// </summary>
        public bool VisibleOnMinimap { get; set; }
    }
}
