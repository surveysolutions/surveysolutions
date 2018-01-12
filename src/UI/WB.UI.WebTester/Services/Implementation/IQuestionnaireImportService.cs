using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.WebTester.Controllers;

namespace WB.UI.WebTester.Services.Implementation
{
    public interface IQuestionnaireImportService
    {
        void ImportQuestionnaire(Guid interviewId, QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireDocument questionnaireDocument,
            string supportingAssembly,
            TranslationDto[] translations, List<QuestionnaireAttachment> attachments);
    }
}