using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Ninject;

namespace RavenQuestionnaire.Core.Tests
{

    [TestFixture]
    public class ViewRepositoryTests
    {
        [Test]
        public void WhenLoadInvokedAndFactoryIsRegistered_ViewIsReturned()
        {
            Mock<IContainer> containerMock = new Mock<IContainer>();
            Mock<IViewFactory<string, string>> viewFactoryMock = new Mock<IViewFactory<string, string>>();
           // containerMock.Setup(x => x.TryGetInstance<IViewFactory<string, string>>()).Returns(viewFactoryMock.Object);
            var kernel = new StandardKernel();
            kernel.Bind<IViewFactory<string, string>>().ToConstant(viewFactoryMock.Object);
            viewFactoryMock.Setup(x => x.Load("hello")).Returns("hi");
            ViewRepository repository = new ViewRepository(kernel);
            String result = repository.Load<string, string>("hello");
            Assert.AreEqual("hi", result);
        }

        [Test]
        public void WhenLoadInvokedAndNoFactoryIsRegistered_NullIsReturned()
        {
            Mock<IContainer> containerMock = new Mock<IContainer>();
            var kernel = new StandardKernel();

            ViewRepository repository = new ViewRepository(kernel);
            String result = repository.Load<string, string>("hello");
            Assert.AreEqual(null, result);
        }
    }
}
