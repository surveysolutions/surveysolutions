using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    internal interface IDataExporter
    {
        void CreateHeaderStructureForPreloadingForQuestionnaire(Guid questionnaireId, long version, string targetFolder);
        string[] ExportAllDataForQuestionnaire(Guid questionnaireId, long version, Func<string, string> fileNameCreationFunc);
        string[] ExportApprovedDataForQuestionnaire(Guid questionnaireId, long version, Func<string, string> fileNameCreationFunc);
    }
}
