﻿using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryViewFactoryTests
{
    internal class InterviewSummaryTestsContext
    {
        public static QuestionnaireDocument CreateQuestionnaireWithTwoPrefieldIncludingOneGPS(Guid questionnaireId)
        {
            if (questionnaireId == null) throw new ArgumentNullException(nameof(questionnaireId));
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            Guid questionId = Guid.Parse("23232323232323232323232323232311");
            Guid question1Id = Guid.Parse("23232323232323232323232323232321");
            

            questionnaireDocument.Add(new GpsCoordinateQuestion()
            {
                QuestionType = QuestionType.GpsCoordinates,
                PublicKey = questionId,
                StataExportCaption = "gps1",
                Featured = true
                
            }, questionnaireId, null);

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = question1Id,
                StataExportCaption = "q2",
                IsInteger = true,
                Featured = true

            }, questionnaireId, null);

            return questionnaireDocument;

        }
    }
}
