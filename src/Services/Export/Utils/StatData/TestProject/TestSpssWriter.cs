using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Converters;
using StatData.Core;
using StatData.Readers;
using StatData.Writers;

namespace TestProject
{
    [TestClass]
    public class TestSpssWriter
    {
        private readonly string[,] _data = new[,] {{"1", "2"}, {"3", "A"}};

        public const bool CreateBenchmarks = true;

        [TestMethod]
        [Description("Overall writing test")]
        public void TestMethod1()
        {
            var meta = DatasetMeta.FromData(_data);
            meta.TimeStamp = new DateTime(2001, 1, 1, 0, 0, 0);

            var writer = new SpssWriter();
            var ms = new MemoryStream();
            writer.WriteToStream(ms, meta, _data);
            if (CreateBenchmarks) writer.WriteToFile("benchmark1new.sav", meta, _data); //

            ms.Seek(0, SeekOrigin.Begin);
            var b = new byte[ms.Length];
            ms.Read(b, 0, (int) ms.Length);
            ms.Close();

            var d = File.ReadAllBytes("benchmark1.sav");
            Assert.IsTrue(b.SequenceEqual(d));
        }


        [TestMethod]
        [Description("Numeric variables forced as strings")]
        public void TestMethod2()
        {
            var v1 = new DatasetVariable("x") {Storage = VariableStorage.StringStorage};
            var v2 = new DatasetVariable("y") {Storage = VariableStorage.StringStorage};

            var meta = new DatasetMeta(new IDatasetVariable[] {v1, v2});
            meta.TimeStamp = new DateTime(2001, 1, 1, 0, 0, 0);
            var writer = new SpssWriter();
            var ms = new MemoryStream();
            writer.WriteToStream(ms, meta, _data);
            ms.Seek(0, SeekOrigin.Begin);
            var b = new byte[ms.Length];
            ms.Read(b, 0, (int) ms.Length);
            ms.Close();

            if (CreateBenchmarks) writer.WriteToFile("benchmark2new.sav", meta, _data); //

            var d = File.ReadAllBytes("benchmark2.sav");
            Assert.IsTrue(b.SequenceEqual(d));
        }

        [TestMethod]
        [Description("Long string content")]
        public void TestMethod3()
        {
            // Should see long content with "1234567890" in the end
            var dt = _data;
            //dt[1, 1] = "HelloWorld".Repeat(500) + "1234567890"; very long
            dt[1, 1] = "HelloWorld".Repeat(26) + "1234567890";

            var meta = DatasetMeta.FromData(dt);
            meta.TimeStamp = new DateTime(2001, 1, 1, 0, 0, 0);

            var ms = new MemoryStream();
            var writer = new SpssWriter();
            writer.WriteToStream(ms, meta, dt);
            if (CreateBenchmarks) writer.WriteToFile("benchmark3new.sav", meta, _data); //

            ms.Seek(0, SeekOrigin.Begin);
            var b = new byte[ms.Length];
            ms.Read(b, 0, (int) ms.Length);
            ms.Close();

            var d = File.ReadAllBytes("benchmark3.sav");
            Assert.IsTrue(b.SequenceEqual(d));
        }

        [TestMethod]
        [Description("Long string content")]
        public void TestMethod4()
        {
            // Should see long content with "1234567890" in the end
            var dt = _data;
            dt[1, 1] =
                "Квітка-Основ'яненко Григорій Федорович — український прозаїк, драматург, журналіст, літературний критик і культурно-громадський діяч."
                    .Repeat(50) + "1234567890";

            var ms = new MemoryStream();
            var writer = new SpssWriter();
            var meta = DatasetMeta.FromData(dt);
            meta.TimeStamp = new DateTime(2001, 1, 1, 0, 0, 0);

            writer.WriteToStream(ms, meta, dt);
            if (CreateBenchmarks) writer.WriteToFile("benchmark4new.sav", meta, dt); // to create a new benchmark file

            ms.Seek(0, SeekOrigin.Begin);
            var b = new byte[ms.Length];
            ms.Read(b, 0, (int) ms.Length);
            ms.Close();

            var d = File.ReadAllBytes("benchmark4.sav");
            Assert.IsTrue(b.SequenceEqual(d));
        }

