using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    class when_creating_interview_export_view_by_interview_with_roster_with_2_rows : ExportViewFactoryTestsContext
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
            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
            result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(questionnarie, 1),
                CreateInterviewDataWith2PropagatedLevels());

        It should_records_count_equals_4 = () =>
           GetLevel(result, new[] { propagationScopeKey }).Records.Length.ShouldEqual(2);

        It should_first_record_id_equals_0 = () =>
           GetLevel(result, new[] { propagationScopeKey }).Records[0].RecordId.ShouldEqual("0");

        It should_second_record_id_equals_1 = () =>
           GetLevel(result, new[] { propagationScopeKey }).Records[1].RecordId.ShouldEqual("1");

        It should_first_rosters_record_parent_ids_contains_only_main_level_record_id = () =>
          GetLevel(result, new[] { propagationScopeKey }).Records[0].ParentRecordIds.ShouldEqual(new string[] { GetLevel(result, new Guid[0]).Records[0].RecordId });

        It should_second_rosters_record_parent_ids_contains_only_main_level_record_id = () =>
           GetLevel(result, new[] { propagationScopeKey }).Records[1].ParentRecordIds.ShouldEqual(new string[] { GetLevel(result, new Guid[0]).Records[0].RecordId});

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

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            for (int i = 0; i < levelCount; i++)
            {
                var vector = new decimal[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { propagationScopeKey }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);

                foreach (var questionId in variableNameAndQuestionId)
                {
                    if (!newLevel.QuestionsSearchCahche.ContainsKey(questionId.Value))
                        newLevel.QuestionsSearchCahche.Add(questionId.Value, new InterviewQuestion(questionId.Value));

                    var question = newLevel.QuestionsSearchCahche[questionId.Value];

                    question.Answer = "some answer";
                }
            }
            return interview;
        }

        private static InterviewDataExportView result;
        private static Dictionary<string, Guid> variableNameAndQuestionId;
        private static Guid propagatedGroup;
        private static Guid propagationScopeKey;
        private static Guid secondQuestionId;
        private static Guid firstQuestionId;
        private static int levelCount;
        private static QuestionnaireDocument questionnarie;
        private static ExportViewFactory exportViewFactory;
    }
}
