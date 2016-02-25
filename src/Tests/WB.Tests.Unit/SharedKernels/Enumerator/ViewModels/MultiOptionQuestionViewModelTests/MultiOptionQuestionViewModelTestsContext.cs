using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    [Subject(typeof(MultiOptionQuestionViewModel))]
    internal class MultiOptionQuestionViewModelTestsContext
    {
        protected static MultiOptionQuestionViewModel CreateViewModel(IUserInteractionService userInteractionService = null, 
            IPlainQuestionnaireRepository questionnaireStorage = null, 
            ILiteEventRegistry eventRegistry = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            IPrincipal principal = null, 
            AnsweringViewModel answeringViewModel = null, 
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewmodel = null)
        {
            return new MultiOptionQuestionViewModel(
                questionStateViewmodel ?? Mock.Of<QuestionStateViewModel<MultipleOptionsQuestionAnswered>>(x => x.Validity == Mock.Of<ValidityViewModel>()),
                questionnaireStorage ?? Mock.Of<IPlainQuestionnaireRepository>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                principal ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
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