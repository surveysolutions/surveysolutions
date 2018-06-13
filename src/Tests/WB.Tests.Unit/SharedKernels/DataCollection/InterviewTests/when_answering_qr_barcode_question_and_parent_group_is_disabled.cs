using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [NUnit.Framework.Ignore("C#, KP-4388 Different question types without validation expressions (barcode)")]
    internal class when_answering_qr_barcode_question_and_parent_group_is_disabled : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true &&
                        _.GetQuestionType(questionId) == QuestionType.QRBarcode &&
                       // _.GetAllGroupsWithNotEmptyCustomEnablementConditions() == new Guid[] { parentGroupId } &&
                        _.IsRosterGroup(parentGroupId) == false &&
                        _.GetRostersFromTopToSpecifiedGroup(parentGroupId) == new Guid[0] &&
                        _.GetAllParentGroupsForQuestion(questionId) == new Guid[] { parentGroupId }
                );

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
             exception = NUnit.Framework.Assert.Throws<Exception>(() =>interview.AnswerQRBarcodeQuestion(userId: userId, questionId: questionId, 
                 answerTime: DateTime.Now, rosterVector: new decimal[0], answer: answer));

        [NUnit.Framework.Test] public void should_raise_InterviewException () =>
           exception.Should().BeOfType<InterviewException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__parent_group_disabled__ () =>
             new [] { "parent", "group", "disabled" }.Should().OnlyContain(
                    keyword => exception.Message.ToLower().TrimEnd('.').Contains(keyword));


        private static Exception exception;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentGroupId = Guid.Parse("22222222222222222222222222222222");
        private static string answer = "some answer here";
    }
}