        [TestMethod]
        [Description("Long dataset")]
        public void TestMethod5()
        {
            //int n = (int)16.5*1000000; // this appears to be the max size of a string matrix in C#
            int n = 1000000;
            var dt = new String[n,1];

            Trace.WriteLine("ALLOC");
            for (var i = 0; i < n; i++)
            {
                dt[i, 0] = (i + 1).ToString(CultureInfo.InvariantCulture);
            }

            Trace.WriteLine("SET");

            using (var ms = new MemoryStream())
            {
                var writer = new SpssWriter();
                writer.WriteToStream(ms, dt);
                ms.Close();

                writer.WriteToFile(@"junkspss.sav", dt);
            }
        }

        [TestMethod]
        [Description("Long query")]
        public void TestMethod6()
        {
            var dt = new MockDataQuery(10/*00000*/, 50);
            IDatasetWriter writer = new SpssWriter();
            writer.WriteToFile(@"junkspss6.sav", dt); // Creates ~400MB file
        }


        [TestMethod]
        [Description("Nastya")]
        public void TestMethod7()
        {
            const string tabFile = @"benchmark.txt";
            const string savFile = @"benchmark_conv.sav";
            const string dtaFile = @"benchmark_conv.dta";

            var first = new DatasetVariable("id") {VarLabel = "Unique ID"};
            var second = new DatasetVariable("txt") {VarLabel = "Do you like this movie?"};
            var third = new DatasetVariable("num") {VarLabel = "How much do you like it?"};
            var dct = new ValueSet();
            dct.Add(1, "don't like");
            dct.Add(2, "a little bit");
            dct.Add(3, "very much");
            var meta = new DatasetMeta(new IDatasetVariable[] {third, first, second});
            meta.AssociateValueSet("num", dct);

            var converter1 = new TabToSpssConverter();
            converter1.Convert(tabFile, savFile, meta);

            var converter2 = new TabToStataConverter();
            converter2.Convert(tabFile, dtaFile, meta);
        }


        [TestMethod]
        [Description("Long unicode string followed by labelled numeric variable")]
        public void TestMethod8()
        {
            // simply tests that the file is created, no verification
            const string tabFile = "benchmark8.tab";

            var meta = DatasetMeta.FromVarlist("name agegroup");
            var dct = new ValueSet();
            dct.Add(1, "первая");
            dct.Add(2, "вторая");
            dct.Add(3, "третья");
            dct.Add(4, "четвёртая");
            meta.AssociateValueSet("agegroup", dct);

            meta.TimeStamp = new DateTime(2001, 1, 1, 0, 0, 0);
            var data = new TabReader().GetData(tabFile);
            var ms = new MemoryStream();
            var writer = new SpssWriter();

            writer.WriteToStream(ms, meta, data);
            if (CreateBenchmarks) writer.WriteToFile("benchmark8new.sav", meta, data); //

            ms.Seek(0, SeekOrigin.Begin);
            var b = new byte[ms.Length];
            ms.Read(b, 0, (int) ms.Length);
            ms.Close();

            var d = File.ReadAllBytes("benchmark8.sav");
            Assert.IsTrue(b.SequenceEqual(d));
        }

