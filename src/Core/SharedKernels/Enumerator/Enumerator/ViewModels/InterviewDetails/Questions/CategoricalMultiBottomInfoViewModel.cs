using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiBottomInfoViewModel: MvxNotifyPropertyChanged, ICompositeEntity
    {
        private string maxAnswersCountMessage;
        public string MaxAnswersCountMessage
        {
            get => maxAnswersCountMessage;
            set => SetProperty(ref maxAnswersCountMessage, value);
        }
    }
}