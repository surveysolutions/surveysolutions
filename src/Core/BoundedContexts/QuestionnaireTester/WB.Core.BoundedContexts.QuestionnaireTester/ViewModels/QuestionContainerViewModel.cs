using System;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class QuestionContainerViewModel<T> : MvxNotifyPropertyChanged, IInterviewEntity,
        ILiteEventBusEventHandler<QuestionsEnabled>,
        ILiteEventBusEventHandler<QuestionsDisabled>
        where T : class, IInterviewEntity
    {
        private Identity identity;
        private Guid interviewId;

        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private IPlainRepository<InterviewModel> interviewRepository;

        public QuestionContainerViewModel(
            ICommandService commandService,
            IPrincipal principal,
            IPlainRepository<QuestionnaireModel> questionnaireRepository,
            IPlainRepository<InterviewModel> interviewRepository)
        {
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (principal == null) throw new ArgumentNullException("principal");
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, Identity identity)
        {
            if (identity == null) throw new ArgumentNullException("identity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);
            
            this.identity = identity;
            this.interviewId = interview.Id;

            BaseQuestionModel questionModel = questionnaire.Questions[identity.Id];
            //var answerModel = this.interviewModel.GetTextAnswerModel(this.identity);

            Editor = Mvx.Create<T>();
            Editor.Init(interviewId, identity);

            this.Title = questionModel.Title;
        }

        private T editor;
        public T Editor
        {
            get { return editor; }
            set { this.editor = value; this.RaisePropertyChanged(() => this.Editor); }
        }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(() => this.Title); }
        }

        private bool enabled;
        public bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; this.RaisePropertyChanged(() => this.Enabled); }
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.Any(i => i.Id != this.identity.Id && i.RosterVector != this.identity.RosterVector))
                return;

            this.Enabled = true;
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (@event.Questions.Any(i => i.Id != this.identity.Id && i.RosterVector != this.identity.RosterVector))
                return;

            this.Enabled = false;
        }
    }
}