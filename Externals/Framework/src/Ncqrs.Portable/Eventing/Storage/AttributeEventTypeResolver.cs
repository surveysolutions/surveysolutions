using System;

namespace Ncqrs.Eventing.Storage
{
    ///<summary>
    /// This specifies the name of an event.
    ///</summary>
    /// <remarks>
    /// This attribute is NOT inherited as each each event type MUST have a different name (this includes aliases).
    /// 
    /// This name is used when serializing and de-serializing an event.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EventNameAttribute : Attribute
    {
        public EventNameAttribute(string name)
        {
            Name = name == null ? "" : name.Trim();
        }

        public string Name { get; private set; }
    }
}
