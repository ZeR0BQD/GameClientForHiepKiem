using HSGameEngine.GameEngine.Network.Protocol;
using HSGameEngine.GameEngine.Network;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.SongJin
{
    public partial class SongJinAIClient
    {

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
                                                    User.UserName,
                                                    UserToken,
                                                    RoleRandToken,
                                                    VerSign,
                                                    UserIsAdult,
                                                    "ai"
                                                        );
                    strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}", strcmd, 0, 0, 0, 0, "", 0);
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
                    string hash = Global.MakeMD5Hash(string.Format("{0}{1}{2}{3}{4}", UserID, User.UserName, time, 1, Key));
                    string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", VerSign, UserID, User.UserName, time, 1, hash);

                    var tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(tcpClient.OutPacketPool, strcmd, (int)TCPGameServerCmds.CMD_LOGIN_ON2);
                    tcpClient.SendData(tcpOutPacket);
                }

            }
        }

        #endregion
    }
}
