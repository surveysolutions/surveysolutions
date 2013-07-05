using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
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
    public class HeaderCollectionTests
    {
        [Test]
        public void GetEnumerator_DictionaryContans1ItemWith2Values_EnumeratorWith2ItesIsReturned()
        {
            var hash = new Dictionary<Guid, IEnumerable<HeaderItem>>();
            hash.Add(Guid.NewGuid(),
                     new HeaderItem[] {new HeaderItem(new SingleQuestion()), new HeaderItem(new SingleQuestion())});
            HeaderCollectionTest test = new HeaderCollectionTest(hash);
            Assert.AreEqual(test.Count(), 2);
        }
        [Test]
        public void Add_SingleQuestion_OneHeaderItemIsAdded()
        {
            var hash = new Dictionary<Guid, IEnumerable<HeaderItem>>();
            HeaderCollectionTest test = new HeaderCollectionTest(hash);
            Guid questionGuid = Guid.NewGuid();
            test.Add(new SingleQuestion(questionGuid, "some question"));
            Assert.AreEqual(hash.Count, 1);
            Assert.AreEqual(hash.First().Key, questionGuid);
        }
        [Test]
        public void Add_MultyQuestion_TwoHeaderItemIsAdded()
        {
            var hash = new Dictionary<Guid, IEnumerable<HeaderItem>>();
            HeaderCollectionTest test = new HeaderCollectionTest(hash);
            Guid questionGuid = Guid.NewGuid();
            var question = new MultyOptionsQuestion("some question") {PublicKey = questionGuid};
            question.Answers = new List<IAnswer>() {new Answer(), new Answer()};
            test.Add(question);
            Assert.AreEqual(test.Count(), 2);
            Assert.AreEqual(hash.First().Key, questionGuid);
        }
        public class HeaderCollectionTest : HeaderCollection
        {
            public HeaderCollectionTest(IDictionary<Guid, IEnumerable<HeaderItem>> hash)
            {
                this.container = hash;
            }
        }
    }
}
