using System;
using System.Net.Http;
using System.Web.Http;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using System.Collections.Generic;
using Ncqrs.Commanding;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation;
using WB.UI.Designer.Api;

namespace WB.Tests.Unit
{
    internal static class Create
    {
        private class SyncAsyncExecutorStub : IAsyncExecutor
        {
            public void ExecuteAsync(Action action)
            {
                action.Invoke();
            }
        }

        public static PdfQuestionnaireView PdfQuestionnaireView(Guid? publicId = null)
        {
            return new PdfQuestionnaireView
            {
                PublicId = publicId ?? Guid.Parse("FEDCBA98765432100123456789ABCDEF"),
            };
        }

        public static PdfQuestionView PdfQuestionView()
        {
            return new PdfQuestionView();
        }

        public static PdfGroupView PdfGroupView()
        {
            return new PdfGroupView();
        }

        public static RoslynExpressionProcessor RoslynExpressionProcessor()
        {
            return new RoslynExpressionProcessor();
        }

        public static CreateInterviewControllerCommand CreateInterviewControllerCommand()
        {
            return new CreateInterviewControllerCommand()
            {
                AnswersToFeaturedQuestions = new List<UntypedQuestionAnswer>()
            };
        }

        public static NCalcToCSharpConverter NCalcToCSharpConverter()
        {
            return new NCalcToCSharpConverter();
        }

        public static NCalcToSharpController NCalcToSharpController(ICommandService commandService = null,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory = null)
        {
            return new NCalcToSharpController(
                Mock.Of<ILogger>(),
                commandService ?? Mock.Of<ICommandService>(),
                questionnaireInfoViewFactory ?? Mock.Of<IQuestionnaireInfoViewFactory>(),
                Create.SyncAsyncExecutor())
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration(),
            };
        }

        public static IAsyncExecutor SyncAsyncExecutor()
        {
            return new SyncAsyncExecutorStub();
        }

        public static NCalcToSharpController.OneQuestionnaireModel OneQuestionnaireModel(Guid id)
        {
            return new NCalcToSharpController.OneQuestionnaireModel { Id = id };
        }
    }
}