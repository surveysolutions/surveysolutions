using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    internal interface IDataExportService
    {
        void CreateHeaderStructureForPreloadingForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string targetFolder);
        string[] GetDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath);
        string[] GetDataFilesForQuestionnaireByInterviewsInApprovedState(Guid questionnaireId, long questionnaireVersion, string basePath);
    }
}
