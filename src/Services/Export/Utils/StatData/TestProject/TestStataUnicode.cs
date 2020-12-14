using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Core;
using StatData.Writers;
using System.Diagnostics;

namespace TestProject
{
    [TestClass]
    public class TestStataUnicode
    {
        [Description("Test for large content (5mln observations)")]
        [TestMethod]
        public void TestUnicodeContent()
        {
            var data3 = new[,] { 
              { "1", "абвгдежз АБВГДЕЖЗ abcdefgh ABCDEFGH" }, 
              { "2", "古城肇庆沿西江而建，坐落在两个风景秀丽的自然公园之间。是探寻喀斯特地貌、溶洞景观和古镇村落的绝佳去处。" } 
            };

            int nrepeats = 2500000; // (5mln observations)
            var nobs = data3.GetLength(0);

            var data4 = new string[nrepeats*nobs, 2];
            for (var i = 0; i < nrepeats; i++)
            {
                data4[i*2, 0] = data3[0, 0];
                data4[i*2, 1] = data3[0, 1];
                data4[i*2 + 1, 0] = data3[1, 0];
                data4[i*2 + 1, 1] = data3[1, 1];
            }

            var v0 = new DatasetVariable("numvar");
            var v1 = new DatasetVariable("strvar");
            var meta = new DatasetMeta(new[] { v0, v1 });
            meta.DataLabel = "Тестовый набор данных";
            meta.TimeStamp = new DateTime(2015, 11, 13, 11, 13, 15);
            meta.Variables[0].VarLabel = "Тестовая числовая переменная";
            meta.Variables[1].VarLabel = "Тестовая строковая переменная";

            var stataWriter = new StataWriter();
            stataWriter.WriteToFile("TestStata14UnicodeContent.dta", meta, data4);
            TestingUtils.RunStataTest("TestStata14UnicodeContent.do", "markerContent.txt");
        }

        [Description("Test of unicode labels")]
        [TestMethod]
        public void TestUnicodeLabels()
        {
            // it is normal that the labels don't fint into Stata columns, however they should be transmitted entirely.
            var data = new[,] {
                {"1","100","1000",""},
                {"2","100","1001","АБВГДЕЖЗ".Repeat(127)+"1234567890123"}, // exact content without truncation
                {"3","100","1002","abcdefgh".Repeat(255)+"12345"},         // exact content without truncation
                {"-99.321","100","1003","123"}
            };

            var meta = DatasetMeta.FromData(data);
            meta.TimeStamp = new DateTime(2015, 11, 13, 11, 13, 15);
            meta.DataLabel = "Тест меток значений";
            meta.SetAsciiComment("Comment to this data file. Русский комментарий.");

            var schema1 = new StatData.Core.ValueSet();
            schema1.Add(1, "Первое значение");
            schema1.Add(2, "Второе значение");
            schema1.Add(3, "Третье значение");

            var schema2 = new StatData.Core.ValueSet();
            schema2.Add(100, "Ровно сто");
            schema2.Add(101, "Сто один");

            var schema3 = new StatData.Core.ValueSet();
            schema3.Add(1000, "Тысяча");
            schema3.Add(1001, "Тысяча один");
            schema3.Add(1002, "Тысяча два");
            schema3.Add(1003, "Тысяча три");

            meta.AssociateValueSet("V0", schema1);
            meta.AssociateValueSet("V1", schema2);
            meta.AssociateValueSet("V2", schema3);

            meta.Variables[0].VarLabel = "DOUBLE variable";
            meta.Variables[1].VarLabel = "BYTE variable";
            meta.Variables[2].VarLabel = "INT variable";
            meta.Variables[3].VarLabel = "STR variable";
            
            var stataWriter = new StataWriter();
            stataWriter.WriteToFile("TestStata14UnicodeLabels.dta", meta, data);

            TestingUtils.RunStataTest("TestStata14UnicodeLabels.do", "MarkerLabels.txt");
        }

        [Description("Test of missing values definitions")]
        [TestMethod]
        public void TestStataMissing()
        {
            // Test all of the system missing values
            Assert.IsTrue(StataCore.GetBytes("B.").SequenceEqual(StataCore.MissByte));
            Assert.IsTrue(StataCore.GetBytes("I.").SequenceEqual(StataCore.MissInt));
            Assert.IsTrue(StataCore.GetBytes("L.").SequenceEqual(StataCore.MissLong));
            Assert.IsTrue(StataCore.GetBytes("F.").SequenceEqual(StataCore.MissFloat)); 
            Assert.IsTrue(StataCore.GetBytes("D.").SequenceEqual(StataCore.MissDouble));

            // Test all of the extended type A missing values
            byte[] MissByteA = new byte[] {102};
            byte[] MissIntA = new byte[] {230, 127};
            byte[] MissLongA = new byte[] {230, 255, 255, 127};
            byte[] MissFloatA = new byte[] {0, 8, 0, 127};
            byte[] MissDoubleA = new byte[] {0, 0, 0, 0, 0, 1, 224, 127};

            Assert.IsTrue(StataCore.GetBytes("B.a").SequenceEqual(MissByteA));
            Assert.IsTrue(StataCore.GetBytes("I.a").SequenceEqual(MissIntA));
            Assert.IsTrue(StataCore.GetBytes("L.a").SequenceEqual(MissLongA));
            Assert.IsTrue(StataCore.GetBytes("F.a").SequenceEqual(MissFloatA));
            Assert.IsTrue(StataCore.GetBytes("D.a").SequenceEqual(MissDoubleA));
        }

        [TestMethod]
        [Description("STATA: Test recoding of extended missing values")]
        public void TestMethod9()
        {
            var emv = "-999999999";  // will be mapped to .a value
            var emv2 = "-999999997"; // will be mapped to .b value

            var data = new[,]
                           {
                               {"5", "5", "5", ""},
                               {"1.2", "1", ".", "a"},
                               {emv, emv, "3.14159265", "b"},
                               {"1", "-1000", emv, "c"},
                               {"9", "-1001", emv2, "d"},
                           };
            var meta = DatasetMeta.FromData(data);
            meta.ExtendedMissings.Add(emv, "missing"); // label is not currently used in Stata output
            meta.ExtendedMissings.Add(emv2, "missing"); // label is not currently used in Stata output

            var stataWriter = new StataWriter();
            stataWriter.WriteToFile("TestStata14Missings.dta", meta, data);
            TestingUtils.RunStataTest("TestStata14ExtendedMissings.do", "markerMissings.txt");
        }
    }
}
