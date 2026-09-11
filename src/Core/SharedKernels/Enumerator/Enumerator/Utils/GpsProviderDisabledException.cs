using System;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class GpsProviderDisabledException : Exception
    {
        public GpsProviderDisabledException() : base("GPS provider is disabled.")
        {
        }

        public GpsProviderDisabledException(string message) : base(message)
        {
        }

        public GpsProviderDisabledException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

