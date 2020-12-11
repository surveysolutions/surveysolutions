using System;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Core;

namespace TestProject
{
    [TestClass]
    public class TestExtendedMissingValuesFacility
    {
        [TestMethod]
        public void TestMethod1()
        {
            var facility = new ExtendedMissingValuesFacility();
            facility.Add("-999", "not specified");
            facility.Add("-997", "no answer");
            
            Assert.IsTrue(facility.IsMissing("-999"));
            Assert.IsTrue(facility.IsMissing("-997"));

            Assert.IsFalse(facility.IsMissing("-990"));
            Assert.IsFalse(facility.IsMissing("997"));
            Assert.IsFalse(facility.IsMissing(".a"));
            Assert.IsFalse(facility.IsMissing("."));

            Assert.AreEqual(0, facility.IndexOf("-999"));
            Assert.AreEqual(1, facility.IndexOf("-997"));

            Assert.AreEqual(-1, facility.IndexOf("-990"));
            Assert.AreEqual(-1, facility.IndexOf("997"));
            Assert.AreEqual(-1, facility.IndexOf(".a"));
            Assert.AreEqual(-1, facility.IndexOf("."));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Duplicate value: -997")]
        public void TestMethod2()
        {
            var facility = new ExtendedMissingValuesFacility();
            facility.Add("-999", "not specified");
            facility.Add("-997", "no answer");
            facility.Add("-997", "other label");
        }

        [TestMethod]
        public void TestMethod3()
        {
            var facility = new ExtendedMissingValuesFacility();
            facility.Add("-999", "not specified");
            facility.Add("-997", "no answer");

            var missings = facility.GetList();
            Assert.AreEqual(2, missings.Count);
            Assert.AreEqual("not specified", missings[0].Label);
            Assert.AreEqual("no answer", missings[1].Label);
        }

        [TestMethod]
        public void TestMethod4()
        {
            const string lbl = "missing";
            var facility = new ExtendedMissingValuesFacility();
            for (var i = 1; i <= 100; i++)
                facility.Add(i.ToString(CultureInfo.InvariantCulture), lbl);

            var missings = facility.GetList();
            Assert.AreEqual(100, missings.Count);

            for (var i = 1; i <= 100; i++)
                Assert.AreEqual(i.ToString(CultureInfo.InvariantCulture), 
                    missings[i - 1].MissingValue);

            for (var i = 1; i <= 100; i++)
                Assert.AreEqual(lbl, missings[i - 1].Label);
        }
    }
}
