using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    [DebuggerDisplay("Complete")]
    public class SideBarCompleteSectionViewModel : MvxNotifyPropertyChanged, ISideBarItem
    {
        private NavigationState navigationState;
        private readonly IMvxMessenger messenger;
        private readonly AnswerNotifier answerNotifier;

        public DynamicTextViewModel Title { get; }
        public GroupStateViewModel SideBarGroupState { get; }

        public bool Expanded { get; } = false;
        public bool HasChildren { get; } = false;
        public int NodeDepth { get; } = 0;

        private bool isCurrent;
        public bool IsCurrent
        {
            get { return this.isCurrent; }
            set { this.RaiseAndSetIfChanged(ref this.isCurrent, value); }
        }
        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set { this.RaiseAndSetIfChanged(ref this.isSelected, value); }
        }

        public string Tag => "SideBar_Complete";

        public ICommand ToggleCommand => new MvxCommand(() => { });
        public ICommand NavigateToSectionCommand => new MvxAsyncCommand(this.NavigateToSection);

        public SideBarCompleteSectionViewModel(
            IMvxMessenger messenger,
            DynamicTextViewModel dynamicTextViewModel,
            InterviewStateViewModel interviewStateViewModel,
            AnswerNotifier answerNotifier)
        {
            this.messenger = messenger;
            this.answerNotifier = answerNotifier;
            this.Title = dynamicTextViewModel;
            this.SideBarGroupState = interviewStateViewModel;
        }

        public void Init(NavigationState navigationState, string interviewId, string itemText)
        {
            this.Title.InitAsStatic(itemText);
            this.SideBarGroupState.Init(interviewId, null);

            this.navigationState = navigationState;
            this.navigationState.ScreenChanged += this.OnScreenChanged;

            this.answerNotifier.Init(interviewId);
            this.answerNotifier.QuestionAnswered += this.QuestionAnswered;
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            this.SideBarGroupState.UpdateFromGroupModel();
            this.RaisePropertyChanged(() => this.SideBarGroupState);
        }

        void OnScreenChanged(ScreenChangedEventArgs eventArgs) => this.UpdateSelection();

        private void UpdateSelection()
            => this.IsSelected = this.IsCurrent = this.navigationState.CurrentScreenType == ScreenType.Complete;

        private async Task NavigateToSection()
        {
            this.messenger.Publish(new SectionChangeMessage(this));
            await this.navigationState.NavigateTo(NavigationIdentity.CreateForCompleteScreen());
        }

        public void Dispose()
        {
            this.navigationState.ScreenChanged -= this.OnScreenChanged;
            this.Title.Dispose();

            this.answerNotifier.QuestionAnswered -= this.QuestionAnswered;
            this.answerNotifier.Dispose();
        }
    }
}
