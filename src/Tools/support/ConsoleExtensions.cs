using System;
using System.Threading.Tasks;
using NConsole;
using NLog;

namespace support
{
    public static class ConsoleExtensions
    {
        public static void WriteLine(this IConsoleHost host, string text = null)
            => host.WriteMessage($"{text}{Environment.NewLine}");

        public static void WriteHighlightedText(this IConsoleHost host, string textToHighlight, ConsoleColor colorOfHighlightedText)
        {
            Console.ForegroundColor = colorOfHighlightedText;
            host.WriteMessage(textToHighlight);
            Console.ResetColor();
        }

        public static void WriteDone(this IConsoleHost host)
        {
            host.WriteHighlightedText("DONE", ConsoleColor.Green);
            host.WriteLine();
        }


        public static void WriteOk(this IConsoleHost host)
        {
            host.WriteHighlightedText("OK", ConsoleColor.Green);
            host.WriteLine();
        }

        public static void WriteFailed(this IConsoleHost host)
        {
            host.WriteHighlightedText("FAILED", ConsoleColor.DarkRed);
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
