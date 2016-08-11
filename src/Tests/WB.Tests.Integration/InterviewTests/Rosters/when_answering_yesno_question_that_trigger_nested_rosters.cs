using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_answering_yesno_question_that_trigger_nested_rosters : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var sectionId = Guid.Parse("44444444444444444444444444444444");
                var roster1Id = Guid.Parse("11111111111111111111111111111111");
                var roster2Id = Guid.Parse("22222222222222222222222222222222");
                var roster3Id = Guid.Parse("33333333333333333333333333333333");

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.Group(sectionId, "Section", children: new IComposite[]
                    {
                        Create.MultyOptionsQuestion(rosterSizeQuestionId, variable: "multi", yesNo: true,
                            options: new List<Answer>
                            {
                                Create.Option(value: "10", text: "A"),
                                Create.Option(value: "20", text: "B"),
                                Create.Option(value: "30", text: "C"),
                                Create.Option(value: "40", text: "D")
                            }),
                        Create.Roster(
                            id: roster1Id,
                            rosterSizeSourceType: RosterSizeSourceType.Question,
                            rosterSizeQuestionId: rosterSizeQuestionId,
                            variable: "first",
                            children: new IComposite[]
                            {
                                Create.Roster(
                                    id: roster2Id,
                                    rosterSizeSourceType: RosterSizeSourceType.Question,
                                    rosterSizeQuestionId: rosterSizeQuestionId,
                                    variable: "second",
                                    children: new IComposite[]
                                    {
                                        Create.Roster(
                                            id: roster3Id,
                                            rosterSizeSourceType: RosterSizeSourceType.Question,
                                            rosterSizeQuestionId: rosterSizeQuestionId,
                                            variable: "third")
                                    })
                            })
                    })
                );

                var result = new InvokeResults();

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(rosterSizeQuestionId, RosterVector.Empty,
                    Yes(10), Yes(20), Yes(40)));

                var sidebarViewModel = Setup.SidebarSectionViewModel(questionnaireDocument, interview);
                sidebarViewModel.Sections.ElementAt(1).Expanded = true;

                using (var eventContext = new EventContext())
                {
                    interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(rosterSizeQuestionId, RosterVector.Empty,
                    Yes(10), Yes(40), Yes(30)));

                    sidebarViewModel.Handle(eventContext.GetSingleEvent<RosterInstancesAdded>());
                    sidebarViewModel.Handle(eventContext.GetSingleEvent<RosterInstancesRemoved>());
                }

                var rosterInstances = sidebarViewModel.Sections.First().Children;
                result.FirstSectionContainsDuplicates = rosterInstances.Count != rosterInstances.Select(x => x.SectionIdentity).Distinct().Count();

                return result;
            });

        It should_not_create_duplicates_links_in_sidebar = () =>
            results.FirstSectionContainsDuplicates.ShouldBeFalse();

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
            public bool FirstSectionContainsDuplicates { get; set; }
        }
    }
}