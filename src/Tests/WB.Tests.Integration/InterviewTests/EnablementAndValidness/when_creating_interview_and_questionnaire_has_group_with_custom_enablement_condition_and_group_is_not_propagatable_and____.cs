using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_creating_interview_and_questionnaire_has_group_with_custom_enablement_condition_and_group_is_not_propagatable_and_has_no_propagatable_ancestor_groups : InterviewTestsContext
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
                var groupId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, Abc.Create.Entity.Group(groupId, "Group X", null, "1 > 2", false, null));

                using (var eventContext = new EventContext())
                {
                    SetupInterviewWithExpressionStorage(questionnaireDocument);
                    return new InvokeResults()
                    {
                        GroupDisabled =
                            HasEvent<GroupsDisabled>(eventContext.Events,
                                where => where.Groups.Any(group => group.Id == groupId && group.RosterVector.Length == 0))
                    };
                }
            });

        It should_raise_GroupsDisabled_event_with_id_of_question_with_custom_enablement_condition_and_zero_dimensional_propagation_vector = () =>
            results.GroupDisabled.ShouldBeTrue();

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
            public bool GroupDisabled { get; set; }
        }
    }
}