using Ncqrs.Commanding;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Designer.Code
{
    public interface ICommandPreprocessor
    {
        void PrepareDeserializedCommandForExecution(ICommand command);
    }
}