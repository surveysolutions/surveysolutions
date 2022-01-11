using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Interviewer.Migrations.Workspaces
{
    [Migration(202103101500)]
    public class M202103101500_UpdateInterviewsMode : IMigration
    {
        private readonly IPlainStorage<InterviewView> interviewStorage;
        public M202103101500_UpdateInterviewsMode(IPlainStorage<InterviewView> interviewStorage)
        {
            this.interviewStorage = interviewStorage;
        }

        public void Up()
        {
            var interviews = interviewStorage.LoadAll();

            foreach (var interview in interviews)
            {
                interview.Mode = InterviewMode.CAPI;
                interviewStorage.Store(interview);
            }
        }
    }
}
