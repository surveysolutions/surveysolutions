namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public enum ErrorCode
    {
        UserWasNotAuthoredOnDesigner = 1,
        DesignerIsInMaintenanceMode = 2,
        DesignerIsUnavailable = 3,
        RequestTimeout = 4,
        InternalServerError = 5,
        AccountIsLockedOnDesigner = 6,
        RequestedUrlWasNotFound = 7,
        RequestedUrlIsForbidden = 8,
        TesterUpgradeIsRequiredToOpenQuestionnaire = 9,
        QuestionnaireContainsErrorsAndCannotBeOpened = 10,
        UnknownError = 9999
    }
}