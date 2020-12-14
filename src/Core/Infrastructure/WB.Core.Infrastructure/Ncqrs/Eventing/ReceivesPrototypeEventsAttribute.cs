using System;

namespace WB.Core.Infrastructure.Ncqrs.Eventing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ReceivesPrototypeEventsAttribute : Attribute
    {

    }
}
