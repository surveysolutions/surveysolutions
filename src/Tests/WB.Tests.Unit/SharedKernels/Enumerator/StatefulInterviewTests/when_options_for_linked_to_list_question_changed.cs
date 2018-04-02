using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_options_for_linked_to_list_question_changed : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            linkedQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            linkSourceId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            linkedQuestionIdentity = Create.Entity.Identity(linkedQuestionId, RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.TextListQuestion(questionId: linkSourceId),
                Create.Entity.MultyOptionsQuestion(linkedQuestionId, linkedToQuestionId: linkSourceId)
            });

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            BecauseOf();
        }

        private void BecauseOf() => interview.AnswerTextListQuestion(Guid.NewGuid(), linkSourceId, RosterVector.Empty, DateTime.UtcNow, new[] { new Tuple<decimal, string>(1, "one"), });

        [NUnit.Framework.Test] public void should_calculate_state_of_options_for_linked_question () 
        {
            interview.GetMultiOptionLinkedToListQuestion(linkedQuestionIdentity)
                .Options.Length.Should().Be(1);
        }

        static StatefulInterview interview;
        static Guid linkedQuestionId;
        static Guid linkSourceId;
        static Identity linkedQuestionIdentity;
    }
}
