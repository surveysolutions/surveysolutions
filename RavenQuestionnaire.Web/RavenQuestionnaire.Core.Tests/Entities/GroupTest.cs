using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class GroupTest
    {
        [Test]
        public void ObserveChanges_All_ObserversNotified()
        {
            Group target = new Group("under test");
            var test = false;
            //   Observable.FromEventPattern<CompositeAddedEventArgs>(target.s)
            var observer =
                target.Where(e => e is CompositeAddedEventArgs).Select(e => e as CompositeAddedEventArgs).Where(e=>e.AddedComposite is Question).Subscribe(
                    Observer.Create<CompositeAddedEventArgs>(
                        (a) => { Assert.AreEqual(((Question)a.AddedComposite).QuestionText, "se");
                                   test = true;
                        }));
            target.Add(new Question("se", QuestionType.Text), null);
            Assert.IsTrue(test);
            target.Add(new Group("test"), null);
            observer.Dispose();
            target.Add(new Question("sea", QuestionType.Text), null);
            test = false;
            var observerRemove =
                target.Where(e => e is CompositeRemovedEventArgs).Select(e => e as CompositeRemovedEventArgs).Subscribe(
                    Observer.Create<CompositeRemovedEventArgs>(
                        (a) => { Assert.IsNotNull(a);
                                   test = true;
                        }));
            target.Remove(target.Children[0]);
            Assert.IsTrue(test);
            //target.Subscribe()
        }

        [Test]
        public void kObserveChanges_InnerObjects_ObserversNotified()
        {
            Group target = new Group("under test");
            var test = false;
            //   Observable.FromEventPattern<CompositeAddedEventArgs>(target.s)
            var observer =
                target.Where(e => e is CompositeAddedEventArgs).Select(e => e as CompositeAddedEventArgs).Where(e => e.AddedComposite is Question).Subscribe(
                    Observer.Create<CompositeAddedEventArgs>(
                        (a) =>
                        {
                            Assert.AreEqual(((Question)a.AddedComposite).QuestionText, "se");
                            test = true;
                        }));
            target.Add(new Group("test"), null);
            target.Add(new Question("se", QuestionType.Text), target.Children[0].PublicKey);
            Assert.IsTrue(test);
        }
       
    }
}
