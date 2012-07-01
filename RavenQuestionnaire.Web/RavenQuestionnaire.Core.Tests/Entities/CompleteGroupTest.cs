using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Ninject;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;


namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class CompleteGroupTest
    {
        [SetUp]
        public void CreateObjects()
        {
            IKernel kernel = new StandardKernel();
         

        }
       
      /*  [Test]
        public void SubscribeOnGroupRemove_ForcingKeyIsIncorrect_GroupIsRemoved()
        {

            Mock<IComposite> document = new Mock<IComposite>();
            CompleteGroup target = new CompleteGroup("target");
            CompositeHandler handler = new CompositeHandler(document.Object);

            CompleteGroup targetGroup = new CompleteGroup("some group") { Propagated = Propagate .Propagated};
            document.Setup(x => x.Find<CompleteGroup>(targetGroup.PublicKey)).Returns(targetGroup);
            GroupObserver observeble = new GroupObserver(targetGroup.PublicKey, Guid.NewGuid());
            observeble.Subscribe(handler);
            handler.Remove(target);

            document.Verify(x => x.Remove(targetGroup), Times.Never());
        }
        [Test]
        public void Unsubscribe_CorrectData_EventIsNotFired()
        {

            Mock<IComposite> document = new Mock<IComposite>();
            CompleteGroup target = new CompleteGroup("target");
            CompositeHandler handler = new CompositeHandler(document.Object);

            CompleteGroup targetGroup = new CompleteGroup("some group") { Propagated = Propagate.Propagated };
            document.Setup(x => x.Find<CompleteGroup>(targetGroup.PublicKey)).Returns(targetGroup);
            GroupObserver observeble = new GroupObserver(targetGroup.PublicKey, target.PublicKey);

            observeble.Subscribe(handler);
            observeble.Unsubscribe();
            
            handler.Add(target);
            handler.Remove(target);

            document.Verify(x => x.Remove(targetGroup), Times.Never());
            document.Verify(x => x.Add(targetGroup,null), Times.Never());
        }*/
        [Test]
        public void RemoveGroup_Propagated_GroupIsRemoved()
        {
            CompleteGroup target = new CompleteGroup("target");
            var propogatedGroup = new CompleteGroup("propagated") {Propagated = Propagate.Propagated};
            target.Children.Add(propogatedGroup);
            var groupForRemove = new CompleteGroup(propogatedGroup, Guid.NewGuid());
            target.Add(groupForRemove, null);
            Assert.AreEqual(target.Children.Count, 2);
            target.Remove(groupForRemove);
            Assert.AreEqual(target.Children.Count, 1);
        }
    }
}
