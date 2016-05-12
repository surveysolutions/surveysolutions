using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class QuestionHeaderViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<SubstitutionTitlesChanged>,
        ILiteEventHandler<VariablesValuesChanged>, IDisposable
    {
        private class SubstitutionVariables
        {
            public IEnumerable<SubstitutionVariable> ByQuestions { get; set; }
            public IEnumerable<SubstitutionVariable> ByVariables { get; set; }
        }
        private class SubstitutionVariable
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public string Instruction { get; set; }
        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        public bool IsInstructionsHidden
        {
            get { return this.isInstructionsHidden; }
            set
            {
                this.isInstructionsHidden = value;
                this.RaisePropertyChanged();
            }
        }

        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry registry;
        private readonly ISubstitutionService substitutionService;
        private readonly IStringConverter stringConverter;
        private readonly IRosterTitleSubstitutionService rosterTitleSubstitutionService;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private Identity questionIdentity;
        private string interviewId;
        private bool isInstructionsHidden;

        private SubstitutionVariables substitutionVariables;

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            var questionTitle = questionnaire.GetQuestionTitle(questionIdentity.Id);
            this.Title = questionTitle;
            this.IsInstructionsHidden = questionnaire.GetHideInstructions(questionIdentity.Id);
            this.Instruction = questionnaire.GetQuestionInstruction(questionIdentity.Id);
            this.questionIdentity = questionIdentity;
            this.interviewId = interviewId;

            this.substitutionVariables = this.GetSubstitutionVariables(questionnaire, questionTitle);

            this.CalculateSubstitutions(questionnaire, interview);

            this.registry.Subscribe(this, interviewId);
        }

        private SubstitutionVariables GetSubstitutionVariables(IQuestionnaire questionnaire, string titleWithSubstitutions)
        {
            var variableNames = this.substitutionService.GetAllSubstitutionVariableNames(titleWithSubstitutions);

            return new SubstitutionVariables
            {
                ByQuestions = variableNames.Where(questionnaire.HasQuestion).Select(variable => new SubstitutionVariable
                {
                    Name = variable,
                    Id = questionnaire.GetQuestionIdByVariable(variable)
                }),
                ByVariables = variableNames.Where(questionnaire.HasVariable).Select(x => new SubstitutionVariable
                {
                    Name = x,
                    Id = questionnaire.GetVariableIdByVariableName(x)
                })
            };
        }

        protected QuestionHeaderViewModel() { }

        public QuestionHeaderViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry registry,
            ISubstitutionService substitutionService,
            IStringConverter stringConverter,
            IRosterTitleSubstitutionService rosterTitleSubstitutionService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.registry = registry;
            this.substitutionService = substitutionService;
            this.stringConverter = stringConverter;
            this.rosterTitleSubstitutionService = rosterTitleSubstitutionService;
        }

        public ICommand ShowInstructions
        {
            get
            {
                return new MvxCommand(() => IsInstructionsHidden = false);
            }
        }

        public void Handle(VariablesValuesChanged @event)
        {
            var changedVariables = this.substitutionVariables.ByVariables.Where(
                substitution => @event.ChangedVariables.Any(variable => variable.VariableIdentity.Id == substitution.Id));

            if (!changedVariables.Any()) return;

            var interview = this.interviewRepository.Get(this.interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            this.CalculateSubstitutions(questionnaire, interview);
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
            var questionTitle = questionnaire.GetQuestionTitle(questionIdentity.Id);

            if (this.substitutionService.ContainsRosterTitle(questionTitle))
            {
                questionTitle = this.rosterTitleSubstitutionService.Substitute(questionTitle,
                    this.questionIdentity, this.interviewId);
            }

            foreach (var substitution in this.substitutionVariables.ByVariables)
            {
                var variableValue = interview.GetVariableValue(new Identity(substitution.Id, this.questionIdentity.RosterVector));
                var variableValueAsString = this.stringConverter.VariableValueToUIString(variableValue);

                questionTitle = this.substitutionService.ReplaceSubstitutionVariable(
                    questionTitle, substitution.Name,
                    string.IsNullOrEmpty(variableValueAsString) ? this.substitutionService.DefaultSubstitutionText : variableValueAsString);
            }

            foreach (var substitution in this.substitutionVariables.ByQuestions)
            {
                var baseInterviewAnswer = interview.FindBaseAnswerByOrDeeperRosterLevel(substitution.Id, this.questionIdentity.RosterVector);
                string answerString = baseInterviewAnswer != null ? this.stringConverter.AnswerToUIString(substitution.Id, baseInterviewAnswer, interview, questionnaire) : null;

                questionTitle = this.substitutionService.ReplaceSubstitutionVariable(
                    questionTitle, substitution.Name, string.IsNullOrEmpty(answerString) ? this.substitutionService.DefaultSubstitutionText : answerString);
            }

            this.Title = questionTitle;
        }

        public void Dispose()
        {
            this.registry.Unsubscribe(this, interviewId);
        }
    }
}