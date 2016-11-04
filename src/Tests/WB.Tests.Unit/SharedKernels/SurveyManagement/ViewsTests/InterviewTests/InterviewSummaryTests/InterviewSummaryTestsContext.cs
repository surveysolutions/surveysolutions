using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryTests
{
    internal class InterviewSummaryTestsContext
    {
        public static QuestionnaireDocument CreateQuestionnaireWithTwoPrefieldIncludingOneGPS(Guid questionnaireId)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            Guid questionId = Guid.Parse("23232323232323232323232323232311");
            Guid question1Id = Guid.Parse("23232323232323232323232323232321");
            

            questionnaireDocument.Add(new GpsCoordinateQuestion()
            {
                QuestionType = QuestionType.GpsCoordinates,
                PublicKey = questionId,
                StataExportCaption = "gps1",
                Featured = true
                
            }, questionnaireId);

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = question1Id,
                StataExportCaption = "q2",
                IsInteger = true,
                Featured = true

            }, questionnaireId);

            return questionnaireDocument;

        }
    }
}
