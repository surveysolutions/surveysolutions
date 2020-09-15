using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Headquarters
{
    public static class LoggerExtensions
    {
        public static async Task LogExecuteTimeAsync(this ILogger logger, Func<Task> action, string operation)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            logger.Info($"Start {operation}");

            try
            {
                await action.Invoke();
            }
            catch (Exception e)
            {
                logger.Error($"Error {operation}. Elapsed time: {stopwatch.Elapsed}", e);
                throw;
            }
            finally
            {
                logger.Info($"Finished {operation}. Elapsed time: {stopwatch.Elapsed}");
            }
        }

    }
}