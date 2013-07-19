using WB.Core.GenericSubdomains.Logging;

namespace Main.Core.Commands
{
    using Ncqrs.Commanding;
    using Ncqrs.Commanding.ServiceModel;
    using Ncqrs.Eventing.Storage;

    

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
        #region Fields

        /// <summary>
        /// The repeat try count.
        /// </summary>
        private const int RepeatTryCount = 50;
        
        /// <summary>
        /// The logger.
        /// </summary>
        
        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        public override void Execute(ICommand command)
        {
            bool inProgress = true;
            int currentTry = 1;

            while (inProgress && (currentTry<ConcurrencyResolveCommandService.RepeatTryCount))
            {
                try
                {
                    base.Execute(command);
                    inProgress = false;
                }
                catch (ConcurrencyException exc)
                {
                    logger.Info(string.Format("Concurrency execution retry ({0})! ({1})", currentTry, exc.Message));
                    currentTry++;
                }
            }
        }

        #endregion
    }
}