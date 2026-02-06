using GameClient.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text;

namespace GameClient
{
    public delegate void LoginDelegate(bool success, string accessToken);

    internal static class AccountController
    {
        private const string HTTP_MD5_KEY = "Jab2hKa821bJac2Laocb2acah2acacak";
        public static string LoginAPIUrl = "";
        public static string RegisterAPIUrl = "";
        public static string VerifyAPIUrl = "";

        public static async void Login(string username, string password, LoginDelegate callback)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                JObject obj = new JObject();
                obj["email"] = username;
                obj["password"] = password;

                StringContent stringContent = new StringContent(obj.ToString(), Encoding.UTF8, "application/json");
                var res = await httpClient.PostAsync(LoginAPIUrl, stringContent);
                if (res.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    callback.Invoke(false, res.StatusCode.ToString());
                    return;
                }


                string content = await res.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<LoginRep>(content);
                if (response == null)
                {
                    callback.Invoke(false, "Login failed");
                    return;
                }

                if (response.ErrorCode != 0)
                {
                    callback.Invoke(false, "Login failed");
                    return;
                }

                callback.Invoke(true, response.AccessToken);
            }
        }

        public static async void Register(string username, string password, Action<bool> callback)
        {
            Console.WriteLine("Register account {0}", username);
            using (HttpClient httpClient = new HttpClient())
            {
                JObject obj = new JObject();
                obj["email"] = username;
                obj["password"] = password;

                StringContent stringContent = new StringContent(obj.ToString(), Encoding.UTF8, "application/json");
                var res = await httpClient.PostAsync(RegisterAPIUrl, stringContent);
                if (res.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    callback.Invoke(false);
                    return;
                }


                string content = await res.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<LoginRep>(content);
                if (response == null)
                {
                    callback.Invoke(false);
                    return;
                }

                if (response.ErrorCode != 0)
                {
                    callback.Invoke(false);
                    return;
                }

                callback.Invoke(true);
            }
        }

        public async static void AccountVerify(string accessToken, Action<bool, ServerVerifySIDData?> callback)
        {
            var lTime = Global.GetCurrentTime().ToString();
            string hash = string.Format("{0}{1}{2}", HTTP_MD5_KEY, accessToken, lTime);
            var strMD5 = Global.MakeMD5Hash(hash);

            using (HttpClient httpClient = new HttpClient())
            {
                JObject obj = new JObject();
                obj["lTime"] = lTime;
                obj["strSID"] = accessToken;
                obj["strMD5"] = strMD5;
                obj["type"] = "1";

                StringContent stringContent = new StringContent(obj.ToString(), Encoding.UTF8, "application/json");
                var res = await httpClient.PostAsync(VerifyAPIUrl, stringContent);
                if (res.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    callback.Invoke(false, null);
                    return;
                }


                string content = await res.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<ServerVerifySIDData>(content);
                if (response == null)
                {
                    callback.Invoke(false, null);
                    return;
                }

                string strUID = response.strPlatformUserID;
                int roleId = int.Parse(strUID);

                if (roleId <= 0)
                {
                    callback.Invoke(false, null);
                    return;
                }

                callback.Invoke(true, response);
            }
        }
    }
}
