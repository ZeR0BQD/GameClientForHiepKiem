using HSGameEngine.Tools.AStarEx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Tools
{
    #region Structs
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NodeFast
    {
        #region Variables Declaration
        public double f;
        public double g;
        public double h;
        public int parentX;
        public int parentY;
        #endregion
    }
    #endregion
    /// <summary>
    /// Quản lý lưới
    /// </summary>
    public class NodeGrid
    {
        private static NodeFast[,] nodes;

        /// <summary>
        /// Mảng thông tin vật cản
        /// </summary>
        private byte[,] obs;

        /// <summary>
        /// Mảng thông tin điểm mờ ở client và khu vực liên thông ở GS
        /// </summary>
        private byte[,] blur;

        /// <summary>
        /// Mảng thông tin khu an toàn
        /// </summary>
        private byte[,] safeAreas;

        /// <summary>
        /// Mảng thông tin Obs động
        /// </summary>
        private byte[,] dynamicObs;

        /// <summary>
        /// Tổng số ô chiều dọc
        /// </summary>
        private static int numCols;

        /// <summary>
        /// Tổng số ô chiều ngang
        /// </summary>
        private static int numRows;

        /// <summary>
        /// Danh sách nhãn obs động đã được mở có thể đi vào
        /// </summary>
        private readonly Dictionary<int, HashSet<byte>> openDynamicObsLabels = new Dictionary<int, HashSet<byte>>();

        /// <summary>
        /// Khởi tạo
        /// </summary>
        /// <param name="numCols"></param>
        /// <param name="numRows"></param>
        public NodeGrid(int numCols, int numRows)
        {
            this.SetSize(numCols, numRows);
        }

        /// <summary>
        /// Trả về danh sách vật cản
        /// </summary>
        /// <returns></returns>
        public byte[,] GetFixedObstruction()
        {
            return this.obs;
        }

        /// <summary>
        /// Trả về danh sách khu vực liên thông
        /// </summary>
        /// <returns></returns>
        public byte[,] GetBlurObstruction()
        {
            return this.blur;
        }

        /// <summary>
        /// Trả về danh sách các điểm khu an toàn
        /// </summary>
        /// <returns></returns>
        public byte[,] GetSafeAreas()
        {
            return this.safeAreas;
        }

        /// <summary>
        /// Trả về danh sách các điểm Obs động
        /// </summary>
        /// <returns></returns>
        public byte[,] GetDynamicObstruction()
        {
            return this.dynamicObs;
        }

        /// <summary>
        /// Thiết lập danh sách vật cản
        /// </summary>
        /// <param name="obs"></param>
        public void SetFixedObstruction(byte[,] obs)
        {
            this.obs = obs;
        }

        /// <summary>
        /// Thiết lập danh sách các khu vực liên thông
        /// </summary>
        /// <param name="blurs"></param>
        public void SetBlurObstruction(byte[,] blurs)
        {
            this.blur = blurs;
        }

        /// <summary>
        /// Thiết lập danh sách các điểm khu an toàn
        /// </summary>
        /// <param name="safeAreas"></param>
        public void SetSafeAreas(byte[,] safeAreas)
        {
            this.safeAreas = safeAreas;
        }

        /// <summary>
        /// Thiết lập danh sách các điểm Obs động
        /// </summary>
        /// <param name="dynObs"></param>
        public void SetDynamicObs(byte[,] dynObs)
        {
            this.dynamicObs = dynObs;
        }

        /// <summary>
        /// Mở nhãn obs động tương ứng
        /// </summary>
        /// <param name="copySceneID"></param>
        /// <param name="label"></param>
        public void OpenDynamicObsLabel(int copySceneID, byte label)
        {
            /// Nếu chưa tồn tại
            if (!this.openDynamicObsLabels.ContainsKey(copySceneID))
            {
                /// Tạo mới
                this.openDynamicObsLabels[copySceneID] = new HashSet<byte>();
            }
            /// Thêm vào
            this.openDynamicObsLabels[copySceneID].Add(label);
        }

        /// <summary>
        /// Xóa toàn bộ nhãn Obs động tương ứng
        /// </summary>
        /// <param name="copySceneID"></param>
        public void ClearDynamicObsLabels(int copySceneID)
        {

            /// Xóa
            this.openDynamicObsLabels.Remove(copySceneID);
        }

        /// <summary>
        /// Đóng nhãn obs động tương ứng
        /// </summary>
        /// <param name="copySceneID"></param>
        /// <param name="label"></param>
        public void CloseDynamicObsLabel(int copySceneID, byte label)
        {
            /// Nếu chưa tồn tại
            if (!this.openDynamicObsLabels.ContainsKey(copySceneID))
            {
                /// Bỏ qua
                return;
            }
            /// Xóa
            this.openDynamicObsLabels.Remove(label);
        }

        /// <summary>
        /// Close tryFix
        /// </summary>
        /// <param name="copySceneID"></param>
        /// <param name="label"></param>
        public void CloseDynamicObsLabelTry(int copySceneID, byte label)
        {
            /// Nếu chưa tồn tại
            if (!this.openDynamicObsLabels.ContainsKey(copySceneID))
            {
                /// Bỏ qua
                return;
            }
            /// Xóa
            this.openDynamicObsLabels.TryGetValue(copySceneID, out HashSet<byte> value);
            if (value != null)
            {
                value.Remove(label);
            }
        }


        /// <summary>
        /// Thiết lập kích thước
        /// </summary>
        /// <param name="numCols"></param>
        /// <param name="numRows"></param>
        private void SetSize(int numCols, int numRows)
        {
            if (NodeGrid.nodes == null || NodeGrid.numCols < numCols || NodeGrid.numRows < numRows)
            {
                NodeGrid.numCols = Math.Max(numCols, NodeGrid.numCols);
                NodeGrid.numRows = Math.Max(numRows, NodeGrid.numRows);

                NodeGrid.nodes = new NodeFast[NodeGrid.numCols, NodeGrid.numRows];
            }

            /// Tạo mới Obs
            this.obs = new byte[numCols, numRows];

            /// Duyệt 2 chiều
            for (int i = 0; i < numCols; i++)
            {
                for (int j = 0; j < numRows; j++)
                {
                    /// Khởi tạo dữ liệu mặc định của Obs
                    this.obs[i, j] = 1;
                }
            }
        }

        /// <summary>
        /// Làm rỗng danh sách Node
        /// </summary>
        public void Clear()
        {
            Array.Clear(NodeGrid.nodes, 0, NodeGrid.nodes.Length);
        }

        /// <summary>
        /// Danh sách Node
        /// </summary>
        public NodeFast[,] Nodes
        {
            get
            {
                return NodeGrid.nodes;
            }
        }

        /// <summary>
        /// Trả về danh sách các nhãn khu Obs động đã được mở
        /// </summary>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        public byte[] GetOpenDynamicObsLabels(int copySceneID)
        {
            /// Nếu tồn tại
            if (this.openDynamicObsLabels.TryGetValue(copySceneID, out HashSet<byte> data))
            {
                return data.ToArray();
            }
            /// Nếu không tồn tại
            else
            {
                return new byte[0];
            }
        }

        /// <summary>
        /// Kiểm tra 2 Node có đi được không
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public bool IsDiagonalWalkable(long node1, long node2)
        {
            int node1x = ANode.GetGUID_X(node1);
            int node1y = ANode.GetGUID_Y(node1);

            int node2x = ANode.GetGUID_X(node2);
            int node2y = ANode.GetGUID_Y(node2);

            if (this.obs[node1x, node2y] == 1 && this.obs[node2x, node1y] == 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra vị trí tương ứng có nằm trong khu Obs động không
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        public bool InDynamicObs(int x, int y, int copySceneID)
        {
            if (x < 0 || y < 0)
            {
                return false;
            }
            else if (x >= this.obs.GetUpperBound(0) || y >= this.obs.GetUpperBound(1))
            {
                return false;
            }

            /// Nếu có Obs động
            if (this.dynamicObs != null && this.openDynamicObsLabels.TryGetValue(copySceneID, out HashSet<byte> dynObs))
            {
                /// Nếu Obs động này chưa được mở
                if (dynamicObs[x, y] > 0 && !dynObs.Contains(this.dynamicObs[x, y]))
                {
                    return true;
                }
            }

            /// Không nằm trong khu Obs động
            return false;
        }

        /// <summary>
        /// Kiểm tra vị trí tương ứng có thể đến được không
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        public bool CanEnter(int x, int y, int copySceneID)
        {
            if (x < 0 || y < 0)
            {
                return false;
            }
            else if (x >= this.obs.GetUpperBound(0) || y >= this.obs.GetUpperBound(1))
            {
                return false;
            }

            /// Nếu là điểm Block
            if (this.obs[x, y] == 0)
            {
                return false;
            }

            /// Nếu có Obs động
            if (this.dynamicObs != null && this.openDynamicObsLabels.TryGetValue(copySceneID, out HashSet<byte> dynObs))
            {
                /// Nếu Obs động này chưa được mở
                if (dynamicObs[x, y] > 0 && !dynObs.Contains(this.dynamicObs[x, y]))
                {
                    return false;
                }
            }

            /// Có thể đến được
            return true;
        }

        /// <summary>
        /// Kiểm tra vị trí tương úng có nằm trong vùng an toàn không
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <returns></returns>
        public bool InSafeArea(int gridX, int gridY)
        {
            /// Nếu không có vùng an toàn
            if (this.safeAreas == null)
            {
                return false;
            }
            /// Quá phạm vi
            else if (gridX < 0 || gridY < 0)
            {
                return false;
            }
            else if (gridX >= this.safeAreas.GetUpperBound(0) || gridY >= this.safeAreas.GetUpperBound(1))
            {
                return false;
            }

            /// Trả về kết quả
            return this.safeAreas[gridX, gridY] == 1;
        }

        /// <summary>
        /// Kiểm tra có đường đi giữa 2 nút cho trước không
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <returns></returns>
        public bool HasPath(Point fromPos, Point toPos)
        {
            return this.blur[(int)fromPos.X, (int)fromPos.Y] / 2 == this.blur[(int)toPos.X, (int)toPos.Y] / 2;
        }

        /**
         * Returns the number of columns in the grid.
         */
        public int NumCols
        {
            get { return NodeGrid.numCols; }
        }

        /**
         * Returns the number of rows in the grid.
         */
        public int NumRows
        {
            get { return NodeGrid.numRows; }
        }
    }

}
