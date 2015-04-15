namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public enum InterviewHistoricalAction
    {
        SupervisorAssigned,
        InterviewerAssigned,
        AnswerSet,
        CommentSet,
        Completed,
        Restarted,
        ApproveBySupervisor,
        ApproveByHeadquarter,
        RejectedBySupervisor,
        RejectedByHeadquarter,
        Deleted,
        Restored,
        QuestionEnabled,
        QuestionDisabled,
        GroupEnabled,
        GroupDisabled,
        QuestionDeclaredValid,
        QuestionDeclaredInvalid,
    }
}