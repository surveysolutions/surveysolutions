using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Writers.Stata14;

namespace TestProject
{
    [TestClass]
    public class TestStata14Variable
    {
        [TestMethod]
        public void TestTypesDetection()
        {
            // Test for bytes
            Assert.AreEqual(Stata14Constants.VarTypeByte, Stata14Variable.DetectNumericType(0));
            Assert.AreEqual(Stata14Constants.VarTypeByte, Stata14Variable.DetectNumericType(1));
            Assert.AreEqual(Stata14Constants.VarTypeByte, Stata14Variable.DetectNumericType(100));
            Assert.AreEqual(Stata14Constants.VarTypeByte, Stata14Variable.DetectNumericType(-100));
            Assert.AreEqual(Stata14Constants.VarTypeByte, Stata14Variable.DetectNumericType(-127));

            // Test for ints
            Assert.AreEqual(Stata14Constants.VarTypeInt, Stata14Variable.DetectNumericType(101));
            Assert.AreEqual(Stata14Constants.VarTypeInt, Stata14Variable.DetectNumericType(1000));
            Assert.AreEqual(Stata14Constants.VarTypeInt, Stata14Variable.DetectNumericType(32740));
            Assert.AreEqual(Stata14Constants.VarTypeInt, Stata14Variable.DetectNumericType(-128));      // this is likely a bug in Stata, since -128 fits into a single byte
            Assert.AreEqual(Stata14Constants.VarTypeInt, Stata14Variable.DetectNumericType(-32767));

            // Test for longs
            Assert.AreEqual(Stata14Constants.VarTypeLong, Stata14Variable.DetectNumericType(-32768));
            Assert.AreEqual(Stata14Constants.VarTypeLong, Stata14Variable.DetectNumericType(-2147483647));
            Assert.AreEqual(Stata14Constants.VarTypeLong, Stata14Variable.DetectNumericType(2147483620));
            Assert.AreEqual(Stata14Constants.VarTypeLong, Stata14Variable.DetectNumericType(100000));

            // No tests for floats
            // Floats are never detected

            // Test for doubles
            Assert.AreEqual(Stata14Constants.VarTypeDouble, Stata14Variable.DetectNumericType(0.1));
            Assert.AreEqual(Stata14Constants.VarTypeDouble, Stata14Variable.DetectNumericType(-0.1));
            Assert.AreEqual(Stata14Constants.VarTypeDouble, Stata14Variable.DetectNumericType(10000.1));
            Assert.AreEqual(Stata14Constants.VarTypeDouble, Stata14Variable.DetectNumericType(-10000.1));
            Assert.AreEqual(Stata14Constants.VarTypeDouble, Stata14Variable.DetectNumericType(8.9884656743 * (10 ^ 307)));
            Assert.AreEqual(Stata14Constants.VarTypeDouble, Stata14Variable.DetectNumericType(-8.9884656743 * (10 ^ 307)));

            // Integer values not fitting to long are to be saved as doubles
            Assert.AreEqual(Stata14Constants.VarTypeDouble, Stata14Variable.DetectNumericType(2147483621));
            Assert.AreEqual(Stata14Constants.VarTypeDouble, Stata14Variable.DetectNumericType(-2147483648));
        }

        [TestMethod]
        public void TestNumeric()
        {
            Assert.AreEqual(true, Stata14Variable.IsVarTypeNumeric(Stata14Constants.VarTypeByte));
            Assert.AreEqual(true, Stata14Variable.IsVarTypeNumeric(Stata14Constants.VarTypeInt));
            Assert.AreEqual(true, Stata14Variable.IsVarTypeNumeric(Stata14Constants.VarTypeLong));
            Assert.AreEqual(true, Stata14Variable.IsVarTypeNumeric(Stata14Constants.VarTypeFloat));
            Assert.AreEqual(true, Stata14Variable.IsVarTypeNumeric(Stata14Constants.VarTypeDouble));
        }

        [TestMethod]
        public void TestByteWidth()
        {
            Assert.AreEqual(1, Stata14Variable.GetVarWidth(Stata14Constants.VarTypeByte));
            Assert.AreEqual(2, Stata14Variable.GetVarWidth(Stata14Constants.VarTypeInt));
            Assert.AreEqual(4, Stata14Variable.GetVarWidth(Stata14Constants.VarTypeLong));
            Assert.AreEqual(4, Stata14Variable.GetVarWidth(Stata14Constants.VarTypeFloat));
            Assert.AreEqual(8, Stata14Variable.GetVarWidth(Stata14Constants.VarTypeDouble));
        }

        [TestMethod]
        public void TestVarInteger()
        {
            Assert.IsTrue(Stata14Variable.IsNumericVarInteger(Stata14Constants.VarTypeByte));
            Assert.IsTrue(Stata14Variable.IsNumericVarInteger(Stata14Constants.VarTypeInt));
            Assert.IsTrue(Stata14Variable.IsNumericVarInteger(Stata14Constants.VarTypeLong));
            Assert.IsFalse(Stata14Variable.IsNumericVarInteger(Stata14Constants.VarTypeFloat));
            Assert.IsFalse(Stata14Variable.IsNumericVarInteger(Stata14Constants.VarTypeDouble));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestVarIntegerEx()
        {
            Stata14Variable.IsNumericVarInteger(1);
        }
        /*
        [TestMethod]
        public void Test128()
        {
            var T = new[,] { { "-128" } };
            var w = new StatData.Writers.StataWriter();
            w.WriteToFile("t128.dta", new StatData.Core.StringArrayDataQuery(T));
        }*/

    }
}
