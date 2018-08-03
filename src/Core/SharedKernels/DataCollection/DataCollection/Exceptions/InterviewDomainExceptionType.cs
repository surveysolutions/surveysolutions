namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public enum InterviewDomainExceptionType
    {
        Undefined,
        InterviewLimitReached,
        QuestionnaireIsMissing,
        InterviewHardDeleted,
        OtherUserIsResponsible,
        StatusIsNotOneOfExpected,
        AnswerNotAccepted,
        InterviewRecievedByDevice,
        CantMoveToUndefinedTeam,
        ExpessionCalculationError,
        DuplicateSyncPackage,
        PackageIsOudated
    }
}
