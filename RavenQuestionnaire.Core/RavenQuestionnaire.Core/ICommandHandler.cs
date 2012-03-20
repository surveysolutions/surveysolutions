using RavenQuestionnaire.Core.Commands;

namespace RavenQuestionnaire.Core
{
    /// <summary>
    /// Describes interface to use for Command Handlers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICommandHandler<in T>  where T :ICommand
    {
        void Handle(T command);
    }
}
