using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using System;

namespace CSRAutoUpdater_yea.Utils
{
    public static class Discord
    {
        private static readonly string _appId = "1334635675403685908";
        private static DiscordRpcClient _client = new DiscordRpcClient(_appId);
        private static RichPresence _presence = new RichPresence();
        public static string CurrentUserId { get; private set; } // ! DEPRECATED ! for whitelist check

        public static void Init()
        {
            _client.OnReady += OnReady;


            if (!_client.Initialize())
            {
                return;
            }

            SetDetails("In Launcher");
            SetLargeArtwork("icon");

            Update();
        }

        public static void Update() => _client.SetPresence(_presence);

        public static void SetDetails(string details) => _presence.Details = details;
        public static void SetState(string state) => _presence.State = state;

        /// <summary>
        ///  eええええええええ、ThisLibaryIsSoShitAtThisPointXD
        /// </summary>
        public static void SetTimestamp(DateTime time)
        {
            if (_presence.Timestamps == null) _presence.Timestamps = new Timestamps();
            _presence.Timestamps.Start = time; 
        }

        public static void SetLargeArtwork(string key)
        {
            if (_presence.Assets == null) _presence.Assets = new Assets();
            _presence.Assets.LargeImageKey = key; 
        }

        public static void SetSmallArtworkText(string small_text) => _presence.Assets.SmallImageText = small_text;

        public static void SetLargeArtworkText(string large_text) => _presence.Assets.LargeImageText = large_text;

        public static void SetSmallArtwork(string key)
        {
            if (_presence.Assets == null) _presence.Assets = new Assets();
            _presence.Assets.SmallImageKey = key;
        }

        private static void OnReady(object sender, ReadyMessage e)
        {
            CurrentUserId = e.User.ID.ToString(); // ! DEPRECATED ! for passing current uid to api
        }
    }
}
