using Ncqrs.Commanding;

namespace WB.UI.Designer.Code
{
    public interface ICommandPreprocessor
    {
        void PrepareDeserializedCommandForExecution(ICommand command);
    }
}