using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_answering_yesno_question_that_trigger_nested_rosters : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void context () {

            base.Setup();

            appDomainContext = AppDomainContext.Create();

            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var sectionId = Guid.Parse("44444444444444444444444444444444");
                var roster1Id = Guid.Parse("11111111111111111111111111111111");
                var roster2Id = Guid.Parse("22222222222222222222222222222222");
                var roster3Id = Guid.Parse("33333333333333333333333333333333");

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.Group(sectionId, "Section", null, null, false, new IComposite[]
                    {
                        Create.Entity.MultyOptionsQuestion(rosterSizeQuestionId, variable: "multi", yesNoView: true,
                            options: new List<Answer>
                            {
                                Create.Entity.Option(value: "10", text: "A"),
                                Create.Entity.Option(value: "20", text: "B"),
                                Create.Entity.Option(value: "30", text: "C"),
                                Create.Entity.Option(value: "40", text: "D")
                            }),
                        Create.Entity.Roster(
                            rosterId: roster1Id,
                            rosterSizeSourceType: RosterSizeSourceType.Question,
                            rosterSizeQuestionId: rosterSizeQuestionId,
                            variable: "first",
                            children: new IComposite[]
                            {
                                Create.Entity.Roster(
                                    rosterId: roster2Id,
                                    rosterSizeSourceType: RosterSizeSourceType.Question,
                                    rosterSizeQuestionId: rosterSizeQuestionId,
                                    variable: "second",
                                    children: new IComposite[]
                                    {
                                        Create.Entity.Roster(
                                            rosterId: roster3Id,
                                            rosterSizeSourceType: RosterSizeSourceType.Question,
                                            rosterSizeQuestionId: rosterSizeQuestionId,
                                            variable: "third")
                                    })
                            })
                    })
                );

                var result = new InvokeResults();

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: rosterSizeQuestionId, 
                    answeredOptions: new [] { Yes(10), Yes(20), Yes(40) }));

                var sidebarViewModel = SetUp.SidebarSectionViewModel(questionnaireDocument, interview);


                using (var eventContext = new EventContext())
                {
                    interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: rosterSizeQuestionId,
                        answeredOptions: new[] { Yes(10), Yes(40), Yes(30) }));

                    foreach (var section in sidebarViewModel.AllVisibleSections.OfType<SideBarSectionViewModel>())
                    {
                        section.Handle(eventContext.GetSingleEventOrNull<RosterInstancesAdded>());
                        section.Handle(eventContext.GetSingleEventOrNull<RosterInstancesRemoved>());
                    }
                }

                sidebarViewModel
                    .AllVisibleSections
                    .OfType<SideBarSectionViewModel>()
                    .First(x => x.SectionIdentity.Equals(Create.Identity(sectionId)))
                    .ToggleCommand
                    .Execute(null);

                var sectionIdentities = sidebarViewModel
                    .AllVisibleSections
                    .OfType<SideBarSectionViewModel>()
                    .Select(x => x.SectionIdentity)
                    .ToList();

                result.FirstSectionContainsDuplicates = sectionIdentities.Count != sectionIdentities.Distinct().Count();

                return result;
            });

        [Test]
        public void should_not_create_duplicates_links_in_sidebar () =>
            results.FirstSectionContainsDuplicates.Should().BeFalse();

        [OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool FirstSectionContainsDuplicates { get; set; }
        }
    }
}
