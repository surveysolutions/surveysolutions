﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Impl;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Services.Impl;
using WB.UI.Shared.Web.Services;
using ISession = NHibernate.ISession;

namespace WB.Tests.Web.TestFactories
{
    public class ServiceFactory
    {
        // public WebCacheBasedCaptchaService WebCacheBasedCaptchaService(int? failedLoginsCount = 5,
        //     int? timeSpanForLogins = 5, IConfigurationManager configurationManager = null)
        // {
        //     return new WebCacheBasedCaptchaService(configurationManager ?? this.ConfigurationManager(
        //                                                new NameValueCollection
        //                                                {
        //                                                    {
        //                                                        "CountOfFailedLoginAttemptsBeforeCaptcha",
        //                                                        (failedLoginsCount ?? 5).ToString()
        //                                                    },
        //                                                    {
        //                                                        "TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt",
        //                                                        (timeSpanForLogins ?? 5).ToString()
        //                                                    },
        //                                                }));
        // }

        public StatefulInterviewSearcher StatefullInterviewSearcher()
        {
            return new StatefulInterviewSearcher(Mock.Of<IInterviewFactory>(x =>
                x.GetFlaggedQuestionIds(It.IsAny<Guid>()) == new Identity[] { }),
                Mock.Of<IWebInterviewInterviewEntityFactory>());
        }

        public IWebInterviewInterviewEntityFactory WebInterviewInterviewEntityFactory(IMapper autoMapper = null,
            IEnumeratorGroupStateCalculationStrategy enumeratorGroupStateCalculationStrategy = null,
            ISupervisorGroupStateCalculationStrategy supervisorGroupStateCalculationStrategy = null)
        {
            return new WebInterviewInterviewEntityFactory(
                autoMapper ?? Mock.Of<IMapper>(),
                enumeratorGroupStateCalculationStrategy ?? Mock.Of<IEnumeratorGroupStateCalculationStrategy>(),
                supervisorGroupStateCalculationStrategy ?? Mock.Of<ISupervisorGroupStateCalculationStrategy>(),
                Mock.Of<IWebNavigationService>(),
                Create.Service.SubstitutionTextFactory());
        }


        public WebNavigationService WebNavigationService()
        {
            var mockOfVirtualPathService = new Mock<IVirtualPathService>();
            mockOfVirtualPathService.Setup(x => x.GetAbsolutePath(It.IsAny<string>())).Returns<string>(x => x);
            mockOfVirtualPathService.Setup(x => x.GetRelatedToRootPath(It.IsAny<string>())).Returns<string>(x => x);

            return new WebNavigationService(mockOfVirtualPathService.Object);
        }

        public SubstitutionTextFactory SubstitutionTextFactory() => new SubstitutionTextFactory(new SubstitutionService(), new VariableToUIStringService());

        public IWebInterviewNotificationService WebInterviewNotificationService(
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IWebInterviewInvoker webInterviewInvoker)
        {
            return new WebInterviewNotificationService(statefulInterviewRepository, questionnaireStorage, webInterviewInvoker);
        }


        public HqWebInterviewInterviewEntityFactory HqWebInterviewInterviewEntityFactory(
            IAuthorizedUser authorizedUser = null)
        {
            var autoMapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
            });

            return new HqWebInterviewInterviewEntityFactory(autoMapperConfig.CreateMapper(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                new EnumeratorGroupGroupStateCalculationStrategy(),
                new SupervisorGroupStateCalculationStrategy(),
                Create.Service.WebNavigationService(),
                Create.Service.SubstitutionTextFactory());
        }

        public SignInManager<HqUser> SignInManager()
        {
            return new SignInManager<HqUser>(UserManager(),
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<HqUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<HqUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<HqUser>>());
        }

        public HqUserManager UserManager()
        {
            var session = new Mock<ISession>();
            session.Setup(s => s.Query<HqUser>())
                .Returns(() => new TestingQueryable<HqUser>(new List<HqUser>().AsQueryable()));
            
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(work => work.Session).Returns(session.Object);
            
            var hqUserStore = new HqUserStore(unitOfWork.Object, new LocalizedIdentityErrorDescriber());
            
            return new HqUserManager(hqUserStore,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<HqUser>>(),
                new List<IUserValidator<HqUser>>(),
                new List<IPasswordValidator<HqUser>>(),
                Mock.Of<ILookupNormalizer>(),
                new LocalizedIdentityErrorDescriber(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<HqUserManager>>(),
                Mock.Of<ISystemLog>(),
                Mock.Of<IAuthorizedUser>());
        }

        public IWorkspacesCache WorkspacesCache(ICollection<string> workspaces = null, ICollection<string> disabledWorkspaces = null)
        {
            disabledWorkspaces ??= new List<string>();

            if (workspaces == null)
            {
                return WorkspacesCache(new List<string> { WorkspaceConstants.DefaultWorkspaceName });
            }

            var result = workspaces?.Select(w
                => new WorkspaceContext(w, w, disabledWorkspaces.Contains(w) ? DateTime.Now : null));

            return WorkspacesCache(result);
        }

        public IWorkspacesCache WorkspacesCache(IEnumerable<WorkspaceContext> workspaces = null)
        {
            workspaces ??= new List<WorkspaceContext> { WorkspaceContext.Default };

            var wc = new WorkspacesCache(
                new NoScopeInScopeExecutor<IWorkspacesService>(
                    Mock.Of<IWorkspacesService>(w => w.GetAllWorkspaces() == workspaces.ToList())
                ));

            wc.InvalidateCache();

            return wc;
        }
    }
}
