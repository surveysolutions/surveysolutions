namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public enum InterviewDomainExceptionType
    {
        Undefined = 0,
        InterviewLimitReached = 1,
        QuestionnaireIsMissing,
        InterviewHardDeleted,
        OtherUserIsResponsible,
        StatusIsNotOneOfExpected,
        AnswerNotAccepted,
        InterviewRecievedByDevice,
        CantMoveToUndefinedTeam,
        ExpessionCalculationError,
        DuplicateSyncPackage,
        PackageIsOudated,
        QuestionIsMissing,
        InterviewSizeLimitReached,
        DuplicateCreationCommand,
        QuestionnaireOutdated,
        InterviewHasIncompatibleMode,
        QuestionnaireDeleted,
        AssignmentLimitReached,
    }
}
