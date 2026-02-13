using System;
using System.Collections.Generic;
using System.Timers;
using System.Xml.Linq;

namespace GameClient
{
    /// <summary>
    /// Kiem tra thoi gian su kien Song Jin
    /// Doc cac khung gio OpenTime tu Battle_SongJin_Low.xml
    /// Tu dong chay timer lien tuc de kiem tra va Console khi den moc
    /// </summary>
    public static class EventTimeChecker
    {
        // Danh sach cac khung gio mo su kien (doc tu XML)
        private static readonly List<TimeSpan> _openTimes = new List<TimeSpan>();

        // Timer chay lien tuc de kiem tra thoi gian
        private static System.Timers.Timer? _checkTimer;

        // Interval kiem tra (ms) - moi 1 giay
        private const int CHECK_INTERVAL_MS = 1000;

        /// <summary>
        /// Khoi tao - doc OpenTime tu Battle_SongJin_Low.xml va bat dau timer kiem tra lien tuc
        /// </summary>
        public static void Init(string battleXmlPath = @"Config/Battle_SongJin_Low.xml")
        {
            _openTimes.Clear();

            try
            {
                var battleXml = XElement.Load(battleXmlPath);

                // Lay danh sach OpenTime
                foreach (var openTimeElement in battleXml.Elements("OpenTime"))
                {
                    string hoursStr = openTimeElement.Attribute("Hours")?.Value ?? "0";
                    string minuteStr = openTimeElement.Attribute("Minute")?.Value ?? "0";
                    int hours = int.Parse(hoursStr);
                    int minute = int.Parse(minuteStr);
                    _openTimes.Add(new TimeSpan(hours, minute, 0));
                }

                // Sap xep tang dan
                _openTimes.Sort();

                Console.WriteLine($"[EventTimeChecker]<Init> Doc thanh cong {_openTimes.Count} khung gio");
                foreach (var time in _openTimes)
                {
                    Console.WriteLine($"[EventTimeChecker]<Init> Khung gio su kien: {time.Hours:D2}:{time.Minutes:D2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventTimeChecker]<Init> Loi doc Battle XML: {ex.Message}");
                return;
            }

            // Khoi tao timer kiem tra lien tuc (giong keyboardTimer trong AIManager)
            _checkTimer = new System.Timers.Timer(CHECK_INTERVAL_MS);
            _checkTimer.Elapsed += CheckTimer_Elapsed;
            _checkTimer.Start();

            Console.WriteLine($"[EventTimeChecker]<Init> Da bat dau timer kiem tra moi {CHECK_INTERVAL_MS / 1000}s");
        }

        /// <summary>
        /// Ham duoc goi lien tuc boi timer de kiem tra thoi gian hien tai
        /// </summary>
        private static void CheckTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            int currentHour = now.Hour;
            int currentMinute = now.Minute;

            foreach (var openTime in _openTimes)
            {
                // So sanh gio:phut hien tai voi moc su kien
                if (currentHour == openTime.Hours && currentMinute == openTime.Minutes)
                {
                    Console.WriteLine($"[EventTimeChecker]<CheckTimer> === DEN GIO SU KIEN SONG JIN === " +
                        $"Khung gio {openTime.Hours:D2}:{openTime.Minutes:D2} - Gio hien tai: {now:HH:mm:ss}");
                    return;
                }
            }
        }

        /// <summary>
        /// Dung timer kiem tra
        /// </summary>
        public static void Stop()
        {
            _checkTimer?.Stop();
            _checkTimer?.Dispose();
            _checkTimer = null;
            Console.WriteLine("[EventTimeChecker]<Stop> Da dung timer kiem tra");
        }
    }
}
