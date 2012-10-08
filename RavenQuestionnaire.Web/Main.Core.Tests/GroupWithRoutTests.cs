// -----------------------------------------------------------------------
// <copyright file="GroupWithRoutTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Utility;
using NUnit.Framework;

namespace Main.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class GroupWithRoutTests
    {
        [Test]
        public void CreateGroupWithRout_GroupGuidsAreEmpty_FirstGroupSelected()
        {
            CompleteGroup doc = new CompleteGroup("test");
            doc.Children.Add(new CompleteGroup("sub1"));
            doc.Children.Add(new CompleteGroup("sub2"));
            GroupWithRout target = new GroupWithRout(doc, null, null);
            Assert.IsTrue(target.Group == doc.Children[0]);
            var rout = target.CurrentRout.ToList();
            Assert.IsTrue(rout[0].Group.PublicKey == doc.PublicKey);
            Assert.IsTrue(rout[0].Level == 0);
            Assert.IsTrue(rout[1].Group.PublicKey == doc.Children[0].PublicKey);
            Assert.IsTrue(rout[1].Level == 1);

        }

        [Test]
        public void CreateGroupWithRout_GroupDoesntExists_FirstGroupSelected()
        {
            CompleteGroup doc = new CompleteGroup("test");
            doc.Children.Add(new CompleteGroup("sub1"));
            doc.Children.Add(new CompleteGroup("sub2"));
            GroupWithRout target = new GroupWithRout(doc, Guid.NewGuid(), Guid.NewGuid());
            Assert.IsTrue(target.Group == doc.Children[0]);
        }

        [Test]
        public void CreateGroupWithRout_GroupExists_GroupReturned()
        {
            CompleteGroup doc = new CompleteGroup("test");
            doc.Children.Add(new CompleteGroup("sub1"));
            doc.Children.Add(new CompleteGroup("sub2"));
            GroupWithRout target = new GroupWithRout(doc, doc.Children[1].PublicKey, null);
            Assert.IsTrue(target.Group == doc.Children[1]);

            var rout = target.CurrentRout.ToList();
            Assert.IsTrue(rout[0].Group.PublicKey == doc.PublicKey);
            Assert.IsTrue(rout[0].Level == 0);
            Assert.IsTrue(rout[1].Group.PublicKey == doc.Children[1].PublicKey);
            Assert.IsTrue(rout[1].Level == 1);
        }

        [Test]
        public void CreateGroupWithRout_GroupExistsWithPropagationKey_GroupReturned()
        {
            CompleteGroup doc = new CompleteGroup("test");
            doc.Children.Add(new CompleteGroup("sub1"));
            var groupForReturn = new CompleteGroup("sub2") {PropogationPublicKey = Guid.NewGuid()};
            doc.Children.Add(groupForReturn);
            GroupWithRout target = new GroupWithRout(doc, groupForReturn.PublicKey, groupForReturn.PropogationPublicKey);
            Assert.IsTrue(target.Group == groupForReturn);
        }

        [Test]
        public void CreateGroupWithRout_GroupExistsWithUnExiteblePropagationKey_FirstGroupSelected()
        {
            CompleteGroup doc = new CompleteGroup("test");
            doc.Children.Add(new CompleteGroup("sub1"));
            var groupForReturn = new CompleteGroup("sub2") {PropogationPublicKey = Guid.NewGuid()};
            doc.Children.Add(groupForReturn);
            GroupWithRout target = new GroupWithRout(doc, groupForReturn.PublicKey, Guid.NewGuid());
            Assert.IsTrue(target.Group == doc.Children[0]);
        }

        [Test]
        public void CompileNavigation_ForPropagatedGroupSingleGroup_NextAndPrevAreEmpty()
        {
            CompleteGroup doc = new CompleteGroup("test") {PropogationPublicKey = Guid.NewGuid()};
            CompleteGroup parent = new CompleteGroup("top");
            parent.Children.Add(doc);
            List<NodeWithLevel> rout = new List<NodeWithLevel>();
            rout.Add(new NodeWithLevel(parent, 0));
            rout.Add(new NodeWithLevel(doc, 1));
            var navigation = new GroupWithRout(rout, doc).Navigation;
            Assert.IsNull(navigation.NextScreen);
            Assert.IsNull(navigation.PrevScreen);
        }
        [Test]
        public void CompileNavigation_ForPropagatedGroupLeftGroup_PrevIsEmpty()
        {
            CompleteGroup doc = new CompleteGroup("test") { PropogationPublicKey = Guid.NewGuid() };
            CompleteGroup parent = new CompleteGroup("top");
            parent.Children.Add(doc);
            parent.Children.Add(new CompleteGroup("test") { PropogationPublicKey = Guid.NewGuid(), PublicKey = doc.PublicKey });
            List<NodeWithLevel> rout = new List<NodeWithLevel>();
            rout.Add(new NodeWithLevel(parent, 0));
            rout.Add(new NodeWithLevel(doc, 1));
            var navigation = new GroupWithRout(rout, doc).Navigation;
            Assert.IsTrue(navigation.NextScreen.PublicKey==parent.Children[1].PublicKey);
            Assert.IsNull(navigation.PrevScreen);
        }
        [Test]
        public void CompileNavigation_ForPropagatedGroupLeftGroupRightGroupIsDisabled_NextAndPrevAreEmpty()
        {
            CompleteGroup doc = new CompleteGroup("test") { PropogationPublicKey = Guid.NewGuid() };
            CompleteGroup parent = new CompleteGroup("top");
            parent.Children.Add(doc);
            parent.Children.Add(new CompleteGroup("test") { PropogationPublicKey = Guid.NewGuid(), PublicKey = doc.PublicKey, Enabled = false});
            List<NodeWithLevel> rout = new List<NodeWithLevel>();
            rout.Add(new NodeWithLevel(parent, 0));
            rout.Add(new NodeWithLevel(doc, 1));
            var navigation = new GroupWithRout(rout, doc).Navigation;
            Assert.IsNull(navigation.NextScreen);
            Assert.IsNull(navigation.PrevScreen);
        }
        [Test]
        public void CompileNavigation_ForPropagatedGroupRightGroup_NextIsEmpty()
        {
            CompleteGroup doc = new CompleteGroup("test") { PropogationPublicKey = Guid.NewGuid() };
            CompleteGroup parent = new CompleteGroup("top");
            
            parent.Children.Add(new CompleteGroup("test") { PropogationPublicKey = Guid.NewGuid(), PublicKey = doc.PublicKey });
            parent.Children.Add(doc);
            List<NodeWithLevel> rout = new List<NodeWithLevel>();
            rout.Add(new NodeWithLevel(parent, 0));
            rout.Add(new NodeWithLevel(doc, 1));
            var navigation = new GroupWithRout(rout, doc).Navigation;
            Assert.IsTrue(navigation.PrevScreen.PublicKey == parent.Children[0].PublicKey);
            Assert.IsNull(navigation.NextScreen);
        }
    }
}
