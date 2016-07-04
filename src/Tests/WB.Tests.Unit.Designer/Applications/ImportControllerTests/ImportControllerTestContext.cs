﻿using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Api;
using WB.UI.Designer.Api.Headquarters;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    [Subject(typeof(ImportController))]
    internal class ImportControllerTestContext
    {
        protected static ImportV2Controller CreateImportController(
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            IQuestionnaireSharedPersonsFactory sharedPersonsViewFactory = null,
            IMembershipUserService membershipUserService = null,
            IDesignerEngineVersionService engineVersionService = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IExpressionProcessorGenerator expressionProcessorGenerator=null,
            ISerializer serializer=null,
            IStringCompressor zipUtils = null,
            IAttachmentService attachmentService = null)
        {
            return new ImportV2Controller(zipUtils ?? Mock.Of<IStringCompressor>(),
                membershipUserService ?? Mock.Of<IMembershipUserService>(),
                Mock.Of<IQuestionnaireListViewFactory>(),
                questionnaireViewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
                sharedPersonsViewFactory ??
                Mock.Of<IQuestionnaireSharedPersonsFactory>(),
                questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                expressionProcessorGenerator??Mock.Of<IExpressionProcessorGenerator>(),
                Mock.Of<IQuestionnaireHelper>(),
                engineVersionService ?? Mock.Of<IDesignerEngineVersionService>(),
                serializer??Mock.Of<ISerializer>(),
                attachmentService: attachmentService ?? Mock.Of<IAttachmentService>());
        }
    }
}
