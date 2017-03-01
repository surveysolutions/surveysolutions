using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_text_list_question_with_too_much_answers : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(questionId: questionId, maxAnswerCount: 2),
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                interview.AnswerTextListQuestion(
                    userId, questionId, rosterVector, DateTime.Now,
                    new[]
                    {
                        new Tuple<decimal, string>(1.5m, "Answer 1"),
                        new Tuple<decimal, string>(2.5m, "Answer 2"),
                        new Tuple<decimal, string>(1.2m, "Answer 3"),
                    }));

        It should_raise_AnswerNotAcceptedException = () =>
            exception.ShouldBeOfExactType<AnswerNotAcceptedException>();

        It should_throw_exception_with_message_containting_specific_words = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("answers", "greater", "maximum");

        private static Exception exception;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] rosterVector = new decimal[] { };
    }
}