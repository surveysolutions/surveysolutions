using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Sql
{
    internal interface IExportedDataAccessor
    {
        string GetAllDataFolder(string basePath);
        string GetApprovedDataFolder(string basePath);

        string InterviewActionsTableName { get; }
        string DataFileName { get; }
        string AllDataFolder { get; }
        string ApprovedDataFolder { get; }
        List<string> ActionFileColumns { get; }
        string InterviewIdColumnName { get; }
        string DataColumnName { get; }
    }
}
