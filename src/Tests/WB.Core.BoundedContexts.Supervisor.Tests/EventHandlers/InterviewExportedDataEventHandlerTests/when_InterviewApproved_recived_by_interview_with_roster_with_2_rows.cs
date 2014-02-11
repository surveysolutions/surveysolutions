using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.InterviewExportedDataEventHandlerTests
{
    class when_InterviewApproved_recived_by_interview_with_roster_with_2_rows : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            firstQuestionId = Guid.Parse("12222222222222222222222222222222");
            secondQuestionId = Guid.Parse("11111111111111111111111111111111");
            propagatedGroup = Guid.Parse("13333333333333333333333333333333");

            levelCount = 2;
            
            variableNameAndQuestionId = new Dictionary<string, Guid>
            {
                { "q1", firstQuestionId },
                { "q2", secondQuestionId }
            };

            propagationScopeKey = Guid.Parse("10000000000000000000000000000000");
            questionnarie = CreateQuestionnaireDocumentWith1PropagationLevel();
            interviewExportedDataEventHandler = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewDataWith2PropagatedLevels, r => result = r);
        };

        Because of = () =>
            interviewExportedDataEventHandler.Handle(CreatePublishableEvent());

        It should_records_count_equals_4 = () =>
           GetLevel(result,propagationScopeKey).Records.Length.ShouldEqual(2);

        It should_first_record_id_equals_0 = () =>
           GetLevel(result, propagationScopeKey).Records[0].RecordId.ShouldEqual(0);

        It should_second_record_id_equals_1 = () =>
           GetLevel(result, propagationScopeKey).Records[1].RecordId.ShouldEqual(1);

        private static QuestionnaireDocument CreateQuestionnaireDocumentWith1PropagationLevel()
        {
            var initialDocument = CreateQuestionnaireDocument(new Dictionary<string, Guid>() { { "auto", propagationScopeKey } });
            var chapter = new Group();
            initialDocument.Children.Add(chapter);
            var roasterGroup = new Group() { PublicKey = propagatedGroup, IsRoster = true, RosterSizeQuestionId = propagationScopeKey };
            chapter.Children.Add(roasterGroup);

            foreach (var question in variableNameAndQuestionId)
            {
                roasterGroup.Children.Add(new NumericQuestion() { StataExportCaption = question.Key, PublicKey = question.Value });
            }

            return initialDocument;
        }

        private static InterviewDataExportLevelView GetLevel(InterviewDataExportView interviewDataExportView, Guid levelId)
        {
            return interviewDataExportView.Levels.FirstOrDefault(l => l.LevelId == levelId);
        }

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            for (int i = 0; i < levelCount; i++)
            {
                var vector = new decimal[1] { i };
                var newLevel = new InterviewLevel(propagationScopeKey, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);

                foreach (var questionId in variableNameAndQuestionId)
                {
                    var question = newLevel.GetOrCreateQuestion(questionId.Value);
                    question.Answer = "some answer";
                }
            }
            return interview;
        }

        private static EventHandler.InterviewExportedDataEventHandler interviewExportedDataEventHandler;
        private static InterviewDataExportView result;
        private static Dictionary<string, Guid> variableNameAndQuestionId;
        private static Guid propagatedGroup;
        private static Guid propagationScopeKey;
        private static Guid secondQuestionId;
        private static Guid firstQuestionId;
        private static int levelCount;
        private static QuestionnaireDocument questionnarie;
    }
}
