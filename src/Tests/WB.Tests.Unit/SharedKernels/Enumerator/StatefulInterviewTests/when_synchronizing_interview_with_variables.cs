using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_synchronizing_interview_with_variables : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            Guid variableId = Guid.Parse("00000000000000000000000000000001");
            RosterVector rosterVector = Create.Entity.RosterVector(1m, 0m);
            variableIdentity = new Identity(variableId, rosterVector);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
            {
                Create.Entity.FixedRoster(fixedTitles: new[] {Create.Entity.FixedTitle(1)}, children: new[]
                {
                    Create.Entity.FixedRoster(fixedTitles: new[] {Create.Entity.FixedTitle(0)}, children: new[]
                    {
                        Create.Entity.Variable(variableIdentity.Id, VariableType.String)
                    })
                })
            });

            interview = Setup.StatefulInterview(questionnaire);

            synchronizationDto = Create.Entity.InterviewSynchronizationDto(
                variables: new Dictionary<InterviewItemId, object>() {{ Create.Entity.InterviewItemId(variableIdentity.Id, variableIdentity.RosterVector), "test"}});
        };

        Because of = () => interview.RestoreInterviewStateFromSyncPackage(userId, synchronizationDto);

        It should_return_variable_set_value = () => interview.GetVariableValueByOrDeeperRosterLevel(variableIdentity.Id, variableIdentity.RosterVector).ShouldEqual("test");

        static InterviewSynchronizationDto synchronizationDto;
        static StatefulInterview interview;
        static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
        static Identity variableIdentity;
    }
}