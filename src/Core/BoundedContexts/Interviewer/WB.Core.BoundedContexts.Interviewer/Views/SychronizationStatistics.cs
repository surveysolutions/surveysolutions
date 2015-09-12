using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SychronizationStatistics : MvxNotifyPropertyChanged
    {
        private int downloadedInterviewsCount;
        public int DownloadedInterviewsCount
        {
            get { return this.downloadedInterviewsCount; }
            set { this.downloadedInterviewsCount = value; this.RaisePropertyChanged(); }
        }

        private int downloadedQuestionnairesCount;
        public int DownloadedQuestionnairesCount
        {
            get { return this.downloadedQuestionnairesCount; }
            set { this.downloadedQuestionnairesCount = value; this.RaisePropertyChanged(); }
        }

        private int downloadedQuestionnaireAssemliesCount;
        public int DownloadedQuestionnaireAssemliesCount
        {
            get { return this.downloadedQuestionnaireAssemliesCount; }
            set { this.downloadedQuestionnaireAssemliesCount = value; this.RaisePropertyChanged(); }
        }

        private int uploadedInterviewsCount;
        public int UploadedInterviewsCount
        {
            get { return this.uploadedInterviewsCount; }
            set { this.uploadedInterviewsCount = value; this.RaisePropertyChanged(); }
        }

        private int uploadedIterviewImagesCount;
        public int UploadedIterviewImagesCount
        {
            get { return this.uploadedIterviewImagesCount; }
            set { this.uploadedIterviewImagesCount = value; this.RaisePropertyChanged(); }
        }

        private int totalCompletedInterviewsToUpload;
        public int TotalCompletedInterviewsToUpload
        {
            get { return this.totalCompletedInterviewsToUpload; }
            set { this.totalCompletedInterviewsToUpload = value; this.RaisePropertyChanged(); }
        }

        private int totalInterviewsToDownload;
        public int TotalInterviewsToDownload
        {
            get { return this.totalInterviewsToDownload; }
            set { this.totalInterviewsToDownload = value; this.RaisePropertyChanged(); }
        }

        private int totalRejectedInterviews;
        public int TotalRejectedInterviews
        {
            get { return this.totalRejectedInterviews; }
            set { this.totalRejectedInterviews = value; this.RaisePropertyChanged(); }
        }
    }
}