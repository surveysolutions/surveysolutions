using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Interviewer.Migrations.Workspace
{
    [Migration(201904081205)]
    public class UpdateAssignmentsWithInterviewsCount : IMigration
    {
        private readonly IPlainStorage<InterviewView> interviewStorage;
        private readonly IAssignmentDocumentsStorage assignmentStorage;

        public UpdateAssignmentsWithInterviewsCount(
            IPlainStorage<InterviewView> interviewStorage,
            IAssignmentDocumentsStorage assignmentStorage)
        {
            this.interviewStorage = interviewStorage;
            this.assignmentStorage = assignmentStorage;
        }
        public void Up()
        {
            var hasEmptyInterviewsCounts = assignmentStorage.Count(x => x.CreatedInterviewsCount == null) > 0;

            if (!hasEmptyInterviewsCounts) return;

            var assignments = assignmentStorage.LoadAll();

            foreach (var assignment in assignments)
            {
                assignment.CreatedInterviewsCount = interviewStorage.Count(x => x.CanBeDeleted && x.Assignment == assignment.Id);
                assignmentStorage.Store(assignment);
            }
        }
    }
}
