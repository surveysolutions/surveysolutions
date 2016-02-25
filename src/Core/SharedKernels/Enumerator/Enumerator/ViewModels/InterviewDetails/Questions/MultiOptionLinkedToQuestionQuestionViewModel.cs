using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionLinkedToQuestionQuestionViewModel : MultiOptionLinkedQuestionViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<AnswerRemoved>,
        ILiteEventHandler<QuestionsDisabled>,
        ILiteEventHandler<QuestionsEnabled>
    {
        private readonly AnswerNotifier answerNotifier;
        private readonly IAnswerToStringService answerToStringService;
        private Guid linkedToQuestionId;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;


        public MultiOptionLinkedToQuestionQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            AnswerNotifier answerNotifier,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            IPlainQuestionnaireRepository questionnaireStorage,
            IPrincipal userIdentity, ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher, 
            IPlainQuestionnaireRepository questionnaireRepository)
            : base(
                questionState, answering, interviewRepository, questionnaireStorage, userIdentity, eventRegistry,
                mainThreadDispatcher)
        {
            this.answerNotifier = answerNotifier;
            this.answerToStringService = answerToStringService;
            this.questionnaireRepository = questionnaireRepository;
        }

        protected override void InitFromModel(IQuestionnaire questionnaire)
        {
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(questionIdentity.Id);
            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(questionIdentity.Id);

            this.answerNotifier.Init(this.interviewId.FormatGuid(), this.linkedToQuestionId);
            this.answerNotifier.QuestionAnswered += this.LinkedToQuestionAnswered;
        }

        protected override IEnumerable<MultiOptionLinkedQuestionOptionViewModel> CreateOptions()
        {
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

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

        public override void Dispose()
        {
            base.Dispose();
            this.answerNotifier.QuestionAnswered -= this.LinkedToQuestionAnswered;
        }

        private void LinkedToQuestionAnswered(object sender, EventArgs e)
        {
            MultiOptionLinkedQuestionOptionViewModel[] actualOptions = this.CreateOptions().ToArray();

            this.mainThreadDispatcher.RequestMainThreadAction(() => // otherwize its f.g magic with those observable collections. This is the only way I found to implement insertions without locks.
            {
                List<MultiOptionLinkedQuestionOptionViewModel> optionsToRemove = this
                    .Options
                    .Where(existingOption => !actualOptions.Any(actualOption => actualOption.Value.Identical(existingOption.Value)))
                    .ToList();

                foreach (var optionToRemove in optionsToRemove)
                {
                    this.Options.Remove(optionToRemove);
                }

                for (int actualOptionIndex = 0; actualOptionIndex < actualOptions.Length; actualOptionIndex++)
                {
                    var actualOption = actualOptions[actualOptionIndex];
                    var existingOption = this.Options.SingleOrDefault(option => option.Value.Identical(actualOption.Value));

                    if (existingOption != null)
                    {
                        existingOption.Title = actualOption.Title;
                    }
                    else
                    {
                        this.Options.Insert(actualOptionIndex, actualOption);
                    }
                }

                this.RaisePropertyChanged(() => this.HasOptions);
            });
        }


        public void Handle(QuestionsDisabled @event)
        {
            foreach (var question in @event.Questions)
            {
                RemoveOptionIfQuestionIsSourceofTheLink(question.Id, question.RosterVector);
            }
        }

        public void Handle(QuestionsEnabled @event)
        {
            var optionListShouldBeUpdated = @event.Questions.Any(x => x.Id == this.linkedToQuestionId);
            if (!optionListShouldBeUpdated)
                return;

            var optionsToUpdate = this.CreateOptions().ToArray();
            this.mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                this.Options.Clear();
                foreach (var option in optionsToUpdate)
                {
                    this.Options.Add(option);
                }
                this.RaisePropertyChanged(() => this.HasOptions);
            });
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                RemoveOptionIfQuestionIsSourceofTheLink(question.Id, question.RosterVector);
            }
        }

        public void Handle(AnswerRemoved @event)
        {
            RemoveOptionIfQuestionIsSourceofTheLink(@event.QuestionId, @event.RosterVector);
        }

        private void RemoveOptionIfQuestionIsSourceofTheLink(Guid removedQuestionId,
           RosterVector removedQuestionRosterVector)
        {
            if (removedQuestionId != this.linkedToQuestionId)
                return;
            var shownAnswer = this.Options.SingleOrDefault(x => removedQuestionRosterVector.Identical(x.Value));
            if (shownAnswer != null)
            {
                this.mainThreadDispatcher.RequestMainThreadAction(() =>
                {
                    this.Options.Remove(shownAnswer);
                });
                this.RaisePropertyChanged(() => this.HasOptions);
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
                QuestionState = this.QuestionState
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