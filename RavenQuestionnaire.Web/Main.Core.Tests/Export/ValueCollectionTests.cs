using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Complete.Question;
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
    public class ValueCollectionTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void This_KeyIsPresent_ValueisReturned()
        {
            ValueCollectionTestable target = new ValueCollectionTestable();
            var key = Guid.NewGuid();
            var result = new string[] {"value"};
            target.Container.Add(key, result);
            Assert.AreEqual(target[key], result);
        }
        [Test]
        public void This_KeyIsAbsent_ValueEmptyStringISREturned()
        {
            ValueCollectionTestable target = new ValueCollectionTestable();
           
            Assert.AreEqual(target[Guid.NewGuid()], string.Empty);
        }
        [Test]
        public void Add_QuestionIsNull_EmptyStringIsAdded()
        {
            ValueCollectionTestable target = new ValueCollectionTestable();
            var key = Guid.NewGuid();
            target.Add(key,null);
            Assert.AreEqual(target.Container[key], new string[] { string.Empty });
        }
        [Test]
        public void Add_QuestionIsDisabled_EmptyStringIsAdded()
        {
            ValueCollectionTestable target = new ValueCollectionTestable();
            var key = Guid.NewGuid();
            target.Add(key, new TextCompleteQuestion("text") {PublicKey = key, Answer = "val", Enabled = false});
            Assert.AreEqual(target.Container[key], new string[] { string.Empty });
        }
        [Test]
        public void Add_QuestionIsDisabledMultyOptions_EmptyStringIsAdded()
        {
            ValueCollectionTestable target = new ValueCollectionTestable();
            var key = Guid.NewGuid();
            target.Add(key, new MultyOptionsCompleteQuestion("text") { PublicKey = key, Answers = new List<IAnswer>{new CompleteAnswer(),new CompleteAnswer()}, Enabled = false });
            Assert.AreEqual(target.Container[key].Count(),2);
            Assert.IsTrue(target.Container[key].All(t => t == string.Empty));
        }
        [Test]
        public void Add_SingleQuestion_AnswerValueIsAdded()
        {
            ValueCollectionTestable target = new ValueCollectionTestable();
            var key = Guid.NewGuid();
            target.Add(key, new TextCompleteQuestion("text"){ PublicKey = key,Answer = "val"});
            Assert.AreEqual(target.Container[key], new string[] { "val" });
        }
        [Test]
        public void Add_MultyQuestion_AnswerValueIsAdded()
        {
            ValueCollectionTestable target = new ValueCollectionTestable();
            var key = Guid.NewGuid();
            target.Add(key, new MultyOptionsCompleteQuestion() { QuestionType=QuestionType.MultyOption, PublicKey = key, Answers = new List<IAnswer>() { new CompleteAnswer(), new CompleteAnswer() } });
            Assert.AreEqual(target.Container[key].Count(), 2);
        }
        public class ValueCollectionTestable : ValueCollection
        {
            public IDictionary<Guid, IEnumerable<string>> Container
            {
                get { return this.container; }
            }
        }
    }
}
