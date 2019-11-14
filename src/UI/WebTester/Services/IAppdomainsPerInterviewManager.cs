﻿using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.UI.WebTester.Services
{
    public interface IAppdomainsPerInterviewManager
    {
        void SetupForInterview(Guid interviewId, 
            QuestionnaireDocument questionnaireDocument,
            List<TranslationDto> translations,
            string supportingAssembly);

        void TearDown(Guid interviewId);

        List<CommittedEvent> Execute(ICommand command);
        
        int? GetLastEventSequence(Guid interviewId);

        /// <summary>
        /// Deletes existing data inside interview but keeps appdomain ready to be reused for new interview
        /// </summary>
        void Flush(Guid interviewId);
    }
}
