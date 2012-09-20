// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewRepositoryTests.cs" company="">
//   
// </copyright>
// <summary>
//   The view repository tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;

namespace RavenQuestionnaire.Core.Tests
{
    using System;
    using System.ComponentModel;

    using Moq;

    using Ninject;

    using NUnit.Framework;

    /// <summary>
    /// The view repository tests.
    /// </summary>
    [TestFixture]
    public class ViewRepositoryTests
    {
        #region Public Methods and Operators

        /// <summary>
        /// The when load invoked and factory is registered_ view is returned.
        /// </summary>
        [Test]
        public void WhenLoadInvokedAndFactoryIsRegistered_ViewIsReturned()
        {
            var containerMock = new Mock<IContainer>();
            var viewFactoryMock = new Mock<IViewFactory<string, string>>();

            // containerMock.Setup(x => x.TryGetInstance<IViewFactory<string, string>>()).Returns(viewFactoryMock.Object);
            var kernel = new StandardKernel();
            kernel.Bind<IViewFactory<string, string>>().ToConstant(viewFactoryMock.Object);
            viewFactoryMock.Setup(x => x.Load("hello")).Returns("hi");
            var repository = new ViewRepository(kernel);
            string result = repository.Load<string, string>("hello");
            Assert.AreEqual("hi", result);
        }

        /// <summary>
        /// The when load invoked and no factory is registered_ null is returned.
        /// </summary>
        [Test]
        public void WhenLoadInvokedAndNoFactoryIsRegistered_NullIsReturned()
        {
            var containerMock = new Mock<IContainer>();
            var kernel = new StandardKernel();

            var repository = new ViewRepository(kernel);
            string result = repository.Load<string, string>("hello");
            Assert.AreEqual(null, result);
        }

        #endregion
    }
}