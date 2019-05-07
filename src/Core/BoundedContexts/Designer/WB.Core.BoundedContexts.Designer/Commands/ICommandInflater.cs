using WB.Core.Infrastructure.CommandBus;

namespace WB.Core.BoundedContexts.Designer.Commands
{
    public interface ICommandInflater
    {
        void PrepareDeserializedCommandForExecution(ICommand command);
    }
}
