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
        private const int SONG_NPC_CODE = 55; // Tống Hiệu Uý
        private const int JIN_NPC_CODE = 51;  // Kim Hiệu Uý

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
        public static void StartSongJin()
        {
            Console.WriteLine("[SongJinScenario]<Init> Khoi tao kich ban Song Jin");
            ClearActionsForAll();
            DivideTeams();
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
            GoToSongJin();
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
            // NPC chon phe (Tong/Kim Hieu Uy) nam o map 32 - TuongDuong
            string songJinMapCode = MapConfigHelper.GetMapCode(32) ?? "TuongDuong";
            var songNPC = MapConfigHelper.GetNPCByCode(songJinMapCode, SONG_NPC_CODE);
            var jinNPC = MapConfigHelper.GetNPCByCode(songJinMapCode, JIN_NPC_CODE);

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
                Console.WriteLine($"[SongJinScenario]<GoToSongJin> TeamSong di chuyen den {songNPC.Name}. So Luong: {teamSong.Count}");
                foreach (var client in teamSong)
                {
                    int offsetX = _random.Next(-10, 30);
                    int offsetY = _random.Next(-10, 30);
                    client.AddAction("Song_Move", () => client.SendMove(new Position() { PosX = songNPC.X + offsetX, PosY = songNPC.Y + offsetY }));
                    client.AddAction("Song_Click", () => ClickNpcSongJin(client));
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
                    client.AddAction("Jin_Move", () => client.SendMove(new Position() { PosX = jinNPC.X - offsetX, PosY = jinNPC.Y - offsetY }));
                    client.AddAction("Jin_Click", () => ClickNpcSongJin(client));
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
        public static void ClickNpcSongJin(AIClient client)
        {
            var team = GetTeam(client.RoleID);
            if (team == "Song")
            {
                client.NPCClick(2130706579);
                client.ReadyAction = false;
                client.AddAction("MoveToTeleporter", () => client.MoveToTeleport());
            }
            else if (team == "Jin")
            {
                client.NPCClick(2130706580);
                client.ReadyAction = false;
                client.AddAction("MoveToTeleporter", () => client.MoveToTeleport());
            }
        }

        /// <summary>
        /// Click NPC cho tat ca client (Dung cho debug hoac force click)
        /// </summary>
        public static void ClickNpcSongJin()
        {
            foreach (var dict in TeamMap.Values)
            {
                foreach (var kvp in dict)
                {
                    ClickNpcSongJin(kvp.Key);
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

        public static void SendGMCommandForAll(string command)
        {
            Console.WriteLine($"[SongJinScenario]<SendGMCommandToAll> Send GM command to all clients: {command}");

            string[] parts = command.Split(' ');
            // Kiểm tra xem có phải lệnh GoTo chuyển Map không (GoTo + MapId + X + Y)
            bool isTeleportToNewMap = parts.Length == 4 && parts[0] == "GoTo";

            foreach (var client in AIManager.Clients)
            {
                if (isTeleportToNewMap)
                {
                    // Nếu dịch chuyển sang map mới, xóa sạch hàng đợi hành động cũ
                    client.ClearActions();
                }
                client.SetStateReadyAction(false);
                // Thêm lệnh vào hàng đợi để thực hiện lần lượt
                client.AddAction("SendGMCommand", () => { client.SendGMCommand(command); });
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
        public static void ClearActionsForAll()
        {
            foreach (var client in AIManager.Clients)
            {
                client.ClearActions();
            }
        }
    }
}