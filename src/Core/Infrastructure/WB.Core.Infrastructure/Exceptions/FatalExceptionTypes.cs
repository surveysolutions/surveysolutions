using System;

namespace WB.Infrastructure.Native
{
    public enum FatalExceptionType
    {
        None,
        HqErrorDuringRunningMigrations,
        HqErrorDuringSiteInitialization,
        HqExportServiceUnavailable
    }

    public static class ExceptionHelper
    {
        public const string FatalExceptionDataField = "FatalException";
        public static Exception WithFatalType(this Exception e, FatalExceptionType type)
        {
            e.Data.Add(FatalExceptionDataField, type);
            return e;
        }

        public static FatalExceptionType? GetFatalType(this Exception e)
        {
            if (e.Data.Contains(FatalExceptionDataField))
            {
                return e.Data[FatalExceptionDataField] as FatalExceptionType?;
            }

            return null;
        }
    }
}
