using System;
using System.Xml.Linq;

namespace GameClient.Data
{
	public class SongJinConfig
	{
		public List<TimeSpan> OpenTime { get; set; } = new List<TimeSpan>();

		public int PreTime { get; set; }

		public GamePosition? SongSigninPos { get; set; }

        public GamePosition? JinSigninPos { get; set; }

        public static void Init()
		{
            var xml = XElement.Load(@"Config/songjin.xml");

            SongJinConfig songJinConfig = new SongJinConfig();
            songJinConfig.PreTime = (int)Global.GetSafeAttributeLong(xml, "OpenTime", "pretime");


            var opentime = Global.GetSafeAttributeStr(xml, "OpenTime", "time");
			var opentimes = opentime.Split(";");
			foreach (var item in opentimes)
			{
				var items = item.Split(":");
				int hour = int.Parse(items[0]);
                int min = int.Parse(items[1]);

				songJinConfig.OpenTime.Add(new TimeSpan(hour, min, 0));
            }

			songJinConfig.SongSigninPos = new GamePosition();
            songJinConfig.SongSigninPos.MapId = (int)Global.GetSafeAttributeLong(xml, "SongSignPos", "MapId");
            songJinConfig.SongSigninPos.NpcId = (int)Global.GetSafeAttributeLong(xml, "SongSignPos", "NpcId");
            songJinConfig.SongSigninPos.PosX = (int)Global.GetSafeAttributeLong(xml, "SongSignPos", "PosX");
            songJinConfig.SongSigninPos.PosY = (int)Global.GetSafeAttributeLong(xml, "SongSignPos", "PosY");

			songJinConfig.JinSigninPos = new GamePosition();
            songJinConfig.JinSigninPos.MapId = (int)Global.GetSafeAttributeLong(xml, "JinSignPos", "MapId");
            songJinConfig.JinSigninPos.PosX = (int)Global.GetSafeAttributeLong(xml, "JinSignPos", "PosX");
            songJinConfig.JinSigninPos.PosY = (int)Global.GetSafeAttributeLong(xml, "JinSignPos", "PosY");
        }
	}

	public class GamePosition
	{
		public int NpcId {  get; set; }

        public int MapId { get; set; }

		public int PosX { get; set; }

		public int PosY { get; set; }
	}
}

