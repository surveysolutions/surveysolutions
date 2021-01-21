using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Core;

namespace TestProject
{
    [TestClass]
    public class TestDatasetVariable
    {
        [TestMethod]
        [Description("Empty variable name")]
        [ExpectedException(typeof(ArgumentException), "Variable name can't be empty")]
        public void TestDatasetVariable1()
        {
            var v1 = new DatasetVariable("");
        }

        [TestMethod]
        [Description("Defaults of all properties")]
        public void TestDatasetVariable2()
        {
            var v1 = new DatasetVariable("x");
            Assert.IsTrue(v1.VarName == "x");
            Assert.IsTrue(v1.VarLabel == "");
            Assert.IsTrue(v1.Storage == VariableStorage.UnknownStorage);
        }

        [TestMethod]
        [Description("Initialization of all properties and working of the interface.")]
        public void TestDatasetVariable3()
        {
            var v1 = new DatasetVariable("xyz") {VarLabel = "This is a label", Storage = VariableStorage.NumericStorage};
            Assert.IsTrue(v1.VarName == "xyz");
            Assert.IsTrue(v1.VarLabel == "This is a label");
            Assert.IsTrue(v1.Storage == VariableStorage.NumericStorage);

            IDatasetVariable v2 = v1;
            Assert.IsTrue(v2.VarName == "xyz");
            Assert.IsTrue(v2.VarLabel == "This is a label");
            Assert.IsTrue(v2.Storage == VariableStorage.NumericStorage);
        }

        [TestMethod]
        [Description("Test that can't set out of range values for FormatDecimals")]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Value is out of range")]
        public void TestDatasetVariable4()
        {
            new DatasetVariable("qwertysmall") {FormatDecimals = -1};
        }

        [TestMethod]
        [Description("Test that can't set out of range values for FormatDecimals")]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Value is out of range")]
        public void TestDatasetVariable5()
        {
            new DatasetVariable("qwertylarge") {FormatDecimals = 16};
        }

        [TestMethod]
        [Description("Test that can set typical values for FormatDecimals")]
        public void TestDatasetVariable6()
        {
            var v1 = new DatasetVariable("qwerty");
            for (var i = 0; i <= 15; i++)
            {
                v1.FormatDecimals = i;
                Assert.AreEqual(i, v1.FormatDecimals);
            }
        }
    }
}
