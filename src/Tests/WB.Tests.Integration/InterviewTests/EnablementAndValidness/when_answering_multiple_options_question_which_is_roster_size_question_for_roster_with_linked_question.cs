using System;
using System.Collections.Generic;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_multiple_options_question_which_is_roster_size_question_for_roster_with_linked_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var linkedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var rosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var multiOptionQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var txtSourceOfLinkId = Guid.NewGuid();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.Roster(variable: "fix", fixedTitles: new[] {"a", "b"},
                        children: new[] {Abc.Create.Entity.TextQuestion(questionId: txtSourceOfLinkId, variable: "txt")}),
                     Create.Entity.MultyOptionsQuestion(multiOptionQuestionId, variable: "q1",
                        options:
                            new List<Answer>
                            {
                                 Create.Entity.Option(value: "1", text: "Hello"),
                                 Create.Entity.Option(value: "2", text: "World")
                            }),
                    Create.Entity.Roster(rosterId,
                        rosterSizeQuestionId: multiOptionQuestionId,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        children: new[]
                        {
                            Create.Entity.SingleOptionQuestion(linkedQuestionId, 
                                variable: "q2",
                                linkedToQuestionId: txtSourceOfLinkId)
                        })
                    );
                
                var interview = SetupInterview(questionnaireDocument, new object[] { });

                interview.AnswerTextQuestion(userId, txtSourceOfLinkId, Create.Entity.RosterVector(new[] {0}), DateTime.Now,"a");
                interview.AnswerTextQuestion(userId, txtSourceOfLinkId, Create.Entity.RosterVector(new[] {1}), DateTime.Now, "b");

                using (var eventContext = new EventContext())
                {

                    interview.AnswerMultipleOptionsQuestion(userId, multiOptionQuestionId, RosterVector.Empty, DateTime.Now, new[] { 1 });
                    return new InvokeResults
                    {
                        WasLinkedOptionsChanged =
                            HasEvent<LinkedOptionsChanged>(eventContext.Events,
                                e =>
                                    e.ChangedLinkedQuestions[0].QuestionId == Create.Identity(linkedQuestionId, 1)
                                    && e.ChangedLinkedQuestions[0].Options.Length==2
                                    && e.ChangedLinkedQuestions[0].Options[0].Identical(Create.RosterVector(0))
                                    && e.ChangedLinkedQuestions[0].Options[1].Identical(Create.RosterVector(1)))
                    };
                }
            });

        [NUnit.Framework.Test] public void should_raise_LinkedOptionsChanged_event () =>
            results.WasLinkedOptionsChanged.Should().BeTrue();

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
            public bool WasLinkedOptionsChanged { get; set; }
        }
    }
}
