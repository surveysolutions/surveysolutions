using System.ComponentModel.Design;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiBottomInfoViewModel: MvxNotifyPropertyChanged, ICompositeEntity
    {
        private string maxAnswersCountMessage;
        private Identity identity;

        public string MaxAnswersCountMessage
        {
            get => maxAnswersCountMessage;
            set => SetProperty(ref maxAnswersCountMessage, value);
        }

        public Identity Identity
        {
            get => identity;
            set => SetProperty(ref identity, value);
        }
    }
}
