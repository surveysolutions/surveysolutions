using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_creating_interview_with_prefilled_numeric_question_and_it_is_roster_size_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        private Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(id: numericQuestionId, variable: "trigger"),
                    Create.Roster(id: roster1Id, rosterSizeSourceType: RosterSizeSourceType.Question, 
                        variable: "r1", rosterSizeQuestionId: numericQuestionId)
                });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    var interview = SetupStatefullInterview(questionnaireDocument, 
                        answersOnPrefilledQuestions: new Dictionary<Guid, object>
                        {
                            { numericQuestionId, 3 }
                        });

                    result.AnyNumericRosterWasCreated =
                        eventContext.AnyEvent<RosterInstancesAdded>(x => x.Instances.Any(r => r.GroupId == roster1Id));
                }

                return result;
            });

        It should_fire_event_that_resters_were_added = () =>
            results.AnyNumericRosterWasCreated.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid roster1Id = Guid.Parse("77777777777777777777777777777777");
        private static readonly Guid numericQuestionId = Guid.Parse("22222222222222222222222222222222");

        [Serializable]
        internal class InvokeResults
        {
            public bool AnyNumericRosterWasCreated { get; set; }
        }
    }
}