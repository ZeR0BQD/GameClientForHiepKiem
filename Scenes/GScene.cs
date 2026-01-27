using HSGameEngine.Tools.AStar;
using System.Drawing;
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
            this.pathFinderFast = new PathFinderFast(this.Obstructions, DynamicObstructions, new byte[0])
            {
                Formula = HeuristicFormula.Manhattan,
                Diagonals = true,
                HeuristicEstimate = 2,
                ReopenCloseNodes = true,
                SearchLimit = limit,
            };
        }

        public List<Vector2> FindPath(Vector2 fromPos, Vector2 toPos)
        {
            Point from = new Point((int)fromPos.x / GridSizeX, (int)fromPos.y / GridSizeY);
            Point to = new Point((int)toPos.x / GridSizeX, (int)toPos.y / GridSizeY);
            return FindPathUsingAStar(from, to);
        }

        private List<Vector2> FindPathUsingAStar(Point fromPos, Point toPos)
        {

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
    }
}
