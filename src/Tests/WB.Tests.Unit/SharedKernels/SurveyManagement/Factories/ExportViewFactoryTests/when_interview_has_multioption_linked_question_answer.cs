using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_interview_has_multioption_linked_question_answer : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            multyOptionLinkedQuestionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            linkedSourceQuestionId = Guid.NewGuid();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Roster(rosterId: Guid.NewGuid(), 
                              variable: "row", 
                              fixedTitles: new[] { "1", "2" },
                              children: new[] {
                                  Create.Entity.TextQuestion(questionId: linkedSourceQuestionId, variable: "varTxt")
                              }),
                Create.Entity.MultyOptionsQuestion(id: multyOptionLinkedQuestionId,
                    variable: "mult",
                    linkedToQuestionId: linkedSourceQuestionId));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(new PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);

            questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire.PublicKey, 1);

            interview = Create.Entity.InterviewData(Create.Entity.InterviewQuestion(multyOptionLinkedQuestionId, new[] { 2 }));
        };

        Because of = () => result = exportViewFactory.CreateInterviewDataExportView(questionnaaireExportStructure, interview);

        It should_put_answers_to_export_in_appropriate_order = () =>
        {
            InterviewDataExportLevelView first = result.Levels.First();
            var exportedQuestion = first.Records.First().GetPlainAnswers().First();
            exportedQuestion.Length.ShouldEqual(2);
            exportedQuestion.ShouldEqual(new[] {"2", ExportFormatSettings.MissingStringQuestionValue});
        };

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static InterviewDataExportView result;
        static InterviewData interview;
        static Guid multyOptionLinkedQuestionId;
        static Guid linkedSourceQuestionId;
    }
}