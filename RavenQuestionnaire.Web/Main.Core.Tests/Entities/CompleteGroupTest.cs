using Microsoft.Practices.ServiceLocation;
using Moq;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    using System;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    using Ninject;

    using NUnit.Framework;

    /// <summary>
    /// The complete group test.
    /// </summary>
    [TestFixture]
    public class CompleteGroupTest
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
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

        /// <summary>
        /// The remove group_ propagated_ group is removed.
        /// </summary>
        [Test]
        public void RemoveGroup_Propagated_GroupIsRemoved()
        {
            var target = new CompleteGroup("target");
            var propogatedGroup = new CompleteGroup("propagated") { Propagated = Propagate.Propagated };
            target.Children.Add(propogatedGroup);
            var groupForRemove = new CompleteGroup(propogatedGroup, Guid.NewGuid());
            target.Children.Add(groupForRemove);
            Assert.AreEqual(target.Children.Count, 2);
            target.Children.Remove(groupForRemove);
            Assert.AreEqual(target.Children.Count, 1);
        }

        #endregion
    }
}