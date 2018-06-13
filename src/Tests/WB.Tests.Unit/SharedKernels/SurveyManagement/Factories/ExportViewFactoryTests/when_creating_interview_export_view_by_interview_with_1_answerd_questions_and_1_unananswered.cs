using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_interview_export_view_by_interview_with_1_answerd_questions_and_1_unananswered:  ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            answeredQuestionId = Guid.Parse("10000000000000000000000000000000");
            unansweredQuestionId = Guid.Parse("11111111111111111111111111111111");
            variableNameAndQuestionId = new Dictionary<string, Guid>
            {
                {"q1", answeredQuestionId},
                {"q2", unansweredQuestionId}
            };
            questionnaireDocument = CreateQuestionnaireDocument(variableNameAndQuestionId);

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(questionnaire);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument.PublicKey, 1),
                CreateInterviewWithAnswers(variableNameAndQuestionId.Values.Take(1)), questionnaire);

        [NUnit.Framework.Test] public void should_records_count_equals_1 () =>
            result.Levels[0].Records.Length.Should().Be(1);

        [NUnit.Framework.Test] public void should__first_record_have_1_answers () =>
            result.Levels[0].Records[0].GetPlainAnswers().Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_first_parent_ids_be_empty () =>
           result.Levels[0].Records[0].ParentRecordIds.Should().BeEmpty();

        [NUnit.Framework.Test] public void should_answered_question_be_not_empty () =>
           result.Levels[0].Records[0].GetPlainAnswers().First().Length.Should().NotBe(0);

        private static ExportViewFactory exportViewFactory;
        private static InterviewDataExportView result;
        private static Dictionary<string, Guid> variableNameAndQuestionId;
        private static Guid answeredQuestionId;
        private static Guid unansweredQuestionId;
        private static QuestionnaireDocument questionnaireDocument;
        private static IQuestionnaire questionnaire;
    }
}
