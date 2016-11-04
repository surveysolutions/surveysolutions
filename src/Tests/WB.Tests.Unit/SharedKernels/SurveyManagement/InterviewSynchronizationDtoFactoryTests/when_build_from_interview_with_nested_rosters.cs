using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewSynchronizationDtoFactoryTests
{
    internal class when_build_from_interview_with_nested_rosters : InterviewSynchronizationDtoFactoryTestContext
    {
        Establish context = () =>
        {
            topRosterId = Guid.Parse("10000000000000000000000000000000");
            nestedRosterId = Guid.Parse("11111111111111111111111111111111");
            rosterSizeQuestionId = Guid.Parse("21111111111111111111111111111111");
            interviewData = CreateInterviewData();
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("trigger")
                {
                    QuestionType = QuestionType.Numeric,
                    PublicKey = rosterSizeQuestionId
                },
                new Group("top roster")
                {
                    PublicKey = topRosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group("nested roster")
                        {
                            PublicKey = nestedRosterId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.Question,
                            RosterSizeQuestionId = rosterSizeQuestionId
                        }
                    }.ToReadOnlyCollection()
                });

            interviewSynchronizationDtoFactory = CreateInterviewSynchronizationDtoFactory(questionnaireDocument);

            AddInterviewLevel(interviewData, new ValueVector<Guid>(new[] { rosterSizeQuestionId }), new decimal[] { 0 },
                new Dictionary<Guid, object>());
            AddInterviewLevel(interviewData, new ValueVector<Guid>(new[] { rosterSizeQuestionId }), new decimal[] { 1 },
              new Dictionary<Guid, object>());
            AddInterviewLevel(interviewData, new ValueVector<Guid>(new[] { rosterSizeQuestionId, rosterSizeQuestionId }), new decimal[] { 0, 0 },
                new Dictionary<Guid, object>());
            AddInterviewLevel(interviewData, new ValueVector<Guid>(new[] { rosterSizeQuestionId, rosterSizeQuestionId }), new decimal[] { 0, 1 },
                new Dictionary<Guid, object>());

            AddInterviewLevel(interviewData, new ValueVector<Guid>(new[] { rosterSizeQuestionId, rosterSizeQuestionId }), new decimal[] { 1, 0 },
               new Dictionary<Guid, object>());
            AddInterviewLevel(interviewData, new ValueVector<Guid>(new[] { rosterSizeQuestionId, rosterSizeQuestionId }), new decimal[] { 1, 1 },
                new Dictionary<Guid, object>());
        };

        Because of = () =>
            result = interviewSynchronizationDtoFactory.BuildFrom(interviewData, null, null, null);

        It should_result_has_3_roster_instances = () =>
            result.RosterGroupInstances.Count().ShouldEqual(3);

        It should_result_has_2_roster_instance_of_top_roster = () =>
           result.RosterGroupInstances[new InterviewItemId(topRosterId)].Count().ShouldEqual(2);

        It should_result_has_2_roster_instance_of_nested_roster_for_first_row = () =>
          result.RosterGroupInstances[new InterviewItemId(nestedRosterId, new decimal[]{0})].Count().ShouldEqual(2);

        It should_result_has_2_roster_instance_of_nested_roster_for_second_row = () =>
          result.RosterGroupInstances[new InterviewItemId(nestedRosterId, new decimal[] { 1 })].Count().ShouldEqual(2);

        private static InterviewSynchronizationDtoFactory interviewSynchronizationDtoFactory;
        private static InterviewData interviewData;
        private static InterviewSynchronizationDto result;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid topRosterId;
        private static Guid nestedRosterId;
        private static Guid rosterSizeQuestionId;
    }
}
