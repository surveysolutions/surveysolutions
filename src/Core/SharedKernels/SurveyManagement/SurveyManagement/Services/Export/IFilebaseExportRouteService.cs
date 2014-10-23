using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    internal interface IFilebaseExportRouteService
    {
        string GetFolderPathOfFilesByQuestionnaireForInterview(Guid questionnaireId, long version, Guid interviewId);
        string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version);
        string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version);
        string PathToExportedData { get; }
        string PathToExportedFiles { get; }
        string PreviousCopiesOfFilesFolderPath { get; }
        string PreviousCopiesFolderPath { get; }
        string ExtensionOfExportedDataFile { get; }
        string SeparatorOfExportedDataFile { get; }
    }
}
