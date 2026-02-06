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
        /// Danh sach thanh vien doi Song (nua dau cua Clients)
        /// </summary>
        public static List<AIClient> TeamSong { get; private set; } = new();

        /// <summary>
        /// Danh sach thanh vien doi Jin (nua sau cua Clients)
        /// </summary>
        public static List<AIClient> TeamJin { get; private set; } = new();

        /// <summary>
        /// Danh sach xen ke Song-Jin (index 0=Song, 1=Jin, 2=Song, 3=Jin...)
        /// </summary>
        public static List<AIClient> AllTeams { get; private set; } = new();

        /// <summary>
        /// Chia doi: lay AIManager.Clients, chia lam 2
        /// - Nua dau (index 0 → half-1) → TeamSong
        /// - Nua sau (index half → end) → TeamJin
        /// </summary>
        public static void DivideTeams()
        {
            var clients = AIManager.Clients;
            int total = clients.Count;
            int half = total / 2;

            TeamSong.Clear();
            TeamJin.Clear();

            // Nua dau cho TeamSong
            for (int i = 0; i < half; i++)
            {
                TeamSong.Add(clients[i]);
            }

            // Nua sau cho TeamJin
            for (int i = half; i < total; i++)
            {
                TeamJin.Add(clients[i]);
            }


            // Build danh sach xen ke Song-Jin
            AllTeams.Clear();
            int maxCount = Math.Max(TeamSong.Count, TeamJin.Count);
            for (int i = 0; i < maxCount; i++)
            {
                if (i < TeamSong.Count)
                    AllTeams.Add(TeamSong[i]);
                if (i < TeamJin.Count)
                    AllTeams.Add(TeamJin[i]);
            }

            Console.WriteLine($"[SongJinScenario]<DivideTeams> TeamSong: {TeamSong.Count}, TeamJin: {TeamJin.Count}, AllTeams: {AllTeams.Count}");
        }

        /// <summary>
        /// Di chuyen 2 team den NPC tuong ung voi random offset
        /// </summary>
        public static void GoToSongJin()
        {
            var songNPC = MapConfigHelper.GetNPCByCode(SONG_NPC_CODE);
            var jinNPC = MapConfigHelper.GetNPCByCode(JIN_NPC_CODE);

            // TeamSong di chuyen den Tống Hiệu Uý
            if (songNPC != null)
            {
                Console.WriteLine($"[SongJinScenario]<GoToSongJin> TeamSong di chuyen den {songNPC.Name}");
                foreach (var client in TeamSong)
                {
                    int offsetX = _random.Next(10, 30);
                    int offsetY = _random.Next(10, 30);
                    client.SendGMCommand($"GoTo 32 {songNPC.X + offsetX} {songNPC.Y + offsetY}");
                }
            }
            else
            {
                Console.WriteLine("[SongJinScenario]<GoToSongJin> Khong tim thay Tong Hieu Uy");
            }

            // TeamJin di chuyen den Kim Hiệu Uý
            if (jinNPC != null)
            {
                Console.WriteLine($"[SongJinScenario]<GoToSongJin> TeamJin di chuyen den {jinNPC.Name}");
                foreach (var client in TeamJin)
                {
                    int offsetX = _random.Next(10, 30);
                    int offsetY = _random.Next(10, 30);
                    client.SendGMCommand($"GoTo 32 {jinNPC.X - offsetX} {jinNPC.Y - offsetY}");
                }
            }
            else
            {
                Console.WriteLine("[SongJinScenario]<GoToSongJin> Khong tim thay Kim Hieu Uy");
            }
        }

        /// <summary>
        /// Click npc 
        /// <summary>
        /// 

        public static void ClickNpcSongJin()
        {
            // TeamSong click Tong Hieu Uy
            for (int i = 0; i < AllTeams.Count; i++)
            {
                var client = AllTeams[i];
                if (i % 2 == 0)
                {
                    client.NPCClick(2130706699);
                }
                else
                {
                    client.NPCClick(2130706700);
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

        public static void AutoMoveAround()
        {
            Console.WriteLine("[SongJinScenario]<AutoMoveAround> Di chuyen tat ca clients di xung quanh");
            foreach (var client in AIManager.Clients)
            {
                client.AutoMoveAround();
            }
        }
    }
}
