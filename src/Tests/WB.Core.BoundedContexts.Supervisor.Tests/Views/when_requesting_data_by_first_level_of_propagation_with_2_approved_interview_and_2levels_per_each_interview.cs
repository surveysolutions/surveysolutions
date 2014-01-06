using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views
{
    class when_requesting_data_by_first_level_of_propagation_with_2_approved_interview_and_2levels_per_each_interview : InterviewDataExportFactoryTestContext
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
            interviewDataExportFactory = CreateInterviewDataExportFactoryForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewDataWith2PropagatedLevels, 2);
        };

        Because of = () =>
            result = interviewDataExportFactory.Load(new InterviewDataExportInputModel(questionnarie.PublicKey, 1));

        It should_records_count_equals_4 = () =>
           GetLevel(result,propagationScopeKey).Records.Length.ShouldEqual(4);

        It should_first_record_id_equals_0 = () =>
           GetLevel(result, propagationScopeKey).Records[0].RecordId.ShouldEqual(0);

        It should_second_record_id_equals_1 = () =>
           GetLevel(result, propagationScopeKey).Records[1].RecordId.ShouldEqual(1);

        It should_third_record_id_equals_0 = () =>
           GetLevel(result, propagationScopeKey).Records[2].RecordId.ShouldEqual(0);

        It should_forth_record_id_equals_1 = () =>
           GetLevel(result, propagationScopeKey).Records[3].RecordId.ShouldEqual(1);

        It should_header_column_count_be_equal_2 = () =>
           GetLevel(result, propagationScopeKey).Header.HeaderItems.Count().ShouldEqual(2);

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

        private static InterviewExportedData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewExportedData interview = CreateInterviewData();
            interview.InterviewDataByLevels = new InterviewExportedLevel[levelCount];
            for (int i = 0; i < levelCount; i++)
            {
                var vector = new decimal[1] { i };
                var exportedQuestionsByLevel = new ExportedQuestion[variableNameAndQuestionId.Count];

                var j = 0;
                foreach (var question in variableNameAndQuestionId)
                {
                    exportedQuestionsByLevel[j] = CreateExportedQuestion(question.Value, "some answer");
                    j++;
                }
                interview.InterviewDataByLevels[i] = new InterviewExportedLevel(propagationScopeKey, vector, exportedQuestionsByLevel);
            }
            return interview;
        }

        private static InterviewDataExportFactory interviewDataExportFactory;
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
