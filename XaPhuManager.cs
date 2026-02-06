using System.Xml.Linq;

namespace GameClient
{
    /// <summary>
    /// Thông tin vị trí của Xa Phu
    /// </summary>
    public class XaPhuLocation
    {
        public int MapCode { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// Quản lý thông tin Xa Phu từ file AutoPath.xml
    /// Sử dụng Singleton pattern để load một lần và dùng lại
    /// </summary>
    public static class XaPhuManager
    {
        // Danh sách tất cả Xa Phu được load từ XML
        private static List<XaPhuLocation> _allXaPhu = new List<XaPhuLocation>();

        // Random instance để lấy ngẫu nhiên
        private static readonly Random _random = new Random();

        // Đường dẫn mặc định đến file XML
        private const string DEFAULT_XML_PATH = @"Config/AutoPath.xml";

        /// <summary>
        /// Khởi tạo và load dữ liệu Xa Phu từ file XML
        /// Gọi hàm này một lần trong Program.cs khi khởi động ứng dụng
        /// </summary>
        /// <param name="xmlFilePath">Đường dẫn đến file AutoPath.xml (mặc định: Config/AutoPath.xml)</param>
        public static void Init(string xmlFilePath = DEFAULT_XML_PATH)
        {
            _allXaPhu.Clear();

            try
            {
                XDocument doc = XDocument.Load(xmlFilePath);

                // Lấy tất cả các phần tử NPC trong thẻ NPCs
                var npcElements = doc.Descendants("NPC");

                foreach (var npc in npcElements)
                {
                    // Chỉ lấy các NPC có Name là "Xa Phu"
                    string name = npc.Attribute("Name")?.Value ?? "";
                    if (name == "Xa Phu")
                    {
                        var location = new XaPhuLocation
                        {
                            MapCode = int.Parse(npc.Attribute("MapCode")?.Value ?? "0"),
                            X = int.Parse(npc.Attribute("X")?.Value ?? "0"),
                            Y = int.Parse(npc.Attribute("Y")?.Value ?? "0")
                        };

                        _allXaPhu.Add(location);
                    }
                }

                Console.WriteLine($"[XaPhuManager]<Init> Da load {_allXaPhu.Count} Xa Phu tu file XML");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[XaPhuManager]<Init> Loi khi doc file XML: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả Xa Phu theo MapCode
        /// </summary>
        /// <param name="mapCode">Mã bản đồ</param>
        /// <returns>Danh sách các Xa Phu trên bản đồ đó</returns>
        public static List<XaPhuLocation> Get_XaPhu(int mapCode)
        {
            return _allXaPhu.Where(x => x.MapCode == mapCode).ToList();
        }

        /// <summary>
        /// Lấy ngẫu nhiên một Xa Phu theo MapCode
        /// </summary>
        /// <param name="mapCode">Mã bản đồ</param>
        /// <returns>Một Xa Phu ngẫu nhiên, hoặc null nếu không tìm thấy</returns>
        public static XaPhuLocation GetRandom_XaPhu(int mapCode)
        {
            var xaPhuList = Get_XaPhu(mapCode);

            if (xaPhuList.Count == 0)
            {
                return null;
            }

            // Lấy ngẫu nhiên trong danh sách (nếu có 1 thì trả về cái đó, nếu nhiều thì ngẫu nhiên)
            int randomIndex = _random.Next(xaPhuList.Count);
            return xaPhuList[randomIndex];
        }

        /// <summary>
        /// Lấy tổng số Xa Phu đã load
        /// </summary>
        public static int Count => _allXaPhu.Count;

        /// <summary>
        /// Kiểm tra xem MapCode có Xa Phu không
        /// </summary>
        /// <param name="mapCode">Mã bản đồ</param>
        /// <returns>True nếu có ít nhất 1 Xa Phu</returns>
        public static bool HasXaPhu(int mapCode)
        {
            return _allXaPhu.Any(x => x.MapCode == mapCode);
        }
    }
}
