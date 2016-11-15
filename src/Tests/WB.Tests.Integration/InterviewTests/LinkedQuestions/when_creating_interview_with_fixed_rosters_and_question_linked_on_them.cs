using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_creating_interview_with_fixed_rosters_and_question_linked_on_them : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();
               
                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
                {
                    Create.SingleQuestion(id: linkedToQuestionId, linkedToRosterId: roster2Id, variable: "linked"),
                    Create.Roster(id: roster1Id, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, variable: "r1",
                        fixedTitles: new[] {Create.FixedTitle(1), Create.FixedTitle(2)},
                        children: new IComposite[]
                        {
                            Create.Roster(id: roster2Id, rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                                variable: "r2", enablementCondition: "@rowcode == 1",
                                fixedTitles: new[] {Create.FixedTitle(1), Create.FixedTitle(2)})
                        }),
                });

                var interview = SetupStatefullInterview(questionnaireDocument);

                return new InvokeResults
                {
                    OptionsCountForLinkedToRosterQuestion = interview.GetLinkedSingleOptionQuestion(Identity.Create(linkedToQuestionId, RosterVector.Empty)).Options.Count
                };
            });

        It should_return_2_options_for_linked_question = () =>
            results.OptionsCountForLinkedToRosterQuestion.ShouldEqual(2);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid roster2Id = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid roster1Id = Guid.Parse("77777777777777777777777777777777");
        private static readonly Guid linkedToQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public int OptionsCountForLinkedToRosterQuestion { get; set; }
        }
    }
}