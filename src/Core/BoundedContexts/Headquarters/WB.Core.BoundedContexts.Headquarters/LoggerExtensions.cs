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
            logger.LogInformation($"Start {operation}");

            try
            {
                await action.Invoke();
            }
            catch (Exception e)
            {
                logger.LogError($"Error {operation}. Elapsed time: {stopwatch.Elapsed}", e);
                throw;
            }
            finally
            {
                logger.LogInformation($"Finished {operation}. Elapsed time: {stopwatch.Elapsed}");
            }
        }

    }
}