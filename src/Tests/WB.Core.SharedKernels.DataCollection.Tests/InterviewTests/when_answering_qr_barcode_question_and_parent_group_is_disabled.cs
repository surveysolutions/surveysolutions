using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Ignore("C#")]
    internal class when_answering_qr_barcode_question_and_parent_group_is_disabled : InterviewTestsContext
    {
        Establish context = () =>
        {
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

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire));

            interview = CreateInterview(questionnaireId: questionnaireId);
        };

        Because of = () =>
             exception = Catch.Exception(() =>interview.AnswerQRBarcodeQuestion(userId: userId, questionId: questionId, 
                 answerTime: DateTime.Now, rosterVector: new decimal[0], answer: answer));

        It should_raise_InterviewException = () =>
           exception.ShouldBeOfExactType<InterviewException>();

        It should_throw_exception_with_message_containting__parent_group_disabled__ = () =>
             new [] { "parent", "group", "disabled" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().TrimEnd('.').Contains(keyword));


        private static Exception exception;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentGroupId = Guid.Parse("22222222222222222222222222222222");
        private static string answer = "some answer here";
    }
}