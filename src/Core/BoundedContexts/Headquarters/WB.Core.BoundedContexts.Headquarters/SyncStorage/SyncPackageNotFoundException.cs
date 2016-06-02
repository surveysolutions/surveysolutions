using System;
using System.Runtime.Serialization;

namespace WB.Core.Synchronization.SyncStorage
{
    [Serializable]
    public class SyncPackageNotFoundException : Exception
    {
        public SyncPackageNotFoundException() {}

        public SyncPackageNotFoundException(string message)
            : base(message) {}

        public SyncPackageNotFoundException(string message, Exception inner)
            : base(message, inner) {}

        protected SyncPackageNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}