        [TestMethod]
        [Description("Test recoding of extended missing values")]
        public void TestMethod9_SPSS()
        {
            var emv = "-999999999";
            var emv2 = "-999999998";
            var emv3 = "-999999997";

            var data = new[,]
                           {
                               {"5", "5", "5", "", ""},
                               {"1.2", "1", ".", "a", emv},
                               {emv, emv, "3.14159265", "b", emv2},
                               {"1", "-1000", emv, "c", emv3}
                           };

            var meta = DatasetMeta.FromData(data);
            var someLabels = new ValueSet();
            someLabels.Add(1, "North of the country");
            someLabels.Add(2, "South");
            someLabels.Add(3, "East");
            someLabels.Add(4, "West");
            someLabels.Add(5, "Center");
            // someLabels.Add(-999999999, "Missing Nines");  // in case a value label is defined specifically for the missing value in a particular variable, it prevails over the missing value label defined in the dataset overall
            meta.AssociateValueSet("V0", someLabels);
            meta.TimeStamp = new DateTime(2017, 10, 31, 12, 12, 12);

            meta.Variables[0].VarLabel =
                "Длинная метка первой переменной для тестирования сохранения полного содержимого и его видимости в статистическом пакете для дальнейшей обработки.";
            meta.ExtendedMissings.Add(emv, "missing");
            meta.ExtendedMissings.Add("-999999998", "other missing");
            meta.ExtendedMissings.Add("-999999997", "yet another missing");
            meta.ExtendedMissings.Add("-999999996", "yet another missing value");
            // up to three EMVs are supported for SPSS format so the
            // value -999999996 will not be marked as missing as it 
            // is the 4th value, but labels will still be applied

            var spssWriter = new SpssWriter();

            spssWriter.WriteToFile("test9.sav", meta, data);
            if (CreateBenchmarks) File.Copy("test9.sav", "benchmark9new.sav", true);

            var ms = new MemoryStream();
            spssWriter.WriteToStream(ms, meta, data);
            ms.Seek(0, SeekOrigin.Begin);
            var b = new byte[ms.Length];
            ms.Read(b, 0, (int) ms.Length);
            ms.Close();

            var d = File.ReadAllBytes("benchmark9.sav");
            Assert.IsTrue(b.SequenceEqual(d));

            //spssWriter.WriteToFile("test9.sav", meta, data);
            //spssWriter.WriteToFile(@"C:\temp\test99.sav", meta, data);
        }

        [TestMethod]
        [Description("Dates writing test")]
        public void TestMethod10_SPSS()
        {
            // this test is testing future functionality that is currently under development
            return;
            
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

            var data10 = new[,]
                             {
                                 {d1, d1, t1, t1},
                                 {d2, d2, t2, t2},
                                 {d3, d3, t3, t3},
                                 {d4, d4, t4, t4},
                                 {d5, d5, t5, t5}
                             };

            var meta = DatasetMeta.FromData(data10);
            meta.TimeStamp = new DateTime(2017, 12, 26, 11, 44, 57);

            meta.Variables[0] = new DatasetVariable("TestDate") {Storage = VariableStorage.DateStorage};
            meta.Variables[1] = new DatasetVariable("OrgDate");
            meta.Variables[2] = new DatasetVariable("TestTime") {Storage = VariableStorage.DateTimeStorage};
            meta.Variables[3] = new DatasetVariable("OrgTime");

            //meta.ExtendedStrMissings.Add(stremv + "   fgjkdf ghdjfhg jkdfhgdjkfgh", stremvlbl);

            var writer = new SpssWriter();
            writer.WriteToFile("data10spss.sav", meta, data10);
            // todo: once the date and datetime storages are permitted update the benchmark for this case.

            if (CreateBenchmarks) File.Copy("data10spss.sav", "benchmark10spssnew.sav");

            Assert.IsTrue(FilesAreEqual("data10spss.sav", "benchmark10spss.sav"));
        }

        private bool FilesAreEqual(string first, string second)
        {
            bool bFilesAreEqual =
                new FileInfo(first).Length == new FileInfo(second).Length &&
                File.ReadAllBytes(first).SequenceEqual(File.ReadAllBytes(second));
            return bFilesAreEqual;
        }


