using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands
{
    public interface ICommand
    {
        UserLight Executor { set;  get; }
    }
}
