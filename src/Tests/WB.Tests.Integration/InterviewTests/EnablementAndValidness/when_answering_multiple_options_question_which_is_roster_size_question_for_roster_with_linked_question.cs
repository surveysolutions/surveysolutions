using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_multiple_options_question_which_is_roster_size_question_for_roster_with_linked_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var linkedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var rosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var multiOptionQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var txtSourceOfLinkId = Guid.NewGuid();

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Roster(variable: "fix", fixedTitles: new[] {"a", "b"},
                        children: new[] {Create.TextQuestion(variable: "txt", id: txtSourceOfLinkId)}),
                    Create.MultyOptionsQuestion(multiOptionQuestionId, variable: "q1",
                        options:
                            new List<Answer>
                            {
                                Create.Option(value: "1", text: "Hello"),
                                Create.Option(value: "2", text: "World")
                            }),
                    Create.Roster(rosterId,
                        rosterSizeQuestionId: multiOptionQuestionId,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        children: new[]
                        {
                            Create.SingleOptionQuestion(linkedQuestionId, variable: "q2",
                                linkedToQuestionId: txtSourceOfLinkId)
                        })
                    );
                
                var interview = SetupInterview(questionnaireDocument, new object[] { });

                interview.AnswerTextQuestion(userId, txtSourceOfLinkId, new decimal[] { 0 }, DateTime.Now,"a");
                interview.AnswerTextQuestion(userId, txtSourceOfLinkId, new decimal[] { 1 }, DateTime.Now, "b");

                using (var eventContext = new EventContext())
                {

                    interview.AnswerMultipleOptionsQuestion(userId, multiOptionQuestionId, Empty.RosterVector, DateTime.Now, new[] { 1 });
                    return new InvokeResults()
                    {
                        WasLinkedOptionsChanged =
                            HasEvent<LinkedOptionsChanged>(eventContext.Events,
                                e =>
                                    e.ChangedLinkedQuestions[0].QuestionId == Create.Identity(linkedQuestionId, new decimal[] {1})
                                    && e.ChangedLinkedQuestions[0].Options.Length==2
                                    && e.ChangedLinkedQuestions[0].Options[0].SequenceEqual(new decimal[] { 0 })
                                    && e.ChangedLinkedQuestions[0].Options[1].SequenceEqual(new decimal[] { 1 }))
                    };
                }
            });

        It should_raise_LinkedOptionsChanged_event = () =>
            results.WasLinkedOptionsChanged.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasLinkedOptionsChanged { get; set; }
        }
    }
}