using System.Collections.Generic;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IErrorDetailsProvider
    {
        void FillExceptionData(Dictionary<string, string> data);
    }
}