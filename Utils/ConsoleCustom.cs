using System.Runtime.InteropServices;

namespace CSRAutoUpdater_yea.Utils
{
    // was taken also from CC
    public static class ConsoleCustom
    {
        [DllImport("kernel32.dll")]
        private static extern System.IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;

        private static System.IntPtr ConsoleHandle = GetConsoleWindow();

        public static void HideConsole()
        {
            ShowWindow(ConsoleHandle, SW_HIDE);
        }
    }
}
