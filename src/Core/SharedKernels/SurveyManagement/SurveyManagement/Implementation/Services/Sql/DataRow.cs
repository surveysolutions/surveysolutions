using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql
{
    internal class DataRow
    {
        public byte[] Data { get; set; }
        public string InterviewId { get; set; }
    }
}
