using System;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;
using EventIdentity = WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class QuestionContainerViewModel<T> : MvxNotifyPropertyChanged, IInterviewItemViewModel,
        ILiteEventBusEventHandler<QuestionsEnabled>,
        ILiteEventBusEventHandler<QuestionsDisabled>
        where T : class, IInterviewItemViewModel
    {
        private Identity identity;

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

            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);
            
            this.identity = questionIdentity;

            BaseQuestionModel questionModel = questionnaire.Questions[questionIdentity.Id];
            //var answerModel = this.interviewModel.GetTextAnswerModel(this.identity);

            Editor = Mvx.Create<T>();
            Editor.Init(interviewId, questionIdentity);

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
            if (!@event.Questions.All(i => EventIdentity.ToIdentity(i) == this.identity))
                return;

            this.Enabled = true;
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (!@event.Questions.Any(i => EventIdentity.ToIdentity(i) == this.identity))
                return;

            this.Enabled = false;
        }
    }
}