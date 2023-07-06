using System;

namespace WB.Infrastructure.Native.Utils;

public class NonZeroExitCodeException : Exception
{
    public int ProcessExitCode { get; }
    public string ErrorOutput { get; }

    public NonZeroExitCodeException(in int processExitCode, string errorOutput = null)
    {
        ProcessExitCode = processExitCode;
        ErrorOutput = errorOutput;
    }
}
