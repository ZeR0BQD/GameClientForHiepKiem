using System;
using System.Collections.Generic;
using GameClient.MapConfig;

namespace GameClient.Scenarios
{
    /// <summary>
    /// Kich ban Song Jin - quan ly chia doi va dieu khien 2 phe Song va Jin
    /// </summary>
    public class SongJinScenario
    {
        // NPC Code constants
        private const int SONG_NPC_CODE = 55;  // Tống Hiệu Uý
        private const int JIN_NPC_CODE = 51;   // Kim Hiệu Uý

        private static Random _random = new Random();

        /// <summary>
        /// Dictionary tra cuu team theo RoleID
        /// Key = RoleID, Value = Dictionary<AIClient, "Song" hoac "Jin">
        /// </summary>
        public static Dictionary<int, Dictionary<AIClient, string>> TeamMap { get; private set; } = new();

        public static List<AIClient> TeamSong
        {
            get
            {
                var list = new List<AIClient>();
                foreach (var dict in TeamMap.Values)
                {
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value == "Song")
                        {
                            list.Add(kvp.Key);
                        }
                    }
                }
                return list;
            }
        }

        public static List<AIClient> TeamJin
        {
            get
            {
                var list = new List<AIClient>();
                foreach (var dict in TeamMap.Values)
                {
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value == "Jin")
                        {
                            list.Add(kvp.Key);
                        }
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// Khoi tao kich ban
        /// </summary>
        public static void Init()
        {
            Console.WriteLine("[SongJinScenario]<Init> Khoi tao kich ban Song Jin");


            DivideTeams();
            GoToSongJin();
            ClickNpcSongJin();
        }

        /// <summary>
        /// Chia doi: lay AIManager.Clients, chia xen ke
        /// - Index chan -> TeamSong
        /// - Index le -> TeamJin
        /// Dong thoi ghi vao TeamMap de tra cuu team theo RoleID
        /// </summary>
        public static void DivideTeams()
        {
            var clients = AIManager.Clients;
            TeamMap.Clear();

            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                string team = (i % 2 == 0) ? "Song" : "Jin";

                if (!TeamMap.ContainsKey(client.RoleID))
                {
                    TeamMap[client.RoleID] = new Dictionary<AIClient, string>();
                }
                TeamMap[client.RoleID][client] = team;
            }

            Console.WriteLine("[SongJinScenario]<DivideTeams> Chia doi thanh cong");
        }

        /// <summary>
        /// Tra cuu team cua 1 client theo RoleID
        /// Tra ve "Song", "Jin" hoac "Unknown"
        /// </summary>
        public static string GetTeam(int roleID)
        {
            if (TeamMap.TryGetValue(roleID, out var dict))
            {
                foreach (var team in dict.Values)
                {
                    return team;
                }
            }
            return "Unknown";
        }

        /// <summary>
        /// Di chuyen 2 team den NPC tuong ung voi random offset
        /// </summary>
        public static void GoToSongJin()
        {
            var songNPC = MapConfigHelper.GetNPCByCode(SONG_NPC_CODE);
            var jinNPC = MapConfigHelper.GetNPCByCode(JIN_NPC_CODE);

            // Clear action queue của tất cả client
            foreach (var dict in TeamMap.Values)
            {
                foreach (var client in dict.Keys)
                {
                    client.ClearActions();
                }
            }

            // TeamSong di chuyen den Tong Hieu Uy
            if (songNPC != null)
            {
                var teamSong = TeamSong;
                Console.WriteLine($"[SongJinScenario]<GoToSongJin> TeamSong di chuyen den {songNPC.Name}. Count: {teamSong.Count}");
                foreach (var client in teamSong)
                {
                    int offsetX = _random.Next(-10, 30);
                    int offsetY = _random.Next(-10, 30);
                    client.AddAction(() => client.SendMove(new Position() { PosX = songNPC.X + offsetX, PosY = songNPC.Y + offsetY }));
                }
            }
            else
            {
                Console.WriteLine("[SongJinScenario]<GoToSongJin> Khong tim thay Tong Hieu Uy");
            }

            // TeamJin di chuyen den Kim Hieu Uy
            if (jinNPC != null)
            {
                var teamJin = TeamJin;
                Console.WriteLine($"[SongJinScenario]<GoToSongJin> TeamJin di chuyen den {jinNPC.Name}. Count: {teamJin.Count}");
                foreach (var client in teamJin)
                {
                    int offsetX = _random.Next(-10, 30);
                    int offsetY = _random.Next(-10, 30);
                    client.AddAction(() => client.SendMove(new Position() { PosX = jinNPC.X - offsetX, PosY = jinNPC.Y - offsetY }));
                }
            }
            else
            {
                Console.WriteLine("[SongJinScenario]<GoToSongJin> Khong tim thay Kim Hieu Uy");
            }
        }

        /// <summary>
        /// Click NPC dang ky Song Jin theo team cua tung client
        /// </summary>
        public static void ClickNpcSongJin()
        {
            foreach (var dict in TeamMap.Values)
            {
                foreach (var kvp in dict)
                {
                    var client = kvp.Key;
                    var team = kvp.Value;

                    if (team == "Song")
                    {
                        client.NPCClick(2130706699);
                    }
                    else if (team == "Jin")
                    {
                        client.NPCClick(2130706700);
                    }
                }
            }
        }

        /// <summary>
        /// Di chuyen tat ca clients den Xa Phu ngau nhien tren map hien tai
        /// </summary>
        public static void MoveToXaPhu()
        {
            Console.WriteLine("[SongJinScenario]<MoveToXaPhu> Di chuyen tat ca clients den Xa Phu");
            foreach (var client in AIManager.Clients)
            {
                client.MoveToXaPhu();
            }
        }



        /// <summary>
        /// Send GM command to all clients
        /// </summary>
        /// 

        public static void SendGMCommand(string command)
        {
            Console.WriteLine($"[SongJinScenario]<SendGMCommandToAll> Send GM command to all clients: {command}");
            foreach (var client in AIManager.Clients)
            {
                client.SendGMCommand(command);
            }
        }

        public static void SendMove(Position pos)
        {
            Console.WriteLine($"[SongJinScenario]<SendGMCommandToAll> SendMove to all clients");
            foreach (var client in AIManager.Clients)
            {
                client.SendMove(pos);
            }
        }

        public static void AutoMoveAroundForAll()
        {
            foreach (var client in AIManager.Clients)
            {
                client.AutoMoveAround();
            }
        }

        public static void AutoMoveToPKForAll()
        {
            foreach (var client in AIManager.Clients)
            {
                client.AutoMoveToPKSongJin();
            }
        }
    }
}
