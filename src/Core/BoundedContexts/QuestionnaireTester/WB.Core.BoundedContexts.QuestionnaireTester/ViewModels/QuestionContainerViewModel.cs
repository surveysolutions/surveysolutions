using System;
using System.Linq;

using Cirrious.CrossCore;

using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class QuestionContainerViewModel<T> : BaseInterviewItemViewModel,
        ILiteEventBusEventHandler<QuestionsEnabled>,
        ILiteEventBusEventHandler<QuestionsDisabled>
        where T : BaseInterviewItemViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private Identity identity;
        private InterviewModel interviewModel;
        private QuestionnaireModel questionnaireModel;

        public QuestionContainerViewModel(ICommandService commandService, IPrincipal principal)
        {
            this.commandService = commandService;
            this.principal = principal;
        }

        public override void Init(Identity identity, InterviewModel interviewModel, QuestionnaireModel questionnaireModel)
        {
            if (identity == null) throw new ArgumentNullException("identity");
            if (interviewModel == null) throw new ArgumentNullException("interviewModel");
            if (questionnaireModel == null) throw new ArgumentNullException("questionnaireModel");

            this.identity = identity;
            this.interviewModel = interviewModel;
            this.questionnaireModel = questionnaireModel;

            var questionModel = this.questionnaireModel.Questions[this.identity.Id];
            //var answerModel = this.interviewModel.GetTextAnswerModel(this.identity);

            Editor = Mvx.Create<T>();
            Editor.Init(identity, interviewModel, questionnaireModel);

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

        private bool isTextChanged = false;

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