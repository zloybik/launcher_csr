using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSRAutoUpdater_yea.Utils
{
    public static class Steam
    {
        [XmlRoot("profile")]
        public class SteamProfile
        {
            /// んんんんんん。。。。
            [XmlElement("avatarFull")]
            public string AvatarFull { get; set; }

            [XmlElement("location")]
            public string Location { get; set; }
        }

        private static readonly HttpClient http = new HttpClient();

        public static async Task<string> GetAvatarFullAsync(string steamId64)
        {
            var stream = await http.GetStreamAsync($"https://steamcommunity.com/profiles/{steamId64}?xml=1");
            var serializer = new XmlSerializer(typeof(SteamProfile));
            var profile = (SteamProfile)serializer.Deserialize(stream);
            return profile.AvatarFull;
        }

        public static async Task<string> GetLocationAsync(string steamId64)
        {
            var stream = await http.GetStreamAsync($"https://steamcommunity.com/profiles/{steamId64}?xml=1");
            var serializer = new XmlSerializer(typeof(SteamProfile));
            var profile = (SteamProfile)serializer.Deserialize(stream);
            return profile.Location;
        }
    }
}
