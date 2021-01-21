using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Core;
using StatData.Writers;

namespace TestProject
{
    [TestClass]
    public class TestStata14Writer
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

            var stataWriter = new StataWriter();
            stataWriter.WriteToFile("TestStata14Missing.dta", meta, DataQ2); // values in the second line should come out as missings in every type.

            var d = File.ReadAllBytes("TestStata14Missing.dta");
            var b = File.ReadAllBytes("TestStata14MissingB.dta");
            Assert.IsTrue(b.SequenceEqual(d));
        }


        [TestMethod]
        [Description("Test truncation of long strings")]
        public void TestMethod2()
        {
            var data3 = new[,] {{"abcdefgh".Repeat(50)}};

            var meta = DatasetMeta.FromData(data3);
            meta.TimeStamp = new DateTime(2015, 11, 13, 11, 13, 15);

            var stataWriter = new StataWriter();
            stataWriter.WriteToFile("TestStata14Trunc.dta", meta, data3); // values in the second line should come out as missings in every type.

            var d = File.ReadAllBytes("TestStata14Trunc.dta");
            var b = File.ReadAllBytes("TestStata14TruncB.dta");
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
        [Description("Test unicode strings")]
        public void TestMethod6()
        {
            var data3 = new[,] { 
              { "1", "{абвгдежз}".Repeat(5000) }, 
              { "2", "古城肇庆沿西江而建，坐落在两个风景秀丽的自然公园之间。是探寻喀斯特地貌、溶洞景观和古镇村落的绝佳去处。" } 
            };

            var meta = DatasetMeta.FromData(data3);
            meta.TimeStamp = new DateTime(2015, 11, 13, 11, 13, 15);
            meta.Variables[0].VarLabel = "Тестовая переменная";
            meta.DataLabel = "Тестовый набор данных";

            var vs1 = new ValueSet();
            vs1.Add(1, "Первый");
            vs1.Add(2, "Второй");
            vs1.Add(3, "Третий");

            meta.AssociateValueSet("V0", vs1);

            var stataWriter = new StataWriter();
            stataWriter.WriteToFile("TestStata14Unicode.dta", meta, data3);

            //var d = File.ReadAllBytes("TestStataTrunc.dta");
            //var b = File.ReadAllBytes("TestStataTruncB.dta");
            //Assert.IsTrue(b.SequenceEqual(d));
        }

        [TestMethod]
        [Description("Test date and date-time variables")]
        public void TestMethod7()
        {
            const string stremv = "##N/A##";
            const string stremvlbl = "No answer";

            string d1 = "2017-09-14";
            string d2 = "2000-01-01";
            string d3 = "1960-02-02";
            string d4 = "1941-06-22";
            string d5 = stremv;

            string t1 = d1 + "T" + "01:02:03Z";
            string t2 = d2 + "T" + "01:02:03Z";
            string t3 = d3 + "T" + "01:02:03Z"; 
            string t4 = d4 + "T" + "01:02:03Z";
            string t5 = d5;

            var data7 = new[,]
                            {
                                {d1, d1, t1, t1},
                                {d2, d2, t2, t2},
                                {d3, d3, t3, t3},
                                {d4, d4, t4, t4},
                                {d5, d5, t5, t5}
                            };

            var meta = DatasetMeta.FromData(data7);
            meta.TimeStamp = new DateTime(2017, 12, 07, 16, 00, 00);

            meta.ExtendedStrMissings.Add(stremv, stremvlbl);
            meta.Variables[0] = new DatasetVariable("TestDate") /* { Storage = VariableStorage.DateStorage }*/;
            meta.Variables[1] = new DatasetVariable("OrgDate");
            meta.Variables[2] = new DatasetVariable("TestTime") /* { Storage = VariableStorage.DateTimeStorage } */;
            meta.Variables[3] = new DatasetVariable("OrgTime");

            var stataWriter = new StataWriter();
            stataWriter.WriteToFile("TestStataDates.dta", meta, data7);

            TestingUtils.RunStataTest("TestStata14Dates.do", "markerDates.txt");
        }

        [TestMethod]
        [Description("Test stata syntax writer")]
        public void TestMethod88()
        {
            var data3 = new[,] { 
              { "1", "{абвгдежз}".Repeat(5000) }, 
              { "2", "古城肇庆沿西江而建，坐落在两个风景秀丽的自然公园之间。是探寻喀斯特地貌、溶洞景观和古镇村落的绝佳去处。" } 
            };

            var tabWriter = new StatData.Writers.TabWriter();
            tabWriter.WriteToFile("88.tab", data3);

            var meta = DatasetMeta.FromData(data3);
            meta.TimeStamp = new DateTime(2015, 11, 13, 11, 13, 15);
            meta.Variables[0].VarLabel = "Тестовая переменная";
            meta.DataLabel = "Тестовый набор данных";

            var vs1 = new ValueSet();
            vs1.Add(1, "Первый");
            vs1.Add(2, "Второй");
            vs1.Add(3, "Третий");

            meta.AssociateValueSet("V0", vs1);
            meta.ExtendedMissings.Add("-999999999", "missing value");

            var tabData = new StatData.Core.TabStreamDataQuery("88.tab");

            var doWriter = new Stata14SyntaxWriter();
            doWriter.WriteToFile("TestStata14Unicode.do", meta, tabData);

            var spsWriter = new SpssSyntaxWriter();
            spsWriter.WriteToFile("TestSpss.sps", meta, tabData);

            //var d = File.ReadAllBytes("TestStataTrunc.dta");
            //var b = File.ReadAllBytes("TestStataTruncB.dta");
            //Assert.IsTrue(b.SequenceEqual(d));
        }

        [TestMethod]
        [Description("Test of overflow on a forced type data")]
        public void Test21()
        {
            var inputFile = "input21.tab";
            var outputFile = "test21.dta";

            var data = new TabStreamDataQuery(inputFile);
            var writer = new StataWriter();

            var vnames = data.GetVarNames();
            var meta = DatasetMeta.FromVarlist(vnames);

            var posTest = -1;
            for (var i = 0; i < vnames.Length; i++)
            {
                if (vnames[i] == "test_item_01")
                {
                    posTest = i;
                    break;
                }
            }

            meta.Variables[posTest] = new DatasetVariable("test_item_01")
            {
                Storage = VariableStorage.StringStorage
            };

            writer.WriteToFile(outputFile, meta, data);
            TestingUtils.RunStataTest("Test21.do", "marker21.txt");
        }
        
        [TestMethod]
        [Description("Test handling of input data in scientific format")]
        public void StataWriter14_T22()
        {
            // must succeed and be confirmed in Stata by a benchmark do-file
            var data = new[,]
                           {
                               {"1", "2"},
                               {"5E-05", "-999999999"},
                               {"-999999999", "9.123E05"},
                               {"3.14159265", "1.2345e05"},
                               {"1e-22", "1e5"}
                           };

            var varx = new DatasetVariable("x")
                           {
                               Storage = VariableStorage.NumericStorage
                           };

            var vary = new DatasetVariable("y")
                           {
                               Storage = VariableStorage.NumericIntegerStorage
                           };

            var meta = new DatasetMeta(new IDatasetVariable[] {varx, vary});
            meta.ExtendedMissings.Add("-999999999", "Missing value");
            var writer = new StataWriter();
            writer.WriteToFile(@"StataWriter14_T22.dta", meta, data);
            TestingUtils.RunStataTest("StataWriter14_T22.do", "StataWriter14_T22.txt");
        }


        [TestMethod]
        [Description("Test handling of integer data")]
        public void StataWriter14_T23()
        {
            // this test is testing future functionality under development
            return;
            var data = new[,]
                           {
                               {"1", "2"},
                               {"3.14159265", "5"},
                               {"-0.00010203", "-5"},
                               {"-999999999", "-999999999"}
                           };

            var varx = new DatasetVariable("x");
            varx.Storage = VariableStorage.NumericStorage;
            var vary = new DatasetVariable("y"); // must come out as long
            vary.Storage = VariableStorage.NumericIntegerStorage;

            var meta = new DatasetMeta(new IDatasetVariable[] { varx, vary });
            meta.ExtendedMissings.Add("-999999999", "Missing value");
            var writer = new StataWriter();
            writer.WriteToFile(@"StataWriter14_T23.dta", meta, data);
            TestingUtils.RunStataTest("StataWriter14_T23.do", "StataWriter14_T23.txt");
        }


        [TestMethod]
        [Description("Test handling of empty value labels")]
        public void StataWriter14_T24()
        {
            var data = new[,]
                           {
                               {"1", "2"},
                               {"3.14159265", "5"},
                               {"-0.00010203", "-5"},
                               {"-999999999", "-999999999"}
                           };

            var varx = new DatasetVariable("x");
            varx.Storage = VariableStorage.NumericStorage;
            var vary = new DatasetVariable("y");
            vary.Storage = VariableStorage.NumericStorage;

            var vs = new ValueSet();
            vs.Add(1,"one");
            vs.Add(2,"two");
            vs.Add(5,"five");

            var meta = new DatasetMeta(new IDatasetVariable[] { varx, vary });
            meta.ExtendedMissings.Add("-999999999", "Missing value");
            meta.AssociateValueSet("x", vs);
            meta.AssociateValueSet("y", vs);

            var vs2 = new ValueSet();
            meta.AssociateValueSet("x", vs2); // variable x is expected to come out without any value set

            var writer = new StataWriter();
            writer.WriteToFile(@"StataWriter14_T24.dta", meta, data);
            TestingUtils.RunStataTest("StataWriter14_T24.do", "StataWriter14_T24.txt");
        }

        [TestMethod]
        [Description("Test handling of decimals in doubles")]
        public void StataWriter14_T25()
        {
            var data = new[,]
                           {
                               {"1", "2"},
                               {"3.14159265", "5"},
                               {"-0.00010203", "-5"},
                               {"-999999999", "-999999999"}
                           };

            var varx = new DatasetVariable("x");
            varx.Storage = VariableStorage.NumericStorage;
            varx.FormatDecimals = 4;
            var vary = new DatasetVariable("y"); 
            vary.Storage = VariableStorage.NumericStorage;
            vary.FormatDecimals = 0;

            var meta = new DatasetMeta(new IDatasetVariable[] { varx, vary });
            meta.ExtendedMissings.Add("-999999999", "Missing value");
            var writer = new StataWriter();
            writer.WriteToFile(@"StataWriter14_T25.dta", meta, data);
            TestingUtils.RunStataTest("StataWriter14_T25.do", "StataWriter14_T25.txt");
        }

        [TestMethod]
        [Description("Test robustness for incorrect numeric specification (int promoted to double)")]
        public void StataWriter14_T26()
        {
            // must succeed and be confirmed in Stata by a benchmark do-file
            var data = new[,]
                           {
                               {"1", "2"},
                               {"5E-05", "-999999999"},
                               {"-999999999", "9.123E05"},
                               {"3.14159265", "1.2345e05"},
                               {"1e-22", "1e5"}
                           };

            var varx = new DatasetVariable("x")
            {
                Storage = VariableStorage.NumericIntegerStorage
            };

            var vary = new DatasetVariable("y")
            {
                Storage = VariableStorage.NumericIntegerStorage
            };

            var meta = new DatasetMeta(new IDatasetVariable[] { varx, vary });
            meta.ExtendedMissings.Add("-999999999", "Missing value");
            var writer = new StataWriter();
            writer.WriteToFile(@"StataWriter14_T26.dta", meta, data);
            TestingUtils.RunStataTest("StataWriter14_T26.do", "StataWriter14_T26.txt");
        }






        /*
        [TestMethod]
        [Description("Test convert all tab files in the T:\TestBug\ folder to Stata format.")]
        public void Test99bug()
        {

            DirectoryInfo d = new DirectoryInfo(@"T:\TestBug\");
            FileInfo[] Files = d.GetFiles("*.tab"); //Getting Text files

            foreach (var f in Files)
            {
                var inputFile = f.FullName;
                var outputFile = @"T:\TestBug\" + Path.GetFileNameWithoutExtension(inputFile) + ".dta";

                var data = new TabStreamDataQuery(inputFile);
                var writer = new StataWriter();

                var vnames = data.GetVarNames();
                var meta = DatasetMeta.FromVarlist(vnames);

                writer.WriteToFile(outputFile, meta, data);
            }
        }
         * */
    }
}
