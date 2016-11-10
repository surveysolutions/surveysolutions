using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionLinkedToQuestionQuestionViewModel : MultiOptionLinkedQuestionViewModel,
        ILiteEventHandler<LinkedOptionsChanged>,
        ILiteEventHandler<RosterInstancesTitleChanged>
    {
        private Guid linkedToQuestionId;
        private HashSet<Guid> parentRosterIds;

        public MultiOptionLinkedToQuestionQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IPrincipal userIdentity, ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
            : base(questionState, answering, instructionViewModel, interviewRepository, questionnaireStorage, userIdentity, eventRegistry,
                mainThreadDispatcher)
        {
        }

        protected override void InitFromModel(IQuestionnaire questionnaire)
        {
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(questionIdentity.Id);
            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(questionIdentity.Id);
            this.parentRosterIds = questionnaire.GetRostersFromTopToSpecifiedEntity(this.linkedToQuestionId).ToHashSet();
        }

        protected override IEnumerable<MultiOptionLinkedQuestionOptionViewModel> CreateOptions()
        {
            var linkedQuestion = interview.GetLinkedMultiOptionQuestion(this.questionIdentity);

            var answeredOptions = linkedQuestion.GetAnswer()?.ToDecimalArrayArray()?.Select(x => new RosterVector(x))?.ToArray() ??
                                  new RosterVector[0];

            foreach (var linkedOption in linkedQuestion.Options)
                yield return this.CreateOptionViewModel(linkedOption, answeredOptions);
        }

        public void Handle(LinkedOptionsChanged @event)
        {
            var newOptions = this.CreateOptions();

            this.mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                this.Options.SynchronizeWith(newOptions.ToList(), (s, t) => s.Value.Identical(t.Value));
                this.RaisePropertyChanged(() => HasOptions);
            });
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var optionListShouldBeUpdated = @event.ChangedInstances.Any(x => this.parentRosterIds.Contains(x.RosterInstance.GroupId));
            if (optionListShouldBeUpdated)
            {
                var newOptions = this.CreateOptions();
                this.mainThreadDispatcher.RequestMainThreadAction(() =>
                {
                    this.Options.SynchronizeWith(newOptions.ToList(), (s, t) => s.Value.Identical(t.Value) && s.Title == t.Title);
                    this.RaisePropertyChanged(() => HasOptions);
                });
            }
        }

        private MultiOptionLinkedQuestionOptionViewModel CreateOptionViewModel(RosterVector linkedOption, RosterVector[] answeredOptions)
            => new MultiOptionLinkedQuestionOptionViewModel(this)
            {
                Title = this.BuildOptionTitle(linkedOption),
                Value = linkedOption,
                Checked = answeredOptions.Contains(linkedOption),
                QuestionState = this.questionState,
                CheckedOrder = Array.FindIndex(answeredOptions, x => x.Identical(linkedOption)) + 1
            };

        private string BuildOptionTitle(RosterVector linkedOption)
        {
            var sourceQuestionIdentity = Identity.Create(this.linkedToQuestionId, linkedOption);

            string answerAsTitle = this.interview.GetAnswerAsString(sourceQuestionIdentity);

            int currentRosterLevel = this.questionIdentity.RosterVector.Length;

            IEnumerable<string> parentRosterTitlesWithoutLastOneAndFirstKnown =
                interview
                    .GetParentRosterTitlesWithoutLast(sourceQuestionIdentity)
                    .Skip(currentRosterLevel);

            string rosterPrefixes = string.Join(": ", parentRosterTitlesWithoutLastOneAndFirstKnown);

            return string.IsNullOrEmpty(rosterPrefixes) ? answerAsTitle : string.Join(": ", rosterPrefixes, answerAsTitle);
        }
    }
}