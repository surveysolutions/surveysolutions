using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.Composite
{
    public class CompositeException : Exception
    {
        public CompositeException()
        {
        }

        public CompositeException(string message) : base(message)
        {
        }

        public CompositeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CompositeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
