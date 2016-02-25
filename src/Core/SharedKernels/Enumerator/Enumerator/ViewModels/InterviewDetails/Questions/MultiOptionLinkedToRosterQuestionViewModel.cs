using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionLinkedToRosterQuestionViewModel : MultiOptionLinkedQuestionViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<RosterInstancesRemoved>,
        ILiteEventHandler<RosterInstancesTitleChanged>
    {
        private Guid linkedToRosterId;

        public MultiOptionLinkedToRosterQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            IStatefulInterviewRepository interviewRepository,
            IPlainQuestionnaireRepository questionnaireStorage,
            IPrincipal userIdentity,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
            : base(
                questionState, answering, interviewRepository, questionnaireStorage, userIdentity, eventRegistry,
                mainThreadDispatcher)
        {
        }

        protected override void InitFromModel(IQuestionnaire questionnaire)
        {
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(questionIdentity.Id);
            this.linkedToRosterId = questionnaire.GetRosterReferencedByLinkedQuestion(questionIdentity.Id);
        }

        protected override IEnumerable<MultiOptionLinkedQuestionOptionViewModel> CreateOptions()
        {
            LinkedMultiOptionAnswer thisQuestionAnswers = interview.GetLinkedMultiOptionAnswer(this.questionIdentity);

            IEnumerable<InterviewRoster> referencedRosters =
            interview.FindReferencedRostersForLinkedQuestion(this.linkedToRosterId, this.questionIdentity);

            List<MultiOptionLinkedQuestionOptionViewModel> options = new List<MultiOptionLinkedQuestionOptionViewModel>();
            foreach (var referencedRoster in referencedRosters)
            {
                var option = this.BuildOption(referencedRoster, thisQuestionAnswers);

                if (option != null)
                {
                    options.Add(option);
                }
            }
            return options;
        }

        private MultiOptionLinkedQuestionOptionViewModel BuildOption(
            InterviewRoster referencedRoster,
            LinkedMultiOptionAnswer linkedMultiOptionAnswer)
        {
        
            var isChecked = linkedMultiOptionAnswer != null &&
                            linkedMultiOptionAnswer.IsAnswered &&
                            linkedMultiOptionAnswer.Answers.Any(x => x.Identical(referencedRoster.RosterVector));

            var title = this.BuildOptionTitle(referencedRoster);

            var option = new MultiOptionLinkedQuestionOptionViewModel(this)
            {
                Title = title,
                Value = referencedRoster.RosterVector,
                Checked = isChecked,
                QuestionState = this.QuestionState
            };
            if (this.areAnswersOrdered && isChecked)
            {
                int selectedItemIndex = Array.FindIndex(linkedMultiOptionAnswer.Answers, x => x.Identical(referencedRoster.RosterVector)) + 1;
                option.CheckedOrder = selectedItemIndex;
            }

            return option;
        }

        private string BuildOptionTitle(InterviewRoster referencedRoster)
        {
            int currentRosterLevel = this.questionIdentity.RosterVector.Length;

            IEnumerable<string> parentRosterTitlesWithoutLastOneAndFirstKnown =
               interview
                   .GetParentRosterTitlesWithoutLastForRoster(referencedRoster.Id, referencedRoster.RosterVector)
                   .Skip(currentRosterLevel);

            string rosterPrefixes = string.Join(": ", parentRosterTitlesWithoutLastOneAndFirstKnown);

            return string.IsNullOrEmpty(rosterPrefixes) ? referencedRoster.Title : string.Join(": ", rosterPrefixes, referencedRoster.Title);
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    foreach (var option in this.Options.Where(option => option.Checked))
                    {
                        option.Checked = false;
                    }
                    this.QuestionState.IsAnswered = false;
                }
            }
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            var optionListShouldBeUpdated = @event.Instances.Any(x => x.GroupId == this.linkedToRosterId);
            var optionsToUpdate = this.CreateOptions().ToArray();
            if (optionListShouldBeUpdated)
                this.mainThreadDispatcher.RequestMainThreadAction(() =>
                {
                    this.Options.Clear();
                    foreach (var singleOptionLinkedQuestionOptionViewModel in optionsToUpdate)
                    {
                        this.Options.Add(singleOptionLinkedQuestionOptionViewModel);
                    }
                    this.RaisePropertyChanged(() => this.HasOptions);
                });
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var optionListShouldBeUpdated = @event.ChangedInstances.Any(x => x.RosterInstance.GroupId == linkedToRosterId);
            var optionsToUpdate = this.CreateOptions().ToArray();
            if (optionListShouldBeUpdated)
                this.mainThreadDispatcher.RequestMainThreadAction(() =>
                {
                    this.Options.Clear();
                    foreach (var singleOptionLinkedQuestionOptionViewModel in optionsToUpdate)
                    {
                        this.Options.Add(singleOptionLinkedQuestionOptionViewModel);
                    }
                    this.RaisePropertyChanged(() => this.HasOptions);
                });
        }
    }
}