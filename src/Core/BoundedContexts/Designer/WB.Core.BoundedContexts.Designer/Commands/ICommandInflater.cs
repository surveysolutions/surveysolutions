using System;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Designer.Code
{
    public interface ICommandInflater
    {
        void PrepareDeserializedCommandForExecution(ICommand command);
    }
}
