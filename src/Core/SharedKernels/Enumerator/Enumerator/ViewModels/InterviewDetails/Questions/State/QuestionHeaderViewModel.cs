using System;
using System.Windows.Input;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class QuestionHeaderViewModel : MvxNotifyPropertyChanged,
        ICompositeEntity,
        IInterviewEntity,
        IDisposable
    {
        public event EventHandler ShowComments;

        public DynamicTextViewModel Title { get; }
        public EnablementViewModel Enablement { get; }

        public string InterviewId { get; private set; }
        public Identity Identity { get; private set; }
        public NavigationState NavigationState { get; private set; }

        public QuestionHeaderViewModel(
            DynamicTextViewModel dynamicTextViewModel,
            EnablementViewModel enablementViewModel)
        {
            this.Enablement = enablementViewModel;
            this.Title = dynamicTextViewModel;
        }

        public void Init(string interviewId, Identity questionIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            this.InterviewId = interviewId;
            this.NavigationState = navigationState;
            this.Identity = questionIdentity;

            this.Title.Init(interviewId, questionIdentity);
            this.Enablement.Init(interviewId, questionIdentity);
        }

        public ICommand ShowCommentEditorCommand => new MvxCommand(() => ShowComments?.Invoke(this, EventArgs.Empty));

        public void Dispose()
        {
            this.Title.Dispose();
            this.Enablement.Dispose();
        }
    }
}
