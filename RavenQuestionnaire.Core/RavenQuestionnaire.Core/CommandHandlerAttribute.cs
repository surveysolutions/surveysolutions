using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core
{
    public class CommandHandlerAttribute:Attribute
    {
        public bool IgnoreAsEvent { get; set; }
    }
}
