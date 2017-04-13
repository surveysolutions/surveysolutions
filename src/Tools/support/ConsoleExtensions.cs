using System;
using NConsole;

namespace support
{
    public static class ConsoleExtensions
    {
        public static void WriteLine(this IConsoleHost host, string text = null)
        {
            host.WriteMessage($"{text}{Environment.NewLine}");
        }

        public static void WriteHighlitedText(this IConsoleHost host, string textToHighlight, ConsoleColor colorOfHighlightedText)
        {
            Console.ForegroundColor = colorOfHighlightedText;
            host.WriteMessage(textToHighlight);
            Console.ResetColor();
        }

        public static void WriteDone(this IConsoleHost host)
        {
            host.WriteHighlitedText("DONE", ConsoleColor.Green);
            host.WriteLine();
        }


        public static void WriteOk(this IConsoleHost host)
        {
            host.WriteHighlitedText("OK", ConsoleColor.Green);
            host.WriteLine();
        }

        public static void WriteFailed(this IConsoleHost host)
        {
            host.WriteHighlitedText("FAILED", ConsoleColor.DarkRed);
            host.WriteLine();
        }
    }
}