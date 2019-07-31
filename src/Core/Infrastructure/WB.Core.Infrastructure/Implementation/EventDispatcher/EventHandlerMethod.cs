using System;
using System.Reflection;

namespace WB.Core.Infrastructure.Implementation.EventDispatcher
{
    public class EventHandlerMethod
    {
        public Type EventType { get; set; }

        public MethodInfo Handle { get; set; }

        public bool ReceivesIgnoredEvents { get; set; }
    }
}