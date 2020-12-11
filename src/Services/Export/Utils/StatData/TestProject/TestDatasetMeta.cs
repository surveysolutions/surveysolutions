using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Core;

namespace TestProject
{
    [TestClass]
    public class TestDatasetMeta
    {
        private readonly string[,] _data = new[,] { { "1", "2" }, { "3", "4" } };

        [TestMethod]
        public void TestDatasetMeta1()
        {
            var meta = DatasetMeta.FromData(_data);
            Assert.IsTrue(meta.Variables.Length == 2);
        }

        [TestMethod]
        public void TestDatasetMeta2()
        {
            var variables = new IDatasetVariable[3];
            variables[0] = new DatasetVariable("first");
            variables[1] = new DatasetVariable("second");
            variables[2] = new DatasetVariable("third");

            var meta = new DatasetMeta(variables);
            Assert.IsTrue(meta.Variables == variables);
        }

        [TestMethod]
        [Description("Duplicate variable names")]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMetaDup()
        {
            var meta = DatasetMeta.FromVarlist("a b x c x y z");
        }

        [TestMethod]
        [Description("Unique variable names")]
        public void TestMetaUniq()
        {
            var meta = DatasetMeta.FromVarlist("a b x c y z t");
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Test attaching value labels to an undefined variable")]
        public void TestMetaUndefined()
        {
            var varx = new DatasetVariable("x");
            varx.Storage = VariableStorage.NumericStorage;
            var meta = new DatasetMeta(new IDatasetVariable[] { varx });
            var vs = new ValueSet();
            meta.AssociateValueSet("z", vs);
        }
    }
}
