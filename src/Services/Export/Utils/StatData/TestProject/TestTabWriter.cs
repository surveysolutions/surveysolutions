using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Core;
using StatData.Readers;
using StatData.Writers;

namespace TestProject
{
    [TestClass]
    public class TestTabWriter
    {
        [TestMethod]
        [Description("Test of smaller meta than data file")]
        public void TestExtract()
        {
            var meta = new DatasetMeta(new IDatasetVariable[] { new DatasetVariable("agegroup") });
            var writer = new TabWriter();

            var src = new TabStreamDataQuery("benchmark8.tab");
            var dst = new FileStream("extract_agegroup.tab", FileMode.OpenOrCreate);
            writer.WriteToStream(dst, meta, src);
            dst.Close();
            Assert.AreEqual("agegroup", TabReader.ReadVarNamesStr("extract_agegroup.tab"));

            meta = new DatasetMeta(new IDatasetVariable[] { new DatasetVariable("name") });
            dst = new FileStream("extract_name.tab", FileMode.OpenOrCreate);
            writer.WriteToStream(dst, meta, src);
            dst.Close();
            Assert.AreEqual("name", TabReader.ReadVarNamesStr("extract_name.tab"));
        }
    }
}
