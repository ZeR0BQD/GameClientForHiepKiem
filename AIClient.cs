using GameClient.Data;
using GameClient.MapConfig;
using GameClient.Scenes;
using HSGameEngine.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Org.BouncyCastle.Utilities;
using Server.Tools;
using System.Collections;
using System.Text;
using UnityEngine;
using static GameClient.Data.Enum;

namespace GameClient
{
    public enum AIState
    {
        Idle,
        AutoMoveAround,
        Move,
        Attack,
        Chase,
        Death,
        REALIVE,
    }

    public class AIClient
    {
        public long UserID;
        public string UserName;
        public string? Password;
        public string? UserToken;
        public string? AccessToken;
        public int UserIsAdult;
        public int RoleRandToken;
        public int RoleID;
        public int ServerID;
        public int Token;

        public RoleData? RoleData;
        protected List<RoleDataMini> ListRoleDataMini = new List<RoleDataMini>();

        // Dùng Random.Shared để đảm bảo thread-safe khi nhiều AIClient chạy song song
        private static System.Random _random => System.Random.Shared;

        private const int VerSign = 20140624;
        private const string Key = "Jab2hKa821bJac2Laocb2acah2acacak";

        private TCPClient? login2Client;
        private TCPClient loginClient;

        private bool IsOnline = false;
        private bool MoveDone = true;
        public bool ReadyAction = true;
        protected bool forcusAttack = false;

        private bool _isChasing = false;


        private readonly object _roleLock = new object();

        public AIState CurrentState { get; set; } = AIState.AutoMoveAround;
        protected Queue<(string Name, Action Action)> ActionQueue = new Queue<(string Name, Action Action)>();
        private readonly object _queueLock = new object();
        public void AddAction(string name, Action action)
        {
            lock (_queueLock)
            {
                ActionQueue.Enqueue((name, action));
                Console.WriteLine($"[AIClient] Added action: {name}. Queue size: {ActionQueue.Count}");
            }
        }
        public void ClearActions()
        {
            lock (_queueLock)
            {
                ActionQueue.Clear();
                Console.WriteLine("[AIClient] Cleared all actions");
            }
        }
        public Action<AIClient>? LoginSuccess { get; set; }

        private System.Timers.Timer timer;

#if DEBUG
        private const long DelayMove = 3000;
#else
        private const long DelayMove = 60000;
#endif
        private const long Reconnect = 60000;

        protected long MoveTime = 0;

        private long ReconnectTick = 0;
        private long MoveTick = 0;

        /// <summary>
        /// Path của lần SendMove gần nhất để tính vị trí thực tế qua nội suy
        /// </summary>
        private List<Vector2> _currentMovePaths = new List<Vector2>();

        /// <summary>
        /// Timer cập nhật RoleData.PosX/Y liên tục theo vị trí thực tế trong lúc di chuyển
        /// </summary>
        private System.Timers.Timer? _positionUpdateTimer;

        private SpriteHeart? SpriteHeart;


        public AIClient(int serverID, long userId, string userName)
        {
            Console.WriteLine("Connect to server: {0}:{1} - ID: {2}", AIManager.Server, AIManager.ServerPort, AIManager.ServerID);
            UserID = userId;
            ServerID = serverID;
            UserName = userName;
            login2Client = new TCPClient();
            login2Client.MyTCPInPacket.TCPCmdPacketEvent += MyTCPInPacket2_TCPCmdPacketEvent;
            login2Client.SocketConnect += Login2Client_SocketConnect;

            loginClient = new TCPClient();
            loginClient.MyTCPInPacket.TCPCmdPacketEvent += MyTCPInPacket_TCPCmdPacketEvent;
            loginClient.SocketConnect += LoginClient_SocketConnect;

            MoveTick = Global.GetCurrentTime();
            ReconnectTick = Global.GetCurrentTime();

            timer = new System.Timers.Timer(100);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }
        #region Timer_Elapsed
        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            CheckReconnect();

            if (loginClient.Connected == false)
            {
                return;
            }

