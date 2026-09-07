using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Vite.Extensions.AspNetCore;
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
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
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
            DesignerDbContext dbContext = null,
            IEmailSender emailSender = null,
            IViewRenderService viewRenderService = null,
            UserManager<DesignerIdentityUser> userManager = null)
        {
            var questionnaireController = new QuestionnaireController(
                questionnaireViewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
                new FileSystemIOAccessor(),
                logger ?? Mock.Of<ILogger<QuestionnaireController>>(),
                questionnaireInfoFactory ?? Mock.Of<IQuestionnaireInfoFactory>(),                
                Mock.Of<IQuestionnaireChangeHistoryFactory>(),
                Mock.Of<IQuestionnaireHistoryVersionsService>(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IQuestionnaireInfoViewFactory>(),
                categoricalOptionsImportService ?? Mock.Of<ICategoricalOptionsImportService>(),
                commandService ?? Mock.Of<ICommandService>(),
                dbContext ?? Create.InMemoryDbContext(),
                reusableCategoriesService: Mock.Of<IReusableCategoriesService>(),
                emailSender ?? Mock.Of<IEmailSender>(),
                viewRenderService ?? Mock.Of<IViewRenderService>(),
                userManager ?? CreateUserManager(),
                Mock.Of<ITagHelperComponentManager>(),
                Mock.Of<IWebHostEnvironment>(),
                Mock.Of<IOptions<ViteTagOptions>>(),
                Mock.Of<IMemoryCache>());
            questionnaireController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = new MockHttpSession(),
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "testuser")
                    }))
                }
            };

            questionnaireController.TempData = new TempDataDictionary(questionnaireController.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
            return questionnaireController;
        }

        internal static IUrlHelper CreateUrlHelper(string returnUrl = "https://example.com/share")
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns(returnUrl);
            return urlHelper.Object;
        }

        internal static IQuestionnaireViewFactory CreateQuestionnaireViewFactory()
        {
            var factory = new Mock<IQuestionnaireViewFactory>();
            factory.Setup(f => f.Load(It.IsAny<QuestionnaireViewInputModel>()))
                   .Returns(Create.QuestionnaireView());
            factory.Setup(f => f.Load(It.IsAny<QuestionnaireRevision>()))
                   .Returns(Create.QuestionnaireView());
            return factory.Object;
        }

        internal static UserManager<DesignerIdentityUser> CreateUserManager(DesignerIdentityUser returnUser = null)
        {
            var store = new Mock<IUserStore<DesignerIdentityUser>>();
            var userManagerMock = new Mock<UserManager<DesignerIdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            userManagerMock
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(returnUser);
            return userManagerMock.Object;
        }

        protected static Stream GenerateStreamFromString(string s)
        {
            var dataWithHeader = "value\ttitle\tparentvalue\r\n" + s;
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(dataWithHeader);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
