using System;
using WB.Core.Infrastructure.CommandBus.Implementation;

namespace WB.Core.Infrastructure.CommandBus
{
    public interface ICommandsMonitoring
    {
        void Report(string commandName, TimeSpan duration);
    }
}