        [TestMethod]
        [Description("SPSS extended missing string values test")]
        public void TestMethod11_SPSS()
        {
            // This test is testing future functionality that is currently under development
            return;

            const string stremv = "##N/A##";
            const string stremvlbl = "No answer";

            string d1 = "Short";
            string d2 = "Some text content more than 8 characters long";
            string d3 = "A".Repeat(1000);
            string dm = stremv;

            //var g = "𠜎".Length;
            var tv = "𠜎";// this should contain a 4-byte unicode character
            // see here: http://www.i18nguy.com/unicode/supplementary-test.html

            tv = tv.Substring(0, 1);
            byte[] bbb = System.Text.Encoding.UTF8.GetBytes(tv);
            if (bbb.Length > 6)
            {
                var g = 2;
            }

            var data11 = new[,]
                             {
                                 //{d3}, //, d2, d3, ""},
                                 //{dm}, //, dm, dm, dm}, 
                                 {d3}, //, d2, d3, ""}
                             };

            var meta = DatasetMeta.FromData(data11);
            meta.TimeStamp = new DateTime(2017, 12, 26, 11, 44, 57);

            meta.Variables[0] = new DatasetVariable("short");
            //meta.Variables[1] = new DatasetVariable("long");
            //meta.Variables[2] = new DatasetVariable("verylong");
            //meta.Variables[3] = new DatasetVariable("last");

            //meta.ExtendedStrMissings.Add(stremv, stremvlbl);

            var writer = new SpssWriter();
            writer.WriteToFile("data11spss.sav", meta, data11);
            // todo: once the date and datetime storages are permitted update the benchmark for this case.

            if (CreateBenchmarks) File.Copy("data11spss.sav", "benchmark11spssnew.sav");

            Assert.IsTrue(FilesAreEqual("data11spss.sav", "benchmark11spss.sav"));
        }

        [TestMethod]
        [Description("SPSS extended missing string values test")]
        public void TestMethod12_SPSS()
        {
            const string stremv = "##N/A##";
            const string stremvlbl = "No answer";
            // this test currently doesn't do what it says, since the extended mv is not used in the test

            string d1 = "Short";
            string d2 = "Some text content more than 8 characters long";
            string d3 = "A".Repeat(1000);

            var data11 = new[,]
                             {
                                 {d1},
                                 {d2}, 
                                 {d3}
                             };

            var meta = DatasetMeta.FromData(data11);
            meta.TimeStamp = new DateTime(2017, 12, 26, 11, 44, 57);

            meta.Variables[0] = new DatasetVariable("long") {Storage = VariableStorage.StringStorage};

            var writer = new SpssWriter();
            writer.WriteToFile("data12spss.sav", meta, data11);

            if (CreateBenchmarks) File.Copy("data12spss.sav", "benchmark12spssnew.sav", true);

            Assert.IsTrue(FilesAreEqual("data12spss.sav", "benchmark12spss.sav"));
        }


        // Test culture


        [TestMethod]
        [Description("SPSS export should support dot as a delimiter by default")]
        public void TestMethod13_SPSS()
        {
            var data = new[,] { { "1" , "1.0", "1.00" , "1.000", "3.14159265" } };
            var writer = new SpssWriter();

            // preserve culture
            var thisCultureT = Thread.CurrentThread.CurrentCulture;
            var thisCultureU = Thread.CurrentThread.CurrentUICulture;

            try
            {
                // reassign culture for test
                var testCulture = CultureInfo.CreateSpecificCulture("uk-UA");
                Thread.CurrentThread.CurrentCulture = testCulture;
                Thread.CurrentThread.CurrentUICulture = testCulture;
                
                writer.WriteToFile("test_spss_13onUA.sav", data);

                // reassign culture for test
                testCulture = CultureInfo.CreateSpecificCulture("ru-RU");
                Thread.CurrentThread.CurrentCulture = testCulture;
                Thread.CurrentThread.CurrentUICulture = testCulture;

                writer.WriteToFile("test_spss_13onRU.sav", data);
            }
            finally
            {
                // restore culture
                Thread.CurrentThread.CurrentCulture = thisCultureT;
                Thread.CurrentThread.CurrentUICulture = thisCultureU;
            }
        }



