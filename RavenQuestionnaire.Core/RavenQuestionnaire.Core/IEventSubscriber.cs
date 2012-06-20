using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;

namespace RavenQuestionnaire.Core
{
    public interface IEventSubscriber<in T> where T : ICommand
    {
        void Invoke(T command);
    }
}
