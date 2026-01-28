using GameClient.Databases;
using GameClient.Scenes;
using HSGameEngine.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    public static class AIManager
    {
        public static int MaxRole = 100;
        public static string Server = "192.168.1.10";
        public static int ServerPort = 3001;
        public static int ServerID = 1;
        public const string HTTP_MD5_KEY = "Jab2hKa821bJac2Laocb2acah2acacak";

        private static List<AIClient> Clients = new List<AIClient>();
        private static List<UserModel> Users = new List<UserModel>();
        public static List<Position> Points = new List<Position>();

        private static System.Timers.Timer timer;

        private const string Account = "itsgamecenter2025{0}";
        //private const string Account = "tpbo0aoakj3gchc8lp{0}";
        private const string Password = "Its@Gamecenter@2025";

        public static void Init()
        {
            Console.WriteLine("Loadding account info...");

            GScene.Instance.Init("BaLangHuyen");

            // Subscribe cac CMD tu Observer
            ClientStateObserver.Subscribe(TCPGameServerCmds.CMD_PLAY_GAME, OnClientOnline);
            ClientStateObserver.Subscribe(TCPGameServerCmds.CMD_SPR_CHANGEPOS, OnClientMoved);

            for (int i = 0; i < MaxRole; i++)
            {
                string username = string.Format(Account, i + 1);
                string name = NameManager.RandomName(0);
                Users.Add(new UserModel()
                {
                    Email = username,
                    Password = Password,
                    Money = 0,
                    UserName = username
                });
            }

            timer = new System.Timers.Timer(2000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        // Handler: Client da online
        private static void OnClientOnline(CmdEventArgs args)
        {
            Console.WriteLine("[AIManager] Client #{0} ({1}) da online", args.ClientId, args.RoleName);
        }

        // Handler: Client da di chuyen
        private static void OnClientMoved(CmdEventArgs args)
        {
            var pos = args.Data as Position;
            if (pos != null)
            {
                Console.WriteLine("[AIManager] Client #{0} ({1}) da toi vi tri {2}/{3}",
                    args.ClientId, args.RoleName, pos.PosX, pos.PosY);
            }
        }

        private static void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var user = Users.FirstOrDefault(u => u.IsOnline == false);
            if (user == null)
            {
                return;
            }

            LoginNewAI(user);
        }

        private static void Logout(AIClient client)
        {
            client.Logout();
        }

        private static void LoginNewAI(UserModel user)
        {
            Console.WriteLine("Login account {0}", user.Email);
            user.IsOnline = true;
            AIClient client = new AIClient(ServerID, user.Id, user.Email);
            client.LoginSuccess = (cl) =>
            {
                Clients.Add(client);
            };
            client.Login(user.UserName, user.Password);
        }
    }
}
