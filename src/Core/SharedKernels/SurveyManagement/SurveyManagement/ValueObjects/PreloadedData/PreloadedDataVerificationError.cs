using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData
{
    public class PreloadedDataVerificationError
    {
        public PreloadedDataVerificationError(string code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        public string Code { get; private set; }
        public string Message { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", this.Code, this.Message);
        }
    }
}
