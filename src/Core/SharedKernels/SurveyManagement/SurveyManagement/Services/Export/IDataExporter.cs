using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    internal interface IDataExporter
    {
        void CreateHeaderStructureForPreloadingForQuestionnaire(string basePath, string targetFolder);
        string[] GetDataFilesForQuestionnaire(string basePath);
        string[] GetDataFilesForQuestionnaireByInterviewsInApprovedState(string basePath);
    }
}
