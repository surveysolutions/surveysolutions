using System;
using System.Runtime.Serialization;

namespace Synchronization.Core.Errors
{
    public class RegistrationException : SynchronizationException
    {
        public RegistrationException(Exception ex)
            : base("Registration failed", ex)
        {
        }

        public RegistrationException()
            : this(null)
        {
        }
    }
}
