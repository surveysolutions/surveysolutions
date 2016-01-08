using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    [Subject(typeof(YesNoQuestionViewModel))]
    public class YesNoQuestionViewModelTestsContext
    {
        protected static YesNoQuestionViewModel CreateViewModel(IUserInteractionService userInteractionService = null,
           IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage = null,
           ILiteEventRegistry eventRegistry = null,
           IStatefulInterviewRepository interviewRepository = null,
           IPrincipal principal = null,
           AnsweringViewModel answeringViewModel = null,
           QuestionStateViewModel<YesNoQuestionAnswered> questionStateViewmodel = null)
        {
            return new YesNoQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                questionnaireStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                questionStateViewmodel ?? Mock.Of<QuestionStateViewModel<YesNoQuestionAnswered>>(x => x.Validity == Mock.Of<ValidityViewModel>()),
                answeringViewModel ?? Mock.Of<AnsweringViewModel>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>()
                );
        }

        protected static QuestionnaireModel BuildDefaultQuestionnaire(Identity questionId, int? maxAllowedAnswers = 2)
        {
            var questionnaire = Create.QuestionnaireModel();
            questionnaire.Questions = new Dictionary<Guid, BaseQuestionModel>
            {
                {
                    questionId.Id, new YesNoQuestionModel
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
                            },
                            new OptionModel
                            {
                                Title = "item3",
                                Value = 3
                            },
                            new OptionModel
                            {
                                Title = "item4",
                                Value = 4
                            },
                            new OptionModel
                            {
                                Title = "item5",
                                Value = 5
                            },
                        },
                        MaxAllowedAnswers = maxAllowedAnswers
                    }
                }
            };
            return questionnaire;
        }
    }
}