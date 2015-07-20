﻿using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters.InstallControllerTests
{
    [Subject(typeof(InstallController))]
    internal class InstallControllerTestsContext
    {
        protected static InstallController CreateController(ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null, ILogger logger = null, IPasswordHasher passwordHasher = null, IFormsAuthentication authentication = null)
        {
            return new InstallController(
                commandService: commandService ?? Mock.Of<ICommandService>(),
                globalInfo: globalInfo ?? Mock.Of<IGlobalInfoProvider>(), 
                logger: logger ?? Mock.Of<ILogger>(),
                passwordHasher: passwordHasher ?? Mock.Of<IPasswordHasher>(),
                authentication: authentication ?? Mock.Of<IFormsAuthentication>());
        }
    }
}
