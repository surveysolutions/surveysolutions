using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.BoundedContexts.Designer.Tests.CodeGenerationTests
{
    
    class CodeGenerationTestsContext
    {
        public static QuestionnaireDocument CreateQuestionnaireForGeneration(Guid responsibleId, Guid? questionnaireId = null)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId ?? Guid.NewGuid() };

            Guid chapterId = Guid.Parse("23232323232323232323232323232323");
            Guid questionId = Guid.Parse("23232323232323232323232323232311");

            questionnaireDocument.AddChapter(chapterId);

            questionnaireDocument.Add(new NumericQuestion()
            {
                PublicKey = questionId,
                StataExportCaption = "persons_n",
                IsInteger = true

            }, chapterId, null);

            var rosterId = Guid.Parse("23232323232323232323232323232322");
            questionnaireDocument.Add(new Group()
            {
                PublicKey = rosterId,
                IsRoster = true,
                RosterSizeQuestionId = questionId

            }, chapterId, null);

            Guid pets_questionId = Guid.Parse("23232323232323232323232323232317");
            questionnaireDocument.Add(new NumericQuestion()
            {
                PublicKey = pets_questionId,
                StataExportCaption = "pets_n",
                IsInteger = true

            }, rosterId, null);

            var groupId = Guid.Parse("12345678912345678912345678912345");
            questionnaireDocument.Add(new Group()
            {
                PublicKey = groupId,
                IsRoster = false,
                ConditionExpression = "pets_n > 0"

            }, rosterId, null);

            questionnaireDocument.Add(new TextQuestion()
            {
                PublicKey = Guid.Parse("12345678912345678912345678912340"),
                StataExportCaption = "pets_text",
                ConditionExpression = "pets_n > 0",
                ValidationExpression = "pets_n == 0"

            }, groupId, null);

            questionnaireDocument.Add(new Group()
            {
                PublicKey = Guid.Parse("23232323232323232323232323232324"),
                IsRoster = true,
                RosterSizeQuestionId = pets_questionId,
                ConditionExpression = "pets_text.Length > 0"

            }, rosterId, null);

            return questionnaireDocument;
        }

    }
}
