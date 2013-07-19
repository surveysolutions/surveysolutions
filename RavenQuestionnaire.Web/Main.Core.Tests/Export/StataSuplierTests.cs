using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Export;
using Main.Core.View.Export;
using Microsoft.Practices.ServiceLocation;
using Moq;
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
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void BuildMerge_TopLevelTable_OnlyImportWasBuild()
        {
            StataSuplierFake target=new StataSuplierFake();
            target.BuildMergeTestable(null,"someColumnName","file name");
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result,
                            "clear\r\ninsheet using \"file name\", comma\r\n");
        }
        [Test]
        public void BuildMerge_SubLevelTable_ImportWithMergeIsBuild()
        {
            StataSuplierFake target = new StataSuplierFake();
            target.BuildMergeTestable("parentPrimaryKeyName", "someColumnName", "file name");
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result,
                            "clear\r\ninsheet using \"file name\", comma\r\nrename PublicKey someColumnName\r\nrename ForeignKey parentPrimaryKeyName\r\nsort parentPrimaryKeyName\r\nmerge m:1 parentPrimaryKeyName using \"`parentPrimaryKeyNameind'\"\r\ndrop _merge\r\n");
        }
        [Test]
        public void BuildLabels_HeaderISEmpty_ResultIsEmpty()
        {
            StataSuplierFake target = new StataSuplierFake();
            target.BuildLabelsTestable(new HeaderCollection());
            Assert.IsTrue(string.IsNullOrEmpty(target.Result));
        }
        [Test]
        public void BuildLabels_OneHeaderLAbelsAreEmpty_OnlyLAbelIsAssigned()
        {
            StataSuplierFake target = new StataSuplierFake();
            target.BuildLabelsTestable(new HeaderCollection()
                {{ new HeaderItem(new SingleQuestion(){StataExportCaption = "q1", QuestionText = "good question"})}});
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result, "label var q1 `\"good question\"'\r\n");
        }
       
        [Test]
        public void BuildLabels_OneHeaderLAbelsAreNotEmpty_ResultIsNotEmpty()
        {
            StataSuplierFake target = new StataSuplierFake();
            var labelGuid = Guid.NewGuid();
            var header = new HeaderCollection();
            var headeritem = new HeaderItem(new SingleQuestion(){ StataExportCaption = "q", PublicKey = labelGuid, QuestionText = "good question"});
            headeritem.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer(){ AnswerValue = "aValue1", AnswerText = "label text 1"}));
            headeritem.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer() { AnswerValue = "aValue2", AnswerText = "label text 2" }));
            header.Add(headeritem);
            target.BuildLabelsTestable(header);
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result, "\r\nlabel define lq aValue1 `\"label text 1\"' aValue2 `\"label text 2\"' \r\nlabel values q lq\r\nlabel var q `\"good question\"'\r\n");
        }

        [Test]
        public void BuildLabels_OneHeaderLAbelsAreNotEmptyNewLineIsPresentInLiterals_ResultIsNotEmptyNewLinesAreRomved()
        {
            StataSuplierFake target = new StataSuplierFake();
            var labelGuid = Guid.NewGuid();
            var header = new HeaderCollection();
            var headeritem = new HeaderItem(new SingleQuestion() { StataExportCaption = "q", PublicKey = labelGuid, QuestionText = "good \r\nquestion" });
            headeritem.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer() { AnswerValue = "aValue1", AnswerText = "label \r\ntext 1" }));
            headeritem.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer() { AnswerValue = "aValue2", AnswerText = "label \r\ntext 2" }));
            header.Add(headeritem);
            target.BuildLabelsTestable(header);
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result, "\r\nlabel define lq aValue1 `\"label text 1\"' aValue2 `\"label text 2\"' \r\nlabel values q lq\r\nlabel var q `\"good question\"'\r\n");
        }
        [Test]
        public void BuildLabels_TwoHeaderLAbelsAreNotEmpty_ResultIsEmpty()
        {
            StataSuplierFake target = new StataSuplierFake();
            var labelGuid1 = Guid.NewGuid();
            var labelGuid2 = Guid.NewGuid();
            var header = new HeaderCollection();
            var headeritem1 = new HeaderItem(new SingleQuestion() { StataExportCaption = "q1", PublicKey = labelGuid1, QuestionText = "question 1"});
            headeritem1.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer() { AnswerValue = "aValue1", AnswerText = "label text 1" }));
            var headeritem2 = new HeaderItem(new SingleQuestion() { StataExportCaption = "q2", PublicKey = labelGuid2, QuestionText = "question 2" });
            headeritem2.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer() { AnswerValue = "aValue2", AnswerText = "label text 2" }));
            header.Add( headeritem1);
            header.Add(headeritem2);
            target.BuildLabelsTestable(header);
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result,
                            string.Format(
                                "\r\nlabel define {0} aValue1 `\"label text 1\"' \r\nlabel values q1 {0}\r\nlabel var q1 `\"question 1\"'\r\n\r\nlabel define {1} aValue2 `\"label text 2\"' \r\nlabel values q2 {1}\r\nlabel var q2 `\"question 2\"'\r\n",
                                "lq1", "lq2"));
        }

        [Test]
        public void BuildLabels_OneHeaderTwoColumnsWithSameLabel_OnlyOneLabalWasCreatedAttachedToBothVariables()
        {
            StataSuplierFake target = new StataSuplierFake();
            var header = new HeaderCollection();
            var labelGuid = Guid.NewGuid();
            var headeritem1 = new HeaderItem(new MultyOptionsQuestion() { StataExportCaption = "q1a", PublicKey = labelGuid, QuestionText = "question 1"});

            headeritem1.Labels.Add(Guid.NewGuid(), new LabelItem(new Answer() { AnswerValue = "aValue1", AnswerText = "label text 1" }));
            var headeritem2 = new HeaderItem(new MultyOptionsQuestion() { StataExportCaption = "q1b", PublicKey = labelGuid, QuestionText = "question 2" });
            headeritem2.Labels.Add(headeritem1.Labels.First().Key, headeritem1.Labels.First().Value);
            header.Add(headeritem1);
            header.Add(headeritem2);
            target.BuildLabelsTestable(header);
            Console.WriteLine(target.Result);
            Assert.AreEqual(target.Result, "\r\nlabel define lq1a aValue1 `\"label text 1\"' \r\nlabel values q1a lq1a\r\nlabel var q1a `\"question 1\"'\r\nlabel values q1b lq1b\r\nlabel var q1b `\"question 2\"'\r\n");
        }

        public class StataSuplierFake : StataSuplier
        {
            public void BuildMergeTestable(string parentPrimaryKeyName, string primaryKeyColumnName, string fileName)
            {
                base.BuildMerge(parentPrimaryKeyName, primaryKeyColumnName, fileName, FileType.Csv);
            }
            public void BuildLabelsTestable(HeaderCollection header)
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
