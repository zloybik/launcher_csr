using CSGSI;
using CSGSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using ConsoleCustom = CSRAutoUpdater_yea.Utils.ConsoleCustom;

namespace CSRAutoUpdater_yea.Utils
{
    public static class Game
    {
        private static Process _process;
        private static GameStateListener _listener;
        private static int _port;
        private static MapNode _node;
        private static PlayerNode _player;

        private static string _map = "main_menu";
        private static int _scoreCT = 0;
        private static int _scoreT = 0;

        public static async Task<bool> Launch()
        {
            List<string> Argumentss = Arguments.GenerateGameArguments();
            if (Argumentss.Count > 0) Console.WriteLine($"Launch Params: {string.Join(" ", Argumentss)}");

            string directory = Directory.GetCurrentDirectory();
            Console.WriteLine($"Directory: {directory}");

            string gameStatePath = $"{directory}/csgo/cfg/gamestate_integration_csr.cfg";
            
            if (!Arguments.Exists("--disable-rpc"))
            {
                _port = GeneratePort();

                _listener = new GameStateListener($"http://localhost:{_port}/");
                _listener.NewGameState += OnNewGameState;
                _listener.Start();

                File.WriteAllText(
                    gameStatePath,
                $@"""Restored""
                {{
                    ""uri""                         ""http://localhost:{_port}""
                    ""timeout""                     ""5.0""
                    ""data""
                    {{
                        ""provider""                ""1""
                        ""map""                     ""1""
                        ""round""                   ""1""
                        ""player_id""               ""1""
                        ""player_weapons""          ""1""
                        ""player_match_stats""      ""1""
                        ""player_state""            ""1""
                        ""allplayers_id""           ""1""
                        ""allplayers_state""        ""1""
                        ""allplayers_match_stats""  ""1""
                    }}
                }}"
                );

            }
            else if (File.Exists(gameStatePath)) File.Delete(gameStatePath);

            _process = new Process();
            _process.StartInfo.FileName = $"{directory}\\csgo.exe";
            _process.StartInfo.Arguments = string.Join(" ", Argumentss);

            return _process.Start();
        }

        public static async Task Monitor()
        {
            while (true)
            {
                if (_process == null)
                    break;

                try
                {
                    Process.GetProcessById(_process.Id);
                }
                catch
                {
                    Environment.Exit(1);
                }

                if (_node != null && _node.Name.Trim().Length != 0)
                {
                    if (_map != _node.Name)
                    {
                        _map = _node.Name;
                        _scoreCT = _node.TeamCT.Score;
                        _scoreT = _node.TeamT.Score;
                        JObject playerJSON = JObject.Parse(_player.JSON);

                        int kills = playerJSON["match_stats"]["kills"]?.Value<int>() ?? 0;
                        int deaths = playerJSON["match_stats"]["deaths"]?.Value<int>() ?? 0;

                        double kd = deaths == 0 ? kills : (double)kills / deaths;

                        Discord.SetDetails($"{kd:F2} KD");
                        Discord.SetState($"CT:{_scoreCT} T:{_scoreT}");
                        Discord.SetLargeArtwork(_map); // actualy in my bot i have more maps XDDD
                        Discord.SetLargeArtworkText(_map); // why not tho
                        Discord.SetSmallArtwork("icon");
                        Discord.Update();
                    }

                    if (_scoreCT != _node.TeamCT.Score || _scoreT != _node.TeamT.Score)
                    {
                        _scoreCT = _node.TeamCT.Score;
                        _scoreT = _node.TeamT.Score;

                        JObject playerJSON = JObject.Parse(_player.JSON);

                        int kills = playerJSON["match_stats"]["kills"]?.Value<int>() ?? 0;
                        int deaths = playerJSON["match_stats"]["deaths"]?.Value<int>() ?? 0;

                        double kd = deaths == 0 ? kills : (double)kills / deaths;

                        Discord.SetLargeArtwork(_map); // actualy in my bot i have more maps XDDD
                        Discord.SetLargeArtworkText(_map); // why not tho
                        Discord.SetDetails($"{kd:F2} KD");
                        Discord.SetState($"CT:{_scoreCT} T:{_scoreT}");
                        Discord.SetSmallArtwork($"{playerJSON["team"].ToString().ToLower()}");
                        if(playerJSON["team"].ToString().ToLower() == "ct") Discord.SetSmallArtworkText("Counter-Terrorist");
                        else if (playerJSON["team"].ToString().ToLower() == "t") Discord.SetSmallArtworkText("Terrorist");
                        else Discord.SetSmallArtworkText("Spectator");
                        Discord.Update();
                    }
                }
                else if (_map != "main_menu")
                {
                    _map = "main_menu";
                    _scoreCT = 0;
                    _scoreT = 0;

                    Discord.SetDetails("In Main Menu");
                    Discord.SetState(null);
                    Discord.SetLargeArtwork("icon");
                    Discord.SetSmallArtwork(null);
                    Discord.SetSmallArtworkText(null);
                    Discord.Update();
                }

                await Task.Delay(2000);
            }
        }

        private static int GeneratePort()
        {
            int port = new Random().Next(1024, 65536);

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            while (properties.GetActiveTcpConnections().Any(x => x.LocalEndPoint.Port == port))
            {
                port = new Random().Next(1024, 65536);
            }

            return port;
        }

        public static void OnNewGameState(GameState gs)
        {
            _node = gs.Map;
            _player = gs.Player;
        }
    }
}
