using Ncqrs.Commanding;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Designer.Code
{
    public interface ICommandPostprocessor
    {
        void ProcessCommandAfterExecution(ICommand command);
    }
}