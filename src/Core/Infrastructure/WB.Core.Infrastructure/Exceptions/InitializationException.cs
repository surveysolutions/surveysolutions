using System;
using System.Runtime.Serialization;
using WB.Core.Infrastructure.Exceptions;

namespace WB.Infrastructure.Native
{
    [Serializable]
    public class InitializationException : Exception
    {
        public Subsystem Subsystem { get; }

        public InitializationException(Subsystem subsystem)
        {
            this.Subsystem = subsystem;
        }

        public InitializationException(Subsystem subsystem, string message)
            : base(message)
        {
            this.Subsystem = subsystem;
        }

        public InitializationException(Subsystem subsystem, string message, Exception inner)
            : base(message, inner)
        {
            this.Subsystem = subsystem;
        }
        

        /// <summary>
        /// Specifies whether the exception is considered transient, that is, whether retrying to operation could
        /// succeed (e.g. a network error or a timeout).
        /// </summary>
        public bool IsTransient { get; set; }
    }
}
