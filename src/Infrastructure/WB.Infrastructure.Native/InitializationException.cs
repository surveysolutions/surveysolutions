using System;
using System.Runtime.Serialization;

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

        protected InitializationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

    }
}