            if (IsOnline)
            {
                if (MoveDone == false)
                {
                    if (Global.GetCurrentTime() >= MoveTick + MoveTime)
                    {
                        MoveDone = true;
                        ReadyAction = true;
                        MoveTime = 0;
                        Console.WriteLine($"[AIClient]<Timer_Elapsed> Move done");
                    }
                    return;
                }
                else if (MoveDone == true && ReadyAction == true)
                {
                    (string Name, Action Action)? nextAction = null;
                    lock (_queueLock)
                    {
                        if (ActionQueue.Count > 0)
                        {
                            nextAction = ActionQueue.Dequeue();
                        }
                    }

                    if (nextAction != null)
                    {
                        Console.WriteLine($"[AIClient]<Timer_Elapsed> Executing action: {nextAction.Value.Name}");
                        nextAction.Value.Action.Invoke();
                    }
                }


                switch (CurrentState)
                {
                    case AIState.AutoMoveAround:
                        AutoMoveAround();
                        break;
                    case AIState.Attack:
                        if (ListRoleDataMini.Count > 0)
                        {
                            SendUseSkill(14000, ListRoleDataMini[0], forcusAttack);
                        }
                        else if (ActionQueue.Count == 0 && ListRoleDataMini.Count <= 0)
                        {
                            /// Không còn địch  queue AutoMoveToPKSongJin qua AddAction
                            CurrentState = AIState.AutoMoveAround;
                            ClearActions();
                            AddAction("AutoMoveToPKSongJin", () =>
                            {
                                AutoMoveToPKSongJin();
                            });
                        }
                        break;
                    case AIState.Move:
                        break;
                    case AIState.Idle:
                        break;
                    case AIState.Death:
                        ListRoleDataMini.Clear();
                        ClearActions();
                        MoveDone = true;
                        ReadyAction = true;
                        AddAction("ClientRevive", () =>
                        {
                            ClientRevive(1);
                        });
                        CurrentState = AIState.Idle;
                        break;
                    case AIState.REALIVE:
                        ReadyAction = true;
                        MoveDone = true;
                        Console.WriteLine("[AIClient] Role REALIVE");
                        /// Xóa sạch mọi Action cũ đang kẹt
                        ClearActions();

                        // Chỉ add hành động chạy đến cổng dịch chuyển
                        AddAction("MoveToTeleport", () =>
                        {
                            MoveToTeleport();
                        });

                        CurrentState = AIState.Idle;
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion
        #region CheckReconnect
        private void CheckReconnect()
        {
            if (loginClient.Connected == false && Global.GetCurrentTime() >= ReconnectTick + Reconnect)
            {
                loginClient.MyTCPInPacket.TCPCmdPacketEvent -= MyTCPInPacket_TCPCmdPacketEvent;
                loginClient.SocketConnect -= LoginClient_SocketConnect;
                ReconnectTick = Global.GetCurrentTime();
                Console.WriteLine("Begin reconnect");

                if (this.IsOnline)
                {
                    loginClient = new TCPClient();
                    loginClient.MyTCPInPacket.TCPCmdPacketEvent += MyTCPInPacket_TCPCmdPacketEvent;
                    loginClient.SocketConnect += LoginClient_SocketConnect;
                    loginClient.Connect(AIManager.Server, AIManager.ServerPort);
                }
                else
                {
                    Login(UserName, Password!);
                }
            }
        }
        #endregion
        #region Các hàm xử lý hành động
        /// <summary>
        /// Các hàm xử lý hành động
        /// </summary>
        /// 



        /// <summary>
        /// Đặt trạng thái hành động động hiện tại
        /// </summary>
        /// <param name="readyAction"></param>
        public void SetStateReadyAction(bool readyAction)
        {
            ReadyAction = readyAction;
        }




        /// <summary>
        /// Di chuyển đến điểm dịch chuyển
        /// </summary>
        public void MoveToTeleport()
        {
            if (RoleData == null) return;
            var team = GameClient.Scenarios.SongJinScenario.GetTeam(this.RoleID);
            var Pos = new Position();

            string? mapCodeStr = MapConfigHelper.GetMapCode(this.RoleData.MapCode);
            if (string.IsNullOrEmpty(mapCodeStr)) return;
            var scene = GScene.Get(mapCodeStr);

            if (team == "Song")
            {
                if (!scene.Teleports.ContainsKey(1)) return;
                var teleport = scene.Teleports[1];
                Pos = new Position { PosX = teleport.X, PosY = teleport.Y };
                this.AddAction("[AiClient]<MoveToTeleport>MoveToTeleport_Song", () =>
                {
                    SendMove(Pos);
                });

                this.AddAction("[AiClient]<MoveToTeleport>ChangePosWithTeleport_Song", () =>
                {
                    SpriteMapConversion(teleport.Code, teleport.To, teleport.ToX, teleport.ToY);
                    RoleData.PosX = teleport.ToX;
                    RoleData.PosY = teleport.ToY;
                });

            }
            else if (team == "Jin")
            {
                if (!scene.Teleports.ContainsKey(2)) return;
                var teleport = scene.Teleports[2];
                Pos = new Position { PosX = teleport.X, PosY = teleport.Y };
                this.AddAction("[AiClient]<MoveToTeleport>MoveToTeleport_Jin", () =>
                {
                    SendMove(Pos);
                });

                this.AddAction("[AiClient]<MoveToTeleport>ChangePosWithTeleport_Jin", () =>
                {
                    SpriteMapConversion(teleport.Code, teleport.To, teleport.ToX, teleport.ToY);
                    RoleData.PosX = teleport.ToX;
                    RoleData.PosY = teleport.ToY;
                });
            }
            return;
        }


        /// <summary>
        /// Tự động di chuyển đến điểm PK
        /// </summary>
        public void AutoMoveToPKSongJin()
        {
            if (AIManager.Points.Count == 0)
            {
                Console.WriteLine("[AIClient]<AutoMoveToPK> AIManager.Points is empty");
                return;
            }

            var index = _random.Next(AIManager.Points.Count);
            this.AddAction("SendMove_AutoMoveToPKSongJin", () =>
            {
                var pos = AIManager.Points[index];
                Console.WriteLine($"[AIClient]<AutoMoveToPKSongJin> SendMove {pos.PosX} {pos.PosY}");
                SendMove(new Position() { PosX = pos.PosX, PosY = pos.PosY });
            });

            this.AddAction("AutoMoveToPKSongJin_ChaseAndAttack", () =>
            {
                ExecuteChaseAndAttack();
            });
        }




        /// <summary>
        /// Thực hiện đuổi và tấn công
        /// </summary>
        private void ExecuteChaseAndAttack()
        {
            if (_isChasing) return;
            _isChasing = true;

            AIClient? targetClient = null;
            float minDistSq = float.MaxValue;

            List<RoleDataMini> snapshot;
            lock (_roleLock)
            {
                snapshot = ListRoleDataMini.ToList();
            }

            // Lọc ra danh sách bot đang tồn tại (loại bỏ người chơi thực sự)
            List<AIClient> botsInView = AIManager.GetClientsFromRoleDataMini(snapshot);

            // Tìm bot gần nhất còn sống
            foreach (var client in botsInView)
            {
                if (client.RoleData != null && client.RoleData.CurrentHP > 0)
                {
                    // Tính khoảng cách bình phương
                    float dx = client.RoleData.PosX - RoleData!.PosX;
                    float dy = client.RoleData.PosY - RoleData!.PosY;
                    float distSq = dx * dx + dy * dy;

                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        targetClient = client;
                    }
                }
            }

            if (targetClient != null)
            {
                int targetPosX = targetClient.RoleData!.PosX;
                int targetPosY = targetClient.RoleData!.PosY;

                Console.WriteLine(
                    "[AIClient]<ExecuteChaseAndAttack> Target RoleID={0} Actual=({1},{2})",
                    targetClient.RoleData!.RoleID, targetPosX, targetPosY
                );

                bool moveSuccess = SendMove(new Position() { PosX = targetPosX, PosY = targetPosY });

                if (moveSuccess)
                {
                    _isChasing = false;
                    // Để nguyên CurrentState là Move do SendMove đã gán.
                    // Xếp hàng việc chuyển sang trạng thái đánh (Attack) vào hàng đợi.
                    // Chỉ khi Timer_Elapsed đếm đủ MoveTime và trả MoveDone = true, action này mới được chạy.
                    ClearActions();
                    AddAction("FinishChase_SwitchToAttack", () =>
                    {
                        Console.WriteLine($"[AIClient]<FinishChase> Da toi noi, chuyen sang state Attack.");
                        CurrentState = AIState.Attack;
                    });
                }
                else
                {
                    _isChasing = false;
                    CurrentState = AIState.AutoMoveAround;
                    ClearActions();
                    AddAction("AutoMoveToPKSongJin", () =>
                    {
                        Console.WriteLine("[AIClient]<ExecuteChaseAndAttack> SendMove that bai, thuc hien AutoMoveToPKSongJin");
                        AutoMoveToPKSongJin();
                    });
                }
            }
            else
            {
                /// Không tìm thấy bot nào phù hợp (toàn người chơi thật hoặc list rỗng)
                /// Dùng AddAction để queue đúng thứ tự
                _isChasing = false;
                CurrentState = AIState.AutoMoveAround;
                ClearActions();
                AddAction("AutoMoveToPKSongJin", () =>
                {
                    Console.WriteLine("[AIClient]<ExecuteChaseAndAttack> Khong tim thay AIClient muc tieu, thuc hien AutoMoveToPKSongJin");
                    AutoMoveToPKSongJin();
                });
            }
        }


        /// <summary>
        /// Tính toán thời gian di chuyển
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        protected long CalculateMoveTime(List<Vector2> paths)
        {
            double time = 0;
            if (this.RoleData == null)
            {
                return 0;
            }

            Queue<Vector2> movePaths = new Queue<Vector2>(paths);

            /// Vị trí xuất phát ban đầu là vị trí hiện tại của nhân vật
            Vector2 currentPos = new Vector2(this.RoleData.PosX, this.RoleData.PosY);

            /// Chừng nào vẫn còn các điểm cần đi
            while (movePaths.Count > 0)
            {
                /// Nếu chết rồi thì thôi
                if (this.CurrentState == AIState.Death)
                {
                    /// Bỏ qua
                    break;
                }

                /// Vị trí tiếp theo cần tới
                Vector2 nextPos = movePaths.Peek();

                /// Vận tốc
                float velocity = this.RoleData.MoveSpeed * 15;

                /// Khoảng cách từ vị trí hiện tại (currentPos) đến đích (nextPos)
                float distance = Vector2.Distance(currentPos, nextPos);

                /// Thời gian cần để đến đích
                if (velocity > 0)
                {
                    time += (distance / velocity);
                }

                /// Cập nhật vị trí hiện tại thành điểm vừa đến để tính tiếp cho đoạn sau
                currentPos = nextPos;

                /// Xóa điểm đến khỏi hàng đợi
                movePaths.Dequeue();
            }
            return (long)(time * 1000);
        }



        /// <summary>
        /// Di chuyển đến Xa Phu
        /// </summary>
        public void MoveToXaPhu()
        {
            int currentMap = this.RoleData?.MapCode ?? 0;

            var xaPhu = XaPhuManager.GetRandom_XaPhu(currentMap);
            if (xaPhu != null)
            {
                Console.WriteLine($"[AIClient]<MoveToXaPhu> Di chuyen den Xa Phu: X={xaPhu.X}, Y={xaPhu.Y}");
                SendMove(new Position() { PosX = xaPhu.X, PosY = xaPhu.Y });
            }
            else
            {
                Console.WriteLine($"[AIClient]<MoveToXaPhu> Khong tim thay Xa Phu tren map {currentMap}");
            }
        }



        /// <summary>
        /// Hành động lựa chọn Selection NPC
        /// </summary>
        /// <param name="npcID"></param>
        /// <param name="dialogID"></param>
        /// <param name="selectionID"></param>
        /// <param name="selectItemInfo"></param>
        /// <param name="otherParams"></param>
        public void ActionSendNPCSelection(int npcID, int dialogID, int selectionID, DialogItemSelectionInfo? selectItemInfo, Dictionary<int, string>? otherParams)
        {
            SendNPCSelection(npcID, dialogID, selectionID, selectItemInfo, otherParams);
            Console.WriteLine("SendNPCSelection");
        }




        /// <summary>
        /// Tự động di chuyển xung quanh
        /// </summary>
        public void AutoMoveAround()
        {
            if (Global.GetCurrentTime() >= MoveTick + DelayMove)
            {
                MoveTick = Global.GetCurrentTime();
                if (RoleData == null || MoveDone == false)
                {
                    return;
                }

                var position = new Position()
                {
                    MapId = RoleData.MapCode,
                    PosX = RoleData.PosX,
                    PosY = RoleData.PosY,
                };
                try
                {
                    List<Vector2>? paths = null;
                    int retryCount = 0;
                    while (paths == null && retryCount < 100)
                    {
                        retryCount++;
                        var newPosition = new Position()
                        {
                            PosX = position.PosX,
                            PosY = position.PosY,
                            MapId = position.MapId,
                        };

                        newPosition.PosX += _random.Next(-300, 300);
                        newPosition.PosY += _random.Next(-300, 300);


                        var from = new Vector2(RoleData.PosX, RoleData.PosY);
                        var to = new Vector2(newPosition.PosX, newPosition.PosY);

                        string? mapCodeStr = MapConfigHelper.GetMapCode(this.RoleData.MapCode);
                        if (string.IsNullOrEmpty(mapCodeStr)) continue;
                        var scene = GScene.Get(mapCodeStr);

                        paths = scene.FindPath(RoleData, from, to);
                        if (paths == null)
                        {
                            continue;
                        }

                        MoveTime = CalculateMoveTime(paths);

                        var lastPos = paths.LastOrDefault();
                        var pathString = string.Join("|", paths.Select(s => string.Format("{0}_{1}", (int)s.x, (int)s.y)).ToArray());
                        SpriteMoveData moveData = new SpriteMoveData()
                        {
                            RoleID = RoleID,
                            FromX = RoleData.PosX,
                            FromY = RoleData.PosY,
                            ToX = newPosition.PosX,
                            ToY = newPosition.PosY,
                            PathString = pathString
                        };
                        byte[] cmdData = DataHelper.ObjectToBytes(moveData);
                        loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, cmdData, 0, cmdData.Length, (int)TCPGameServerCmds.CMD_SPR_MOVE));

                        RoleData.PosX = moveData.ToX;
                        RoleData.PosY = moveData.ToY;
                        /// Lưu path để GetCurrentActualPosition dùng khi rẽ hướng giữa đường
                        _currentMovePaths = paths;
                        MoveTick = Global.GetCurrentTime();
                        MoveDone = false;
                        this.CurrentState = AIState.AutoMoveAround;
                        break;
                    }

                    if (paths == null)
                    {
                        Console.WriteLine("[AIClient]<AutoMoveAround> Khong tim thay duong di");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        #endregion

        #region Event packet

        private bool MyTCPInPacket_TCPCmdPacketEvent(object sender)
        {
            var tcpInPacket = sender as TCPInPacket;
            if (tcpInPacket == null)
            {
                return false;
            }
            var command = (TCPGameServerCmds)tcpInPacket.PacketCmdID;

            if (command != TCPGameServerCmds.CMD_SPR_UPDATE_ROLEDATA)
            {
                // Console.WriteLine($"Command: {command}");
            }
            // count++;
#if DEBUG
            if (command != TCPGameServerCmds.CMD_SPR_UPDATE_ROLEDATA)
            {
                // var data = DataHelper.BytesToObject<RoleData>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize) as RoleData;
                // Console.WriteLine("RoleData");
                // if (data != null && data.RoleID != 0)
                // {
                //     Console.WriteLine("RoleData: Name: {0}, Map: {1}, Pos: X:{2}, Y:{3}", data.RoleName, data.MapCode, data.PosX, data.PosY);
                // }
            }
#endif

            if (command == TCPGameServerCmds.CMD_SPR_UPDATE_ROLEDATA)
            {
                SpriteLifeChangeData data =
                    DataHelper.BytesToObject<SpriteLifeChangeData>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                if (data.RoleID == RoleData.RoleID)
                {
                    RoleData.CurrentHP = data.HP;
                    RoleData.MaxHP = data.MaxHP;
                }
                else
                {
                    // Cap nhat HP cho ke dich trong danh sach theo RoleID
                    lock (_roleLock)
                    {
                        var role = ListRoleDataMini.FirstOrDefault(r => r.RoleID == data.RoleID);
                        if (role != null)
                        {
                            role.HP = data.HP;
                            role.MaxHP = data.MaxHP;
                        }
                    }
                }

                if (RoleData.CurrentHP <= 0)
                {
                    ClearActions();
                    CurrentState = AIState.Death;
                }

            }

            if (command == TCPGameServerCmds.CMD_LOGIN_ON)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                var param = strData.Split(":");
                Token = Convert.ToInt32(param[0]);

                GetRoleList();



            }
            else if (command == TCPGameServerCmds.CMD_ROLE_LIST)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                var param = strData.Split(":");
                string[] roles = param[1].Split('|');
                var role = roles.FirstOrDefault();
                if (string.IsNullOrEmpty(role))
                {
                    CreateNewRole();
                }
                else
                {
                    string[] temp = role.Split('$');
                    RoleID = Convert.ToInt32(temp[0]);
                    Console.WriteLine("Login role name: {0}", temp[4]);
                    InitGame();
                }



            }
            else if (command == TCPGameServerCmds.CMD_CREATE_ROLE)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                var param = strData.Split(":");
                var role = param[1];
                string[] temp = role.Split('$');
                RoleID = Convert.ToInt32(temp[0]);
                InitGame();



            }
            else if (command == TCPGameServerCmds.CMD_INIT_GAME)
            {
                RoleData = DataHelper.BytesToObject<RoleData>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                SetUpRoleData();
                Thread.Sleep(1000);
                GamePlay();



            }
            else if (command == TCPGameServerCmds.CMD_PLAY_GAME)
            {
                EnterMap();
                if (!IsOnline)
                {
                    IsOnline = true;
                    LoginSuccess?.Invoke(this);
                }

                ReadyAction = true;
                MoveDone = true;
            }
            else if (command == TCPGameServerCmds.CMD_SPR_MOVE)
            {
                SpriteNotifyOtherMoveData moveData = DataHelper.BytesToObject<SpriteNotifyOtherMoveData>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);

                /// Tìm đối tượng trong danh sách theo RoleID
                var roleDataMini = ListRoleDataMini.FirstOrDefault(r => r.RoleID == moveData.RoleID);
                if (roleDataMini != null)
                {
                    /// Mặc định lấy tọa độ đích (ToX, ToY)
                    int newPosX = moveData.ToX;
                    int newPosY = moveData.ToY;
                    if (!string.IsNullOrEmpty(moveData.PathString))
                    {
                        string[] points = moveData.PathString.Split('|');
                        if (points.Length > 0)
                        {
                            string[] coords = points[points.Length - 1].Split('_');
                            if (coords.Length >= 2 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y))
                            {
                                newPosX = x;
                                newPosY = y;
                            }

                            if (CurrentState == AIState.Attack && ListRoleDataMini.Count > 0
                                && moveData.RoleID == ListRoleDataMini[0].RoleID)
                            {
                                roleDataMini.PosX = newPosX;
                                roleDataMini.PosY = newPosY;

                                _isChasing = false;
                                MoveDone = true;
                                ReadyAction = true;
                                ClearActions();
                                AddAction("ExecuteChaseAndAttack", () =>
                                {
                                    ExecuteChaseAndAttack();
                                });
                            }
                        }
                    }

                    roleDataMini.PosX = newPosX;
                    roleDataMini.PosY = newPosY;


                }




            }
            else if (command == TCPGameServerCmds.CMD_KT_G2C_NPCDIALOG)
            {
                G2C_LuaNPCDialog result = DataHelper.BytesToObject<G2C_LuaNPCDialog>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);

