using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Sql
{
    internal interface ISqlDataAccessor
    {
        IEnumerable<string> GetListofTables(ISqlService sqlService);
        IEnumerable<string> GetListOfColumns(ISqlService sqlService, string tableName);
        string GetAllDataFolder(string basePath);
        string GetApprovedDataFolder(string basePath);

        string ParentId { get; }
        string InterviewActions { get; }
        string DataFile { get; }
        string AllDataFolder { get; }
        string ApprovedDataFolder { get; }
    }
}
