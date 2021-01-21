using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Core;
using StatData.Writers;
using StatData.Writers.Stata;

namespace TestProject
{
    [TestClass]
    public class TestStataHelper
    {
        private static readonly string[,] Ddd = new[,] {{"1", "2"}, {"3", "4"}};

        private readonly IDataQuery _data = new StringArrayDataQuery(Ddd);

        [TestMethod]
        [ExpectedException(typeof (ArgumentException), "Invalid variable name")]
        public void TestMethod1()
        {
            var m = DatasetMeta.FromVarlist("str1 b c");
            var h = new StataWriterHelper(m, _data);
            Assert.Fail("Didn't catch invalid variable name");
        }

        [TestMethod]
        public void TestMethod2()
        {
            var m = DatasetMeta.FromVarlist("str1a b c");
            var h = new StataWriterHelper(m, _data);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var m = DatasetMeta.FromVarlist("Str1 b c");
            var h = new StataWriterHelper(m, _data);
        }

        [TestMethod]
        public void TestMethodNegByte()
        {
            // Version 1.0 threw an exception:
            // System.OverflowException: Value was either too large or too small for an unsigned byte.

            // Correctly must NOT through exception

            var d = (string[,]) Ddd.Clone();
            d[1, 1] = "-4"; // introduce a negative value which fits into a signed byte, but not unsigned byte
            var w = new Stata12Writer();
            w.WriteToStream(new MemoryStream(), d);
            w.WriteToFile("delme.dta", d);
        }

