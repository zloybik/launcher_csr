using CSRAutoUpdater_yea.Utils;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using Args = CSRAutoUpdater_yea.Utils.Arguments;
using CCM = CSRAutoUpdater_yea.Utils.ConsoleColourMessages;
using Versions = CSRAutoUpdater_yea.Utils.Versions;

namespace CSRAutoUpdater_yea
{
    internal class Program
    {
        public static string api_url = "";
        public static string download_url = "";

        static async Task Main(string[] args)
        {
            AnsiConsole.MarkupLine($"{CCM._prefix} [white]Launcher![/]");
            CCM.Init();
            if (!File.Exists("csgo.exe"))
            {
                AnsiConsole.Markup(
                    "[red]Launcher was started from the wrong directory.[/]\n" +
                    "Please place the launcher in the root folder of the game (where csgo.exe is located) " +
                    "and run it again.\n\n" +
                    "Launcher will close in 10 seconds."
                );
                await Task.Delay(10000);
                Environment.Exit(1);
            }
            List<string> Argumentss = Arguments.GenerateGameArguments();
            if (Argumentss.Count > 0) Console.WriteLine($"Launch Params: {string.Join(" ", Argumentss)}");
            if (File.Exists("updater.exe"))
                File.Delete("updater.exe");
            if(!Arguments.Exists("--disable-rpc")) Discord.Init();
            string region = "Retard";
            if (Arguments.GetFastRegion() == null)
            {
                AnsiConsole.Markup("[yellow]Checking ping and status of servers...[/]");
                var regions = new (string Name, string Host)[]
                {
                    ("Europe", "download-api.csrestored.com")
                };

                var regionDict = regions.ToDictionary(
                    r => $"{r.Name} [grey](ping: {GetPing(r.Host)} ms | load: {GetLoad(r.Host)}%)[/]",
                    r => r.Name // Region
                );
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop);
                var selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Select Region for API & Download[/]")
                        .PageSize(10)
                        .AddChoices(regionDict.Keys)
                );
                region = regionDict[selection];
            }
            else region = Arguments.GetFastRegion();
            AnsiConsole.MarkupLine($"You selected: [yellow]{region}[/]");

            if (region.ToLowerInvariant() == "europe")
            {
                api_url = "https://download-api.csrestored.com";
                download_url = "https://download.csrestored.com";
            }
            /*else if (region.ToLowerInvariant() == "russia")
            {
                api_url = "";
                download_url = "";
            }*/
            else
            {
                AnsiConsole.Markup(
                    "[red]--fast-pick-region was found in the launch parameters, but the region is invalid. " +
                    "Please remove this launch parameter or provide a valid value.[/]\n" +
                    "Launcher is closing in 10 seconds."
                );
                await Task.Delay(10000);
                Environment.Exit(1);
            }
            Console.WriteLine("Checking for new updates...");
            await Versions.GetLastVersionOfLauncher();
        }

        static int GetPing(string host)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(host, 5000);
                    return reply.Status == IPStatus.Success ? (int)reply.RoundtripTime : -1;
                }
            }
            catch
            {
                return -1;
            }
        }

        static int GetLoad(string host)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(5);
                    string content = httpClient.GetStringAsync($"https://{host}/metrics").GetAwaiter().GetResult();
                    JObject responseJson = JObject.Parse(content);
                    return Convert.ToInt32(responseJson["load_percent"]);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
