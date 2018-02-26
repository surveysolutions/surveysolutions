using System;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ICsvWriterService:IDisposable
    {
        void WriteField<T>(T cellValue);
        void NextRecord();

        string RemoveNewLine(string cell);
    }
}
