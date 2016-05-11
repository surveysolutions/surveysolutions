using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewSynchronizationDtoFactoryTests
{
    internal class when_build_from_interview_with_variables : InterviewSynchronizationDtoFactoryTestContext
    {
        Establish context = () =>
        {
            interviewData = CreateInterviewData();

            questionnaireDocument = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Roster(rosterId: rosterId, variable: "fix",
                    children: new[] {Create.Variable(id: variableId, variableName: "txt")})
            });

            interviewVariables = Create.InterviewVariables();
            interviewVariables.VariableValues.Add(new InterviewItemId(variableId, new decimal[] {1}), 1);
            interviewVariables.VariableValues.Add(new InterviewItemId(variableId, new decimal[] {2}), 2);
            interviewVariables.VariableValues.Add(new InterviewItemId(variableId, new decimal[] {3}), null);
            interviewVariables.DisabledVariables.Add(new InterviewItemId(variableId, new decimal[] {3}));
            var variableStorage =
                Mock.Of<IReadSideKeyValueStorage<InterviewVariables>>(
                    _ => _.GetById(Moq.It.IsAny<string>()) == interviewVariables);

            interviewSynchronizationDtoFactory = CreateInterviewSynchronizationDtoFactory(questionnaireDocument,
                variableStorage);
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
        private static InterviewVariables interviewVariables;
    }
}