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

        [Test]
        public void GetIndexLeter_IndexIs0CountIs2_LetterA()
        {
            HeaderItemTestable target=new HeaderItemTestable();
            Assert.AreEqual(target.GetIndexLeterTestable(0, 2), "A");
        }
        [Test]
        public void GetIndexLeter_IndexIs0CountIs26_LetterAA()
        {
            HeaderItemTestable target = new HeaderItemTestable();
            var result = target.GetIndexLeterTestable(0, 26);
            Console.WriteLine(result);
            Assert.AreEqual(result, "AA");
        }

        [Test]
        public void GetIndexLeter_IndexIs6CountIs44_LetterAG()
        {
            HeaderItemTestable target = new HeaderItemTestable();
            var result = target.GetIndexLeterTestable(6, 44);
            Console.WriteLine(result);
            Assert.AreEqual(result, "AG");
        }
        [Test]
        public void GetIndexLeter_IndexIs32CountIs44_LetterBG()
        {
            HeaderItemTestable target = new HeaderItemTestable();
            var result = target.GetIndexLeterTestable(32, 44);
            Console.WriteLine(result);
            Assert.AreEqual(result, "BG");
        }

        [Test]
        public void GetIndexLeter_IndexIs720CountIs800_LetterBBS()
        {
            HeaderItemTestable target = new HeaderItemTestable();
            var result = target.GetIndexLeterTestable(720, 800);
            Console.WriteLine(result);
            Assert.AreEqual(result, "BBS");
        }
        public class HeaderItemTestable : HeaderItem
        {

            public  string GetIndexLeterTestable(int index, int count)
             {
                 return base.GetIndexLeter(index, count);
             }
        }
    }
}
