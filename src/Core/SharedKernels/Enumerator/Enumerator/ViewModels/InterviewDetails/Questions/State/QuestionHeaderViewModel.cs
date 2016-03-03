using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class QuestionHeaderViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<SubstitutionTitlesChanged>, IDisposable
    {
        public string Instruction { get; set; }
        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry registry;
        private readonly ISubstitutionService substitutionService;
        private readonly IAnswerToStringService answerToStringService;
        private readonly IRosterTitleSubstitutionService rosterTitleSubstitutionService;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private Identity questionIdentity;
        private string interviewId;

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            this.Title = questionnaire.GetQuestionTitle(questionIdentity.Id);
            this.Instruction = questionnaire.GetQuestionInstruction(questionIdentity.Id);
            this.questionIdentity = questionIdentity;
            this.interviewId = interviewId;

            this.CalculateSubstitutions(questionnaire, interview);

            this.registry.Subscribe(this, interviewId);
        }

        protected QuestionHeaderViewModel() { }

        public QuestionHeaderViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry registry,
            ISubstitutionService substitutionService,
            IAnswerToStringService answerToStringService,
            IRosterTitleSubstitutionService rosterTitleSubstitutionService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.registry = registry;
            this.substitutionService = substitutionService;
            this.answerToStringService = answerToStringService;
            this.rosterTitleSubstitutionService = rosterTitleSubstitutionService;
        }

        public void Handle(SubstitutionTitlesChanged @event)
        {
            bool thisQuestionChanged = @event.Questions.Any(x => this.questionIdentity.Equals(x));

            if (thisQuestionChanged)
            {
                var interview = this.interviewRepository.Get(this.interviewId);
                IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

                this.CalculateSubstitutions(questionnaire, interview);
            }
        }

        private void CalculateSubstitutions(IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            string questionTitle = questionnaire.GetQuestionTitle(this.questionIdentity.Id);
            if (this.substitutionService.ContainsRosterTitle(questionTitle))
            {
                questionTitle = this.rosterTitleSubstitutionService.Substitute(questionTitle,
                    this.questionIdentity, this.interviewId);
            }
            string[] variablesToReplace = this.substitutionService.GetAllSubstitutionVariableNames(questionTitle);

            foreach (var variable in variablesToReplace)
            {
                var substitutedQuestionId = questionnaire.GetQuestionIdByVariable(variable);

                var baseInterviewAnswer = interview.FindBaseAnswerByOrDeeperRosterLevel(substitutedQuestionId, this.questionIdentity.RosterVector);
                string answerString = baseInterviewAnswer != null ? this.answerToStringService.AnswerToUIString(substitutedQuestionId, baseInterviewAnswer, interview, questionnaire) : null;

                questionTitle = this.substitutionService.ReplaceSubstitutionVariable(
                    questionTitle, variable, string.IsNullOrEmpty(answerString) ? this.substitutionService.DefaultSubstitutionText : answerString);
            }

            this.Title = questionTitle;
        }

        public void Dispose()
        {
            this.registry.Unsubscribe(this, interviewId);
        }
    }
}