using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core
{
    public interface ICommandInvoker
    {
        void Execute<T>(T command) where T : ICommand;
        void Execute(ICommand command, Guid eventPublicKey, Guid clientPublicKey);
    }
}
