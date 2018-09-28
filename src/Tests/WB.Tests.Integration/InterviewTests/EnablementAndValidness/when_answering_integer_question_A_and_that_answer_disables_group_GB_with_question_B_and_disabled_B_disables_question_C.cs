using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_integer_question_A_and_that_answer_disables_group_GB_with_question_B_and_disabled_B_disables_question_C_and_B_was_answered : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var emptyRosterVector = new decimal[] { };
                var  userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var groupGBId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(questionAId, "a"),
                    Abc.Create.Entity.Group(groupGBId, "Group X", null, "a > 0", false, new IComposite[] {
                        Abc.Create.Entity.NumericIntegerQuestion(questionBId, "b")
                    }),
                    Abc.Create.Entity.NumericIntegerQuestion(questionCId, "c", enablementCondition: "b > 0")
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>
                {
                    Abc.Create.Event.QuestionsEnabled(DateTimeOffset.Now, new []
                    {
                        Abc.Create.Identity(questionAId),
                        Abc.Create.Identity(questionBId),
                        Abc.Create.Identity(questionCId)
                    }),
                    Abc.Create.Event.GroupsEnabled(new [] { Abc.Create.Entity.Identity(groupGBId) }),
                    Abc.Create.Event.NumericIntegerQuestionAnswered(
                        questionBId, null, 1, null, null
                    )
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionAId, emptyRosterVector, DateTime.Now, 0);

                    return new InvokeResults()
                    {
                        GroupGBDisabled = GetFirstEventByType<GroupsDisabled>(eventContext.Events).Groups.FirstOrDefault(g => g.Id == groupGBId) != null,
                        QuestionCDisabled = GetFirstEventByType<QuestionsDisabled>(eventContext.Events).Questions.FirstOrDefault(g => g.Id == questionCId) != null
                    };
                }
            });

        [NUnit.Framework.Test] public void should_disable_question_B () =>
            results.GroupGBDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_disable_question_C () =>
            results.QuestionCDisabled.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool GroupGBDisabled { get; set; }
            public bool QuestionCDisabled { get; set; }
        }
    }
}
