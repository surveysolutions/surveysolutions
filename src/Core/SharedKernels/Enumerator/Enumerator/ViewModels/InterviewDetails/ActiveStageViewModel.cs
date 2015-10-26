using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class ActiveStageViewModel : 
        MvxNotifyPropertyChanged,
        IDisposable
    {
        protected readonly IInterviewViewModelFactory interviewViewModelFactory;
        private NavigationState navigationState;
        string interviewId;
        public ActiveStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            EnumerationStageViewModel enumerationStage)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.EnumerationStage = enumerationStage;
        }

        private EnumerationStageViewModel EnumerationStage { get; set; }


        public void Init(string interviewId, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.navigationState = navigationState;

            this.EnumerationStage.Init(interviewId, navigationState);

            this.navigationState.ScreenChanged += this.OnScreenChanged;

        }

        private void OnScreenChanged(ScreenChangedEventArgs eventArgs)
        {
            this.RaisePropertyChanged(() => this.Items);
            this.RaisePropertyChanged(() => this.Name);
        }

        public ObservableRangeCollection<dynamic> Items
        {
            get
            {
                if (this.navigationState.CurrentScreenType == ScreenType.Complete)
                {
                    var completionInterview = this.CompletionInterviewViewModel();
                    completionInterview.Init(this.interviewId);
                    return new ObservableRangeCollection<dynamic>(completionInterview.ToEnumerable());
                }
                else
                {
                    return this.EnumerationStage.Items;
                }
            }
        }

        protected virtual CompleteInterviewViewModel CompletionInterviewViewModel()
        {
            return this.interviewViewModelFactory.GetNew<CompleteInterviewViewModel>();
        }

        public string Name
        {
            get
            {
                if (this.navigationState.CurrentScreenType == ScreenType.Complete)
                {
                    return UIResources.Interview_Complete_Screen_Title;
                }
                else
                {
                    return this.EnumerationStage.Name;
                }
            }
        }

        public void Dispose()
        {
            this.navigationState.ScreenChanged -= this.OnScreenChanged;
            this.EnumerationStage.Dispose();
        }
    }
}