using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_synchronizing_interview_with_variables : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid variableId = Guid.Parse("00000000000000000000000000000001");
            RosterVector rosterVector = Create.Other.RosterVector(1m, 0m);

            IPlainQuestionnaireRepository questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Create.Other.QuestionnaireIdentity(questionnaireId, 1), Create.Other.QuestionnaireDocument(id: questionnaireId));

            interview = Create.Other.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            variableIdentity = new Identity(variableId, rosterVector);
            synchronizationDto = Create.Other.InterviewSynchronizationDto(questionnaireId: questionnaireId,
                variables: new Dictionary<InterviewItemId, object>() {{ Create.Other.InterviewItemId(variableIdentity.Id, variableIdentity.RosterVector), "test"}},
                disabledVariables:new HashSet<InterviewItemId>() {Create.Other.InterviewItemId(Guid.NewGuid(), RosterVector.Empty) });
        };

        Because of = () => interview.RestoreInterviewStateFromSyncPackage(userId, synchronizationDto);

        It should_return_variable_set_value = () => interview.GetVariableValueByOrDeeperRosterLevel(variableIdentity.Id, variableIdentity.RosterVector).ShouldEqual("test");

        static InterviewSynchronizationDto synchronizationDto;
        static StatefulInterview interview;
        static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
        static Identity variableIdentity;
    }
}