#undef DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GameClient.Data
{
    public class TeleportData
    {
        public int Code { get; set; }
        public int To { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Radius { get; set; }
        public int Camp { get; set; }  // Thêm Camp nếu cần dùng

        /// <summary>
        /// Đọc file Teleport XML và lấy thông tin theo Code
        /// </summary>
        /// <param name="xmlPath">Đường dẫn file XML</param>
        /// <param name="code">Mã Code cần tìm</param>
        /// <returns>TeleportData hoặc null nếu không tìm thấy</returns>
        public static TeleportData? GetTeleportByCode(string xmlPath, int code)
        {
            try
            {
                if (!File.Exists(xmlPath))
                {
                    Console.WriteLine($"[TeleportData] File not found: {xmlPath}");
                    return null;
                }

                XDocument doc = XDocument.Load(xmlPath);

                var element = doc.Descendants("Teleport")
                                 .FirstOrDefault(x => (int?)x.Attribute("Code") == code);

                if (element != null)
                {
                    return new TeleportData
                    {
                        Code = (int?)element.Attribute("Code") ?? 0,
                        To = (int?)element.Attribute("To") ?? 0,
                        ToX = (int?)element.Attribute("ToX") ?? 0,
                        ToY = (int?)element.Attribute("ToY") ?? 0,
                        X = (int?)element.Attribute("X") ?? 0,
                        Y = (int?)element.Attribute("Y") ?? 0,
                        Radius = (int?)element.Attribute("Radius") ?? 0,
                        Camp = (int?)element.Attribute("Camp") ?? 0
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TeleportData] Error reading file: {ex.Message}");
            }

            return null;
        }
    }
}
