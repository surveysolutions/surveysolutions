using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_question_that_enables_section_with_nested_rosters_inside_it_after_preloading : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.Chapter(children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(numId, variable: "x1")
                    }),
                    Create.Chapter(enablementCondition: "x1 == 1", children: new IComposite[]
                    {
                        Create.ListQuestion(list1Id, variable: "l1"),
                        Create.Roster(roster1Id, rosterSizeQuestionId: list1Id, variable: "r1", rosterSizeSourceType:RosterSizeSourceType.Question, children: new IComposite[]
                        {
                            Create.ListQuestion(list2Id, variable: "l2"),
                            Create.Roster(roster2Id, rosterSizeQuestionId: list2Id, variable: "r2", rosterSizeSourceType:RosterSizeSourceType.Question, children: new IComposite[]
                            {
                                Create.TextQuestion(textId)
                            })
                        })
                    }));

                var preloadedDataDto = Create.PreloadedDataDto(
                    Create.PreloadedLevelDto(RosterVector.Empty, new Dictionary<Guid, AbstractAnswer> { { list1Id, TextListAnswer.FromTextListAnswerRows(new List<TextListAnswerRow> { new TextListAnswerRow(1, "Hello") }) } }),
                    Create.PreloadedLevelDto(Create.RosterVector(1), new Dictionary<Guid, AbstractAnswer> { { list2Id, TextListAnswer.FromTextListAnswerRows(new List<TextListAnswerRow> { new TextListAnswerRow(1, "World") }) } }),
                    Create.PreloadedLevelDto(Create.RosterVector(1, 1), new Dictionary<Guid, AbstractAnswer> { { textId, TextAnswer.FromString("text") } }));

                var interview = SetupPreloadedInterview(preloadedDataDto, questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), numId, RosterVector.Empty, DateTime.Now, 1);
                    return new InvokeResults
                    {
                        TopRosterIsEnabled = interview.IsEnabled(Create.Identity(roster1Id, Create.RosterVector(1))),
                        NestedRosterIsEnabled = interview.IsEnabled(Create.Identity(roster2Id, Create.RosterVector(1, 1)))
                    };
                }
            });

        It should_mark_nested_roster_as_enabled = () =>
            results.NestedRosterIsEnabled.ShouldBeTrue();

        It should_mark_top_level_roster_as_enabled = () =>
            results.TopRosterIsEnabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static Guid questionnaireId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid roster1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid roster2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid numId = Guid.Parse("11111111111111111111111111111111");
        private static Guid list1Id = Guid.Parse("22222222222222222222222222222222");
        private static Guid list2Id = Guid.Parse("33333333333333333333333333333333");
        private static Guid textId = Guid.Parse("44444444444444444444444444444444");

        [Serializable]
        internal class InvokeResults
        {
            public bool TopRosterIsEnabled { get; set; }
            public bool NestedRosterIsEnabled { get; set; }
        }
    }
}