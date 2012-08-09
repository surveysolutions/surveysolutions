using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Ninject;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;

namespace RavenQuestionnaire.Web.Tests.Membership
{
    [TestFixture]
    public class QuestionnaireRoleProviderTest
    {
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public QuestionnaireRoleProvider Provider { get; set; }

        /*[SetUp]
        public void CreateObjects()
        {
            CommandInvokerMock = new Mock<ICommandInvoker>();
            ViewRepositoryMock = new Mock<IViewRepository>();
            IKernel kernel = new StandardKernel();
            kernel.Bind<ICommandInvoker>().ToConstant(CommandInvokerMock.Object);
            kernel.Bind<IViewRepository>().ToConstant(ViewRepositoryMock.Object);
            KernelLocator.SetKernel(kernel);

            Provider = new QuestionnaireRoleProvider();
            Provider.Initialize(Provider.GetType().Name, null);
        }*/
    }
}
