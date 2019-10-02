﻿using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Integration.WebInterviewTests;
using WB.Tests.Web;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Integration.WebTester.Services
{
    [TestOf(typeof(AppdomainsPerInterviewManager))]
    public class AppdomainsPerInterviewManagerTestsBase : InterviewTestsContext
    {
        protected AppdomainsPerInterviewManager Manager;

        protected AppdomainsPerInterviewManager CreateManager()
        {
            var bin = Path.GetDirectoryName(typeof(when_configured).Assembly.Location);
            return new AppdomainsPerInterviewManager(bin, Mock.Of<ILogger>());
        }

        protected void SetupAppDomainInterview(AppdomainsPerInterviewManager manager, 
            Guid interviewId, 
            QuestionnaireDocument questionnaire, List<TranslationDto> translations = null)
        {
            questionnaire.IsUsingExpressionStorage = true;
            var readOnlyQuestionnaireDocument = questionnaire.AsReadOnly();
            var playOrderProvider = Abc.Create.Service.ExpressionsPlayOrderProvider();
            questionnaire.ExpressionsPlayOrder = playOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            questionnaire.DependencyGraph = playOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            questionnaire.ValidationDependencyGraph = playOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument);

            var supportingAssembly = IntegrationCreate.CompileAssembly(questionnaire);
            manager.SetupForInterview(interviewId, questionnaire, translations, supportingAssembly);
            manager.Execute(Abc.Create.Command.CreateInterview(interviewId: interviewId,
                userId: interviewId,
                questionnaireIdentity: Abc.Create.Entity.QuestionnaireIdentity(questionnaire.PublicKey, 1)));
        }
    }
}
