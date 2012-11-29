// -----------------------------------------------------------------------
// <copyright file="StataSuplierTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Export;
using Main.Core.View.Export;
using NUnit.Framework;

namespace Main.Core.Tests.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class StataSuplierTests
    {
        [Test]
        public void BuildMerge_TopLevelTable_OnlyImportWasBuild()
        {
            StataSuplierFake target=new StataSuplierFake();
            target.BuildMergeTestable(null,"someColumnName","file name");
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result,
                            "clear\r\ninsheet using \"file name\", comma\r\nsort someColumnName\r\ntempfile ind\r\nsave \"`ind'\"\r\n");
        }
        [Test]
        public void BuildMerge_SubLevelTable_ImportWithMergeIsBuild()
        {
            StataSuplierFake target = new StataSuplierFake();
            target.BuildMergeTestable("parentPrimaryKeyName", "someColumnName", "file name");
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result,
                            "clear\r\ninsheet using \"file name\", comma\r\ngen someColumnName=string(PublicKey)\r\ndrop PublicKey\r\ngen parentPrimaryKeyName=string(ForeignKey)\r\ndrop ForeignKey\r\nsort parentPrimaryKeyName\r\nmerge m:1 parentPrimaryKeyName using \"`ind'\"\r\ndrop _merge\r\n");
        }
        [Test]
        public void BuildLabels_HeaderISEmpty_ResultIsEmpty()
        {
            StataSuplierFake target = new StataSuplierFake();
            target.BuildLabelsTestable(new Dictionary<Guid, HeaderItem>());
            Assert.IsTrue(string.IsNullOrEmpty(target.Result));
        }
        [Test]
        public void BuildLabels_OneHeaderLAbelsAreEmpty_ResultIsEmpty()
        {
            StataSuplierFake target = new StataSuplierFake();
            target.BuildLabelsTestable(new Dictionary<Guid, HeaderItem>()
                {{Guid.NewGuid(), new HeaderItem(new SingleQuestion())}});
            Assert.IsTrue(string.IsNullOrEmpty(target.Result));
        }
        [Test]
        public void BuildLabels_OneHeaderLAbelsAreNotEmpty_ResultIsEmpty()
        {
            StataSuplierFake target = new StataSuplierFake();
            var labelGuid = Guid.NewGuid();
            var header = new Dictionary<Guid, HeaderItem>();
            var headeritem = new HeaderItem(new SingleQuestion(){ StataExportCaption = "q"});
            headeritem.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer(){ AnswerValue = "aValue1", AnswerText = "label text 1"}));
            headeritem.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer() { AnswerValue = "aValue2", AnswerText = "label text 2" }));
            header.Add(labelGuid, headeritem);
            target.BuildLabelsTestable(header);
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result, string.Format("\r\nlabel define {0} aValue1 \"label text 1\" aValue2 \"label text 2\" \r\nlabel var q {0}\r\n", labelGuid));
        }
        [Test]
        public void BuildLabels_TwoHeaderLAbelsAreNotEmpty_ResultIsEmpty()
        {
            StataSuplierFake target = new StataSuplierFake();
            var labelGuid1 = Guid.NewGuid();
            var labelGuid2 = Guid.NewGuid();
            var header = new Dictionary<Guid, HeaderItem>();
            var headeritem1 = new HeaderItem(new SingleQuestion() { StataExportCaption = "q1" });
            headeritem1.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer() { AnswerValue = "aValue1", AnswerText = "label text 1" }));
            var headeritem2 = new HeaderItem(new SingleQuestion() { StataExportCaption = "q2" });
            headeritem2.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer() { AnswerValue = "aValue2", AnswerText = "label text 2" }));
            header.Add(labelGuid1, headeritem1);
            header.Add(labelGuid2, headeritem2);
            target.BuildLabelsTestable(header);
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result,
                            string.Format(
                                "\r\nlabel define {0} aValue1 \"label text 1\" \r\nlabel var q1 {0}\r\n\r\nlabel define {1} aValue2 \"label text 2\" \r\nlabel var q2 {1}\r\n",
                                labelGuid1, labelGuid2));
        }
        public class StataSuplierFake : StataSuplier
        {
            public void BuildMergeTestable(string parentPrimaryKeyName, string primaryKeyColumnName, string fileName)
            {
                base.BuildMerge(parentPrimaryKeyName, primaryKeyColumnName, fileName);
            }
            public void BuildLabelsTestable(IDictionary<Guid, HeaderItem> header)
            {
                base.BuildLabels(header);
            }

            public string Result
            {
                get { return base.doContent.ToString(); }
            }
        }
    }
}
