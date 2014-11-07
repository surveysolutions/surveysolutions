using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.CommandBus;
using CommandService = Ncqrs.Commanding.ServiceModel.CommandService;

namespace Ncqrs
{
    /// <summary>
    /// Repeates command execution until success.
    /// </summary>
    public class ConcurrencyResolveCommandService : CommandService
    {
        private readonly ILogger logger;

        public ConcurrencyResolveCommandService(ILogger logger)
        {
            this.logger = logger;
        }

        private const int RepeatTryCount = 50;

        public override void Execute(ICommand command, string origin)
        {
            bool inProgress = true;
            int currentTry = 1;

            while (inProgress && (currentTry<ConcurrencyResolveCommandService.RepeatTryCount))
            {
                try
                {
                    base.Execute(command, origin);
                    inProgress = false;
                }
                catch (ConcurrencyException exc)
                {
                    logger.Info(string.Format("Concurrency execution retry ({0})! ({1})", currentTry, exc.Message));
                    currentTry++;
                }
            }
        }
    }
}