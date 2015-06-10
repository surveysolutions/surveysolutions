using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class LinkedMultiOptionQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAnswerToStringService answerToStringService;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage;
        private Guid linkedToQuestionId;
        public QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public LinkedMultiOptionQuestionViewModel(QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage,
            ILiteEventRegistry eventRegistry)
        {
            this.interviewRepository = interviewRepository;
            this.answerToStringService = answerToStringService;
            this.questionnaireStorage = questionnaireStorage;
            this.QuestionState = questionState;
            this.Answering = answering;
            this.Options = new ObservableCollection<LinkedMultiOptionQuestionOptionViewModel>();
            eventRegistry.Subscribe(this);
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            QuestionnaireModel questionnaire = this.questionnaireStorage.GetById(interview.QuestionnaireId);
            LinkedMultiOptionQuestionModel linkedQuestionModel = questionnaire.GetLinkedMultiOptionQuestion(entityIdentity.Id);

            linkedToQuestionId = linkedQuestionModel.LinkedToQuestionId;

            IEnumerable<BaseInterviewAnswer> linkedQuestionAnswers = interview.FindBaseAnswerByOrShorterRosterLevel(linkedQuestionModel.LinkedToQuestionId, entityIdentity.RosterVector);
            foreach (var answer in linkedQuestionAnswers)
            {
                if (answer != null && answer.IsAnswered)
                {
                    string title = this.answerToStringService.AnswerToString(questionnaire.Questions[entityIdentity.Id], answer);

                    this.Options.Add(new LinkedMultiOptionQuestionOptionViewModel
                    {
                        Title = title,
                        Value = answer.RosterVector
                    });
                }
            }
        }

        public ObservableCollection<LinkedMultiOptionQuestionOptionViewModel> Options { get; private set; }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (question.Id == this.linkedToQuestionId)
                {
                    var shownAnswer = this.Options.SingleOrDefault(x => x.Value.SequenceEqual(question.RosterVector));
                    if (shownAnswer != null)
                    {
                        MvxMainThreadDispatcher.Instance.RequestMainThreadAction(() => this.Options.Remove(shownAnswer));
                    }
                }
            }
        }
    }
}