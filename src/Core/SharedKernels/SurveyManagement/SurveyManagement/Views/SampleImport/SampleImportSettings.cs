namespace WB.Core.SharedKernels.SurveyManagement.Views.SampleImport
{
    public class SampleImportSettings
    {
        public SampleImportSettings()
        {
            this.InterviewsImportParallelTasksLimit = 1;
        }

        public SampleImportSettings(int interviewsImportParallelTasksLimit)
        {
            this.InterviewsImportParallelTasksLimit = interviewsImportParallelTasksLimit;
        }

        public int InterviewsImportParallelTasksLimit { get; private set; }
    }
}