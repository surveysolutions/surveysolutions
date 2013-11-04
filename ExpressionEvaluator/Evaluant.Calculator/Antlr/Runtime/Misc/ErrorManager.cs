namespace Antlr.Runtime.Misc
{
    using System;
    using System.Diagnostics;
    using System.Text;

    public class ErrorManager
    {
        public static void Error(object arg)
        {
            new StringBuilder().AppendFormat("internal error: {0} ", arg);
        }

        private static StackFrame GetLastNonErrorManagerCodeLocation(Exception e)
        {
            StackTrace trace = new StackTrace(e);
            int index = 0;
            while (index < trace.FrameCount)
            {
                if (trace.GetFrame(index).ToString().IndexOf("ErrorManager") < 0)
                {
                    break;
                }
                index++;
            }
            return trace.GetFrame(index);
        }

        public static void InternalError(object error)
        {
            Error(GetLastNonErrorManagerCodeLocation(new Exception()) + ": " + error);
        }

        public static void InternalError(object error, Exception e)
        {
            StackFrame lastNonErrorManagerCodeLocation = GetLastNonErrorManagerCodeLocation(e);
            Error(string.Concat(new object[] { "Exception ", e, "@", lastNonErrorManagerCodeLocation, ": ", error }));
        }
    }
}

