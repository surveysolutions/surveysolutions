using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    internal class when_removing_asnwer_which_changes_value_of_a_variable_that_uses_in_substitution : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: QuestionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(n1Id, "n1"),
                    Create.Entity.NumericIntegerQuestion(n2Id, "n2"),
                    Create.Entity.Variable(variableId, VariableType.LongInteger, "v1", "n1+n2"),
                    Create.Entity.NumericIntegerQuestion(n3Id, "n3", questionText: "title with %v1%")
                });

            interview = SetupStatefullInterview(questionnaire);
            interview.AnswerNumericIntegerQuestion(userId, n1Id, new decimal[0], DateTime.Now, 1);
            interview.AnswerNumericIntegerQuestion(userId, n2Id, new decimal[0], DateTime.Now, 2);
            eventContext = new EventContext();

            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
            interview.RemoveAnswer(n1Id, new decimal[0], userId, DateTime.Now);

        [NUnit.Framework.Test] public void should_update_title_of_question_n3 () =>
            interview.GetTitleText(Create.Identity(n3Id)).Should().Be("title with [...]");

        private static EventContext eventContext;
        private static StatefulInterview interview;
        private static readonly Guid QuestionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static readonly Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static readonly Guid n1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid n2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid n3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid variableId =  Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
