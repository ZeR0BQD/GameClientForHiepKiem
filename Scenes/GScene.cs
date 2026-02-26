using GameClient.Data;
using HSGameEngine.Tools.AStar;
using HSGameEngine.Tools.AStarEx;
using System.Drawing;
using System.Security.Policy;
using System.Xml.Linq;
using UnityEngine;

namespace GameClient.Scenes
{
    public class GScene
    {
        private PathFinderFast? pathFinderFast;

        private static Dictionary<string, GScene> _loadedScenes = new Dictionary<string, GScene>();
        private static object locker = new object();

        /// <summary>
        /// Lấy GScene theo MapCode. Nếu chưa có sẽ load mới và cache lại.
        /// </summary>
        /// <param name="mapCode"></param>
        /// <returns></returns>
        public static GScene Get(string mapCode)
        {
            lock (locker)
            {
                if (_loadedScenes.TryGetValue(mapCode, out var scene))
                {
                    return scene;
                }

                var newScene = new GScene();
                newScene.Init(mapCode);
                _loadedScenes[mapCode] = newScene;

                return newScene;
            }
        }

        protected GScene()
        {
        }

        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public int GridSizeXNum { get; set; }
        public int GridSizeYNum { get; set; }

        /// <summary>
        /// Kích thước lưới X (POT)
        /// </summary>
        public int GridSizeX { get; set; } = 10;

        /// <summary>
        /// Kích thước lưới Y (POT)
        /// </summary>
        public int GridSizeY { get; set; } = 10;

        /// <summary>
        /// Kích thước lưới X gốc
        /// </summary>
        public int OriginGridSizeXNum { get; set; }

        /// <summary>
        /// Kích thước lưới Y gốc
        /// </summary>
        public int OriginGridSizeYNum { get; set; }

        /// <summary>
        /// Khu vực an toàn
        /// </summary>
        public byte[,] SafeAreas { get; set; }
        /// <summary>
        /// Vùng Block không đi được
        /// </summary>
        public byte[,] Obstructions { get; set; }
        /// <summary>
        /// Vùng làm mờ
        /// </summary>
        public byte[,] BlurPositions { get; set; }

        /// <summary>
        /// Vùng Block động được đóng mở tù ý
        /// </summary>
        public byte[,] DynamicObstructions { get; set; }
        /// <summary>
        /// Danh sách các nhãn Obs động được mở
        /// </summary>
        public HashSet<byte> OpenedDynamicObsLabels { get; set; }

        /// <summary>
        /// Danh sách Teleport, Key=Code, Value=TeleportData
        /// </summary>
        public Dictionary<int, TeleportData> Teleports { get; set; } = new Dictionary<int, TeleportData>();

