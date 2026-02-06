using Newtonsoft.Json;

namespace GameClient.Data
{
    public class User
    {
        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("rolename")]
        public string RoleName { get; set; }

        public bool IsOnline { get; set; }

        public bool IsSong { get; set; }
    }
}
