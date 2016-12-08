using System;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class MissingPermissionsException : Exception
    {
        public MissingPermissionsException()
        {
        }

        public MissingPermissionsException(string message) : base(message)
        {
        }

        public MissingPermissionsException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}