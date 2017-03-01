using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewSynchronizationDtoFactoryTests
{
    internal class when_build_from_interview_with_variables : InterviewSynchronizationDtoFactoryTestContext
    {
        Establish context = () =>
        {
            interviewData = CreateInterviewData();

            questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.Roster(rosterId: rosterId, variable: "fix",
                    children: new[] {Create.Entity.Variable(id: variableId, variableName: "txt")})
            });

            var fixedRosterScope = new ValueVector<Guid>(new[] {rosterId});

            AddInterviewLevel(interviewData, scopeVector: fixedRosterScope,
                rosterVector: new decimal[] {1}, variables: new Dictionary<Guid, object>() {{variableId, 1}});

            AddInterviewLevel(interviewData, scopeVector: fixedRosterScope,
                rosterVector: new decimal[] {2}, variables: new Dictionary<Guid, object>() {{variableId, 2}});

            AddInterviewLevel(interviewData, scopeVector: fixedRosterScope,
                rosterVector: new decimal[] {3}, variables: new Dictionary<Guid, object>() {{variableId, null}},
                disableVariables: new HashSet<Guid>() {variableId});

            interviewSynchronizationDtoFactory = CreateInterviewSynchronizationDtoFactory(questionnaireDocument);
        };

        Because of = () =>
            result = interviewSynchronizationDtoFactory.BuildFrom(interviewData, "comment", null, null);

        It should_create_sync_package_with_one_disabled_variable = () =>
            result.DisabledVariables.Contains(new InterviewItemId(variableId, new decimal[] { 3 })).ShouldBeTrue();

        It should_create_sync_package_with_3_value_variables = () =>
           result.Variables.Count.ShouldEqual(3);

        private static InterviewSynchronizationDtoFactory interviewSynchronizationDtoFactory;
        private static InterviewData interviewData;
        private static InterviewSynchronizationDto result;
        private static QuestionnaireDocument questionnaireDocument;

        private static Guid rosterId = Guid.Parse("21111111111111111111111111111111");
        private static Guid variableId = Guid.Parse("11111111111111111111111111111111");
    }
}