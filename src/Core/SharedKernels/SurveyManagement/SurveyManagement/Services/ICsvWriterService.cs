using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ICsvWriterService:IDisposable
    {
        void WriteField<T>(T cellValue);
        void NextRecord();
    }
}
