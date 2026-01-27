using GameClient.Data;
using GameClient.Databases;
using Newtonsoft.Json;
using System.Security.Principal;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GameClient
{
	class Program
	{
        public static bool Started = false;
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);
        private static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            XElement xml = null;

            System.Console.WriteLine("Init App Config");
#if DEBUG
            string configPath = @"AppConfig.Debug.xml";
#else
            string configPath = @"AppConfig.xml";
#endif
            try
            {
                xml = XElement.Load(configPath);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read XML file '{0}' faild.", @"AppConfig.xml"));
            }

            AIManager.ServerID = (int)Global.GetSafeAttributeLong(xml, "GameClient", "ServerID");
            AIManager.MaxRole = (int)Global.GetSafeAttributeLong(xml, "GameClient", "MaxRole");
            AIManager.Server = Global.GetSafeAttributeStr(xml, "GameClient", "Address");
            AIManager.ServerPort = (int)Global.GetSafeAttributeLong(xml, "GameClient", "ServerPort");

            AccountController.LoginAPIUrl = Global.GetSafeAttributeStr(xml, "GameClient", "LoginAPIUrl");
            AccountController.RegisterAPIUrl = Global.GetSafeAttributeStr(xml, "GameClient", "RegisterAPIUrl");
            AccountController.VerifyAPIUrl = Global.GetSafeAttributeStr(xml, "GameClient", "VerifyAPIUrl");

            //CreateAccounts(xml, AIManager.MaxRole);

            SongJinConfig.Init();

            using (var stream = System.IO.File.OpenRead("Config/Positions.xml"))
            {
                var serializer = new XmlSerializer(typeof(List<Position>));
                var points = serializer.Deserialize(stream) as List<Position>;
                if (points != null)
                {
                    AIManager.Points.AddRange(points);
                }
            }

            Console.WriteLine("Server started");

            Started = true;
            NameManager.Init();
            AIManager.Init();
            //SongJinAIManager.Init();

            _quitEvent.WaitOne();

            Console.WriteLine("Server stopped");
        }


        //private const string Account = "tpbo00yo3kj3gthc8lp";
        private const string Account = "itsgamecenter2025";
        private const string PasswordHash = "$2y$12$J/YAqyMrpceV8ygrCmATyOLlW8y2DfSbSjss7EekpQVyMYRUgKAOG";

        private static void CreateAccounts(XElement xml, int maxAccount)
        {
            string login = Global.GetSafeAttributeStr(xml, "Database", "uname");
            string pass = Global.GetSafeAttributeStr(xml, "Database", "upasswd");
            string ip = Global.GetSafeAttributeStr(xml, "Database", "ip");
            string dname = Global.GetSafeAttributeStr(xml, "Database", "dname");
            string names = Global.GetSafeAttributeStr(xml, "Database", "names");

            AccountContext.ConnectionString = string.Format("Server={0};Database={1};port=3306;User Id={2};password={3};Charset={4};", ip, dname, login, pass, names);
            Console.WriteLine(AccountContext.ConnectionString);


            using (AccountContext accountContext = new AccountContext())
            {
                for (int i = 0; i < maxAccount; i++)
                {
                    string account = string.Format("{0}{1}", Account, i + 1);
                    var user = accountContext.Users.FirstOrDefault(u => u.UserName == account);
                    if (user == null)
                    {
                        user = new UserModel()
                        {
                            Email = account,
                            UserName = account,
                            Name = account,
                            Password = PasswordHash,
                        };

                        accountContext.Users.Add(user);

                        accountContext.SaveChanges();
                    }
                }
            }
        }
	}
}