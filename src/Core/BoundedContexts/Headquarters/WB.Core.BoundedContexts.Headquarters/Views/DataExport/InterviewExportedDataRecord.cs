using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public class InterviewExportedDataRecord : IView
    {
        public virtual string InterviewId { get; set; }
        public virtual Dictionary<string, string[]> Data { get; set; } // file name, rows
    }
}