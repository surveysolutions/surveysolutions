using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_handling_AnswersRemoved_for_cascading_question : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();
            var childAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 3);
            var parentOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 1);

            StatefulInterviewMock.Setup(x => x.Id).Returns(interviewGuid);
            StatefulInterviewMock.Setup(x => x.QuestionnaireId).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionAnswer(questionIdentity)).Returns(childAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionAnswer(parentIdentity)).Returns(parentOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewGuid.FormatGuid()) == StatefulInterviewMock.Object);

            var cascadingQuestionModel = Mock.Of<CascadingSingleOptionQuestionModel>(_
                => _.Id == questionIdentity.Id
                   && _.Options == Options
                   && _.CascadeFromQuestionId == parentIdentity.Id
                   && _.RosterLevelDepthOfParentQuestion == 1);

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.Questions == new Dictionary<Guid, BaseQuestionModel> { { questionIdentity.Id, cascadingQuestionModel } });

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(questionnaireId) == questionnaireModel);

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            cascadingModel.Init(interviewGuid.FormatGuid(), questionIdentity, navigationState);
        };

        Because of = () =>
            cascadingModel.Handle(Create.Event.AnswersRemoved(questionIdentity));

        It should_set_ShouldClearText_in_null = () =>
            cascadingModel.ResetTextInEditor.ShouldBeNull();

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
        
        private static readonly Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();
    }
}