using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    class LinkedQuestionsTests: InterviewTestsContext
    {
        [Test]
        public void when_removing_item_from_list_question_that_source_for_answered_multi_linked()
        {
            Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            Guid linkedToQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var titleQuestionId = Guid.NewGuid();
            
            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Abc.Create.Entity.TextListQuestion(questionId:titleQuestionId),
                Abc.Create.Entity.MultipleOptionsQuestion(questionId: linkedToQuestionId, linkedToQuestionId: titleQuestionId,variable: "multi_single")
            });

            using (var appDomainContext = AppDomainContext.Create())
            {
                var interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument: questionnaireDocument);

                interview.AnswerTextListQuestion(userId: userId, questionId: titleQuestionId, new decimal[0], originDate: DateTimeOffset.Now,
                    new[]
                    {
                        new Tuple<decimal, string>(1, "house 1"),
                        new Tuple<decimal, string>(2, "house 2"),
                        new Tuple<decimal, string>(3, "house 3")
                    });

                interview.AnswerMultipleOptionsQuestion(userId: userId, questionId: linkedToQuestionId, new decimal[0], originDate: DateTimeOffset.Now,new []{1,2,3});

                //act
                interview.AnswerTextListQuestion(userId: userId, questionId: titleQuestionId, new decimal[0], originDate: DateTimeOffset.Now,
                    new[]
                    {
                        new Tuple<decimal, string>(1, "house 1"),
                        new Tuple<decimal, string>(2, "house 2"),
                    });

                var linkedQuestion =
                     interview.GetMultiOptionLinkedToListQuestion(new Identity(linkedToQuestionId,
                        new decimal[0]));

                Assert.That(linkedQuestion.IsAnswered(), Is.True);
                Assert.That(linkedQuestion.GetAnswer().CheckedValues, Is.EqualTo(new int[] { 1, 2 }));
            }
        }
    }
}
