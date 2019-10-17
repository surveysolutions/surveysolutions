namespace WB.Services.Export.Interview
{
    public enum InterviewHistoricalAction
    {
        SupervisorAssigned = 0,
        InterviewerAssigned = 1,
        AnswerSet = 3,
        AnswerRemoved = 4,
        CommentSet = 5,
        Completed = 6,
        Restarted = 7,
        ApproveBySupervisor = 8,
        ApproveByHeadquarter = 9,
        RejectedBySupervisor = 10,
        RejectedByHeadquarter = 11,
        Deleted = 12,
        Restored = 13,
        QuestionEnabled = 14,
        QuestionDisabled = 15,
        GroupEnabled = 16,
        GroupDisabled = 17,
        QuestionDeclaredValid = 18,
        QuestionDeclaredInvalid = 19,
        UnapproveByHeadquarters = 20,
        ReceivedByInterviewer = 21,
        ReceivedBySupervisor = 22,
        VariableSet = 23,
        VariableEnabled = 24,
        VariableDisabled = 25,
        Paused = 26,
        Resumed = 27,
        KeyAssigned = 28,
        TranslationSwitched = 29,
        OpenedBySupervisor = 30,
        ClosedBySupervisor = 31
    }
}