        public void Init(string map)
        {
            string configPath = string.Format("MapConfig/{0}/Obs.xml", map);
            XElement xmlNode = XElement.Load(configPath);

            this.MapWidth = int.Parse(xmlNode.Attribute("MapWidth").Value);
            this.MapHeight = int.Parse(xmlNode.Attribute("MapHeight").Value);

            GridSizeX = 20;
            GridSizeY = 20;

            this.OriginGridSizeXNum = int.Parse(xmlNode.Attribute("OriginGridSizeXNum").Value);
            this.OriginGridSizeYNum = int.Parse(xmlNode.Attribute("OriginGridSizeYNum").Value);

            int wGridsNum = (this.MapWidth - 1) / this.GridSizeX + 1;
            int hGridsNum = (this.MapHeight - 1) / this.GridSizeY + 1;

            wGridsNum = (int)Math.Ceiling(Math.Log(wGridsNum, 2));
            wGridsNum = (int)Math.Pow(2, wGridsNum);

            hGridsNum = (int)Math.Ceiling(Math.Log(hGridsNum, 2));
            hGridsNum = (int)Math.Pow(2, hGridsNum);


            this.Obstructions = new byte[wGridsNum, hGridsNum];
            this.BlurPositions = new byte[wGridsNum, hGridsNum];

            byte[] obsBytes = File.ReadAllBytes(string.Format("MapConfig/{0}/Obs.txt", map));
            byte[] blurBytes = File.ReadAllBytes(string.Format("MapConfig/{0}/Blur.txt", map));
            string dynamicObs = string.Format("MapConfig/{0}/DynamicObs.txt", map);
            byte[] dynObsBytes = new byte[0];
            this.DynamicObstructions = new byte[wGridsNum, hGridsNum];
            if (File.Exists(dynamicObs))
            {
                dynObsBytes = File.ReadAllBytes(dynamicObs);
                this.DynamicObstructions.FromBytes<byte>(dynObsBytes);
            }
            byte[] safeAreaBytes = File.ReadAllBytes(string.Format("MapConfig/{0}/SafeArea.txt", map));

            this.Obstructions.FromBytes<byte>(obsBytes);
            this.BlurPositions.FromBytes<byte>(blurBytes);
            this.SafeAreas = new byte[wGridsNum, hGridsNum];
            this.SafeAreas.FromBytes<byte>(safeAreaBytes);

            var limit = this.MapWidth * this.MapHeight;
            this.pathFinderFast = new PathFinderFast(this.Obstructions, this.DynamicObstructions, this.OpenedDynamicObsLabels)
            {
                Formula = HeuristicFormula.Manhattan,
                Diagonals = true,
                HeuristicEstimate = 2,
                ReopenCloseNodes = true,
                SearchLimit = limit,
            };


            /// Load Teleport Data
            this.Teleports.Clear();
            string teleportPath = string.Format("MapConfig/{0}/teleports.xml", map);
            if (File.Exists(teleportPath))
            {
                try
                {
                    XDocument doc = XDocument.Load(teleportPath);
                    foreach (var teleport in doc.Descendants("Teleport"))
                    {
                        int code = (int?)teleport.Attribute("Code") ?? 0;
                        int x = (int?)teleport.Attribute("X") ?? 0;
                        int y = (int?)teleport.Attribute("Y") ?? 0;
                        int to = (int?)teleport.Attribute("To") ?? 0;
                        int toX = (int?)teleport.Attribute("ToX") ?? 0;
                        int toY = (int?)teleport.Attribute("ToY") ?? 0;
                        int radius = (int?)teleport.Attribute("Radius") ?? 0;
                        int camp = (int?)teleport.Attribute("Camp") ?? 0;

                        this.Teleports[code] = new TeleportData()
                        {
                            Code = code,
                            X = x,
                            Y = y,
                            To = to,
                            ToX = toX,
                            ToY = toY,
                            Radius = radius,
                            Camp = camp
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GScene] Error loading teleports: {ex.Message}");
                }
            }
        }

        public List<Vector2> FindPath(RoleData roleData, Vector2 fromPos, Vector2 toPos)
        {
            Point from = new Point((int)fromPos.x / GridSizeX, (int)fromPos.y / GridSizeY);
            Point to = new Point((int)toPos.x / GridSizeX, (int)toPos.y / GridSizeY);

            /// Nếu vị trí đích nằm trên ô có vật cản
            if (!this.CanMove(to))
            {
                /// Tìm một điểm bất kỳ trên đường nối 2 điểm mà không chứa vật cản
                if (this.FindLinearNoObsMaxPoint(roleData, to, out Point maxPoint))
                {
                    to = maxPoint;

                    /// Cập nhật lại vị trí ToPos
                    toPos = this.GridPositionToWorldPosition(new Vector2(to.X, to.Y));
                }
                /// Nếu không tìm thấy điểm không chứa vật cản
                else
                {
                    return null;
                }
            }

            /// Sử dụng A* tìm đường đi
            List<Vector2> nodes = this.FindPathUsingAStar(from, to);
            nodes.Reverse();
            /// Nếu danh sách nút tìm được nhỏ hơn 2
            if (nodes.Count < 2)
            {
                return new List<Vector2>();
            }

            /// Danh sách điểm trên đường đi
            List<Vector2> result = new List<Vector2>();

            {
                result.Add(fromPos);
            }

            /// Thêm tất cả các nút tìm được trên đường đi vào danh sách
            for (int i = 1; i < nodes.Count; i++)
            {
                /// Chuyển từ tọa độ lưới ra tọa độ trên bản đồ
                Vector2 toWorldPos = new Vector2(nodes[i].x * GridSizeX, nodes[i].y * GridSizeY);
                result.Add(toWorldPos);
            }
            result[result.Count - 1] = toPos;
            return result;
        }

        private List<Vector2> FindPathUsingAStar(Point fromPos, Point toPos)
        {
            if (null == this.pathFinderFast)
            {
                var limit = this.MapWidth * this.MapHeight;
                this.pathFinderFast = new PathFinderFast(this.Obstructions, this.DynamicObstructions, this.OpenedDynamicObsLabels)
                {
                    Formula = HeuristicFormula.Manhattan,
                    Diagonals = true,
                    HeuristicEstimate = 2,
                    ReopenCloseNodes = true,
                    SearchLimit = limit,
                    Punish = null,
                    MaxNum = Math.Max(this.GridSizeXNum, this.GridSizeYNum),
                };
            }

            this.pathFinderFast.EnablePunish = false;
            List<PathFinderNode> nodeList = this.pathFinderFast.FindPath(fromPos, toPos);
            if (null == nodeList || nodeList.Count <= 0)
            {
                return new List<Vector2>();
            }

            List<Vector2> path = new List<Vector2>();

            for (int i = 0; i < nodeList.Count; i++)
            {
                path.Add(new Vector2(nodeList[i].X, nodeList[i].Y));
            }

            /// Làm mịn đường đi
            path = SmoothPath(path, this.Obstructions);

            return path;
        }

        /// <summary>
        /// Làm mịn đường đi
        /// </summary>
        /// <param name="path">Đường đi</param>
        /// <param name="Obstructions">Ma trận mô tả bản đồ gốc (1: Đi được, 0: Không đi được)</param>
        /// <returns></returns>
        public List<Vector2> SmoothPath(List<Vector2> path, byte[,] Obstructions)
        {
            if (path.Count < 2)
            {
                return path;
            }

            List<Vector2> newPath = new List<Vector2>();

            int len = path.Count;
            int x0 = (int)path[0].x,        // path start x
                y0 = (int)path[0].y,        // path start y
                x1 = (int)path[len - 1].x,  // path end x
                y1 = (int)path[len - 1].y,  // path end y
                sx, sy,                     // current start coordinate
                ex, ey,                     // current end coordinate
                i, j;
            Vector2 coord, testCoord;
            List<Vector2> line;
            bool blocked;

            sx = x0;
            sy = y0;
            newPath.Add(new Vector2(sx, sy));

            for (i = 2; i < len; ++i)
            {
                coord = path[i];
                ex = (int)coord.x;
                ey = (int)coord.y;
                line = Interpolate(sx, sy, ex, ey);

                blocked = false;
                for (j = 1; j < line.Count; ++j)
                {
                    testCoord = line[j];

                    if (Obstructions[(int)testCoord.x, (int)testCoord.y] == 0)
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked)
                {
                    Vector2 lastValidCoord = path[i - 1];
                    newPath.Add(lastValidCoord);
                    sx = (int)lastValidCoord.x;
                    sy = (int)lastValidCoord.y;
                }
            }
            newPath.Add(new Vector2(x1, y1));

            return newPath;
        }

        /// <summary>
		/// Hàm nội suy
		/// </summary>
		/// <param name="x0"></param>
		/// <param name="y0"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <returns></returns>
		private List<Vector2> Interpolate(int x0, int y0, int x1, int y1)
        {
            List<Vector2> line = new List<Vector2>();
            int sx, sy, dx, dy, err, e2;

            dx = Math.Abs(x1 - x0);
            dy = Math.Abs(y1 - y0);

            sx = (x0 < x1) ? 1 : -1;
            sy = (y0 < y1) ? 1 : -1;

            err = dx - dy;

            while (true)
            {
                line.Add(new Vector2(x0, y0));

                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }

            return line;
        }

        /// <summary>
        /// Kiểm tra ô tương ứng có thể đi được không (tọa độ lưới)
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool CanMove(Point node)
        {
            /// Nếu vượt quá phạm vi X
            if (node.X >= this.Obstructions.GetUpperBound(0) || node.X >= this.OriginGridSizeXNum)
            {
                return false;
            }
            /// Nếu vượt quá phạm vi Y
            else if (node.Y >= this.Obstructions.GetUpperBound(1) || node.Y >= this.OriginGridSizeYNum)
            {
                return false;
            }
            /// Nếu tọa độ âm
            else if (node.X < 0 || node.Y < 0)
            {
                return false;
            }

            /// Nếu dính điểm Block
            if (this.Obstructions[node.X, node.Y] == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tìm ô gần nhất xung quanh không chứa vật cản
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool FindLinearNoObsMaxPoint(RoleData roleData, Point p, out Point maxPoint)
        {
            maxPoint = new Point(0, 0);
            List<ANode> path = new List<ANode>();
            Bresenham(path, (int)((roleData.PosX / this.GridSizeX)), (int)((roleData.PosY / this.GridSizeY)), (int)((p.X)), (int)((p.Y)), this.Obstructions);
            if (path.Count > 1)
            {
                maxPoint = new Point(path[path.Count - 1].x, path[path.Count - 1].y);
                path.Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Giải thuật Bresenham vẽ tập hợp các điểm tạo thành đường thẳng
        /// </summary>
        /// <param name="s"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public static bool Bresenham(List<ANode> s, int x1, int y1, int x2, int y2, byte[,] obs)
        {
            int t, x, y, dx, dy, error;
            bool flag = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
            if (flag)
            {
                t = x1;
                x1 = y1;
                y1 = t;
                t = x2;
                x2 = y2;
                y2 = t;
            }

            bool reverse = false;
            if (x1 > x2)
            {
                t = x1;
                x1 = x2;
                x2 = t;
                t = y1;
                y1 = y2;
                y2 = t;
                reverse = true;
            }
            dx = x2 - x1;
            dy = Math.Abs(y2 - y1);
            error = dx / 2;
            for (x = x1, y = y1; x <= x2; ++x)
            {
                if (flag)
                {
                    if (null != s)
                    {
                        s.Add(new ANode(y, x));
                    }
                }
                else
                {
                    if (null != s)
                    {
                        s.Add(new ANode(x, y));
                    }
                }

                error -= dy;
                if (error < 0)
                {
                    if (y1 < y2)
                        ++y;
                    else
                        --y;
                    error += dx;
                }
            }

            if (reverse)
            {
                s.Reverse();
            }

            List<ANode> s1 = GetLinearPath(s, obs);
            bool res = (s1.Count == s.Count);

            s.Clear();
            for (int i = 0; i < s1.Count; i++)
            {
                s.Add(s1[i]);
            }

            return res;
        }

        /// <summary>
        /// Trả về danh sách các điểm có thể đi được trên đường đi
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static List<ANode> GetLinearPath(List<ANode> s, byte[,] obs)
        {
            List<ANode> s1 = new List<ANode>();
            for (int i = 0; i < s.Count; i++)
            {
                if (s[i].x >= obs.GetUpperBound(0) || s[i].y >= obs.GetUpperBound(1))
                {
                    continue;
                }

                if (0 == obs[s[i].x, s[i].y])
                {
                    break;
                }

                s1.Add(s[i]);
            }

            return s1;
        }
        /// <summary>
        /// Chuyển từ tọa độ lưới ra tọa độ trên bản đồ
        /// </summary>
        /// <param name="gamePos"></param>
        /// <returns></returns>
        public Vector2 GridPositionToWorldPosition(Vector2 gamePos)
        {
            float x = gamePos.x * this.GridSizeX;
            float y = gamePos.y * this.GridSizeY;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Chuyển từ tọa độ trên bản đồ về tọa độ lưới
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public Vector2 WorldPositionToGridPosition(Vector2 worldPos)
        {
            float x = worldPos.x / this.GridSizeX;
            float y = worldPos.y / this.GridSizeY;

            return new Vector2(x, y);
        }
        public bool InSafeArea(Vector2 position)
        {
            /// Nếu không có khu an toàn
            if (this.SafeAreas == null)
            {
                /// Không có
                return false;
            }

            try
            {
                /// Tọa độ lưới
                Vector2 gridPos = this.WorldPositionToGridPosition(position);
                /// Trả về kết quả
                return this.SafeAreas[(int)gridPos.x, (int)gridPos.y] == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
