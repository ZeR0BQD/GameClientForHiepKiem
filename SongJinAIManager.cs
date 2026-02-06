using GameClient.Data;
using GameClient.Databases;
using GameClient.Scenes;
using GameClient.SongJin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    public static class SongJinAIManager
    {
        public static SongJinConfig Config { get; set; } = new SongJinConfig();

        public static int MaxRole = 100;
        public static string Server = "192.168.1.10";
        public static int ServerPort = 3001;
        public static int ServerID = 1;


        private static List<SongJinAIClient> Clients = new List<SongJinAIClient>();
        private static List<User> Users = new List<User>();
        public static List<Position> Points = new List<Position>();

        private static System.Timers.Timer timer;

        public static void Init()
        {
            Console.WriteLine("Init map");
            GScene.Instance.Init("TongKimSoCap");

            Console.WriteLine("Loadding account info...");

            var json = File.ReadAllText("Config/users.json");
            Users.Clear();
            var users = JsonConvert.DeserializeObject<List<User>>(json);
            if (users != null)
            {
                Users.AddRange(users);
            }

            timer = new System.Timers.Timer(2000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private static bool IsSong = false;

        private static void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var user = Users.OrderBy(u => Guid.NewGuid()).FirstOrDefault(u => u.IsOnline == false);
            if (user == null)
            {
                return;
            }

            user.IsSong = IsSong;

            LoginNewAI(user);

            IsSong = !IsSong;
        }

        private static void Logout(SongJinAIClient client)
        {
            client.Logout();
        }

        private static void LoginNewAI(User user)
        {
            Console.WriteLine("Login account {0}", user.UserName);
            user.IsOnline = true;
            SongJinAIClient client = new SongJinAIClient(ServerID, user);
            client.LoginSuccess = (cl) =>
            {
                Clients.Add(client);
            };
            client.Login();
        }
    }
}