        [TestMethod]
        public void TestMethod4()
        {
            var m = DatasetMeta.FromVarlist("str2_2 b c");
            var h = new StataWriterHelper(m, _data);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException), "variables can't be empty")]
        public void TestMethod5()
        {
            var m = DatasetMeta.FromVarlist("");
            var h = new StataWriterHelper(m, _data);
        }


        [TestMethod]
        [ExpectedException(typeof (ArgumentException), "Invalid variable name")]
        public void TestMethod22()
        {
            var m = DatasetMeta.FromVarlist("a _all c");
            var h = new StataWriterHelper(m, _data);
        }

        [TestMethod]
        public void TestIsInvalidVarName()
        {
            var blacklist =
                new List<string>(
                    "byte int long float double if in _pi using with _all _b _n _N _cons _coef _rc _skip _pred _weight".
                        Split());

            foreach (var name in blacklist)
                Assert.IsTrue(StataVariable.IsInvalidVarName(name));

            Assert.IsTrue(StataVariable.IsInvalidVarName(""));
            Assert.IsTrue(StataVariable.IsInvalidVarName("8"));
            Assert.IsTrue(StataVariable.IsInvalidVarName("8abcd"));
            Assert.IsFalse(StataVariable.IsInvalidVarName("abcd8"));
            Assert.IsFalse(StataVariable.IsInvalidVarName("abcd_890"));
            Assert.IsFalse(StataVariable.IsInvalidVarName("abcdefghijklmnopqrstuvwxyz012345"));
            Assert.IsTrue(StataVariable.IsInvalidVarName("abcdefghijklmnopqrstuvwxyz0123456789"));
        }

        [TestMethod]
        [Description("We exclude _ even though it is supported.")]
        public void TestIsInvalidVarName2()
        {
            // although the variable name "_" is valid in Stata, it is better not use it
            Assert.IsTrue(StataVariable.IsInvalidVarName("_"));
        }

        [TestMethod]
        public void TestDetectNumericType()
        {
            Assert.IsTrue(StataVariable.DetectNumericType(0) == StataConstants.VarTypeByte);
            Assert.IsTrue(StataVariable.DetectNumericType(-101) == StataConstants.VarTypeByte);
            Assert.IsTrue(StataVariable.DetectNumericType(99) == StataConstants.VarTypeByte);
            Assert.IsTrue(StataVariable.DetectNumericType(101) == StataConstants.VarTypeInt);
            Assert.IsTrue(StataVariable.DetectNumericType(-200) == StataConstants.VarTypeInt);
            Assert.IsTrue(StataVariable.DetectNumericType(39000) == StataConstants.VarTypeLong);
            Assert.IsTrue(StataVariable.DetectNumericType(-40000) == StataConstants.VarTypeLong);
        }

        [TestMethod]
        [Description("Formatting of dates")]
        public void TestStataDate()
        {
            Assert.IsTrue(StataCore.StataDateTime(new DateTime(2014, 6, 2, 23, 59, 1)) == " 2 Jun 2014 23:59");
            Assert.IsTrue(StataCore.StataDateTime(new DateTime(2000, 12, 11, 3, 5, 9)) == "11 Dec 2000 03:05");
        }
        
        [TestMethod]
        [Description("Measuring numeric columns")]
        public void TestMeasureColumn()
        {
            var meta = DatasetMeta.FromData(_data);
            var types = StataWriterHelper.DetectTypes(_data, meta);
            Assert.IsTrue(types[0] == 251);
            Assert.IsTrue(types[1] == 251);
            var data2 = new[,] {{"", "102"}, {"", "2000024"}};
            var d2 = new StringArrayDataQuery(data2);
            types = StataWriterHelper.DetectTypes(d2);
            Assert.IsTrue(types[0]==251);
            Assert.IsTrue(types[1]==253);
        }
        
        [TestMethod]
        [Description("Measuring string variables")]
        public void TestMeasureColumn2()
        {
            var d1 = new StringArrayDataQuery(new[,] {{"alpha", "beta"}, {"gamma", "epsilon"}});
            var types = StataWriterHelper.DetectTypes(d1);
            Assert.IsTrue(types[0] == 5);
            Assert.IsTrue(types[1] == 7);
        }
        
        [TestMethod]
        [Description("Measuring mixed variables")]
        public void TestMeasureColumn3()
        {
            var d1 = new StringArrayDataQuery(new[,] {{"2", "beta"}, {"gamma", "5"}, {"aaa", "bbbbbbbbbbb"}});
            var types = StataWriterHelper.DetectTypes(d1);
            Assert.IsTrue(types[0] == 5);
            Assert.IsTrue(types[1] == 11);
        }
        
        [TestMethod]
        [Description("Overall test")]
        public void TestSaveToStream()
        {
            
            var writer = new Stata12Writer();
            var meta = DatasetMeta.FromData(_data);
            meta.TimeStamp = new DateTime(2014, 7, 3, 15, 0, 0);
            meta.DataLabel = "Benchmark test";

            //writer.WriteToFile(@"t:\2014\conv2spss\TestProject\data\benchmark1.dta", meta, _data); // if need to update the benchmark

            byte[] b;
            using (var ms = new MemoryStream())
            {
                writer.WriteToStream(ms, meta, _data); // this might be a frequent usage

                ms.Seek(0, SeekOrigin.Begin);
                b = new byte[ms.Length];
                ms.Read(b, 0, (int) ms.Length);
                ms.Close();
            }

            if (TestSpssWriter.CreateBenchmarks) writer.WriteToFile("benchmark1new.dta", meta, _data);//

            var d = File.ReadAllBytes("benchmark1.dta");

            if (!b.SequenceEqual(d))
            {
                //MessageBox.Show(b.Length.ToString());
                File.WriteAllBytes("error.dta", b);
            }

            Assert.IsTrue(b.SequenceEqual(d));
        }

        [TestMethod]
        [Description("Overall test - saving to file")]
        public void TestSaveToFile()
        {
            var ms = new MemoryStream();
            var writer = new StatData.Writers.Stata12Writer();

            var data = new[,] {{"1", "2"}, {"3", "a"}};

            var meta = DatasetMeta.FromData(data);
            meta.TimeStamp = new DateTime(2014, 7, 3, 15, 0, 0);
            meta.DataLabel = "Benchmark test";

            writer.WriteToFile("benchmark1f.dta", meta, data);
            writer.WriteToStream(ms, meta, data);

            ms.Seek(0, SeekOrigin.Begin);
            var b = new byte[ms.Length];
            ms.Read(b, 0, (int)ms.Length);
            ms.Close();

            var d = File.ReadAllBytes("benchmark1f.dta");
            Assert.IsTrue(b.SequenceEqual(d));
        }

        [TestMethod]
        public void TestGetLabel()
        {
            var vsa = new ValueSet {{0, "Zero"}, {1, "One"}};
            var vsb = new ValueSet {{777, "Winner"}, {000, "Loser"}, {555, "Next time"}};

            var meta = DatasetMeta.FromVarlist("a b c");
            meta.AssociateValueSet("a", vsa);
            meta.AssociateValueSet("b", vsb);

            var h = new StataWriterHelper(meta, _data);
            Assert.IsTrue(h.GetDctSize(0) == 2);
            Assert.IsTrue(h.GetDctCode(0, 0) == 0);
            Assert.IsTrue(h.GetDctCode(0, 1) == 1);

            Assert.IsTrue(h.GetDctSize(1) == 3);
            Assert.IsTrue(h.GetDctCode(1, 0) == 777);
            Assert.IsTrue(h.GetDctCode(1, 1) == 0);
            Assert.IsTrue(h.GetDctCode(1, 2) == 555);

            var labelled = meta.GetLabelledVariables();
            Assert.IsTrue(labelled.Count == 2);
            Assert.IsTrue(labelled[0] == "a");
            Assert.IsTrue(labelled[1] == "b");
        }
        /*
        [TestMethod]
        [Description("Measuring numeric columns as strings")]
        public void TestMeasureColumn4()
        {
            var x = new DatasetVariable("x") {Storage = VariableStorage.StringStorage};
            var w = StataWriterHelper.MeasureColumn(_data, 0, x);
            Assert.IsTrue(w == 1);
        }
        */
        [TestMethod]
        [Description("Measuring columns as numeric")]
        public void TestMeasureColumn5()
        {
            var x = new DatasetVariable("x") {Storage = VariableStorage.NumericStorage};
            var y = new DatasetVariable("y") {Storage = VariableStorage.UnknownStorage};
            var meta = new DatasetMeta(new[] {x, y});

            var w = StataWriterHelper.DetectTypes(_data, meta);

            Assert.IsTrue(w[0] == 255);
            Assert.IsTrue(w[1] == 251);
        }


        [TestMethod]
        [Description("Overall test with incorrect type")]
        [ExpectedException(typeof(FormatException))]
        public void TestSaveToStreamF()
        {
            var writer = new StatData.Writers.Stata12Writer();
            var data = new[,] {{"A", "1"}, {"2", "B"}};
            var v1 = new DatasetVariable("x") {Storage = VariableStorage.NumericStorage};
            var v2 = new DatasetVariable("y") {Storage = VariableStorage.NumericStorage};
            var meta = new DatasetMeta(new IDatasetVariable[] {v1, v2});
            meta.TimeStamp = new DateTime(2014, 7, 3, 15, 0, 0);
            meta.DataLabel = "Benchmark test";

            var ms = new MemoryStream();
            writer.WriteToStream(ms, meta, data);
            ms.Close();
        }


        [TestMethod]
        [Description("Overall test - saving to file")]
        public void TestSaveToFileLab()
        {
            var ms = new MemoryStream();
            var writer = new StatData.Writers.Stata12Writer();

            var data = new[,] { { "1", "2", "a" }, { "3", "4", "b" } };

            var v1 = new DatasetVariable("x");
            var v2 = new DatasetVariable("y");
            var v3 = new DatasetVariable("s");
            var v = new IDatasetVariable[] {v1, v2, v3};
            var meta = new DatasetMeta(v);
            
            var vsa = new ValueSet { { 0, "Zero" }, { 1, "One" } };
            var vsb = new ValueSet { { 777, "Winner" }, { 000, "Loser" }, { 555, "Next time" } };

            meta.AssociateValueSet("x", vsa);
            meta.AssociateValueSet("y", vsb);

            meta.TimeStamp = new DateTime(2014, 7, 3, 15, 0, 0);
            meta.DataLabel = "Benchmark test";

            writer.WriteToFile("benchmark3.dta", meta, data);
            writer.WriteToStream(ms, meta, data);

            ms.Seek(0, SeekOrigin.Begin);
            var b = new byte[ms.Length];
            ms.Read(b, 0, (int)ms.Length);
            ms.Close();

            var d = File.ReadAllBytes("benchmark3.dta");
            Assert.IsTrue(b.SequenceEqual(d));
        }


        [TestMethod]
        [Description("Overall test - suggested codepage cyrillic")]
        public void TestSaveToFilePage()
        {
            var ms = new MemoryStream();
            var writer = new StatData.Writers.Stata12Writer();

            var data = new[,] { { "ABCD EFGH", "АБВГ ДЕЖЗ" }, { "IJKL MNOP", "ИЙКЛ МНОП" } };

            var meta = DatasetMeta.FromData(data);
            meta.TimeStamp = new DateTime(2014, 7, 3, 15, 0, 0);
            meta.DataLabel = "Benchmark test";
            meta.AppropriateScript = DatasetScript.Cyrillic;

            if (TestSpssWriter.CreateBenchmarks) writer.WriteToFile("benchmark1pnew.dta", meta, data);
            writer.WriteToStream(ms, meta, data);

            ms.Seek(0, SeekOrigin.Begin);
            var b = new byte[ms.Length];
            ms.Read(b, 0, (int)ms.Length);
            ms.Close();

            var d = File.ReadAllBytes("benchmark1p.dta");
            Assert.IsTrue(b.SequenceEqual(d));
        }

        [TestMethod]
        [Description("Test of fractional and too long value labels - should pass without exceptions")]
        public void TestSaveExtraLongLabel()
        {
            var ms = new MemoryStream();
            var writer = new StatData.Writers.Stata12Writer();

            var data = new[,] {{"-21111111111"}, {"-777"}, {"-1"}, {"0"}, {"1"}, {"2"}, {"777"}, {"11111111111"}};
            var v1 = new DatasetVariable("x");
            var v = new IDatasetVariable[] {v1};
            var meta = new DatasetMeta(v);

            var vsa = new ValueSet
                          {
                              {-21111111111, "Very large negative value"},
                              {-1.5, "Negative fraction"},
                              {-777, "Negative value"},
                              {-1, "Small negative value"},
                              {0, "Zero"},
                              {1, "Small positive value"},
                              {1.5, "Positive fraction"},
                              {777, "Positive value"},
                              {11111111111, "Very large positive value"}
                          };
            
            meta.AssociateValueSet("x", vsa);
            
            meta.TimeStamp = new DateTime(2015, 8, 24, 15, 10, 0);
            meta.DataLabel = "Benchmark test";
            
            writer.WriteToStream(ms, meta, data);
            if (TestSpssWriter.CreateBenchmarks) writer.WriteToFile("benchmark9new.dta", meta, data); //

            ms.Seek(0, SeekOrigin.Begin);
            var b = new byte[ms.Length];
            ms.Read(b, 0, (int)ms.Length);
            ms.Close();

            var d = File.ReadAllBytes("benchmark9.dta");
            Assert.IsTrue(b.SequenceEqual(d));
        }
    }
}
