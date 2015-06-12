using System;
using System.Collections.Generic;
using Chance.MvvmCross.Plugins.UserInteraction;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.MultiOptionQuestionViewModelTests
{
    [Subject(typeof(MultiOptionQuestionViewModel))]
    public class MultiOptionQuestionViewModelTestsContext
    {
        protected static MultiOptionQuestionViewModel CreateViewModel(IUserInteraction userInteraction = null, 
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage = null, 
            ILiteEventRegistry eventRegistry = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            IPrincipal principal = null, 
            AnsweringViewModel answeringViewModel = null, 
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewmodel = null)
        {
            var userInteractionFunc = userInteraction == null
                ? (Func<IUserInteraction>) (Mock.Of<IUserInteraction>)
                : (() => userInteraction);

            return new MultiOptionQuestionViewModel(
                questionStateViewmodel ?? Mock.Of<QuestionStateViewModel<MultipleOptionsQuestionAnswered>>(x => x.Validity == Mock.Of<ValidityViewModel>()),
                questionnaireStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                principal ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                userInteractionFunc,
                answeringViewModel ?? Mock.Of<AnsweringViewModel>());
        }

        protected static QuestionnaireModel BuildDefaultQuestionnaire(Identity questionId)
        {
            var questionnaire = Create.QuestionnaireModel();
            questionnaire.Questions = new Dictionary<Guid, BaseQuestionModel>();
            questionnaire.Questions.Add(questionId.Id,
                new MultiOptionQuestionModel
                {
                    AreAnswersOrdered = true,
                    Id = questionId.Id,
                    Instructions = "instructions",
                    Options = new List<OptionModel>
                    {
                        new OptionModel
                        {
                            Title = "item1",
                            Value = 1
                        },
                        new OptionModel
                        {
                            Title = "item2",
                            Value = 2
                        }
                    },
                    MaxAllowedAnswers = 1
                });
            return questionnaire;
        }
    }
}