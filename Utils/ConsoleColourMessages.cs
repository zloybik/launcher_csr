using Spectre.Console;

namespace CSRAutoUpdater_yea.Utils
{
    public static class ConsoleColourMessages
    {
        // This thing was taken from CC launcher. I dont see any reason to re-create it
        public static string _prefix = "[white]CS:[/][red]R[/]";
        public static string _grey = "grey82";
        public static string _seperator = "[grey50]|[/]";
        public static void Init()
        {
            AnsiConsole.MarkupLine($"{_prefix} {_seperator} [{_grey}]Coded by [/][lightcoral]konakona[/][{_grey}][/]");
            AnsiConsole.MarkupLine($"{_prefix} {_seperator} [{_grey}]Some sources were taken from [/][lightcoral]heapy[/]'s launcher.[{_grey}][/]");
            AnsiConsole.MarkupLine($"{_prefix} {_seperator} [{_grey}]https://csrestored.com [/]");
            AnsiConsole.MarkupLine($"{_prefix} {_seperator} [{_grey}]Version: {Versions.CurrentVersionOfLauncher}[/]");
        }

        public static void Print(object message)
            => AnsiConsole.MarkupLine($"{_prefix} {_seperator} [{_grey}]{Markup.Escape(message?.ToString() ?? string.Empty)}[/]");
        public static void WriteLine(object message)
            => AnsiConsole.MarkupLine($"{_prefix} {_seperator} {message}");

        public static void Success(object message)
            => AnsiConsole.MarkupLine($"{_prefix} {_seperator} [green1]{Markup.Escape(message?.ToString() ?? string.Empty)}[/]");

        public static void Warning(object message)
            => AnsiConsole.MarkupLine($"{_prefix} {_seperator} [yellow]{Markup.Escape(message?.ToString() ?? string.Empty)}[/]");

        public static void Error(object message)
            => AnsiConsole.MarkupLine($"{_prefix} {_seperator} [red]{Markup.Escape(message?.ToString() ?? string.Empty)}[/]");
    }
}