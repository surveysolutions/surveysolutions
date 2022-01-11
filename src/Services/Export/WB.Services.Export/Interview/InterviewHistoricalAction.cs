namespace WB.Services.Export.Interview
{
    public enum InterviewHistoricalAction
    {
        SupervisorAssigned = 0,
        InterviewerAssigned = 1,
        AnswerSet = 2,
        AnswerRemoved = 3,
        CommentSet = 4,
        Completed = 5,
        Restarted = 6,
        ApproveBySupervisor = 7,
        ApproveByHeadquarter = 8,
        RejectedBySupervisor = 9,
        RejectedByHeadquarter = 10,
        Deleted = 11,
        Restored = 12,
        QuestionEnabled = 13,
        QuestionDisabled = 14,
        GroupEnabled = 15,
        GroupDisabled = 16,
        QuestionDeclaredValid = 17,
        QuestionDeclaredInvalid = 18,
        UnapproveByHeadquarters = 19,
        ReceivedByInterviewer = 20,
        ReceivedBySupervisor = 21,
        VariableSet = 22,
        VariableEnabled = 23,
        VariableDisabled = 24,
        Paused = 25,
        Resumed = 26,
        KeyAssigned = 27,
        TranslationSwitched = 28,
        OpenedBySupervisor = 29,
        ClosedBySupervisor = 30,
        InterviewModeChanged = 31,
        InterviewCreated = 32
    }
}
