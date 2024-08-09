using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

public class InterviewPermissionsInvariants
{
    static class ExceptionKeys
    {
        public static readonly string UserId = "UserId";
        public static readonly string InterviewId = "InterviewId";
        public static readonly string QuestionId = "Question ID";
    }

    private Identity QuestionIdentity { get; }
    private InterviewTree InterviewTree { get; }
    private InterviewEntities.InterviewProperties InterviewProperties { get; }
    

    public InterviewPermissionsInvariants(InterviewEntities.InterviewProperties interviewProperties,
        Identity questionIdentity,
        InterviewTree interviewTree)
    {
        this.InterviewProperties = interviewProperties;
        this.QuestionIdentity = questionIdentity;
        this.InterviewTree = interviewTree;
    }

    public void RequireCanAnswer()
    {
        RequireAllowAnswerByRole();
    }
            
    public void RequireAllowAnswerByRole()
    {
        if (this.InterviewProperties.Status >= InterviewStatus.Completed)
        {
            var question = this.InterviewTree.GetQuestion(QuestionIdentity);
            if (question.IsInterviewer)
            {
                throw new InterviewException(
                    $"Interviewer question cannot be edited on completed interview")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewProperties.Id},
                        {ExceptionKeys.QuestionId, this.QuestionIdentity},
                    }
                };
            }
        }
    }
}
