using HSGameEngine.GameEngine.Network.Protocol;
using ProtoBuf;

namespace GameClient
{
    public class SpriteHeart
    {
        private System.Timers.Timer timer;
        private TCPClient? Client;
        private int RoleID;
        private int Token;
        private long SendHeartTicks = 0;
#if DEBUG
        private long HeartTicks = 10000;
#else
        private long HeartTicks = 60000;
#endif
        public SpriteHeart(TCPClient? client, int roleID, int token)
        {
            SendHeartTicks = Global.GetCurrentTime();
            timer = new System.Timers.Timer(100);
            timer.Elapsed += Timer_Elapsed;
            Client = client;
            RoleID = roleID;
            Token = token;
        }

        public void Start()
        {
            Console.WriteLine("Role {0} heart start", RoleID);
            timer.Start();
        }

        public void Stop()
        {
            timer.Elapsed -= Timer_Elapsed;
            timer.Stop();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            ///1 phut send 1 lan
            if (Global.GetCurrentTime() > SendHeartTicks + HeartTicks)
            {
                SendHeartTicks = Global.GetCurrentTime();

                SendClientHeart();
            }
        }

        private void SendClientHeart()
        {
            Console.WriteLine("Role {0} send heart", RoleID);

            SCClientHeart ClientHeart = new SCClientHeart();
            ClientHeart.RoleID = RoleID;
            ClientHeart.RandToken = Token;
            ClientHeart.ClientTicks = Global.GetCurrentTime();
            byte[] bData = Server.Tools.DataHelper.ObjectToBytes<SCClientHeart>(ClientHeart);

            TCPOutPacket outPacket = new TCPOutPacket();
            outPacket.PacketCmdID = (int)TCPGameServerCmds.CMD_SPR_CLIENTHEART;
            outPacket.FinalWriteData(bData, 0, bData.Length);

            Client?.SendData(outPacket);
        }
    }

    /// <summary>
    /// Gói tin Ping gửi từ Server về Client
    /// </summary>
    [ProtoContract]
    public class SCClientHeart
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Random Token
        /// </summary>
        [ProtoMember(2)]
        public int RandToken { get; set; }

        /// <summary>
        /// Tick hiện tại của Server
        /// </summary>
        [ProtoMember(3)]
        public long Ticks { get; set; }

        /// <summary>
        /// Tick hiện tại của Client
        /// </summary>
        [ProtoMember(4)]
        public long ClientTicks { get; set; }
    }
}