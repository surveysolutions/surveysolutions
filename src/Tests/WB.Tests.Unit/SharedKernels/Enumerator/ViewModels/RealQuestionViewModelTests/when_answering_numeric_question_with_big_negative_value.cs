using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RealQuestionViewModelTests
{
    internal class when_answering_numeric_question_with_big_negative_value : RealQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            questionnaireId = "Questionnaire Id";
            interviewId = "Some interviewId";
            questionIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[] { 1, 2 });

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetRealNumericAnswer(questionIdentity) == new Mock<RealNumericAnswer>().Object);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithNumericQuestion(questionIdentity.Id);

            validityModelMock = new Mock<ValidityViewModel>();
            answeringViewModelMock = new Mock<AnsweringViewModel>();

            model = CreateViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                validityViewModel: validityModelMock.Object,
                answeringViewModel: answeringViewModelMock.Object);

            model.Init(interviewId, questionIdentity, Create.NavigationState());
        };

        Because of = () =>
        {
            model.Answer =  -9999999999999999m + -1;
            model.ValueChangeCommand.Execute();
        };

        It should_mark_question_as_invalid_with_message = () =>
            validityModelMock.Verify(x => x.MarkAnswerAsNotSavedWithMessage("Entered value can not be parsed as decimal value"), Times.Once);

        It should_not_send_answer_command = () =>
            answeringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerNumericRealQuestionCommand>()), Times.Never);

        private static RealQuestionViewModel model;
        private static Identity questionIdentity;
        private static string interviewId;
        private static string questionnaireId;

        private static Mock<ValidityViewModel> validityModelMock;
        private static Mock<AnsweringViewModel> answeringViewModelMock;
    }
}