using System;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    /// <summary>
    /// Thrown when the only location fixes received were rejected because they violate the
    /// workspace-configured <see cref="WB.Core.SharedKernels.DataCollection.ValueObjects.AcceptableGpsLocationSource"/>
    /// (for example a mock location while mock is not permitted, or a non-GPS provider while only
    /// the built-in GPS is allowed) and no acceptable fix was obtained before the timeout.
    /// It lets the UI tell the user the location was refused as a restricted source rather than
    /// showing a generic timeout.
    /// </summary>
    public class RestrictedLocationSourceException : Exception
    {
        public RestrictedLocationSourceException() : base("Received location fixes were rejected as a restricted source.")
        {
        }

        public RestrictedLocationSourceException(string message) : base(message)
        {
        }

        public RestrictedLocationSourceException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
