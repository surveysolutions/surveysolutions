using System;
using System.Collections.Generic;
using FluentAssertions;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_child_interviewer_entities_after_getting_all_child_entities : PlainQuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            variableId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rootChapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: rootChapterId,
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(scope: QuestionScope.Interviewer, questionId: questionId),
                    Create.Entity.Variable(id: variableId)
                });

            plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);
            plainQuestionnaire.GetChildEntityIds(rootChapterId);
        };

        Because of = () => foundQuestions = plainQuestionnaire.GetAllUnderlyingInterviewerEntities(rootChapterId);

        It should_return_questions_and_variables_as_interviewer_entities = () => 
            foundQuestions.Should().Contain(questionId).And.Contain(variableId);

        static Guid questionId;
        static Guid variableId;
        static PlainQuestionnaire plainQuestionnaire;
        private static Guid rootChapterId;
        private static IReadOnlyList<Guid> foundQuestions;
    }
}