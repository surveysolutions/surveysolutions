using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class NavigationState
    {
        private readonly ICommandService commandService;

        public event GroupChanged OnGroupChanged;
        public string InterviewId { get; private set; }
        public string QuestionnaireId { get; private set; }
        public Identity CurrentGroup { get; private set; }

        private readonly Stack<Identity> navigationStack = new Stack<Identity>();

        public NavigationState(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        public void Init(string interviewId, string questionnaireId)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
        }

        public async Task NavigateTo(Identity groupIdentity)
        {
            await this.commandService.WaitPendingCommandsAsync();

            while (this.navigationStack.Contains(groupIdentity))
            {
                this.navigationStack.Pop();
            }

            this.navigationStack.Push(groupIdentity);

            this.CurrentGroup = groupIdentity;

            if (OnGroupChanged != null)
                OnGroupChanged(groupIdentity);
        }

        public async Task NavigateBack(Action navigateToIfHistoryIsEmpty)
        {
            await this.commandService.WaitPendingCommandsAsync();

            if (navigateToIfHistoryIsEmpty == null) throw new ArgumentNullException("navigateToIfHistoryIsEmpty");

            // remove current group from stack
            this.navigationStack.Pop();

            if (this.navigationStack.Count == 0)
                navigateToIfHistoryIsEmpty.Invoke();
            else
            {
                var previousGroupIdentity = this.navigationStack.Pop();
                await this.NavigateTo(previousGroupIdentity);
            }
        }
    }

    public delegate void GroupChanged(Identity newGroupIdentity);
}