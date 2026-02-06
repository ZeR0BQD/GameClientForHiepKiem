using GameClient.Data;
using GameClient.Scenes;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Tools;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace GameClient.SongJin
{
    public partial class SongJinAIClient
    {
        public long UserID;
        public string? UserToken;
        public string? AccessToken;
        public int UserIsAdult;
        public int RoleRandToken;
        public int RoleID;
        public int ServerID;
        public int Token;

        public RoleData? RoleData;
        public User User;
        public Position? ToPosition;

        private const int VerSign = 20140624;
        private const string Key = "Jab2hKa821bJac2Laocb2acah2acacak";

        private TCPClient? login2Client;
        private TCPClient loginClient;

        private bool IsOnline = false;
        public Action? NextStep { get; set; }
        public Action<SongJinAIClient>? LoginSuccess { get; set; }

        private System.Timers.Timer timer;

        private const long DelayMove = 10000;
        private const long Reconnect = 60000;

        private const long DelayJoin = 10000;
        private long JoinTick = 0;

        private long ReconnectTick = 0;
        private long MoveTick = 0;

        private SpriteHeart? SpriteHeart;
        public SongJinAIClient(int serverID, User user)
        {
            Console.WriteLine("Connect to server: {0}:{1} - ID: {2}", AIManager.Server, AIManager.ServerPort, AIManager.ServerID);
            User = user;
            ServerID = serverID;
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

            if (User == null)
            {
                return;
            }

            if (User.IsSong)
            {
                SongProcess();
            }
            else
            {
                JinProcess();
            }
        }

        private void CheckReconnect()
        {
            if (loginClient.Connected == false && Global.GetCurrentTime() >= ReconnectTick + Reconnect)
            {
                ReconnectTick = Global.GetCurrentTime();
                Console.WriteLine("Begin reconnect");
                Login();
                return;
            }
        }

        private void AutoMove()
        {
            if (RoleData == null) return;

            if (Global.GetCurrentTime() >= MoveTick + DelayMove)
            {
                MoveTick = Global.GetCurrentTime();

                if (ToPosition != null && Vector2.Distance(new Vector2(ToPosition.PosX, ToPosition.PosY), new Vector2(RoleData.PosX, RoleData.PosY)) >= 400)
                {
                    return;
                }

                ToPosition = AIManager.Points.OrderBy(p => Guid.NewGuid()).FirstOrDefault();
                if (ToPosition == null)
                {
                    return;
                }
                SendMove(ToPosition);
            }
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
                Console.WriteLine(command);
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
                LoginSuccess?.Invoke(this);
            }
            else if (command == TCPGameServerCmds.CMD_SPR_MOVE)
            {
            }
            else if (command == TCPGameServerCmds.CMD_SPR_UPDATE_ROLEDATA)
            {
                var data = DataHelper.BytesToObject<SpriteLifeChangeData>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                CMD_SPR_UPDATE_ROLEDATA(data);
            }
            else if (command == TCPGameServerCmds.CMD_SPR_CHANGEPOS)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                var param = strData.Split(":");
                CMD_SPR_CHANGEPOS(param);
            }
            else if (command == TCPGameServerCmds.CMD_SPR_NOTIFYCHGMAP)
            {
                string strData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                var param = strData.Split(":");
                CMD_SPR_NOTIFYCHGMAP(param);
            }
            else if (command == TCPGameServerCmds.CMD_KT_G2C_ITEMDIALOG)
            {
                G2C_LuaItemDialog result = DataHelper.BytesToObject<G2C_LuaItemDialog>(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                if (result != null)
                {
                    CMD_KT_G2C_ITEMDIALOG(result);
                }
            }
            else if (command == TCPGameServerCmds.CMD_SPR_MAPCHANGE)
            {
                GamePlay();
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
            User.UserName = fields[1];
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

        #region Commands

        private void GetRoleList()
        {
            string strcmd = string.Format("{0}:{1}", UserID, ServerID);
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_ROLE_LIST);
            loginClient.SendData(tcpOutPacket);
        }

        private void InitGame()
        {
            SpriteHeart = new SpriteHeart(loginClient, RoleID, Token);
            string strcmd = string.Format("{0}:{1}:{2}", UserID, RoleID, "ai");
            var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_INIT_GAME);
            loginClient.SendData(tcpOutPacket);
        }

        private void CreateNewRole()
        {
            var rand = new System.Random();
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

            string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", UserID, User.UserName, sex, series, User.RoleName, ServerID);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_CREATE_ROLE));
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

        private void SendMove(Position position)
        {
            if (RoleData == null)
            {
                return;
            }
            try
            {
                var posX = position.PosX;
                var posY = position.PosY;

                Console.WriteLine("Auto move to {0}/{1}", position.PosX, position.PosY);
                Vector2 from = new Vector2(RoleData.PosX, RoleData.PosY);
                Vector2 to = new Vector2(position.PosX, position.PosY);

                var paths = GScene.Instance.FindPath(RoleData, from, to);
                var pathString = string.Join("|", paths.Select(s => string.Format("{0}_{1}", (int)s.x, (int)s.y)).ToArray());
                SpriteMoveData moveData = new SpriteMoveData()
                {
                    RoleID = RoleID,
                    FromX = RoleData.PosX,
                    FromY = RoleData.PosY,
                    ToX = posX,
                    ToY = posY,
                    PathString = pathString,
                };
                byte[] cmdData = DataHelper.ObjectToBytes(moveData);
                loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, cmdData, 0, cmdData.Length, (int)TCPGameServerCmds.CMD_SPR_MOVE));

                RoleData.PosX = position.PosX;
                RoleData.PosY = position.PosY;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ChangeMap(int mapId)
        {
            var thp = RoleData?.GoodsDataList?.FirstOrDefault(g => g.GoodsID == 9680);
            if (thp == null)
            {
                Console.WriteLine("Không tìm thấy Thần hành phù");
                return;
            }
            if (RoleData == null)
            {
                return;
            }
            CS_SprUseGoods useGoods = new CS_SprUseGoods();
            useGoods.RoleID = RoleData.RoleID;
            useGoods.DbIds = new List<int>();
            useGoods.DbIds.Add(thp.Id);
            byte[] bData = DataHelper.ObjectToBytes<CS_SprUseGoods>(useGoods);

            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bData, 0, bData.Length, (int)(TCPGameServerCmds.CMD_SPR_USEGOODS)));

            /*C2G_AutoPathChangeMap autoPathChangeMap = new C2G_AutoPathChangeMap()
            {
                ToMapCode = mapId,
                ItemID = 9680,
                UseNPC = false,
            };
            byte[] cmdData = DataHelper.ObjectToBytes<C2G_AutoPathChangeMap>(autoPathChangeMap);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, cmdData, 0, cmdData.Length, (int)(TCPGameServerCmds.CMD_KT_C2G_AUTOPATH_CHANGEMAP)));*/
        }

        #endregion

        #region SongJin

        private void JinProcess()
        {
            if (RoleData == null)
            {
                return;
            }
            if (Global.GetCurrentTime() < JoinTick + DelayJoin)
            {
                return;
            }
            JoinTick = Global.GetCurrentTime();
            if (SongJinAIManager.Config.JinSigninPos == null)
            {
                return;
            }
            if (IsOpen() == false)
            {
                return;
            }

            ///Map đăng ký
            if (RoleData.MapCode == 38)
            {
                var distance = Vector2.Distance(new Vector2(RoleData.PosX, RoleData.PosY), new Vector2(12423, 7876));
                if (distance <= 100)
                {
                    SendGMCommand("JinGotoBegin");
                }
                else
                {
                    AutoMove();
                }
                return;
            }
            SendGMCommand("JinRegister");
            return;
        }

        private void SongProcess()
        {
            if (RoleData == null)
            {
                return;
            }
            if (Global.GetCurrentTime() < JoinTick + DelayJoin)
            {
                return;
            }
            JoinTick = Global.GetCurrentTime();
            if (SongJinAIManager.Config.SongSigninPos == null)
            {
                return;
            }
            if (IsOpen() == false)
            {
                return;
            }

            ///Map đăng ký

            if (RoleData.MapCode == 38)
            {
                var distance = Vector2.Distance(new Vector2(RoleData.PosX, RoleData.PosY), new Vector2(1399, 983));
                if (distance <= 100)
                {
                    SendGMCommand("SongGotoBegin");
                }
                else
                {
                    AutoMove();
                }
                return;
            }
            SendGMCommand("SongRegister");
            return;
        }

        private bool IsOpen()
        {
            var now = DateTime.Now;
            var today = DateTime.Today;

            var openTime = SongJinAIManager.Config.OpenTime.Select(o => new
            {
                OpenTime = today.Add(o),
                PreTime = today.Add(o).AddSeconds(-SongJinAIManager.Config.PreTime),
                EndTime = today.Add(o).AddMinutes(60)
            }).Count(o => now > o.PreTime && now <= o.EndTime);

            return openTime > 0;
        }

        private void SendGMCommand(string command)
        {
            byte[] bytes = KTCrypto.Encrypt(command);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_GM_COMMAND)));
        }

        #endregion

        #region Account
        public void Login()
        {
            AccountController.Login(User.UserName, User.Password, (succes, token) =>
            {
                if (succes)
                {
                    Console.WriteLine("Login successed...");

                    AccessToken = token;

                    AccountVerify();
                }
                else
                {
                    Console.WriteLine(token);

                    AccountController.Register(User.UserName, User.Password, registerSuccess =>
                    {
                        if (registerSuccess)
                        {
                            Login();
                        }
                    });
                }
            });
        }

        private void AccountVerify()
        {
            AccountController.AccountVerify(AccessToken ?? string.Empty, (succes, verify) =>
            {
                if (login2Client == null)
                {
                    return;
                }
                if (succes && verify != null)
                {
                    UserID = int.Parse(verify.strPlatformUserID);
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
        #endregion
    }
}
