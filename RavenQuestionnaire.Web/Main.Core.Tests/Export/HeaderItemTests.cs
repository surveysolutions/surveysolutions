using Main.Core.View.Export;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;

namespace Main.Core.Tests.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class HeaderItemTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [TestCase(0, "A")]
        [TestCase(1, "B")]
        [TestCase(6, "G")]
        [TestCase(25, "Z")]
        [TestCase(26, "AA")]
        [TestCase(132, "EC")]
        [TestCase(720, "AAS")]
        [TestCase(1000, "ALM")]
        [TestCase(10000, "NTQ")]
        [TestCase(100000, "EQXE")]
        [TestCase(int.MaxValue - 1, "FXSHRXW")]
        public void GetIntAsWord_Returns_CorrectResult(int index, string expectedWord)
        {
            HeaderItemTestable target = new HeaderItemTestable();
            Assert.AreEqual(target.GetIndexLeterTestable(index), expectedWord);
        }


        public class HeaderItemTestable : HeaderItem
        {

            public string GetIndexLeterTestable(int index)
            {
                return base.GetIntAsWord(index);
            }
        }

    }
}
