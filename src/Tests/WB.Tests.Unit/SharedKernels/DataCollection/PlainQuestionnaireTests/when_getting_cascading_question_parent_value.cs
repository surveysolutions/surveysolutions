using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_cascading_question_parent_value : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.SingleQuestion(id: questionId, 
                        options: new[] { new Answer { AnswerValue = "1", ParentValue = "1" },
                                         new Answer { AnswerValue = "2", ParentValue = "2" } }.ToList()),
                
            });
            
            plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, 0);
        };

        Because of = () =>
            result = plainQuestionnaire.GetCascadingParentValue(questionId, 1m);

        It result_should_be_equal_1 = () =>
            result.ShouldEqual(1m);

        private static PlainQuestionnaire plainQuestionnaire;
        private static decimal result;
        private static readonly Guid questionId = Guid.Parse("00000000000000000000000000000000");
    }
}