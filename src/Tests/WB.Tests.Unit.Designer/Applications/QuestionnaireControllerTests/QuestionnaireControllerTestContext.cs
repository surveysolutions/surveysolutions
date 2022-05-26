using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Unit.Designer.Services;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.ImportExport;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class QuestionnaireControllerTestContext
    {
        internal static QuestionnaireController CreateQuestionnaireController(
            ICommandService commandService = null,
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            ILogger<QuestionnaireController> logger = null,
            IQuestionnaireInfoFactory questionnaireInfoFactory = null,
            ICategoricalOptionsImportService categoricalOptionsImportService = null,
            DesignerDbContext dbContext = null)
        {
            var questionnaireController = new QuestionnaireController(
                questionnaireViewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
                Mock.Of<IFileSystemAccessor>(),
                logger ?? Mock.Of<ILogger<QuestionnaireController>>(),
                questionnaireInfoFactory ?? Mock.Of<IQuestionnaireInfoFactory>(),                
                Mock.Of<IQuestionnaireChangeHistoryFactory>(),
                Mock.Of<IQuestionnaireHistoryVersionsService>(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IQuestionnaireInfoViewFactory>(),
                categoricalOptionsImportService ?? Mock.Of<ICategoricalOptionsImportService>(),
                commandService ?? Mock.Of<ICommandService>(),
                dbContext ?? Create.InMemoryDbContext(),
                categoriesService: Mock.Of<ICategoriesService>(),
                Mock.Of<IEmailSender>(),
                Mock.Of<IViewRenderService>(),
                null!);
            questionnaireController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = new MockHttpSession()
                }
            };

            questionnaireController.TempData = new TempDataDictionary(questionnaireController.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
            return questionnaireController;
        }

        protected static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
