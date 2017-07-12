using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CascadingSingleOptionQuestionViewModel : BaseFilteredQuestionViewModel<CascadingComboboxItemViewModel>, 
         ILiteEventHandler<SingleOptionQuestionAnswered>
    {
        private readonly IQuestionnaireStorage questionnaireRepository;

        private Identity parentQuestionIdentity;
        private int? answerOnParentQuestion;

        public CascadingSingleOptionQuestionViewModel(IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry, 
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel, 
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel) :
            base(principal: principal, questionStateViewModel: questionStateViewModel, answering: answering,
                instructionViewModel: instructionViewModel, interviewRepository: interviewRepository,
                eventRegistry: eventRegistry)
        {
            this.questionnaireRepository = questionnaireRepository ??
                                           throw new ArgumentNullException(nameof(questionnaireRepository));
        }

        protected override void Initialize(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            var cascadingQuestionParentId = questionnaire.GetCascadingQuestionParentId(entityIdentity.Id);
            if (!cascadingQuestionParentId.HasValue) throw new NullReferenceException($"Parent of cascading question {entityIdentity} is missing");
            
            var parentRosterVector = entityIdentity.RosterVector.Take(questionnaire.GetRosterLevelForEntity(cascadingQuestionParentId.Value)).ToArray();

            this.parentQuestionIdentity = new Identity(cascadingQuestionParentId.Value, parentRosterVector);

            var parentSingleOptionQuestion = interview.GetSingleOptionQuestion(this.parentQuestionIdentity);
            if (parentSingleOptionQuestion.IsAnswered)
            {
                this.answerOnParentQuestion = parentSingleOptionQuestion.GetAnswer().SelectedValue;
            }
        }

        protected override bool CanSendAnswerCommand(CascadingComboboxItemViewModel answer)
            => this.interview.GetSingleOptionQuestion(this.Identity).GetAnswer()?.SelectedValue != answer.Value;

        protected override CategoricalOption GetAnsweredOption(int answer)
            => this.interview.GetOptionForQuestionWithoutFilter(this.Identity, answer, this.answerOnParentQuestion);

        protected override CascadingComboboxItemViewModel ToViewModel(CategoricalOption option, string filter)
            => new CascadingComboboxItemViewModel
            {
                Text = GetHighlightedText(option.Title, filter),
                OriginalText = option.Title,
                Value = option.Value,
                ParentValue = option.ParentValue.Value
            };

        protected override IEnumerable<CategoricalOption> GetSuggestions(string filter)
        {
            if (!this.answerOnParentQuestion.HasValue)
                yield break;

            var categoricalOptions = this.interview.GetTopFilteredOptionsForQuestion(this.Identity,
                this.answerOnParentQuestion.Value, filter, SuggestionsMaxCount);

            foreach (var categoricalOption in categoricalOptions)
                yield return categoricalOption;
        }

        protected override AnswerQuestionCommand CreateAnswerCommand(CascadingComboboxItemViewModel answer)
            => new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.principal.CurrentUserIdentity.UserId,
                this.Identity.Id,
                this.Identity.RosterVector,
                DateTime.UtcNow,
                answer.Value);

        protected override void SaveAnswer(CascadingComboboxItemViewModel categoricalOption)
        {
            if (!this.answerOnParentQuestion.HasValue)
                return;

            base.SaveAnswer(categoricalOption);
        }

        public void Handle(SingleOptionQuestionAnswered @event)
        {
            if (this.parentQuestionIdentity.Equals(@event.QuestionId, @event.RosterVector))
            {
                var parentSingleOptionQuestion = this.interview.GetSingleOptionQuestion(this.parentQuestionIdentity);
                if (parentSingleOptionQuestion.IsAnswered)
                {
                    this.answerOnParentQuestion = parentSingleOptionQuestion.GetAnswer().SelectedValue;
                    this.FilterText = string.Empty;
                }              
            }
        }
    }
}