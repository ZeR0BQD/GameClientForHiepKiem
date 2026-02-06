using System.Text;
using Newtonsoft.Json;
using ProtoBuf;

namespace GameClient.Data
{
    public class ServerVerifySIDDataRequest
    {
		[JsonProperty("lTime")]
		public int Time { get; set; }

        [JsonProperty("strSID")]
        public string UserToken { get; set; }

		[JsonProperty("strMD5")]
		public string Hash
        {
            get
            {
                string hash = string.Format("{0}{1}{2}", AIManager.HTTP_MD5_KEY, UserToken, Time);
                return CreateMD5(hash);
            }
        }

        [JsonProperty("type")]
        public string Type { get; set; }

        private string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToUpper();
            }
        }
    }

	public class ServerVerifySIDData
	{
        public string strPlatformUserID { get; set; }

        public string strAccountName { get; set; }

        public long lTime { get; set; }

        public string strCM { get; set; }

        public string strToken { get; set; }
    }
}

