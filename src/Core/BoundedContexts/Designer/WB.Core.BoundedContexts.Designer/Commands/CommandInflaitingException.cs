using System;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandInflaitingException : Exception
    {
        public readonly CommandInflatingExceptionType ExceptionType;

        public CommandInflaitingException(string message) : this(CommandInflatingExceptionType.Common, message) { }

        public CommandInflaitingException(CommandInflatingExceptionType exceptionType, string message) : base(message)
        {
            this.ExceptionType = exceptionType;
        }
    }
}
