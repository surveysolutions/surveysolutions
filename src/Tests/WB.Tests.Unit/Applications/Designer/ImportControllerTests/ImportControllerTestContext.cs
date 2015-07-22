using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Applications.Designer.ImportControllerTests
{
    [Subject(typeof(ImportController))]
    internal class ImportControllerTestContext
    {
        protected static ImportController CreateImportController(
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory = null,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory = null,
            IMembershipUserService membershipUserService = null,
            IExpressionsEngineVersionService expressionsEngineVersionService = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IExpressionProcessorGenerator expressionProcessorGenerator=null)
        {
            return new ImportController(Mock.Of<IStringCompressor>(),
                membershipUserService ?? Mock.Of<IMembershipUserService>(),
                Mock.Of<IQuestionnaireListViewFactory>(),
                questionnaireViewFactory ?? Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(),
                sharedPersonsViewFactory ??
                Mock.Of<IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons>>(),
                questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                expressionProcessorGenerator??Mock.Of<IExpressionProcessorGenerator>(),
                Mock.Of<IQuestionnaireHelper>(),
                expressionsEngineVersionService ?? Mock.Of<IExpressionsEngineVersionService>(),
                Mock.Of<IJsonUtils>());
        }
    }
}
