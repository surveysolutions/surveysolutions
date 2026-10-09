using System;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    /// <summary>
    /// Thrown when there is no location provider that satisfies the workspace-configured
    /// <see cref="WB.Core.SharedKernels.DataCollection.ValueObjects.AcceptableGpsLocationSource"/>.
    /// Unlike <see cref="GpsProviderDisabledException"/>, this does not imply the physical GPS
    /// sensor is off - it is used when the acceptance mode does not demand the built-in GPS chip
    /// but no acceptable provider is available.
    /// </summary>
    public class NoSuitableLocationProviderException : Exception
    {
        public NoSuitableLocationProviderException() : base("No suitable location provider is available.")
        {
        }

        public NoSuitableLocationProviderException(string message) : base(message)
        {
        }

        public NoSuitableLocationProviderException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
