using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RealQuestionViewModelTests
{
    [TestOf(typeof(RealQuestionViewModel))]
    internal class when_answering_non_negative_numeric_question_with_negative_value : RealQuestionViewModelTestContext
    {
        [SetUp]
        public async Task Context()
        {
            SetUp();
            var realQuestion = Mock.Of<InterviewTreeDoubleQuestion>(_ => _.IsAnswered() == false);
            var interview = Mock.Of<IStatefulInterview>(_ =>
                _.QuestionnaireId == QuestionnaireId &&
                _.GetDoubleQuestion(QuestionIdentity) == realQuestion);
            var questionnaire = Mock.Of<IQuestionnaire>(_ =>
                _.IsQuestionNonNegative(QuestionIdentity.Id) == true);
            var model = CreateRealQuestionViewModel(
                Mock.Of<IQuestionnaireStorage>(x =>
                    x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire),
                Mock.Of<IStatefulInterviewRepository>(x => x.Get(InterviewId) == interview));

            model.Init(InterviewId, QuestionIdentity, NavigationState);
            model.Answer = -4.5;
            await model.ValueChangeCommand.ExecuteAsync();
        }

        [Test]
        public void should_mark_question_as_invalid()
            => ValidityModelMock.Verify(
                x => x.MarkAnswerAsNotSavedWithMessage("Negative values are not allowed for this question"),
                Times.Once);

        [Test]
        public void should_not_send_answer_command()
            => AnsweringViewModelMock.Verify(
                x => x.SendQuestionCommandAsync(Moq.It.IsAny<AnswerNumericRealQuestionCommand>()),
                Times.Never);
    }
}
