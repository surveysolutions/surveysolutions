using System.Collections.Generic;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IErrorDetailsProvider
    {
        void FillExceptionData(Dictionary<string, string> data);
    }
}