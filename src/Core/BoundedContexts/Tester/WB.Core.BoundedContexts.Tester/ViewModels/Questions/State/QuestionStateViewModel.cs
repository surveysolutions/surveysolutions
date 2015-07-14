using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Questions.State
{
    public class QuestionStateViewModel<TAnswerEvent>: MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<TAnswerEvent>
        where TAnswerEvent : QuestionAnswered
    {
        public QuestionHeaderViewModel Header { get; private set; }
        public virtual ValidityViewModel Validity { get; private set; }
        public EnablementViewModel Enablement { get; private set; }
        public CommentsViewModel Comments { get; private set; }

        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private Identity questionIdentity;
        private string interviewId;

        protected QuestionStateViewModel() { }

        public QuestionStateViewModel(
            ILiteEventRegistry liteEventRegistry,
            IStatefulInterviewRepository interviewRepository,
            ValidityViewModel validityViewModel, 
            QuestionHeaderViewModel questionHeaderViewModel,
            EnablementViewModel enablementViewModel,
            CommentsViewModel commentsViewModel)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.Validity = validityViewModel;
            this.Header = questionHeaderViewModel;
            this.Enablement = enablementViewModel;
            this.Comments = commentsViewModel;
        }

        public virtual void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            this.liteEventRegistry.Subscribe(this);

            var interview = this.interviewRepository.Get(interviewId);
            this.IsAnswered = interview.WasAnswered(entityIdentity);

            this.Header.Init(interviewId, entityIdentity);
            this.Validity.Init(interviewId, entityIdentity, navigationState);
            this.Comments.Init(interviewId, entityIdentity, navigationState);
            this.Enablement.Init(interviewId, entityIdentity, navigationState);
        }       
        
        private bool isAnswered;
        public bool IsAnswered
        {
            get { return this.isAnswered; }
            set { this.isAnswered = value; this.RaisePropertyChanged(); }
        }

        public void Handle(TAnswerEvent @event)
        {
            if (@event.QuestionId == this.questionIdentity.Id &&
                @event.PropagationVector.SequenceEqual(this.questionIdentity.RosterVector))
            {
                var interview = this.interviewRepository.Get(this.interviewId);
                this.IsAnswered = interview.WasAnswered(this.questionIdentity);
            }
        }

        private IMvxCommand showCommentEditorCommand;
        public IMvxCommand ShowCommentEditorCommand
        {
            get { return this.showCommentEditorCommand ?? (this.showCommentEditorCommand = new MvxCommand(this.ShowCommentsCommand)); }
        }

        private void ShowCommentsCommand()
        {
            if (this.Enablement.Enabled)
            {
                this.Comments.ShowCommentInEditor();
            }
        }
    }
}