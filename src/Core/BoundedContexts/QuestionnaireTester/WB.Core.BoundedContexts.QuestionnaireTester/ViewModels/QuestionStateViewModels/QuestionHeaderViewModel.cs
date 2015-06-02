using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels
{
    public class QuestionHeaderViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<SubstitutionTitlesChanged>
    {
        public string Instruction { get; set; }
        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry registry;
        private readonly ISubstitutionService substitutionService;
        private readonly IAnswerToStringService answerToStringService;
        private Identity questionIdentity;
        private string interviewId;

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            QuestionnaireModel questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = questionnaire.Questions[questionIdentity.Id];

            this.Title = questionModel.Title;
            this.Instruction = questionModel.Instructions;
            this.questionIdentity = questionIdentity;
            this.interviewId = interviewId;

            CalculateSubstitutions(questionnaire, interview);

            this.registry.Subscribe(this);
        }

        public QuestionHeaderViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            ILiteEventRegistry registry,
            ISubstitutionService substitutionService,
            IAnswerToStringService answerToStringService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.registry = registry;
            this.substitutionService = substitutionService;
            this.answerToStringService = answerToStringService;
        }

        public void Handle(SubstitutionTitlesChanged @event)
        {
            bool thisQuestionChanged = @event.Questions.Any(x => this.questionIdentity.ToIdentityForEvents().Equals(x));

            if (thisQuestionChanged)
            {
                var interview = this.interviewRepository.Get(this.interviewId);
                QuestionnaireModel questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

                this.CalculateSubstitutions(questionnaire, interview);
            }
        }

        private void CalculateSubstitutions(QuestionnaireModel questionnaire, IStatefulInterview interview)
        {
            BaseQuestionModel questionModel = questionnaire.Questions[this.questionIdentity.Id];

            string[] variablesToReplace = this.substitutionService.GetAllSubstitutionVariableNames(questionModel.Title);
            string questionTitle = questionModel.Title;
            foreach (var variable in variablesToReplace)
            {
                var substitutedQuestionModel = questionnaire.Questions.Single(x => x.Value.Variable == variable);

                var baseInterviewAnswer = interview.GetAnswer(substitutedQuestionModel.Key, this.questionIdentity.RosterVector);
                string answerString = baseInterviewAnswer != null ? this.answerToStringService.AnswerToString(substitutedQuestionModel.Value, baseInterviewAnswer) : null;

                questionTitle = this.substitutionService.ReplaceSubstitutionVariable(
                    questionTitle, variable, answerString ?? substitutionService.DefaultSubstitutionText);
            }

            this.Title = questionTitle;
        }
    }
}