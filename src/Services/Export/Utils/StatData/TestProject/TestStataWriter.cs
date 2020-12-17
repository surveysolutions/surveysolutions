using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Core;
using StatData.Writers;

namespace TestProject
{
    [TestClass]
    public class TestStataWriter
    {
        private static readonly string[,] Data1 = new[,] { { "1", "2" }, { "3", "4" } };
        private static readonly string[,] Data2 = new[,]
                                 {
                                     {"1", "1", "1", "1", "1"}, 
                                     {"", "", "", "", ""},
                                     {"-5", "101", "32741", "2147483621", "3.14"}
                                 };

        private static readonly IDataQuery DataQ2 = new StringArrayDataQuery(Data2);
        
        [TestMethod]
        [Description("Test missings for different types")]
        public void TestMethod1()
        {
            var meta = DatasetMeta.FromData(Data2);
            meta.TimeStamp = new DateTime(2015, 11, 13, 11, 13, 15);

            var stataWriter = new Stata12Writer();
            stataWriter.WriteToFile("TestStataMissing.dta", meta, DataQ2); // values in the second line should come out as missings in every type.

            var d = File.ReadAllBytes("TestStataMissing.dta");
            var b = File.ReadAllBytes("TestStataMissingB.dta");
            Assert.IsTrue(b.SequenceEqual(d));
        }


        [TestMethod]
        [Description("Test truncation of long strings")]
        public void TestMethod2()
        {
            var data3 = new[,] {{"abcdefgh".Repeat(50)}};

            var meta = DatasetMeta.FromData(data3);
            meta.TimeStamp = new DateTime(2015, 11, 13, 11, 13, 15);

            var stataWriter = new Stata12Writer();
            stataWriter.WriteToFile("TestStataTrunc.dta", meta, data3); // values in the second line should come out as missings in every type.

            var d = File.ReadAllBytes("TestStataTrunc.dta");
            var b = File.ReadAllBytes("TestStataTruncB.dta");
            Assert.IsTrue(b.SequenceEqual(d));
        }

        [TestMethod]
        [Description("Test of smaller meta than data")]
        public void TestMethod3()
        {
            var data = DataQ2;
            var meta = new DatasetMeta(new IDatasetVariable[] {new DatasetVariable("x"), new DatasetVariable("y"),});
            var stataWriter = new Stata12Writer();
            stataWriter.WriteToFile("TestXY.dta", meta, data);
        }

        [TestMethod]
        [Description("Test of dots (Desk #2266)")]
        public void TestMethod6()
        {
            // passes if completes without an exception
            var data = new[,] { { "1", "." }, { "2", "." } };
            var meta = DatasetMeta.FromData(data);
            var stataWriter = new StataWriter();
            var ms = new MemoryStream();
            stataWriter.WriteToStream(ms, meta, data);
        }
        /*
        [TestMethod]
        [Description("Trying to replicate Kevin's case")]
        public void TestMethodKevin()
        {
            // trying to replicate a bug reported by Kevin
            var converter = new StatData.Converters.TabToStataConverter();
            converter.Convert(@"T:\Temp\Kevin.txt", @"T:\Temp\Kevin.dta");
        }*/

        [TestMethod]
        [Description("Verification of Kevin's variable name problem is solved")]
        public void TestMethod7()
        {
            Assert.IsFalse(StatData.Writers.Stata.StataVariable.IsInvalidVarName("id_str1"));
            Assert.IsTrue(StatData.Writers.Stata.StataVariable.IsInvalidVarName("str1"));
            Assert.IsFalse(StatData.Writers.Stata.StataVariable.IsInvalidVarName("str1a"));
            Assert.IsFalse(StatData.Writers.Stata.StataVariable.IsInvalidVarName("id_str12345678901234567890"));
            Assert.IsTrue(StatData.Writers.Stata.StataVariable.IsInvalidVarName("str12345678901234567890"));
            Assert.IsFalse(StatData.Writers.Stata.StataVariable.IsInvalidVarName("str12345678901234567890a"));
            Assert.IsFalse(StatData.Writers.Stata14.Stata14Variable.IsInvalidVarName("id_str1"));
            Assert.IsTrue(StatData.Writers.Stata14.Stata14Variable.IsInvalidVarName("str1"));
            Assert.IsFalse(StatData.Writers.Stata14.Stata14Variable.IsInvalidVarName("str1a"));
            Assert.IsFalse(StatData.Writers.Stata14.Stata14Variable.IsInvalidVarName("id_str12345678901234567890"));
            Assert.IsTrue(StatData.Writers.Stata14.Stata14Variable.IsInvalidVarName("str12345678901234567890"));
            Assert.IsFalse(StatData.Writers.Stata14.Stata14Variable.IsInvalidVarName("str12345678901234567890a"));
        }

        /*
        // This doesn't work yet
        [TestMethod]
        [Description("Test of smaller meta than data file")]
        public void TestMethod5()
        {
            var meta = new DatasetMeta(new IDatasetVariable[] {new DatasetVariable("agegroup")});
            var converter = new TabToStataConverter();
            converter.Convert("benchmark8.tab", "benchmark8agegroup.dta", meta);
        }
        */
        // This doesn't work yet

    }
}
