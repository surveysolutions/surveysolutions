using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public interface IInterviewTreeUpdater
    {
        void UpdateEnablement(IInterviewTreeNode entity);
        void UpdateEnablement(InterviewTreeGroup entity);
        void UpdateEnablement(InterviewTreeRoster entity);

        void UpdateSingleOptionQuestion(InterviewTreeQuestion question);
        void UpdateMultiOptionQuestion(InterviewTreeQuestion question);
        void UpdateYesNoQuestion(InterviewTreeQuestion question);
        void UpdateCascadingQuestion(InterviewTreeQuestion question);
        void UpdateLinkedQuestion(InterviewTreeQuestion question);
        void UpdateLinkedToListQuestion(InterviewTreeQuestion question);

        void UpdateRoster(InterviewTreeRoster roster);
        void UpdateVariable(InterviewTreeVariable variable);
        void UpdateValidations(InterviewTreeStaticText staticText);
        void UpdateValidations(InterviewTreeQuestion question);
    }
}