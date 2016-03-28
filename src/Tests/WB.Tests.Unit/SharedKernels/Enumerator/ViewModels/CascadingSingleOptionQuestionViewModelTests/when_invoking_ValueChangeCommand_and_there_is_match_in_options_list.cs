using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_invoking_ValueChangeCommand_and_there_is_match_in_options_list : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();

            var childAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == answerOnChildQuestion);
            var parentOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 1);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetSingleOptionAnswer(questionIdentity) == childAnswer
                   && _.GetSingleOptionAnswer(parentIdentity) == parentOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            QuestionStateMock.Setup(x => x.Validity).Returns(ValidityModelMock.Object);

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            cascadingModel.InitAsync(interviewId, questionIdentity, navigationState).WaitAndUnwrapException();

            cascadingModel.FilterText = "o";
        };

        Because of = () =>
            cascadingModel.ValueChangeCommand.Execute("title klo 3");

        It should_not_mark_question_as_invalid = () =>
            ValidityModelMock.Verify(x => x.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Text_MaskError), Times.Never);

        It should_send_answer_command = () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerSingleOptionQuestionCommand>()), Times.Once);

        private static CascadingSingleOptionQuestionViewModel cascadingModel;

        private const int answerOnChildQuestion = 2;

        private static readonly Mock<ValidityViewModel> ValidityModelMock = new Mock<ValidityViewModel>();
    }
}