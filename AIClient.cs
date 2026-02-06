using GameClient;
using GameClient.Data;
using GameClient.MapConfig;
using GameClient.Scenes;
using HSGameEngine.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Org.BouncyCastle.Utilities;
using Server.Tools;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameClient
{
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

        private const int VerSign = 20140624;
        private const string Key = "Jab2hKa821bJac2Laocb2acah2acacak";

        private TCPClient? login2Client;
        private TCPClient loginClient;

        private bool IsOnline = false;
        public Action? NextStep { get; set; }
        public Action<AIClient>? LoginSuccess { get; set; }

        private System.Timers.Timer timer;

#if DEBUG
        private const long DelayMove = 6000;
#else
        private const long DelayMove = 60000;
#endif
        private const long Reconnect = 60000;

        private long ReconnectTick = 0;
        private long MoveTick = 0;

        private SpriteHeart? SpriteHeart;

        private bool _canMove = true;
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

            if (IsOnline && _canMove)
            {
                //MoveToXaPhu();
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

        public void ActionSendNPCSelection(int npcID, int dialogID, int selectionID, DialogItemSelectionInfo? selectItemInfo, Dictionary<int, string>? otherParams)
        {
            SendNPCSelection(npcID, dialogID, selectionID, selectItemInfo, otherParams);
            Console.WriteLine("SendNPCSelection");
        }

        public void AutoMoveAround()
        {
            if (Global.GetCurrentTime() >= MoveTick + DelayMove)
            {
                MoveTick = Global.GetCurrentTime();
                if (RoleData == null)
                {
                    return;
                }

                //var position = new Position()
                //{
                //    MapId = RoleData.MapCode,
                //    PosX = RoleData.PosX,
                //    PosY = RoleData.PosY,
                //};
                var position = AIManager.Points.OrderBy(p => Guid.NewGuid()).FirstOrDefault();
                if (position == null)
                {
                    Console.WriteLine("[AIClient]<AutoMoveAround> Khong co diem nao trong AIManager.Points");
                    return;
                }
                try
                {
                    var random = new System.Random();
                    var newPosition = new Position()
                    {
                        PosX = position.PosX,
                        PosY = position.PosY,
                        MapId = position.MapId,
                    };
                    newPosition.PosX += random.Next(-10, 10);
                    newPosition.PosY += random.Next(-10, 10);

                    var from = new Vector2(RoleData.PosX, RoleData.PosY);
                    var to = new Vector2(newPosition.PosX, newPosition.PosY);

                    var paths = GScene.Instance.FindPath(RoleData, from, to);
                    if (paths == null || paths.Count == 0)
                    {
                        Console.WriteLine("[AIClient]<AutoMoveAround> Khong tim thay duong di");
                        return;
                    }
                    var pathString = string.Join("|", paths.Select(s => string.Format("{0}_{1}", (int)s.x, (int)s.y)).ToArray());

                    SpriteMoveData moveData = new SpriteMoveData()
                    {
                        RoleID = RoleID,
                        FromX = RoleData.PosX,
                        FromY = RoleData.PosY,
                        ToX = newPosition.PosX,
                        ToY = newPosition.PosY,
                        //PathString = pathString,
                        PathString = "",
                    };
                    byte[] cmdData = DataHelper.ObjectToBytes<SpriteMoveData>(moveData);
                    loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, cmdData, 0, cmdData.Length, (int)(TCPGameServerCmds.CMD_SPR_MOVE)));

                    RoleData.PosX = newPosition.PosX;
                    RoleData.PosY = newPosition.PosY;
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

            // Console.WriteLine("Command {0}: {1}", count, command);
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
                GamePlay();
            }
            else if (command == TCPGameServerCmds.CMD_PLAY_GAME)
            {
                SetUpRoleData();
                EnterMap();
                if (!IsOnline)
                {
                    IsOnline = true;
                    LoginSuccess?.Invoke(this);
                }
            }
            else if (command == TCPGameServerCmds.CMD_KT_G2C_NPCDIALOG)
            {
                G2C_LuaNPCDialog result = DataHelper.BytesToObject<G2C_LuaNPCDialog>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                ActionSendNPCSelection(result.NPCID, result.ID, 2, null, null);
            }
            else if (command == TCPGameServerCmds.CMD_SPR_NOTIFYCHGMAP)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                var param = strData.Split(":");
                CMD_SPR_NOTIFYCHGMAP(param);
            }
            else if (command == TCPGameServerCmds.CMD_SPR_MAPCHANGE)
            {
                int mapID = DataHelper.BytesToObject<SCMapChange>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize).MapCode;
                MapConfigHelper.InitSceneByMapId(mapID);
                GamePlay();
            }
            //if (RoleData?.MapCode != 0)
            //{
            //    SpritePosition();
            //}
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
        private void CMD_SPR_NOTIFYCHGMAP(params string[] args)
        {
            Thread.Sleep(1000);
            if (RoleData == null)
            {
                return;
            }

            int teleportID = Convert.ToInt32(args[0]);
            int toMapCode = Convert.ToInt32(args[1]);
            int toMapX = Convert.ToInt32(args[2]);
            int toMapY = Convert.ToInt32(args[3]);
            SCMapChange mapChangeData = new SCMapChange
            {
                RoleID = RoleID,
                TeleportID = -1,
                MapCode = toMapCode,
                PosX = toMapX,
                PosY = toMapY,
            };

            RoleData.MapCode = mapChangeData.MapCode;
            RoleData.PosX = toMapX;
            RoleData.PosY = toMapY;

            byte[] bData = DataHelper.ObjectToBytes<SCMapChange>(mapChangeData);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bData, 0, bData.Length, (int)(TCPGameServerCmds.CMD_SPR_MAPCHANGE)));
        }

        protected void SpritePosition()
        {
            Console.WriteLine("SpritePosition");
            if (RoleData == null)
            {
                return;
            }
            try
            {
                byte[] bData = DataHelper.ObjectToBytes<RoleData>(new RoleData()
                {
                    RoleID = RoleData.RoleID,
                    MapCode = RoleData.MapCode,
                    PosX = RoleData.PosX,
                    PosY = RoleData.PosY,
                });
                loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bData, 0, bData.Length, (int)(TCPGameServerCmds.CMD_SPR_POSITION)));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AIClient]<SpritePosition> Error: {ex.Message}");
            }

        }

        public void SendGMCommand(string command)
        {
            byte[] bytes = KTCrypto.Encrypt(command);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_GM_COMMAND)));
        }
        private void GetRoleList()
        {
            string strcmd = string.Format("{0}:{1}", UserID, ServerID);
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_ROLE_LIST);
            loginClient.SendData(tcpOutPacket);
        }

        private void InitGame()
        {
            SpriteHeart = new SpriteHeart(loginClient, RoleID, Token);
            string strcmd = string.Format("{0}:{1}:{2}:39", this.UserID, this.RoleID, "ai");
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_INIT_GAME);
            loginClient.SendData(tcpOutPacket);
        }

        private void CreateNewRole()
        {
            System.Random rand = new System.Random();
            int series = rand.Next(1, 5);
            int sex = rand.Next(0, 1);
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

        private void GamePlay()
        {
            string strcmd = string.Format("{0}", RoleID);
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_PLAY_GAME);
            loginClient.SendData(tcpOutPacket);
        }

        private void EnterMap()
        {
            byte[] bData = new ASCIIEncoding().GetBytes("");
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bData, 0, bData.Length, (int)(TCPGameServerCmds.CMD_SPR_ENTERMAP)));
        }

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

        public void SendMove(Position position)
        {
            if (RoleData == null)
            {
                return;
            }
            try
            {

                var from = new Vector2(RoleData.PosX, RoleData.PosY);
                var to = new Vector2(position.PosX, position.PosY);

                var paths = GScene.Instance.FindPath(RoleData, from, to);
                if (paths == null || paths.Count == 0)
                {
                    Console.WriteLine("[AIClient]<SendMove> Khong tim thay duong di");
                    return;
                }
                var lastPos = paths.LastOrDefault();
                var pathString = string.Join("|", paths.Select(s => string.Format("{0}_{1}", (int)s.x, (int)s.y)).ToArray());
                Console.WriteLine("Send move to MapId:{0} / PosX:{1} / PosY:{2} / Path:{3}", position.MapId, position.PosX, position.PosY, pathString);
                Console.WriteLine("LastPos: {0}", lastPos);
                SpriteMoveData moveData = new SpriteMoveData()
                {
                    RoleID = RoleID,
                    FromX = RoleData.PosX,
                    FromY = RoleData.PosY,
                    ToX = position.PosX,
                    ToY = position.PosY,
                    //PathString = pathString
                    PathString = ""
                };
                byte[] cmdData = DataHelper.ObjectToBytes(moveData);
                loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, cmdData, 0, cmdData.Length, (int)TCPGameServerCmds.CMD_SPR_MOVE));


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

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
