using System;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IStringFormat
    {
        string ShortTime(DateTimeOffset dateTime);
        string ShortDateTime(DateTimeOffset dateTime);
    }
}