using System;

namespace WB.UI.WebTester.Services.Implementation
{
    public interface IImportQuestionnaireAndCreateInterviewService
    {
        Guid StartImportQuestionnaireAndCreateInterview(Guid designerToken,
            Guid? originalInterviewId, int? scenarioId);

        CreationResult? GetStatus(Guid key);
        CreationResult? RemoveStatus(Guid key);
    }
}