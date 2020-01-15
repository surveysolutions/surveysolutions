using System;
using System.Collections.Specialized;
using AutoMapper;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Web.TestFactories
{
    public class ServiceFactory
    {
        public IConfigurationManager ConfigurationManager(NameValueCollection appSettings = null,
            NameValueCollection membershipSettings = null)
        {
            return new ConfigurationManager(appSettings ?? new NameValueCollection(),
                membershipSettings ?? new NameValueCollection());
        }

        public WebCacheBasedCaptchaService WebCacheBasedCaptchaService(int? failedLoginsCount = 5,
            int? timeSpanForLogins = 5, IConfigurationManager configurationManager = null)
        {
            return new WebCacheBasedCaptchaService(configurationManager ?? this.ConfigurationManager(
                                                       new NameValueCollection
                                                       {
                                                           {
                                                               "CountOfFailedLoginAttemptsBeforeCaptcha",
                                                               (failedLoginsCount ?? 5).ToString()
                                                           },
                                                           {
                                                               "TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt",
                                                               (timeSpanForLogins ?? 5).ToString()
                                                           },
                                                       }));
        }

        public StatefullInterviewSearcher StatefullInterviewSearcher()
        {
            return new StatefullInterviewSearcher(Mock.Of<IInterviewFactory>(x =>
                x.GetFlaggedQuestionIds(It.IsAny<Guid>()) == new Identity[] { }));
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
    }
}
