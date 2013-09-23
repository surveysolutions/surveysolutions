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
            Assert.AreEqual(target.GetIndexLeterTestable(0), "A");
        }
        [Test]
        public void GetIndexLeter_IndexIs0CountIs1000_LetterAA()
        {
            HeaderItemTestable target = new HeaderItemTestable();
            var result = target.GetIndexLeterTestable(1000);
            Console.WriteLine(result);
            Assert.AreEqual(result, "ALM");
        }

        [Test]
        public void GetIndexLeter_IndexIs6CountIs44_LetterAG()
        {
            HeaderItemTestable target = new HeaderItemTestable();
            var result = target.GetIndexLeterTestable(6);
            Console.WriteLine(result);
            Assert.AreEqual(result, "G");
        }
        [Test]
        public void GetIndexLeter_IndexIs32CountIs44_LetterBG()
        {
            HeaderItemTestable target = new HeaderItemTestable();
            var result = target.GetIndexLeterTestable(32);
            Console.WriteLine(result);
            Assert.AreEqual(result, "AG");
        }

        [Test]
        public void GetIndexLeter_IndexIs720CountIs800_LetterBBS()
        {
            HeaderItemTestable target = new HeaderItemTestable();
            var result = target.GetIndexLeterTestable(720);
            Console.WriteLine(result);
            Assert.AreEqual(result, "AAS");
        }

        [Test]
        public void GetIndexLeter_IndexIs26_LetterBBS()
        {
            HeaderItemTestable target = new HeaderItemTestable();
            var result = target.GetIndexLeterTestable(26);
            Console.WriteLine(result);
            Assert.AreEqual(result, "AA");
        }
        

        [Test]
        public void GetIndexLeter_IndexIs32_LetterBBS()
        {
            HeaderItemTestable target = new HeaderItemTestable();
            var result = target.GetIndexLeterTestable(132);
            Console.WriteLine(result);
            Assert.AreEqual(result, "EC");
        }

        /*public class HeaderItemTestable : HeaderItem
        {

            public  string GetIndexLeterTestable(int index, int count)
             {
                 return base.GetIndexLeter(index, count);
             }
        }*/

        public class HeaderItemTestable : HeaderItem
        {

            public string GetIndexLeterTestable(int index)
            {
                return base.GetIndexLeter(index);
            }
        }
    }
}
