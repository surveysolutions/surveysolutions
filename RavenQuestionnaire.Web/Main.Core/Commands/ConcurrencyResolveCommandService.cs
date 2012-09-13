// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConcurrencyResolveCommandService.cs" company="">
//   
// </copyright>
// <summary>
//   Repeates command execution until success.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Commands
{
    using NLog;

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
        /// The logger.
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

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
            try
            {
                base.Execute(command);
            }
            catch (ConcurrencyException exc)
            {
                this.logger.Info("Concurrency execution retry");
                this.Execute(command);
            }
        }

        #endregion
    }
}