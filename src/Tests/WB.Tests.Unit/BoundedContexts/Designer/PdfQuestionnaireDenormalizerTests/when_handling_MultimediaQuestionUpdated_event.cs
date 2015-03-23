using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_MultimediaQuestionUpdated_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            var pdfGroupView = CreateGroup(Guid.Parse(parentGroupId));
            pdfGroupView.AddChild(CreateQuestion(Guid.Parse(questionId)));
            pdfQuestionnaireDocument = CreatePdfQuestionnaire(pdfGroupView);

            var documentStorage =
                Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(
                    writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(Create.MultimediaQuestionUpdatedEvent(questionId: questionId,
                questionTitle: questionTitle, questionVariable: questionVariable,
                questionConditionExpression: questionEnablementCondition));

        It should_update_not_null_question = () =>
            GetQuestion().ShouldNotBeNull();

        It should_update_question_of_Multimedia_type = () =>
            GetQuestion().QuestionType.ShouldEqual(QuestionType.Multimedia);

        It should_update_question_title = () =>
            GetQuestion().Title.ShouldEqual(questionTitle);

        It should_update_question_variable_name = () =>
            GetQuestion().VariableName.ShouldEqual(questionVariable);

        private static PdfQuestionView GetQuestion()
        {
            return pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(Guid.Parse(questionId));
        }

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static string questionId = "11111111111111111111111111111111";
        private static string parentGroupId = "22222222222222222222222222222222";
        private static string questionTitle = "someTitle";
        private static string questionVariable = "var";
        private static string questionEnablementCondition = "some condition";
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}