        [TestMethod]
        [Description("SPSS export should be configurable to also support other cultures")]
        public void TestMethod14_SPSS()
        {
            var meta = new DatasetMeta(new[]
                                 {
                                     new DatasetVariable("a"),
                                     new DatasetVariable("b"),
                                     new DatasetVariable("c"),
                                     new DatasetVariable("d"),
                                     new DatasetVariable("e")
                                 })
                           {
                               Culture = CultureInfo.CreateSpecificCulture("uk-UA")
                           };

            var data = new[,] { { "1", "1,0", "1,00", "1,000", "3,14159265" } };
            var writer = new SpssWriter();

            // preserve culture
            var thisCultureT = Thread.CurrentThread.CurrentCulture;
            var thisCultureU = Thread.CurrentThread.CurrentUICulture;

            try
            {
                // reassign culture for test
                var testCulture = CultureInfo.CreateSpecificCulture("uk-UA");
                Thread.CurrentThread.CurrentCulture = testCulture;
                Thread.CurrentThread.CurrentUICulture = testCulture;
                writer.WriteToFile("test_spss_14onUA.sav", meta, data);

                // reassign culture for test
                testCulture = CultureInfo.CreateSpecificCulture("en-US");
                Thread.CurrentThread.CurrentCulture = testCulture;
                Thread.CurrentThread.CurrentUICulture = testCulture;
                writer.WriteToFile("test_spss_14onUS.sav", meta, data);
            }
            finally
            {
                // restore culture
                Thread.CurrentThread.CurrentCulture = thisCultureT;
                Thread.CurrentThread.CurrentUICulture = thisCultureU;
            }
        }

        /*
        [TestMethod]
        [Description("Test NaN values")]
        public void TestMethod13_SPSS()
        {
            var writer = new SpssWriter();

            var data = new[,] {{"1"}, {"NaN"}};
            //var vars = new DatasetVariable[1];
            //vars[0] = new DatasetVariable("nanvar");
            //vars[0].Storage = VariableStorage.NumericStorage;
            //vars[0].Storage = VariableStorage.UnknownStorage;
            //var meta = new DatasetMeta(vars);

            var meta = DatasetMeta.FromData(data);
            meta.ExtendedMissings.Add("NaN","NaN");

            writer.WriteToFile(@"C:\temp\testNAN.sav", meta, data);
        }

        [TestMethod]
        [Description("Test NaN values2")]
        public void TestMethod14_SPSS()
        {
            var converter = new TabToSpssConverter();
            converter.Convert(@"C:\temp\testNAN.txt", @"C:\temp\testNaN.sav");
        }*/

        [TestMethod]
        [Description("SPSS export obey the decimals settings of the variables in meta")]
        public void TestMethod15_SPSS()
        {
            var data = new[,]
                           {
                               {"1", "2"},
                               {"1.0", "2.11111111"},
                               {"1.00", "2.222222"},
                               {"1.000", "2.3456"},
                               {"1.14159265", "2.728923235"}
                           };
            var writer = new SpssWriter();
            var x = new DatasetVariable("x") {FormatDecimals = 3, VarLabel = "x with 3 decimal digits"};
            var y = new DatasetVariable("y") {FormatDecimals = 5, VarLabel = "y with 5 decimal digits"};
            var meta = new DatasetMeta(new[] {x, y});
            meta.TimeStamp = new DateTime(2017, 10, 31, 12, 12, 12);

            writer.WriteToFile("test_spss_15.sav", meta, data);
            if (CreateBenchmarks) File.Copy("test_spss_15.sav", @"new/benchmark15spss.sav", true);
            Assert.IsTrue(FilesAreEqual("test_spss_15.sav", "benchmark15spss.sav"));
        }

        [TestInitialize]
        public void TestInit()
        {
            Directory.CreateDirectory("new");
        }

        

    }



    
}
