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
    internal class when_removing_asnwer_which_changes_value_of_a_variable_that_uses_in_substitution1 : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: QuestionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(n1Id, "n1"),
                    Create.Entity.NumericIntegerQuestion(n2Id, "n2"),
                    Create.Entity.Group(gId, title: "group #%v1%", children: new IComposite[]
                    {
                        Create.Entity.Variable(variableId, VariableType.LongInteger, "v1", "n1+n2")
                    }),
                    Create.Entity.NumericIntegerQuestion(n3Id, "n3", questionText: "title #%v1%"),
                    Create.Entity.NumericRoster(rId, title: "roster #%v2%", variable:"r1", rosterSizeQuestionId: n2Id, children: new IComposite[]
                    {
                        Create.Entity.Variable(variable1Id, VariableType.LongInteger, "v2", "(n1+n2)*n3"),
                    }),
                    Create.Entity.Variable(variable2Id, VariableType.LongInteger, "v3", "r1.Sum(x => x.v2 * (x.@rowindex + 1))"),
                    Create.Entity.NumericIntegerQuestion(n4Id, "n4", questionText: "title #%v3%")
                });

            interview = SetupStatefullInterview(questionnaire);
            interview.AnswerNumericIntegerQuestion(userId, n1Id, new decimal[0], DateTime.Now, 1);
            interview.AnswerNumericIntegerQuestion(userId, n3Id, new decimal[0], DateTime.Now, 10);
            eventContext = new EventContext();

            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
            interview.AnswerNumericIntegerQuestion(userId, n2Id, new decimal[0], DateTime.Now, 2);

        [NUnit.Framework.Test] public void should_update_title_of_question_n3 () =>
            interview.GetTitleText(Create.Identity(n3Id)).Should().Be("title #3");

        [NUnit.Framework.Test] public void should_update_title_of_group_g () =>
            interview.GetTitleText(Create.Identity(gId)).Should().Be("group #3");

        [NUnit.Framework.Test] public void should_update_title_of_roster_0 () =>
            interview.GetTitleText(Create.Identity(rId, Create.RosterVector(0))).Should().Be("roster #30");

        [NUnit.Framework.Test] public void should_update_title_of_question_n4 () =>
            interview.GetTitleText(Create.Identity(n4Id)).Should().Be("title #90");

        private static EventContext eventContext;
        private static StatefulInterview interview;
        private static readonly Guid QuestionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static readonly Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static readonly Guid n1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid n2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid n3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid n4Id = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid gId = Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid rId = Guid.Parse("66666666666666666666666666666666");
        private static readonly Guid variableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid variable1Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid variable2Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
