// -----------------------------------------------------------------------
// <copyright file="ConcurrencyResolveCommandService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands
{
    using Ncqrs.Commanding;
    using Ncqrs.Commanding.ServiceModel;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// Repeates command execution until success.
    /// </summary>
    public class ConcurrencyResolveCommandService : CommandService
    {
        readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void Execute(ICommand command)
        {
            try
            {
                base.Execute(command);
            }
            catch (ConcurrencyException exc)
            {
                logger.Info("Concurrency execution retry");
                this.Execute(command);
            }
        }
    }
}
