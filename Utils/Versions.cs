using Newtonsoft.Json.Linq;
using Spectre.Console;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Main = CSRAutoUpdater_yea.Program;

namespace CSRAutoUpdater_yea.Utils
{
    public static class Versions
    {
        public static string CurrentVersionOfLauncher = "1.0.0.1";
        public static string LastVersionOfLauncher = "0.0.0.0";
        public static string GetFileMd5Hash(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Not found: {filePath}");

            using (var stream = File.OpenRead(filePath))
            {
                var md5 = MD5.Create();
                byte[] hashBytes = md5.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static async Task GetLastVersionOfGame()
        {
            Console.WriteLine("     2. Validating game files... ");
            var url = $"{Main.api_url}"; // хуярить сюда свой api так скажем

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var content = await httpClient.GetStringAsync(url);
                    JArray responsoe = JArray.Parse(content);
                    if (responsoe == null) return;
                    if (File.Exists("./csgo/pak01_085.vpk")) //  && GetFileMd5Hash("./csgo/pak01_085.vpk") == "f2412d5045a66ec2e6759ca051b128d2"
                    {
                        AnsiConsole.MarkupLine("[red]pak01_085.vpk found. Deleting...[/]");
                        File.Delete("./csgo/pak01_085.vpk");
                    }
                    else AnsiConsole.MarkupLine("[green]pak01_085.vpk was not found.[/]");

                    bool there_is_an_update = false;
                    foreach (var item in responsoe)
                    {
                        AnsiConsole.Markup($"[white]Checking {item["file"].ToString()} [/]");
                        if (File.Exists(item["file"].ToString()))
                        {
                            var fileInfo = new FileInfo(item["file"].ToString());

                            if (fileInfo.Length == (long)item["lenght"] && !Arguments.Exists("--check-only-by-hash") && !fileInfo.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
                            {
                                AnsiConsole.MarkupLine("[green]Already on the path[/]");
                                continue;
                            }
                            if (GetFileMd5Hash(item["file"].ToString()) != item["hash"].ToString()) File.Delete(item["file"].ToString());

                            else
                            {
                                AnsiConsole.MarkupLine("[green]Already on the path[/]");
                                continue;
                            }
                        }

                        AnsiConsole.Markup("[red]Not found. Downloading...[/]");
                        there_is_an_update = true;

                        using (var response = await httpClient.GetAsync($"{Main.download_url}/{item["file"]}", HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode();

                            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                            var canReportProgress = totalBytes != -1;

                            string finalFileName = item["file"].ToString();
                            string tempFileName = finalFileName + ".tmp";

                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            using (var fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                            {
                                var buffer = new byte[8192];
                                long totalRead = 0;
                                int read;
                                var stopwatch = Stopwatch.StartNew();

                                await AnsiConsole.Progress()
                                    .AutoClear(false)
                                    .HideCompleted(false)
                                    .StartAsync(async ctx =>
                                    {
                                        var task = ctx.AddTask("Downloading...", maxValue: canReportProgress ? totalBytes : 100);

                                        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                        {
                                            await fileStream.WriteAsync(buffer, 0, read);
                                            totalRead += read;

                                            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                                            var speed = elapsedSeconds > 0 ? totalRead / elapsedSeconds : 0; // bytes/sec
                                            var speedMB = speed / (1024 * 1024);

                                            if (canReportProgress)
                                            {
                                                task.MaxValue = totalBytes;
                                                task.Value = totalRead;
                                                task.Description = $"Downloading... {totalRead / (1024 * 1024)} / {totalBytes / (1024 * 1024)} MB ({speedMB:F2} MB/s)";
                                            }
                                            else
                                            {
                                                task.Increment(read);
                                                task.Description = $"Downloading... {totalRead / (1024 * 1024)} MB ({speedMB:F2} MB/s)";
                                            }
                                        }
                                    });
                            } 
                            if (File.Exists(finalFileName)) File.Delete(finalFileName);
                            File.Move(tempFileName, finalFileName);
                        }
                    }

                    if (there_is_an_update)
                    {
                        if (Directory.Exists("csgo/resource/flash/econ/weapons/cached"))
                        {
                            foreach (string file in Directory.GetFiles("csgo/resource/flash/econ/weapons/cached"))
                            {
                                File.Delete(file);
                            }
                        }
                        there_is_an_update = false;
                    }

                    await Game.Launch();
                    if (!Arguments.Exists("--disable-rpc"))
                    {
                        Discord.SetDetails("In Main Menu");
                        Discord.Update();
                        await Game.Monitor();
                        ConsoleColourMessages.WriteLine("The game has started. The launcher will minimize in 5 seconds.");
                        await Task.Delay(5000);
                        ConsoleCustom.HideConsole();
                    }
                    else {
                        ConsoleColourMessages.WriteLine("The game has started. The launcher will close in 5 seconds.");
                        await Task.Delay(5000);
                        Environment.Exit(0);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.Markup(
                        $"[red]Unable to connect to the CSR server!\n" +
                        $"Reason: {ex.Message}[/]\n" +
                        "Launcher is closing in 10 seconds."
                    );
                    await Task.Delay(10000);
                    Environment.Exit(1);
                }
            }
        }

        // was taken from CC launcher also, but a lot of changes.
        public static async Task GetLastVersionOfLauncher()
        {
            Console.Write("     1. Fetching the latest launcher version... ");
            var url = $"https://api.github.com/repos/zloybik/launcher_csr/releases/latest"; // нужно использовать GitHub, это легче, сегодня же нужно сделать СВОЙ updater.exe.

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"CSR-Launcher/{Versions.CurrentVersionOfLauncher}");
                try
                {
                    var content = await httpClient.GetStringAsync(url);
                    JObject responseJson = JObject.Parse(content);
                    if (responseJson["tag_name"].ToString() != CurrentVersionOfLauncher)
                    {
                        LastVersionOfLauncher = responseJson["tag_name"].ToString();
                        AnsiConsole.MarkupLine($"[red]{responseJson["tag_name"]}[/]");
                        Console.WriteLine("You are using an outdated launcher. Updating...");
                        try
                        {
                            var content_updater = await httpClient.GetStringAsync("https://api.github.com/repos/zloybik/launcher_updater_csr/releases/latest");

                            JObject responseJson_updater = JObject.Parse(content_updater);
                            string fileUrl = responseJson_updater["assets"][0]["browser_download_url"].ToString();
                            string filename = responseJson_updater["assets"][0]["name"].ToString();
                            await DownloadUpdaterAsync(fileUrl, filename);
                        }
                        catch (Exception ex_updater)
                        {
                            Console.WriteLine($"Error while downloading updater.exe: {ex_updater.Message}");
                            await Task.Delay(5000);
                        }
                    }

                    else
                    {
                        AnsiConsole.MarkupLine($"[green]{responseJson["tag_name"]}[/]");
                        await Versions.GetLastVersionOfGame();
                    }
                    //Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    AnsiConsole.Markup(
                        $"[red]Unable to connect to the GitHub server!\n" +
                        $"Reason: {ex.Message}[/]\n" +
                        "Launcher is closing in 10 seconds."
                    );
                    await Task.Delay(10000);
                    Environment.Exit(1);
                }
            }
        }

        // Метод для скачивания файла
        public static async Task DownloadUpdaterAsync(string fileUrl, string filename)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(fileUrl))
                {
                    response.EnsureSuccessStatusCode();
                    using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }
            }

            // После скачивания запускаем процесс
            Process updaterProcess = new Process();
            updaterProcess.StartInfo.FileName = "updater.exe";
            updaterProcess.StartInfo.Arguments = $"--version={LastVersionOfLauncher} {string.Join(" ", Arguments.GenerateGameArguments(true))}";
            updaterProcess.StartInfo.UseShellExecute = true;
            updaterProcess.Start();
        }
    }
}
