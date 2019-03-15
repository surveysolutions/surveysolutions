namespace WB.Core.BoundedContexts.Headquarters.Views.SystemLog
{
    public enum LogEntryType
    {
        Unknown = 0,
        QuestionnaireImported = 1,
        QuestionnaireDeleted = 2,
        ExportStared = 3,
        AssignmentsUpgradeStarted = 4,
        UserCreated = 5,
        AssignmentSizeChanged = 6,
        ExportEncryptionChanged = 7,
        UserMovedToAnotherTeam = 8
    }
}
