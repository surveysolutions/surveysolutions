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
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views
{
    [Subject(typeof(InterviewDataExportFactory))]
    class when_requesting_data_by_first_level_of_propagation_with_2_approved_interview_and_2levels_per_each_interview : InterviewDataExportFactoryTestContext
    {
        private Establish context = () =>
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

            interviewDataExportFactory = CreateInterviewDataExportFactoryForQuestionnarieCreatedByMethod(
                CreateQuestionnaireDocumentWith1PropagationLevel,
                CreateInterviewDataWith2PropagatedLevels, 2, CreateQuestionnairePropagationStructure);
        };

        private Because of = () =>
            result = interviewDataExportFactory.Load(new InterviewDataExportInputModel(Guid.NewGuid(), 1, propagationScopeKey));

        private It should_records_count_equals_4 = () =>
          result.Records.Length.ShouldEqual(4);

        private It should_first_record_id_equals_0 = () =>
            result.Records[0].RecordId.ShouldEqual(0);

        private It should_second_record_id_equals_1 = () =>
            result.Records[1].RecordId.ShouldEqual(1);

        private It should_third_record_id_equals_0 = () =>
            result.Records[2].RecordId.ShouldEqual(0);

        private It should_forth_record_id_equals_1 = () =>
            result.Records[3].RecordId.ShouldEqual(1);

        private It should_header_column_count_be_equal_2 = () =>
           result.Header.Count().ShouldEqual(2);

        private static QuestionnaireDocument CreateQuestionnaireDocumentWith1PropagationLevel()
        {
            var initialDocument = CreateQuestionnaireDocument(new Dictionary<string, Guid>() { { "auto", propagationScopeKey } });
            var chapter = new Group();
            initialDocument.Children.Add(chapter);
            var roasterGroup = new Group() { Propagated = Propagate.AutoPropagated, PublicKey = propagatedGroup };
            chapter.Children.Add(roasterGroup);

            foreach (var question in variableNameAndQuestionId)
            {
                roasterGroup.Children.Add(new NumericQuestion() { StataExportCaption = question.Key, PublicKey = question.Value });
            }

            return initialDocument;
        }

        private static QuestionnairePropagationStructure CreateQuestionnairePropagationStructure()
        {
            var propagationStructure = new QuestionnairePropagationStructure();
            propagationStructure.PropagationScopes.Add(propagationScopeKey, new HashSet<Guid>() { propagatedGroup });
            return propagationStructure;
        }

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview =  CreateInterviewData();
            for (int i = 0; i < levelCount; i++)
            {
                var vector = new int[1] { i };
                var interviewLevel = new InterviewLevel(propagationScopeKey, vector);

                foreach (var question in variableNameAndQuestionId)
                {
                    var interviewQuestion = interviewLevel.GetOrCreateQuestion(question.Value);
                    interviewQuestion.Answer = "some answer";
                }

                interview.Levels.Add(i.ToString(), interviewLevel);
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
    }
}
