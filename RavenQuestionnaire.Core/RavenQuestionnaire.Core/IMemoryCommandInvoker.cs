using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;

namespace RavenQuestionnaire.Core
{
    public interface IMemoryCommandInvoker
    {
        void Execute<T>(T command) where T : ICommand;
        void Execute(ICommand command, Guid eventPublicKey, Guid clientPublicKey);
        void Flush();
    }
}
