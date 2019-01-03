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
        public EnablementViewModel Enablement { get; private set; }

        public string InterviewId { get; private set; }
        public Identity Identity { get; private set; }
        public NavigationState NavigationState { get; private set; }

        public QuestionHeaderViewModel(
            DynamicTextViewModel dynamicTextViewModel)
        {
            this.Title = dynamicTextViewModel;
        }

        public void Init(string interviewId, Identity questionIdentity, EnablementViewModel enablement,
        	NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            this.InterviewId = interviewId;
            this.NavigationState = navigationState;
            this.Identity = questionIdentity;

            this.Enablement = enablement;
            this.Title.Init(interviewId, questionIdentity);
        }

        public ICommand ShowCommentEditorCommand => new MvxCommand(() => ShowComments?.Invoke(this, EventArgs.Empty));

        public void Dispose()
        {
            this.Title.Dispose();
            this.Enablement.Dispose();
        }
    }
}
