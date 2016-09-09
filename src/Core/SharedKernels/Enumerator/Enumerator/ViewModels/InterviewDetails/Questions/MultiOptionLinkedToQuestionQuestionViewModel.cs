using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionLinkedToQuestionQuestionViewModel : MultiOptionLinkedQuestionViewModel,
        ILiteEventHandler<LinkedOptionsChanged>,
        ILiteEventHandler<RosterInstancesTitleChanged>
    {
        private readonly IAnswerToStringService answerToStringService;
        private Guid linkedToQuestionId;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private HashSet<Guid> parentRosterIds;

        public MultiOptionLinkedToQuestionQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            IQuestionnaireStorage questionnaireStorage,
            IPrincipal userIdentity, ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher, 
            IQuestionnaireStorage questionnaireRepository)
            : base(
                questionState, answering, instructionViewModel, interviewRepository, questionnaireStorage, userIdentity, eventRegistry,
                mainThreadDispatcher)
        {
            this.answerToStringService = answerToStringService;
            this.questionnaireRepository = questionnaireRepository;
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
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity, this.interview.Language);

            LinkedMultiOptionAnswer thisQuestionAnswers = interview.GetLinkedMultiOptionAnswer(this.questionIdentity);
            IEnumerable<BaseInterviewAnswer> linkedToQuestionAnswers =
                interview.FindAnswersOfReferencedQuestionForLinkedQuestion(this.linkedToQuestionId, this.questionIdentity);

            List<MultiOptionLinkedQuestionOptionViewModel> options = new List<MultiOptionLinkedQuestionOptionViewModel>();
            foreach (var answer in linkedToQuestionAnswers)
            {
                var option = this.BuildOption(questionnaire, this.linkedToQuestionId, answer, thisQuestionAnswers);

                if (option != null)
                {
                    options.Add(option);
                }
            }
            return options;
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

        private MultiOptionLinkedQuestionOptionViewModel BuildOption(IQuestionnaire questionnaire,
            Guid linkedToQuestionId,
            BaseInterviewAnswer linkedToAnswer,
            LinkedMultiOptionAnswer linkedMultiOptionAnswer)
        {
            var isChecked = linkedMultiOptionAnswer != null &&
                            linkedMultiOptionAnswer.IsAnswered &&
                            linkedMultiOptionAnswer.Answers.Any(x => x.Identical(linkedToAnswer.RosterVector));

            if (!linkedToAnswer.IsAnswered && !isChecked)
            {
                return null;
            }

            var title = this.BuildOptionTitle(questionnaire, linkedToQuestionId, linkedToAnswer);

            var option = new MultiOptionLinkedQuestionOptionViewModel(this)
            {
                Title = title,
                Value = linkedToAnswer.RosterVector,
                Checked = isChecked,
                QuestionState = this.questionState
            };
            if (this.areAnswersOrdered && isChecked)
            {
                int selectedItemIndex = Array.FindIndex(linkedMultiOptionAnswer.Answers, x => x.Identical(linkedToAnswer.RosterVector)) + 1;
                option.CheckedOrder = selectedItemIndex;
            }

            return option;
        }

        private string BuildOptionTitle(IQuestionnaire questionnaire, Guid linkedToQuestionId, BaseInterviewAnswer linkedToAnswer)
        {
            string answerAsTitle = this.answerToStringService.AnswerToUIString(linkedToQuestionId, linkedToAnswer, interview, questionnaire);

            int currentRosterLevel = this.questionIdentity.RosterVector.Length;

            IEnumerable<string> parentRosterTitlesWithoutLastOneAndFirstKnown =
                interview
                    .GetParentRosterTitlesWithoutLast(linkedToAnswer.Id, linkedToAnswer.RosterVector)
                    .Skip(currentRosterLevel);

            string rosterPrefixes = string.Join(": ", parentRosterTitlesWithoutLastOneAndFirstKnown);

            return string.IsNullOrEmpty(rosterPrefixes) ? answerAsTitle : string.Join(": ", rosterPrefixes, answerAsTitle);
        }
    }
}