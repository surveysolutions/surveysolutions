using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_options_for_linked_to_list_question_changed : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            linkSourceId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            linkedQuestionIdentity = Create.Entity.Identity(linkedQuestionId, RosterVector.Empty);

            var questionnaire =
                Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Entity.TextListQuestion(questionId: linkSourceId),
                    Create.Entity.MultyOptionsQuestion(linkedQuestionId, linkedToQuestionId: linkSourceId)
                }));

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            
        };

        Because of = () => interview.AnswerTextListQuestion(Guid.NewGuid(), linkSourceId, RosterVector.Empty, DateTime.UtcNow, new[] { new Tuple<decimal, string>(1, "one"), });

        It should_calculate_state_of_options_for_linked_question = () =>
        {
            interview.GetMultiOptionLinkedToListQuestion(linkedQuestionIdentity)
                .Options.Count.ShouldEqual(1);
        };

        static StatefulInterview interview;
        static Guid linkedQuestionId;
        static Guid linkSourceId;
        static Identity linkedQuestionIdentity;
    }
}