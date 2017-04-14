using System;
using System.Threading.Tasks;
using NConsole;
using NLog;

namespace support
{
    public static class ConsoleExtensions
    {
        public static void WriteLine(this IConsoleHost host, string text = null, bool isHighlighted = false)
        {
            const string underline = "\x1B[4m";
            const string reset = "\x1B[0m";

            host.WriteMessage(isHighlighted ? $"{underline}{text}{reset}{Environment.NewLine}" : $"{text}{Environment.NewLine}");
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

        public static async Task TryExecuteActionWithAnimationAsync(this IConsoleHost host, ILogger logger, Task<bool> action)
        {
            SpinAnimation.Start();

            var isActionExecutedSuccessfully = false;

            try
            {
                isActionExecutedSuccessfully = await action;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unexpected exception");
            }

            SpinAnimation.Stop();

            if (isActionExecutedSuccessfully)
                host.WriteOk();
            else
                host.WriteFailed();
        }
    }
}