using GameClient.Scenes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace GameClient.MapConfig
{
    /// <summary>
    /// Helper class để load và lấy MapCode từ MapConfig.xml
    /// </summary>
    public static class MapConfigHelper
    {
        private static Dictionary<int, string>? _mapCache;
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapConfig", "MapConfig.xml");

        /// <summary>
        /// Cache danh sach NPC theo mapCode, key = mapCode (vi du: "TuongDuong")
        /// Moi map duoc load 1 lan va luu lai, khong bi ghi de khi client khac o map khac
        /// </summary>
        private static readonly Dictionary<string, List<NPCInfo>> _npcCache = new Dictionary<string, List<NPCInfo>>();

        /// <summary>
        /// Load config từ file XML (tự động gọi nếu chưa load)
        /// </summary>
        private static void EnsureLoaded()
        {
            if (_mapCache != null) return;

            _mapCache = new Dictionary<int, string>();

            if (!File.Exists(ConfigPath))
            {
                Console.WriteLine($"[MapConfigHelper] Không tìm thấy file config: {ConfigPath}");
                return;
            }

            try
            {
                XDocument doc = XDocument.Load(ConfigPath);
                foreach (var map in doc.Descendants("Map"))
                {
                    if (int.TryParse(map.Attribute("ID")?.Value, out int id))
                    {
                        string mapCode = map.Attribute("MapCode")?.Value ?? "";
                        _mapCache[id] = mapCode;
                    }
                }
                Console.WriteLine($"[MapConfigHelper] Đã load {_mapCache.Count} maps từ config");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapConfigHelper] Lỗi khi load config: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy MapCode theo ID
        /// </summary>
        /// <param name="mapId">ID của map (ví dụ: 32)</param>
        /// <returns>MapCode string (ví dụ: "TuongDuong"), hoặc null nếu không tìm thấy</returns>
        public static string? GetMapCode(int mapId)
        {
            EnsureLoaded();
            return _mapCache?.TryGetValue(mapId, out var code) == true ? code : null;
        }

        /// <summary>
        /// Tim NPC theo mapCode va Code cua NPC
        /// Moi map co danh sach NPC rieng biet, khong bi conflict khi nhieu client o cac map khac nhau
        /// </summary>
        /// <param name="mapCode">MapCode (vi du: "TuongDuong")</param>
        /// <param name="code">Code cua NPC (vi du: 55 = Tong Hieu Uy)</param>
        /// <returns>NPCInfo hoac null neu khong tim thay</returns>
        public static NPCInfo? GetNPCByCode(string mapCode, int code)
        {
            if (_npcCache.TryGetValue(mapCode, out var npcs))
            {
                return npcs.FirstOrDefault(n => n.Code == code);
            }
            return null;
        }

        /// <summary>
        /// Load danh sach NPC tu file npcs.xml cua map.
        /// Co cache: neu da load roi thi khong load lai, tranh conflict khi nhieu client o cung map.
        /// </summary>
        /// <param name="mapCode">MapCode (vi du: "TuongDuong")</param>
        private static void LoadNPCs(string mapCode)
        {
            // Da co trong cache roi, khong can load lai
            if (_npcCache.ContainsKey(mapCode)) return;

            string npcPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapConfig", mapCode, "npcs.xml");

            if (!File.Exists(npcPath))
            {
                Console.WriteLine($"[MapConfigHelper]<LoadNPCs> Khong tim thay file NPC: {npcPath}");
                // Luu empty list de tranh load lai nhieu lan
                _npcCache[mapCode] = new List<NPCInfo>();
                return;
            }

            try
            {
                var npcList = new List<NPCInfo>();
                XDocument doc = XDocument.Load(npcPath);

                foreach (var npc in doc.Descendants("NPC"))
                {
                    npcList.Add(new NPCInfo
                    {
                        Code = int.Parse(npc.Attribute("Code")?.Value ?? "0"),
                        Name = npc.Attribute("Name")?.Value ?? "",
                        X = int.Parse(npc.Attribute("X")?.Value ?? "0"),
                        Y = int.Parse(npc.Attribute("Y")?.Value ?? "0"),
                        Dir = int.Parse(npc.Attribute("Dir")?.Value ?? "0"),
                        ScriptID = int.Parse(npc.Attribute("ScriptID")?.Value ?? "0"),
                        Title = npc.Attribute("Title")?.Value,
                        MinimapName = npc.Attribute("MinimapName")?.Value,
                        VisibleOnMinimap = npc.Attribute("VisibleOnMinimap")?.Value == "1"
                    });
                }

                _npcCache[mapCode] = npcList;
                Console.WriteLine($"[MapConfigHelper]<LoadNPCs> Da load {npcList.Count} NPCs tu map {mapCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapConfigHelper]<LoadNPCs> Loi khi load NPC: {ex.Message}");
                _npcCache[mapCode] = new List<NPCInfo>();
            }
        }

        /// <summary>
        /// Lấy MapCode theo ID và tự động gọi GScene.Instance.Init()
        /// </summary>
        /// <param name="mapId">ID của map (ví dụ: 32)</param>
        /// <returns>MapCode string đã được init, hoặc null nếu không tìm thấy</returns>
        public static string? InitSceneByMapId(int mapId)
        {
            string? mapCode = GetMapCode(mapId);

            if (string.IsNullOrEmpty(mapCode))
            {
                Console.WriteLine($"[MapConfigHelper]<InitSceneByMapId> Không tìm thấy MapCode cho ID: {mapId}");
                return null;
            }

            Console.WriteLine($"[MapConfigHelper]<InitSceneByMapId> Init scene với MapCode: {mapCode} (ID: {mapId})");
            GScene.Get(mapCode);

            // Load NPC sau khi init scene (co cache, neu da load roi se bo qua)
            LoadNPCs(mapCode);

            return mapCode;
        }
    }
}
