using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WB.Core.BoundedContexts.Headquarters
{
    public static class LoggerExtensions
    {
        public static async Task LogExecuteTimeAsync(this ILogger logger, Func<Task> action, string operation)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            logger.LogInformation("Start {operation}", operation);

            try
            {
                await action.Invoke();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error {operation}. Elapsed time: {elapsed}", operation, stopwatch.Elapsed);
                throw;
            }
            finally
            {
                logger.LogInformation("Finished {operation}. Elapsed time: {elapsed}", operation, stopwatch.Elapsed);
            }
        }

    }
}