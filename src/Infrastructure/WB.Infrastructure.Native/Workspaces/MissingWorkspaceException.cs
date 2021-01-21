using System;
using System.Runtime.Serialization;

namespace WB.Infrastructure.Native.Workspaces
{
    [Serializable]
    public class MissingWorkspaceException : Exception
    {
        public MissingWorkspaceException()
        {
        }

        public MissingWorkspaceException(string message) : base(message)
        {
        }

        public MissingWorkspaceException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MissingWorkspaceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
