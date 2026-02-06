using GameClient.Data;
using HSGameEngine.Tools.AStar;
using HSGameEngine.Tools.AStarEx;
using Org.BouncyCastle.Asn1.Pkcs;
using System.Drawing;
using System.Security.Policy;
using System.Xml.Linq;
using UnityEngine;

namespace GameClient.Scenes
{
    public class GScene
    {
        private PathFinderFast? pathFinderFast;

        private static GScene? instance;
        private static object locker = new object();
        public static GScene Instance
        {
            get
            {
                lock (locker)
                {
                    if (instance == null) instance = new GScene();

                    return instance;
                }
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
        public byte[,]? SafeAreas { get; set; }
        /// <summary>
        /// Vùng Block không đi được
        /// </summary>
        public byte[,]? Obstructions { get; set; }
        /// <summary>
        /// Vùng làm mờ
        /// </summary>
        public byte[,]? BlurPositions { get; set; }

        /// <summary>
        /// Vùng Block động được đóng mở tù ý
        /// </summary>
        public byte[,]? DynamicObstructions { get; set; }
        /// <summary>
        /// Danh sách các nhãn Obs động được mở
        /// </summary>
        public HashSet<byte> OpenedDynamicObsLabels { get; set; } = new HashSet<byte>();

        public void Init(string map)
        {
            string configPath = string.Format("MapConfig/{0}/Obs.xml", map);
            XElement xmlNode = XElement.Load(configPath);

            this.MapWidth = int.Parse(xmlNode.Attribute("MapWidth")?.Value ?? "0");
            this.MapHeight = int.Parse(xmlNode.Attribute("MapHeight")?.Value ?? "0");

            GridSizeX = 20;
            GridSizeY = 20;

            this.OriginGridSizeXNum = int.Parse(xmlNode.Attribute("OriginGridSizeXNum")?.Value ?? "0");
            this.OriginGridSizeYNum = int.Parse(xmlNode.Attribute("OriginGridSizeYNum")?.Value ?? "0");

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
        }

        public bool CanMove(Point node)
        {
            /// Nếu Obstructions chưa được khởi tạo
            if (this.Obstructions == null)
            {
                return false;
            }
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

        public Point GetRandomNoObsPointAroundPos(Point gridPoint)
        {
            if (this.Obstructions == null)
            {
                return gridPoint;
            }
            byte[,] obs = this.Obstructions;

            int gridX = (int)(gridPoint.X);
            int gridY = (int)(gridPoint.Y);
            if (gridX >= obs.GetUpperBound(0) || gridY >= obs.GetUpperBound(1))
            {
                return gridPoint;
            }
            if (obs[gridX, gridY] == 1)
            {
                return gridPoint;
            }
            Point p = gridPoint;
            int maxGridX = (int)((this.MapWidth - 1) / this.GridSizeX) + 1;
            int maxGridY = (int)((this.MapHeight - 1) / this.GridSizeY) + 1;
            int added = 1;
            int newX1 = 0;
            int newY1 = 0;
            int newX2 = 0;
            int newY2 = 0;
            while (true)
            {
                newX1 = gridX + added;
                newY1 = gridY + added;
                newX2 = gridX - added;
                newY2 = gridY - added;
                int total = 8;
                if ((0 <= newX1 && newX1 < maxGridX) && (0 <= newY1 && newY1 < maxGridY))
                {
                    total--;
                    if (obs[newX1, newY1] == 1)
                    {
                        p = new Point(newX1, newY1);
                        break;
                    }
                }
                if ((0 <= newX1 && newX1 < maxGridX) && (0 <= newY2 && newY2 < maxGridY))
                {
                    total--;
                    if (obs[newX1, newY2] == 1)
                    {
                        p = new Point(newX1, newY2);
                        break;
                    }
                }
                if ((0 <= newX2 && newX2 < maxGridX) && (0 <= newY1 && newY1 < maxGridY))
                {
                    total--;
                    if (obs[newX2, newY1] == 1)
                    {
                        p = new Point(newX2, newY1);
                        break;
                    }
                }
                if ((0 <= newX2 && newX2 < maxGridX) && (0 <= newY2 && newY2 < maxGridY))
                {
                    total--;
                    if (obs[newX2, newY2] == 1)
                    {
                        p = new Point(newX2, newY2);
                        break;
                    }
                }
                if ((0 <= newX1 && newX1 < maxGridX))
                {
                    total--;
                    if (obs[newX1, gridY] == 1)
                    {
                        p = new Point(newX1, gridY);
                        break;
                    }
                }
                if ((0 <= newY1 && newY1 < maxGridY))
                {
                    total--;
                    if (obs[gridX, newY1] == 1)
                    {
                        p = new Point(gridX, newY1);
                        break;
                    }
                }
                if ((0 <= newX2 && newX2 < maxGridX))
                {
                    total--;
                    if (obs[newX2, gridY] == 1)
                    {
                        p = new Point(newX2, gridY);
                        break;
                    }
                }
                if ((0 <= newY2 && newY2 < maxGridY))
                {
                    total--;
                    if (obs[gridX, newY2] == 1)
                    {
                        p = new Point(gridX, newY2);
                        break;
                    }
                }
                if (total >= 8)
                {
                    break;
                }
                added++;
            }
            return p;
        }

        public Vector2 GridPositionToWorldPosition(Vector2 gamePos)
        {
            float x = gamePos.x * this.GridSizeX;
            float y = gamePos.y * this.GridSizeY;

            return new Vector2(x, y);
        }

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

        public bool Bresenham(List<ANode>? s, int x1, int y1, int x2, int y2, byte[,]? obs)
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
                s!.Reverse();
            }

            List<ANode> s1 = GetLinearPath(s!, obs!);
            bool res = (s1.Count == s!.Count);

            s.Clear();
            for (int i = 0; i < s1.Count; i++)
            {
                s.Add(s1[i]);
            }

            return res;
        }

        private bool FindLinearNoObsMaxPoint(RoleData roleData, Point p, out Point maxPoint)
        {
            maxPoint = new Point(0, 0);
            if (this.Obstructions == null)
            {
                return false;
            }
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

        public List<Vector2>? FindPath(RoleData roleData, Vector2 fromPos, Vector2 toPos)
        {
            Point fromGridPOINT = new Point((int)fromPos.x / GridSizeX, (int)fromPos.y / GridSizeY);
            Point toGridPOINT = new Point((int)toPos.x / GridSizeX, (int)toPos.y / GridSizeY);

            /// Nếu vị trí hiện đang đứng nằm trên ô có vật cản
            if (!this.CanMove(fromGridPOINT))
            {
                /// Tìm một vị trí bất kỳ xung quanh không có vật cản
                fromGridPOINT = GetRandomNoObsPointAroundPos(fromGridPOINT);

                /// Cập nhật lại vị trí FromPos
                fromPos = GridPositionToWorldPosition(new Vector2(fromGridPOINT.X, fromGridPOINT.Y));

                /// Nếu không tìm thấy vị trí nào xung quanh không có vật cản
                if (!this.CanMove(fromGridPOINT))
                {
                    return null;
                }
            }

            /// Nếu vị trí đích nằm trên ô có vật cản
			if (!this.CanMove(toGridPOINT))
            {
                /// Tìm một điểm bất kỳ trên đường nối 2 điểm mà không chứa vật cản
                if (this.FindLinearNoObsMaxPoint(roleData, toGridPOINT, out Point maxPoint))
                {
                    toGridPOINT = maxPoint;

                    /// Cập nhật lại vị trí ToPos
                    toPos = GridPositionToWorldPosition(new Vector2(toGridPOINT.X, toGridPOINT.Y));
                }
                /// Nếu không tìm thấy điểm không chứa vật cản
                else
                {
                    return null;
                }
            }

            /// Nếu vị trí đầu và cuối cùng một ô lưới thì cho chạy giữa 2 vị trí này luôn
			if (fromGridPOINT == toGridPOINT)
            {
                return new List<Vector2>()
                {
                    fromPos, toPos
                };
            }

            /// Nếu vị trí đích không thể đi được
			if (!this.CanMove(toGridPOINT))
            {
                return null;
            }
            List<Vector2> nodes = this.FindPathUsingAStar(fromGridPOINT, toGridPOINT);
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
                result.Add(GridPositionToWorldPosition(nodes[i]));
            }

            {
                result[result.Count - 1] = toPos;
            }
            return result;
        }

        private List<Vector2> FindPathUsingAStar(Point fromPos, Point toPos)
        {
            if (null == this.pathFinderFast)
            {
                if (this.Obstructions == null || this.DynamicObstructions == null)
                {
                    return new List<Vector2>();
                }
                var limit = this.MapWidth * this.MapHeight;
                this.pathFinderFast = new PathFinderFast(this.Obstructions, this.DynamicObstructions, this.OpenedDynamicObsLabels)
                {
                    Formula = HeuristicFormula.Manhattan,
                    Diagonals = true,
                    HeuristicEstimate = 2,
                    ReopenCloseNodes = true,
                    SearchLimit = limit,
                    Punish = null!,
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
            path = SmoothPath(path, this.Obstructions!);

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
    }
}
