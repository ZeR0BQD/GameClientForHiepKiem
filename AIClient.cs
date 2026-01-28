using System.Text;
using GameClient.Data;
using GameClient.Scenes;
using HSGameEngine.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Tools;

namespace GameClient
{
    public class AIClient
    {
        // Client ID duy nhat, thread-safe
        private static int _nextClientId = 0;
        private readonly int _clientId;
        public long UserID;
        public string UserName;
        public string Password;
        public string UserToken;
        public string AccessToken;
        public int UserIsAdult;
        public int RoleRandToken;
        public int RoleID;
        public int ServerID;
        public int Token;

        public RoleData RoleData;

        private const int VerSign = 20140624;
        private const string Key = "Jab2hKa821bJac2Laocb2acah2acacak";

        private TCPClient login2Client;
        private TCPClient loginClient;

        private bool IsOnline = false;
        public Action NextStep { get; set; }
        public Action<AIClient> LoginSuccess { get; set; }

        private System.Timers.Timer timer;

#if DEBUG
        private const long DelayMove = 16000;
#else
        private const long DelayMove = 60000;
#endif
        private const long Reconnect = 60000;

        private long ReconnectTick = 0;
        private long MoveTick = 0;

        private SpriteHeart? SpriteHeart;

        // Target position cho Observer pattern
        private Position? _targetPosition = null;
        private bool IsMove = false;

        public AIClient(int serverID, long userId, string userName)
        {
            _clientId = Interlocked.Increment(ref _nextClientId);
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

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            CheckReconnect();

            if (loginClient.Connected == false)
            {
                return;
            }

            //return;
            if (IsOnline)
            {
                AutoMoveAround();

                //if (Global.GetCurrentTime() < MoveTick + DelayMove)
                //{
                //    return;
                //}
                //MoveTick = Global.GetCurrentTime();

                //SendMove(new Position() { PosX = 5711, PosY = 3022 });
                //Console.WriteLine("SendMove");
            }
        }

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
                    Login(UserName, Password);
                }
            }
        }


        // Di chuyen tu dong
        private void AutoMoveAround()
        {
            // Check thoi gian delay
            if (Global.GetCurrentTime() < MoveTick + DelayMove)
            {
                return;
            }
            MoveTick = Global.GetCurrentTime();

            var position = AIManager.Points.OrderBy(p => Guid.NewGuid()).FirstOrDefault();
            if (position == null)
            {
                return;
            }

            // Add jitter
            var random = new Random();
            var newPosition = new Position
            {
                PosX = position.PosX + random.Next(-10, 10),
                PosY = position.PosY + random.Next(-10, 10),
                MapId = position.MapId
            };

            _targetPosition = newPosition;
            SendMove(_targetPosition);
        }

        #region Event packet

        private bool MyTCPInPacket_TCPCmdPacketEvent(object sender)
        {
            var tcpInPacket = sender as TCPInPacket;
            if (tcpInPacket == null)
            {
                return false;
            }
            var command = (TCPGameServerCmds)tcpInPacket.PacketCmdID;
#if DEBUG
            if (command != TCPGameServerCmds.CMD_SPR_UPDATE_ROLEDATA)
            {
                //Console.WriteLine("TCPGameServerCmds.{0}\n", command);
            }
#endif

            Console.WriteLine("TCPGameServerCmds.{0}", command);
            if (command == TCPGameServerCmds.CMD_LOGIN_ON)
            {
                
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                var param = strData.Split(":");
                Token = Convert.ToInt32(param[0]);

                GetRoleList();
            }
            else if (command == TCPGameServerCmds.CMD_ROLE_LIST)
            {
                Console.WriteLine("TCPGameServerCmds.{0}\n", command);
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
                EnterMap();
                IsOnline = true;

                // Notify Observer
                ClientStateObserver.Notify(command, new CmdEventArgs
                {
                    ClientId = _clientId,
                    RoleId = RoleID,
                    RoleName = RoleData?.RoleName,
                    Cmd = command
                });

                LoginSuccess?.Invoke(this);
            }
            else if (command == TCPGameServerCmds.CMD_SPR_MOVE)
            {
                // CMD_SPR_MOVE dung binary format (SpriteMoveData)
                var moveData = DataHelper.BytesToObject<SpriteMoveData>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                if (moveData != null)
                {
                    Console.WriteLine("[CMD_SPR_MOVE] RoleID: {0}, From: {1}/{2}, To: {3}/{4}",
                        moveData.RoleID, moveData.FromX, moveData.FromY, moveData.ToX, moveData.ToY);
                }
            }
            else if (command == TCPGameServerCmds.CMD_SPR_CHANGEPOS)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                var param = strData.Split(":");
                var receivedRoleId = Convert.ToInt32(param[0]);

                // Chi xu ly neu la cua client hien tai
                if (receivedRoleId == RoleData?.RoleID)
                {
                    RoleData.PosX = Convert.ToInt32(param[1]);
                    RoleData.PosY = Convert.ToInt32(param[2]);

                    // Notify Observer
                    ClientStateObserver.Notify(command, new CmdEventArgs
                    {
                        ClientId = _clientId,
                        RoleId = RoleID,
                        RoleName = RoleData?.RoleName,
                        Cmd = command,
                        Data = new Position { PosX = RoleData.PosX, PosY = RoleData.PosY }
                    });
                }
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

            login2Client.SocketConnect -= LoginClient_SocketConnect;
            login2Client.MyTCPInPacket.TCPCmdPacketEvent -= MyTCPInPacket_TCPCmdPacketEvent;
            login2Client.Destroy();
            login2Client = null;

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

        private void GetRoleList()
        {
            string strcmd = string.Format("{0}:{1}", UserID, ServerID);
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_ROLE_LIST);
            loginClient.SendData(tcpOutPacket);
        }

        private void InitGame()
        {
            //SpriteHeart = new SpriteHeart(loginClient, RoleID, Token);
            //SpriteHeart.Start();
            string strcmd = string.Format("{0}:{1}:{2}:39", this.UserID, this.RoleID, "ai");
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_INIT_GAME);
            loginClient.SendData(tcpOutPacket);
        }

        private void CreateNewRole()
        {
            Random rand = new Random();
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
            string strcmd = "";
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_SPR_ENTERMAP);
            loginClient.SendData(tcpOutPacket);
        }

        private void JoinJin()
        {

        }

        private void SendMove(Position position)
        {
            if (RoleData == null)
            {
                return;
            }
            try
            {
                var newPosition = position;

                Console.WriteLine("Auto move to {0}/{1}", newPosition.PosX, newPosition.PosY);
                SpriteMoveData moveData = new SpriteMoveData()
                {
                    RoleID = RoleID,
                    FromX = RoleData.PosX,
                    FromY = RoleData.PosY,
                    ToX = newPosition.PosX,
                    ToY = newPosition.PosY,
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
            AccountController.AccountVerify(AccessToken, (succes, verify) =>
            {
                if (succes && verify != null)
                {
                    this.UserID = int.Parse(verify.strPlatformUserID);
                    UserToken = verify.strToken;
                    login2Client.Connect(AIManager.Server, AIManager.ServerPort);
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
