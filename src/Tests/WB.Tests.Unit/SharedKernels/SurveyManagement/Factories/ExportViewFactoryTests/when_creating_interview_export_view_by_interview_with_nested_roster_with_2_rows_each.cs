using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_interview_export_view_by_interview_with_nested_roster_with_2_rows_each : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionInsideRosterGroupId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("11111111111111111111111111111111");
            nestedRosterId = Guid.Parse("13333333333333333333333333333333");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(rosterId: rosterId, obsoleteFixedTitles: new[] {"t1", "t2"},
                    children: new IComposite[]
                    {
                        Create.Entity.FixedRoster(rosterId: nestedRosterId, obsoleteFixedTitles: new[] {"t1", "t2"},
                            children: new IComposite[]
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = questionInsideRosterGroupId,
                                    QuestionType = QuestionType.Numeric,
                                    StataExportCaption = "q1"
                                }
                            })
                    }));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(new PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
        };

        Because of = () =>
               result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaire.PublicKey, 1)),
                CreateInterviewDataWith2PropagatedLevels());

        It should_records_count_equals_4 = () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records.Length.ShouldEqual(4);

        It should_first_record_id_equals_0 = () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records[0].RecordId.ShouldEqual("0");

        It should_second_record_id_equals_1 = () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records[1].RecordId.ShouldEqual("1");

        It should_third_record_id_equals_1 = () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records[2].RecordId.ShouldEqual("0");

        It should_fourth_record_id_equals_1 = () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records[3].RecordId.ShouldEqual("1");

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            for (int i = 0; i < levelCount; i++)
            {
                var vector = new decimal[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { rosterId }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);
                for (int j = 0; j < levelCount; j++)
                {
                    var nestedVector = new decimal[] { i, j };
                    var nestedLevel = new InterviewLevel(new ValueVector<Guid> { rosterId, nestedRosterId }, null, nestedVector);
                    interview.Levels.Add(string.Join(",", nestedVector), nestedLevel);

                    if (!nestedLevel.QuestionsSearchCache.ContainsKey(questionInsideRosterGroupId))
                        nestedLevel.QuestionsSearchCache.Add(questionInsideRosterGroupId, new InterviewQuestion(questionInsideRosterGroupId));

                    var question = nestedLevel.QuestionsSearchCache[questionInsideRosterGroupId];

                    question.Answer = "some answer";
                }

            }
            return interview;
        }

        private static InterviewDataExportView result;
        private static Guid nestedRosterId;
        private static Guid rosterId;
        private static Guid questionInsideRosterGroupId;
        private static int levelCount=2;
        private static QuestionnaireDocument questionnaire;
        private static ExportViewFactory exportViewFactory;
    }
}
