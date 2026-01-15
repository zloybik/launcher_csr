using System;
using System.Collections.Generic;

namespace CSRAutoUpdater_yea.Utils
{
    public static class Arguments
    {
        // Kinda alsow was stolen from cc, again, there's no need to remake the car xd
        private static List<string> _launcherArguments = new List<string>
        {
            "--disable-rpc",
            "--check-only-by-hash",
            "--fast-pick-region=europe"
        };

        public static string GetFastRegion()
        {
            IEnumerable<string> arguments = Environment.GetCommandLineArgs();

            foreach (string arg in arguments)
                if (arg.ToLowerInvariant().StartsWith("--fast-pick-region="))
                    return arg.Replace("--fast-pick-region=", "");

            return null;
        }

        private static List<string> _additionalArguments = new List<string>();
        public static bool Exists(string argument)
        {
            // я не ебу нахуя, но допустим.
            IEnumerable<string> arguments = Environment.GetCommandLineArgs();

            foreach (string arg in arguments)
                if (arg.ToLowerInvariant() == argument) return true;

            return false;
        }


        public static List<string> GenerateGameArguments(bool passLauncherArguments = false)
        {
            IEnumerable<string> launcherArguments = Environment.GetCommandLineArgs();
            List<string> gameArguments = new List<string>();

            foreach (string arg in launcherArguments)
                if ((passLauncherArguments || !_launcherArguments.Contains(arg.ToLowerInvariant()))
                    && !arg.EndsWith(".exe"))
                    gameArguments.Add(arg.ToLowerInvariant());

            gameArguments.AddRange(_additionalArguments);
            return gameArguments;
        }
    }
}
