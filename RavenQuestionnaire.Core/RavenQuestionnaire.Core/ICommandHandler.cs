using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core
{
    public interface ICommandHandler<T>
    {
        void Handle(T command);
    }
}
