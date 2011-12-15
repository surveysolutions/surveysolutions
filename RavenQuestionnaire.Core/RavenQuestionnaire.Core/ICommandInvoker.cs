using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;

namespace RavenQuestionnaire.Core
{
    public interface ICommandInvoker
    {
        void Execute<T>(T command) where T : ICommand;
    }
}
