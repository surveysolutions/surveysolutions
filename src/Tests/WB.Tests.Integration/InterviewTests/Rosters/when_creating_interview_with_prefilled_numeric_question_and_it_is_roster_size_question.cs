using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_creating_interview_with_prefilled_numeric_question_and_it_is_roster_size_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
                {
                    Abc.Create.Entity.NumericIntegerQuestion(id: numericQuestionId, variable: "trigger"),
                    Abc.Create.Entity.Roster(rosterId: roster1Id, rosterSizeSourceType: RosterSizeSourceType.Question, 
                        variable: "r1", rosterSizeQuestionId: numericQuestionId)
                });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    SetupStatefullInterview(questionnaireDocument, answers: new List<InterviewAnswer>
                    {
                        Create.Entity.InterviewAnswer(Create.Identity(numericQuestionId), NumericIntegerAnswer.FromInt(3))
                    });

                    result.AnyNumericRosterWasCreated = eventContext.AnyEvent<RosterInstancesAdded>(x => x.Instances.Any(r => r.GroupId == roster1Id));

                    result.CountOfAddedRosters = eventContext.GetSingleEvent<RosterInstancesAdded>().Instances.Length;
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_fire_event_that_resters_were_added () =>
            results.AnyNumericRosterWasCreated.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid roster1Id = Guid.Parse("77777777777777777777777777777777");
        private static readonly Guid numericQuestionId = Guid.Parse("22222222222222222222222222222222");

        [Serializable]
        internal class InvokeResults
        {
            public bool AnyNumericRosterWasCreated { get; set; }
            public int CountOfAddedRosters { get; set; }
        }
    }
}