                //gửi thông tin chọn selction cho npc
                ActionSendNPCSelection(result.NPCID, result.ID, 2, null, null);


            }
            else if (command == TCPGameServerCmds.CMD_SPR_NOTIFYCHGMAP)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                var param = strData.Split(":");
                if (RoleData != null)
                {
                    RoleData.MapCode = Convert.ToInt32(param[1]);
                    RoleData.PosX = Convert.ToInt32(param[2]);
                    RoleData.PosY = Convert.ToInt32(param[3]);
                }
                CMD_SPR_NOTIFYCHGMAP(param);



            }
            else if (command == TCPGameServerCmds.CMD_SPR_MAPCHANGE)
            {
                int mapID = DataHelper.BytesToObject<SCMapChange>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize).MapCode;
                if (RoleData != null)
                {
                    RoleData.MapCode = mapID;
                }
                MapConfigHelper.InitSceneByMapId(mapID);
                GamePlay();



            }
            else if (command == TCPGameServerCmds.CMD_KT_SHOW_NOTIFICATIONTIP)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);

                if (strData.Contains("Rời khỏi khu an toàn!"))
                {
                    CurrentState = AIState.Attack;
                }



            }
            else if (command == TCPGameServerCmds.CMD_SPR_DEAD)
            {
                ReadyAction = false;
                MoveDone = false;
                CurrentState = AIState.Death;


            }
            else if (command == TCPGameServerCmds.CMD_SPR_REALIVE)
            {
                CurrentState = AIState.REALIVE;



            }
            else if (command == TCPGameServerCmds.CMD_KT_G2C_RENEW_SKILLLIST)
            {
                // var data = DataHelper.BytesToObject<KeyValuePair<int, List<SkillData>>>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
            }
            else if (command == TCPGameServerCmds.CMD_NEW_OBJECTS)
            {
                AddNewObjects data = DataHelper.BytesToObject<AddNewObjects>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);

                string myTeam = GameClient.Scenarios.SongJinScenario.GetTeam(this.RoleID);

                foreach (RoleDataMini rdMini in data.Players)
                {
                    /// Toác
                    if (rdMini == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    if (!string.IsNullOrEmpty(myTeam) && myTeam != "Unknown" && !string.IsNullOrEmpty(rdMini.Title))
                    {
                        if (myTeam == "Song")
                        {
                            if (!rdMini.Title.Contains("Kim")) continue;
                        }
                        else if (myTeam == "Jin")
                        {
                            if (!rdMini.Title.Contains("Tống")) continue;
                        }
                    }
                    else continue;  // Team chưa xác định hoặc Title rỗng bỏ qua

                    lock (_roleLock)
                    {
                        int existIndex = ListRoleDataMini.FindIndex(r => r.RoleID == rdMini.RoleID);
                        if (existIndex >= 0)
                        {
                            /// Cập nhật dữ liệu mới
                            ListRoleDataMini[existIndex] = rdMini;
                        }
                        else
                        {
                            /// Thêm mới vào danh sách
                            ListRoleDataMini.Add(rdMini);
                        }
                    }
                }



            }
            else if (command == TCPGameServerCmds.CMD_REMOVE_OBJECTS)
            {
                RemoveObjects removeObjects = DataHelper.BytesToObject<RemoveObjects>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);

                /// Xóa người chơi khỏi danh sách
                if (removeObjects.Players != null && removeObjects.Players.Count > 0)
                {
                    lock (_roleLock)
                    {
                        ListRoleDataMini.RemoveAll(r => removeObjects.Players.Contains(r.RoleID));
                    }

                }



            }
            else if (command == TCPGameServerCmds.CMD_KT_C2G_USESKILL)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                string[] fields = strData.Split(':');
                int res = int.Parse(fields[0]);
                switch (res)
                {
                    case (int)UseSkillResult.Success:
                        /// Skill thành công
                        break;
                    case (int)UseSkillResult.Skill_Is_Cooldown:
                        // Server dang cooldown
                        break;
                    case (int)UseSkillResult.Target_Not_In_Range:
                        // Ke dich ra ngoai tam danh
                        if (ListRoleDataMini.Count > 0)
                        {
                            ClearActions();
                            _isChasing = false;
                            AddAction("ExecuteChaseAndAttack", () =>
                            {
                                ExecuteChaseAndAttack();
                            });
                        }
                        break;
                }
            }
            else if (command == TCPGameServerCmds.CMD_SPR_CHANGEPOS)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                string[] fields = strData.Split(':');
                RoleData.PosX = Convert.ToInt32(fields[1]);
                RoleData.PosY = Convert.ToInt32(fields[2]);
            }
            return true;



        }

        private bool MyTCPInPacket2_TCPCmdPacketEvent(object sender)
        {
            var tcpInPacket = sender as TCPInPacket;
            if (tcpInPacket == null)
            {
                return false;
            }
            string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
            string[] fields = strData.Split(':');

            UserID = long.Parse(fields[0]);
            UserName = fields[1];
            UserToken = fields[2];
            UserIsAdult = Convert.ToInt32(fields[3]);

            if (login2Client != null)
            {
                login2Client.SocketConnect -= LoginClient_SocketConnect;
                login2Client.MyTCPInPacket.TCPCmdPacketEvent -= MyTCPInPacket_TCPCmdPacketEvent;
                login2Client.Destroy();
                login2Client = null;
            }

            loginClient.Connect(AIManager.Server, AIManager.ServerPort);

            return true;
        }

        #endregion

        #region Connection Handle

        private void LoginClient_SocketConnect(object sender, SocketConnectEventArgs e)
        {
            var tcpClient = sender as TCPClient;
            if (tcpClient == null)
            {
                return;
            }
            if (e.NetSocketType == 0)
            {
                if (e.Error == "Success")
                {
                    string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}",
                                                    UserID,
                                                    UserName,
                                                    UserToken,
                                                    RoleRandToken,
                                                    VerSign,
                                                    UserIsAdult,
                                                    "ai"
                                                        );
                    strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}", strcmd, 0, 0, 0, 0, "", 1);
                    var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(tcpClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_LOGIN_ON);
                    tcpClient.SendData(tcpOutPacket);
                }

            }
        }

        private void Login2Client_SocketConnect(object sender, SocketConnectEventArgs e)
        {
            var tcpClient = sender as TCPClient;
            if (tcpClient == null)
            {
                return;
            }
            if (e.NetSocketType == 0)
            {
                if (e.Error == "Success")
                {
                    int time = DataHelper.UnixSecondsNow();
                    ///userID + userName + lastTime + isadult + key;
                    string hash = Global.MakeMD5Hash(string.Format("{0}{1}{2}{3}{4}", UserID, UserName, time, 1, Key));
                    string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", VerSign, UserID, UserName, time, 1, hash);

                    var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(tcpClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_LOGIN_ON2);
                    tcpClient.SendData(tcpOutPacket);
                }

            }
        }

        #endregion

        #region Commands



        /// <summary>
        /// Dịch chuyển nhân vật với teleport
        /// </summary>
        /// <param name="teleportID"></param>
        /// <param name="toMapCode"></param>
        /// <param name="toMapX"></param>
        /// <param name="toMapY"></param>
        public void SpriteMapConversion(int teleportID, int toMapCode, int toMapX, int toMapY)
        {
            if (RoleData == null)
            {
                return;
            }
            SCMapChange mapChangeData = new SCMapChange
            {
                RoleID = RoleData.RoleID,
                TeleportID = teleportID,
                MapCode = toMapCode,
                PosX = toMapX,
                PosY = toMapY,
            };

            byte[] bData = DataHelper.ObjectToBytes<SCMapChange>(mapChangeData);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bData, 0, bData.Length, (int)(TCPGameServerCmds.CMD_SPR_MAPCHANGE)));
        }


        /// <summary>
        /// Gửi yêu cầu sử dụng kỹ năng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="roleDataMini"></param>
        /// <param name="ignoreTarget"></param>
        public void SendUseSkill(int skillID, RoleDataMini roleDataMini, bool ignoreTarget = false)
        {
            try
            {
                if (RoleData == null) return;
                RoleDataMini target = roleDataMini;
                if (!ignoreTarget && ListRoleDataMini.Count > 0)
                {
                    target = ListRoleDataMini[_random.Next(ListRoleDataMini.Count)];
                }

                /// Vector chỉ hướng di chuyển
                Vector2 dirVector = new Vector2(target.PosX, target.PosY) - new Vector2(RoleData.PosX, RoleData.PosY);

                /// Hướng quay của đối tượng
                float rotationAngle = KTMath.GetAngle360WithXAxis(dirVector);
                Direction newDir = KTMath.GetDirectionByAngle360(rotationAngle);

                C2G_UseSkill useSkill = new C2G_UseSkill()
                {
                    Direction = (int)newDir,
                    SkillID = skillID,
                    TargetID = target.RoleID,
                    PosX = RoleData.PosX,
                    PosY = RoleData.PosY,
                    TargetPosX = -1,
                    TargetPosY = -1,
                };

                byte[] bytes = DataHelper.ObjectToBytes<C2G_UseSkill>(useSkill);
                loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_C2G_USESKILL)));
                CurrentState = AIState.Attack;
            }
            catch (Exception) { }
        }


        /// <summary>
        /// Thiết lập dữ liệu nhân vật
        /// </summary>
        public void SetUpRoleData()
        {
            SendGMCommand("SetLevel 60");
            if (RoleData?.RoleSex == 0)
            {
                SendGMCommand("JoinFaction 2");
            }
            else if (RoleData?.RoleSex == 1)
            {
                SendGMCommand("JoinFaction 3");
            }
        }



        /// <summary>
        /// Hồi sinh nhân vật
        /// </summary>
        /// <param name="reviveMethodID"></param>
        public void ClientRevive(int reviveMethodID)
        {

            C2G_ClientRevive data = new C2G_ClientRevive()
            {
                SelectedID = reviveMethodID,
            };
            byte[] bytes = DataHelper.ObjectToBytes<C2G_ClientRevive>(data);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_C2G_CLIENTREVIVE)));

            Console.WriteLine("ClientRevive");
        }



        /// <summary>
        /// Click NPC
        /// </summary>
        /// <param name="npcID"></param>
        public void NPCClick(int npcID)
        {
            if (RoleData == null)
            {
                return;
            }
            Console.WriteLine("NPCClick ID: {0}", npcID);
            string strcmd = npcID.ToString();

            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)(TCPGameServerCmds.CMD_KT_CLICKON_NPC)));
        }



        /// <summary>
        /// Xử lý thay đổi map
        /// </summary>
        /// <param name="args"></param>
        /// <param name="teleId"></param>
        public void CMD_SPR_NOTIFYCHGMAP(string[] args, int teleId = -1)
        {
            Thread.Sleep(1000);
            if (RoleData == null)
            {
                return;
            }
            int teleportID = teleId;
            int toMapCode;
            int toMapX;
            int toMapY;

            if (teleportID == -1)
            {
                toMapCode = Convert.ToInt32(args[1]);
                toMapX = Convert.ToInt32(args[2]);
                toMapY = Convert.ToInt32(args[3]);
            }
            else
            {
                string? mapCodeStr = MapConfigHelper.GetMapCode(this.RoleData.MapCode);
                if (string.IsNullOrEmpty(mapCodeStr)) return;
                var scene = GScene.Get(mapCodeStr);

                if (!scene.Teleports.ContainsKey(teleportID)) return;
                var teleport = scene.Teleports[teleportID];
                toMapCode = teleport.To;
                toMapX = teleport.ToX;
                toMapY = teleport.ToY;
            }

            SCMapChange mapChangeData = new SCMapChange
            {
                RoleID = RoleID,
                TeleportID = teleportID,
                MapCode = toMapCode,
                PosX = toMapX,
                PosY = toMapY,
            };

            RoleData.MapCode = mapChangeData.MapCode;
            RoleData.PosX = toMapX;
            RoleData.PosY = toMapY;

            byte[] bData = DataHelper.ObjectToBytes<SCMapChange>(mapChangeData);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bData, 0, bData.Length, (int)(TCPGameServerCmds.CMD_SPR_MAPCHANGE)));
            ReadyAction = false;
        }



        /// <summary>
        /// Gửi lệnh GM
        /// </summary>
        /// <param name="command"></param>
        public void SendGMCommand(string command)
        {
            byte[] bytes = KTCrypto.Encrypt(command);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_GM_COMMAND)));
            Console.WriteLine("SendGMCommand: {0}", command);
        }
        private void GetRoleList()
        {
            string strcmd = string.Format("{0}:{1}", UserID, ServerID);
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_ROLE_LIST);
            loginClient.SendData(tcpOutPacket);
        }




        /// <summary>
        /// Khởi tạo game
        /// </summary>
        private void InitGame()
        {
            SpriteHeart = new SpriteHeart(loginClient, RoleID, Token);
            string strcmd = string.Format("{0}:{1}:{2}:39", this.UserID, this.RoleID, "ai");
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_INIT_GAME);
            loginClient.SendData(tcpOutPacket);
        }



        /// <summary>
        /// Gửi gói tin tạo nhân vật mới
        /// </summary>
        private void CreateNewRole()
        {
            int series = _random.Next(1, 5);
            int sex = _random.Next(0, 1);
            //Hệ kim chỉ có Nam
            if (series == 1)
            {
                sex = 0;
            }
            ///Hệ thủy chỉ có nữ
            else if (series == 3)
            {
                sex = 1;
            }

            string name = NameManager.RandomName(sex);
            string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", UserID, UserName, sex, series, name, ServerID);

            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)(TCPGameServerCmds.CMD_CREATE_ROLE)));
        }



        /// <summary>
        /// Gửi gói tin bắt đầu vào game
        /// </summary>
        private void GamePlay()
        {
            string strcmd = string.Format("{0}", RoleID);
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_PLAY_GAME);
            loginClient.SendData(tcpOutPacket);
            ReadyAction = true;

        }



        /// <summary>
        /// Gửi gói tin vào map
        /// </summary>
        private void EnterMap()
        {
            byte[] bData = new ASCIIEncoding().GetBytes("");
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bData, 0, bData.Length, (int)(TCPGameServerCmds.CMD_SPR_ENTERMAP)));
        }



        /// <summary>
        /// Gửi gói tin lựa chọn selection ở NPC
        /// </summary>
        /// <param name="npcID"></param>
        /// <param name="dialogID"></param>
        /// <param name="selectionID"></param>
        /// <param name="selectItemInfo"></param>
        /// <param name="otherParams"></param>
        public void SendNPCSelection(int npcID, int dialogID, int selectionID, DialogItemSelectionInfo? selectItemInfo, Dictionary<int, string>? otherParams)
        {
            C2G_LuaNPCDialog data = new C2G_LuaNPCDialog()
            {
                ID = dialogID,
                NPCID = npcID,
                SelectionID = selectionID,
                SelectedItem = selectItemInfo,
                OtherParams = otherParams,
            };

            byte[] bytes = DataHelper.ObjectToBytes<C2G_LuaNPCDialog>(data);

            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_C2G_NPCDIALOG)));
        }



        /// <summary>
        /// Gửi gói tin di chuyển
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool SendMove(Position position)
        {
            if (RoleData == null)
            {
                return false;
            }
            try
            {

                MoveTick = Global.GetCurrentTime();

                var from = new Vector2(RoleData.PosX, RoleData.PosY);
                var to = new Vector2(position.PosX, position.PosY);

                string? mapCodeStr = MapConfigHelper.GetMapCode(this.RoleData.MapCode);
                if (string.IsNullOrEmpty(mapCodeStr)) return false;
                var scene = GScene.Get(mapCodeStr);

                var paths = scene.FindPath(RoleData, from, to);
                if (paths == null || paths.Count == 0)
                {
                    return false;
                }

                MoveTime = CalculateMoveTime(paths);

                _currentMovePaths = paths;

                Console.WriteLine("[AIClient]<SendMove> MoveTime: {0}", MoveTime);

                var lastPos = paths.LastOrDefault();
                var pathString = string.Join("|", paths.Select(s => string.Format("{0}_{1}", (int)s.x, (int)s.y)).ToArray());


                Console.WriteLine("Send move to PosX:{0} / PosY:{1} / Path:{2}", position.PosX, position.PosY, pathString);
                Console.WriteLine("LastPos: {0}", lastPos);


                SpriteMoveData moveData = new SpriteMoveData()
                {
                    RoleID = RoleID,
                    FromX = RoleData.PosX,
                    FromY = RoleData.PosY,
                    ToX = position.PosX,
                    ToY = position.PosY,
                    PathString = pathString
                };

                byte[] cmdData = DataHelper.ObjectToBytes(moveData);
                loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, cmdData, 0, cmdData.Length, (int)TCPGameServerCmds.CMD_SPR_MOVE));

                RoleData.PosX = moveData.ToX;
                RoleData.PosY = moveData.ToY;


                this.CurrentState = AIState.Move;
                MoveDone = false;
                ReadyAction = false;
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion



        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void Login(string username, string password)
        {
            UserName = username;
            Password = password;
            AccountController.Login(username, password, (loginSuccess, token) =>
            {
                if (loginSuccess)
                {
                    this.AccessToken = token;

                    AccountVerify();
                }
                else
                {
                    Console.WriteLine(token);

                    AccountController.Register(username, password, registerSuccess =>
                    {
                        if (registerSuccess)
                        {
                            Login(username, password);
                        }
                    });
                }
            });
        }

        private void AccountVerify()
        {
            AccountController.AccountVerify(AccessToken!, (succes, verify) =>
            {
                if (succes && verify != null)
                {
                    this.UserID = int.Parse(verify!.strPlatformUserID);
                    UserToken = verify.strToken;
                    login2Client!.Connect(AIManager.Server, AIManager.ServerPort);
                }
                else
                {
                    Console.WriteLine("Verify failed");
                }
            });
        }

        public void Logout()
        {
            string strcmd = "";
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_LOG_OUT);
            loginClient.SendData(tcpOutPacket);
        }
    }
}
