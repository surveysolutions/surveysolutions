using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_parent_for_linked_question_in_nested_roster : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Group(expectedParentGroupId, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId),
                    Create.Entity.Roster(rosterId: rosterId, rosterSizeQuestionId: rosterSizeQuestionId,
                        children: new IComposite[]
                        {
                            Create.Entity.MultipleOptionsQuestion(questionId: nestedRosterSizeQuestionId),
                            Create.Entity.Roster(rosterId: nestedRosterId,
                                rosterSizeQuestionId: nestedRosterSizeQuestionId,
                                children: new IComposite[]
                                {
                                    Create.Entity.TextQuestion(questionId: linkedSourceQuestionId)
                                }),
                            Create.Entity.SingleQuestion(id: linkedOnQuestionId,
                                linkedToQuestionId: linkedSourceQuestionId)
                        })
                })
            });
            plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, 0);
        };

        Because of = () => parentForLinkedQuestion = plainQuestionnaire.GetParentForLinkedQuestion(linkedOnQuestionId);

        It should_top_roster_be_parent_for_linked_question = () =>
            parentForLinkedQuestion.ShouldEqual(expectedParentGroupId);

        private static PlainQuestionnaire plainQuestionnaire;
        private static readonly Guid linkedOnQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid nestedRosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid linkedSourceQuestionId = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid rosterId = Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid nestedRosterId = Guid.Parse("66666666666666666666666666666666");
        private static readonly Guid expectedParentGroupId = Guid.Parse("77777777777777777777777777777777");
        private static Guid? parentForLinkedQuestion;
        private static QuestionnaireDocument questionnaireDocument;
    }
}