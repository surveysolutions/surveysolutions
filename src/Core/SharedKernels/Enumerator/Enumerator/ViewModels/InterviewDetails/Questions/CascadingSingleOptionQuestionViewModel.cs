using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CascadingSingleOptionQuestionViewModel : BaseFilteredQuestionViewModel, 
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
            if (parentSingleOptionQuestion.IsAnswered())
            {
                this.answerOnParentQuestion = parentSingleOptionQuestion.GetAnswer().SelectedValue;
            }
        }
        protected override CategoricalOption GetOptionByFilter(string filter)
            => this.interview.GetOptionForQuestionWithFilter(this.Identity, filter, this.answerOnParentQuestion);

        protected override CategoricalOption GetAnsweredOption(int answer)
            => this.interview.GetOptionForQuestionWithoutFilter(this.Identity, answer, this.answerOnParentQuestion);

        protected override IEnumerable<CategoricalOption> GetSuggestions(string filter)
        {
            if (!this.answerOnParentQuestion.HasValue)
                yield break;

            var categoricalOptions = this.interview.GetTopFilteredOptionsForQuestion(this.Identity,
                this.answerOnParentQuestion.Value, filter, SuggestionsMaxCount);

            foreach (var categoricalOption in categoricalOptions)
                yield return categoricalOption;
        }

        protected override async Task SaveAnswerAsync(string optionText)
        {
            if (!this.answerOnParentQuestion.HasValue)
                return;

            await base.SaveAnswerAsync(optionText);
        }

        public async void Handle(SingleOptionQuestionAnswered @event)
        {
            if (!this.parentQuestionIdentity.Equals(@event.QuestionId, @event.RosterVector)) return;

            var parentSingleOptionQuestion = this.interview.GetSingleOptionQuestion(this.parentQuestionIdentity);
            if (!parentSingleOptionQuestion.IsAnswered()) return;

            this.answerOnParentQuestion = parentSingleOptionQuestion.GetAnswer().SelectedValue;

            await this.UpdateFilterAndSuggestionsAsync(string.Empty);
        }
    }
}