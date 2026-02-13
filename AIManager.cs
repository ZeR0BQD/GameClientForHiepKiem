using GameClient.Databases;
using GameClient.MapConfig;
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
using UnityEngine;

namespace GameClient
{
    public static class AIManager
    {
        public static int MaxRole = 100;
        public static string Server = "192.168.1.10";
        public static int ServerPort = 3001;
        public static int ServerID = 1;
        public const string HTTP_MD5_KEY = "Jab2hKa821bJac2Laocb2acah2acacak";

        public static List<AIClient> Clients = new List<AIClient>();
        private static List<UserModel> Users = new List<UserModel>();
        public static List<Position> Points = new List<Position>();

        private static System.Timers.Timer? timer;
        private static System.Timers.Timer? keyboardTimer;

        /// <summary>
        /// So luong client da login thanh cong
        /// </summary>
        private static int CurrentRoleCount = 0;

        private const string Account = "itsgamecenter2025{0}";

        private const string Password = "Its@Gamecenter@2025";

        public static void Init()
        {
            Console.WriteLine("Loadding account info...");

            // Khoi tao scene theo Map ID (32 = TuongDuong)
            MapConfigHelper.InitSceneByMapId(32);

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

            // Timer de doc phim tu ban phim - goi Scenario methods
            keyboardTimer = new System.Timers.Timer(100);
            keyboardTimer.Elapsed += KeyboardTimer_Elapsed;
            keyboardTimer.Start();
        }

        /// <summary>
        /// Doc phim tu ban phim va goi Scenario methods tuong ung
        /// </summary>
        private static void KeyboardTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                switch (key.KeyChar)
                {
                    case '1':
                        Console.WriteLine("[AIManager]<KeyboardTimer_Elapsed> Init SongJin");
                        Scenarios.SongJinScenario.Init();
                        break;
                    case '2':
                        Console.WriteLine("[AIManager]<KeyboardTimer_Elapsed> MoveToXaPhu");
                        Scenarios.SongJinScenario.MoveToXaPhu();
                        break;
                    case '3':
                        Console.WriteLine("[AIManager]<KeyboardTimer_Elapsed> SendGMCommand");
                        Scenarios.SongJinScenario.SendGMCommand("GoTo 32 4947 4283"); ;
                        break;
                    case '4':
                        Console.WriteLine("[AIManager]<KeyboardTimer_Elapsed> SendMove");
                        Scenarios.SongJinScenario.SendMove(new Position { PosX = 1372, PosY = 1920 });
                        break;
                    case '5':
                        Console.WriteLine("[AIManager]<KeyboardTimer_Elapsed> AutoMoveAround");
                        Scenarios.SongJinScenario.AutoMoveAroundForAll();
                        break;
                    case '6':
                        Scenarios.SongJinScenario.DivideTeams();
                        break;
                    case '7':
                        Scenarios.SongJinScenario.GoToSongJin();
                        break;
                    case '8':
                        Scenarios.SongJinScenario.ClickNpcSongJin();
                        break;
                    case '9':
                        Scenarios.SongJinScenario.AutoMoveToPKForAll();
                        break;
                    default:
                        break;
                }
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
                CurrentRoleCount++;
                Console.WriteLine($"[AIManager]<LoginNewAI> CurrentRoleCount: {CurrentRoleCount}/{MaxRole}");

                //// Khi tat ca client da login, tu dong chay scenario
                //if (CurrentRoleCount >= MaxRole)
                //{
                //    Console.WriteLine("[AIManager]<LoginNewAI> All clients logged in, starting SongJin scenario...");
                //    Scenarios.SongJinScenario.DivideTeams();
                //    Scenarios.SongJinScenario.GoToSongJin();
                //}
            };
            client.Login(user.UserName, user.Password);
        }
    }
}
