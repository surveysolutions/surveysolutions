// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConcurrencyResolveCommandService.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Repeates command execution until success.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
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
        #region Fields

        /// <summary>
        /// The repeat try count.
        /// </summary>
        private const int RepeatTryCount = 50;
        
        /// <summary>
        /// The logger.
        /// </summary>
#warning 'if MONODROID' is bad. should use abstract logger (ILogger?) which implementation will be different in different apps
#if MONODROID
		private readonly AndroidLogger.ILog logger = AndroidLogger.LogManager.GetLogger(typeof(ConcurrencyResolveCommandService));
#else
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
#endif
        
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

            while (inProgress && (currentTry < ConcurrencyResolveCommandService.RepeatTryCount))
            {
                try
                {
                    base.Execute(command);
                    inProgress = false;
                }
                catch (ConcurrencyException exc)
                {
                    this.logger.Info(string.Format("Concurrency execution retry ({0})! ({1})" , currentTry, exc.Message));
                    currentTry++;
                }
            }
        }

        #endregion
    }
}