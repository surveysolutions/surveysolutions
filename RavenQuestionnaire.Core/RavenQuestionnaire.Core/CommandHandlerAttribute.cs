using System;

namespace RavenQuestionnaire.Core
{
    public class CommandHandlerAttribute:Attribute
    {
        public bool IgnoreAsEvent { get; set; }
    }
}
