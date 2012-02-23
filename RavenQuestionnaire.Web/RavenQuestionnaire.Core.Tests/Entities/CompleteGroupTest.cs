using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
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
        [Test]
        public void SubscribeOnGroupAdd_CorrectData_GroupIsAdded()
        {
            CompleteQuestionnaireDocument document = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionnaire = new CompleteQuestionnaire(document);

         //   Mock<IComposite> document = new Mock<IComposite>();
            CompleteGroup baseGroup = new CompleteGroup("target");
            document.Groups.Add(baseGroup);
           
       //     CompositeHandler handler = new CompositeHandler(document.Object);
            

            CompleteGroup targetGroup = new CompleteGroup("some group") { Propagated = Propagate.Propagated };
            targetGroup.Triggers.Add(baseGroup.PublicKey);
            document.Groups.Add(targetGroup);
           /* document.Setup(x => x.Find<CompleteGroup>(targetGroup.PublicKey)).Returns(targetGroup);
            GroupObserver observeble = new GroupObserver(targetGroup.PublicKey, target.PublicKey);
            observeble.Subscribe(handler);

            handler.Add(target);*/
            PropagatableCompleteGroup target = new PropagatableCompleteGroup(baseGroup, Guid.NewGuid());
            questionnaire.Add(target, null);
            Assert.AreEqual(document.Groups.Count, 4);
            questionnaire.Remove(target);
            Assert.AreEqual(document.Groups.Count, 2);
            /*   document.Verify(
                x =>
                x.Add(
                    It.Is<PropagatableCompleteGroup>(
                        a => a.PublicKey.Equals(targetGroup.PublicKey)), null), Times.Once());*/
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
            target.Groups.Add(propogatedGroup);
            var groupForRemove = new PropagatableCompleteGroup(propogatedGroup, Guid.NewGuid());
            target.Add(groupForRemove, null);
            Assert.AreEqual(target.Groups.Count, 2);
            target.Remove(groupForRemove);
            Assert.AreEqual(target.Groups.Count, 1);
        }
    }
}
