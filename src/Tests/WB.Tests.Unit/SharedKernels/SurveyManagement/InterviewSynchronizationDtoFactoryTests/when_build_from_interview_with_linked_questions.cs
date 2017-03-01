using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewSynchronizationDtoFactoryTests
{
    internal class when_build_from_interview_with_linked_questions : InterviewSynchronizationDtoFactoryTestContext
    {
        Establish context = () =>
        {
            interviewData = CreateInterviewData();

            AddInterviewLevel(interviewData, new ValueVector<Guid>(new[] {rosterId}), new decimal[] {1},
                new Dictionary<Guid, object>() { { txtQuestionId ,"aaa"} });

            AddInterviewLevel(interviewData, new ValueVector<Guid>(new[] {rosterId}), new decimal[] {2},
                new Dictionary<Guid, object>() {{txtQuestionId, "bbb"}});

            AddInterviewLevel(interviewData, new ValueVector<Guid>(new[] { rosterId }), new decimal[] { 3 },
              new Dictionary<Guid, object>());

            questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.Roster(rosterId: rosterId, variable: "fix",
                    children: new[] {Create.Entity.TextQuestion(questionId: txtQuestionId, variable: "txt")}),
                Create.Entity.SingleQuestion(id:linkOnRosterQuestionId , variable: "link_on_roster", linkedToRosterId: rosterId),
                Create.Entity.SingleQuestion(id: singleLinkQuestionId, variable: "link_on_question", linkedToQuestionId: txtQuestionId),
            });

            interviewSynchronizationDtoFactory = CreateInterviewSynchronizationDtoFactory(questionnaireDocument);
        };

        Because of = () =>
            result = interviewSynchronizationDtoFactory.BuildFrom(interviewData, "comment", null, null);

        It should_create_linked_options_for_question_linked_on_roster = () =>
            result.LinkedQuestionOptions[new InterviewItemId(linkOnRosterQuestionId, new decimal[0])].ShouldEqual(new [] {new RosterVector(new decimal[] {1}), new RosterVector(new decimal[]{2 }), new RosterVector(new decimal[] { 3}) });

        It should_create_linked_options_for_question_linked_on_question = () =>
            result.LinkedQuestionOptions[new InterviewItemId(singleLinkQuestionId, new decimal[0])].ShouldEqual(new[] { new RosterVector(new decimal[] { 1 }), new RosterVector(new decimal[] { 2 }) });

        private static InterviewSynchronizationDtoFactory interviewSynchronizationDtoFactory;
        private static InterviewData interviewData;
        private static InterviewSynchronizationDto result;
        private static QuestionnaireDocument questionnaireDocument;

        private static Guid rosterId= Guid.Parse("21111111111111111111111111111111");
        private static Guid txtQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid singleLinkQuestionId = Guid.Parse("31111111111111111111111111111111");
        private static Guid linkOnRosterQuestionId = Guid.Parse("41111111111111111111111111111111");
    }
}