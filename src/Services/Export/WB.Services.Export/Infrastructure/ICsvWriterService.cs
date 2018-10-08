using System;

namespace WB.Services.Export.Infrastructure
{
    public interface ICsvWriterService : IDisposable
    {
        void WriteField<T>(T cellValue);
        void NextRecord();
    }
}
