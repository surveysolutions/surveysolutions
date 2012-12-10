// -----------------------------------------------------------------------
// <copyright file="QuestionnaireTreeViewTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.View.ScreenByScreenNavigation;
using NUnit.Framework;

namespace Main.Core.Tests.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class QuestionnaireTreeViewTests
    {
        [Test]
        public void AddItem_ParentIsNull_ElementISAddedOnFirstLevel()
        {
            Dictionary<GroupKey, TreeViewItem> treeHash=new Dictionary<GroupKey, TreeViewItem>();
            List<TreeViewItem> children=new List<TreeViewItem>();
            QuestionnaireTreeView target = new QuestionnaireTreeView(treeHash, children);
            var key = new GroupKey(Guid.NewGuid(), null);
            target.AddItem(new TreeViewItem(key, TreeViewItemType.Leaf, "title"));
            
            Assert.IsTrue(treeHash.ContainsKey(key));
            Assert.IsTrue(treeHash[key].ItemTitle == "title");

            Assert.IsTrue(children[0].Key.PublicKey == key.PublicKey);
        }

        [Test]
        public void AddItem_ParentIsNotNull_ElementISAddedToParent()
        {

            Dictionary<GroupKey, TreeViewItem> treeHash = new Dictionary<GroupKey, TreeViewItem>();
            List<TreeViewItem> children = new List<TreeViewItem>();
            QuestionnaireTreeView target = new QuestionnaireTreeView(treeHash, children);
            var key = new GroupKey(Guid.NewGuid(), null);
            var parent = new TreeViewItem(new GroupKey(Guid.NewGuid(), null), TreeViewItemType.Group, "parent");
            target.AddItem(parent);
            var node = parent.Add(key, TreeViewItemType.Leaf, "title");

            Assert.IsTrue(parent.Children[0].Key.PublicKey == key.PublicKey);
            Assert.IsTrue(node.Parent == parent);
        }

        [Test]
        public void GetItemByKey_KeyIsAbsent_NullIsReturned()
        {
            Dictionary<GroupKey, TreeViewItem> treeHash = new Dictionary<GroupKey, TreeViewItem>();
            List<TreeViewItem> children = new List<TreeViewItem>();
            QuestionnaireTreeView target = new QuestionnaireTreeView(treeHash, children);
            var node = target.GetItemByKey(new GroupKey(Guid.NewGuid(), null));

            Assert.IsTrue(node==null);
        }
        [Test]
        public void GetItemByKey_KeyIsPresent_NodeIsReturned()
        {
            Dictionary<GroupKey, TreeViewItem> treeHash = new Dictionary<GroupKey, TreeViewItem>();
            List<TreeViewItem> children = new List<TreeViewItem>();
            QuestionnaireTreeView target = new QuestionnaireTreeView(treeHash, children);
            var key = new GroupKey(Guid.NewGuid(), null);
            var parent = new TreeViewItem(key, TreeViewItemType.Group, "object for search");
            treeHash.Add(key,parent);
            var node = target.GetItemByKey(key);

            Assert.IsTrue(node == parent);
        }

        [Test]
        public void RemoveItem_KeyIsAbsent_ArgumentException()
        {
            Dictionary<GroupKey, TreeViewItem> treeHash = new Dictionary<GroupKey, TreeViewItem>();
            List<TreeViewItem> children = new List<TreeViewItem>();
            QuestionnaireTreeView target = new QuestionnaireTreeView(treeHash, children);

            Assert.Throws<ArgumentException>(() => target.RemoveItem(new GroupKey(Guid.NewGuid(), null)));
        }
        [Test]
        public void RemoveItem_KeyIsPresentParentISAbsent_ItemISRemoved()
        {
            Dictionary<GroupKey, TreeViewItem> treeHash = new Dictionary<GroupKey, TreeViewItem>();
            List<TreeViewItem> children = new List<TreeViewItem>();
            QuestionnaireTreeView target = new QuestionnaireTreeView(treeHash, children);

            var key = new GroupKey(Guid.NewGuid(), null);
            var parent = new TreeViewItem(key, TreeViewItemType.Group, "object for search");
            

            treeHash.Add(key,parent);
            children.Add(parent);

            target.RemoveItem(key);

            Assert.IsTrue(treeHash.Count==0);
            Assert.IsTrue(children.Count == 0);
        }
        [Test]
        public void RemoveItem_KeyIsPresentParentISPresentt_ItemISRemovedParentISStillInTree()
        {
            Dictionary<GroupKey, TreeViewItem> treeHash = new Dictionary<GroupKey, TreeViewItem>();
            List<TreeViewItem> children = new List<TreeViewItem>();
            QuestionnaireTreeView target = new QuestionnaireTreeView(treeHash, children);

            var key = new GroupKey(Guid.NewGuid(), Guid.NewGuid());
            var parent = new TreeViewItem(new GroupKey(Guid.NewGuid(), null), TreeViewItemType.Group, "parent");
            var objectForDelete = parent.Add(key, TreeViewItemType.Group, "object for search");

            target.Children.Add(parent);
            treeHash.Add(key, objectForDelete);
            treeHash.Add(parent.Key, parent);

            target.RemoveItem(key);

            Assert.IsTrue(treeHash.Count == 1);
            Assert.IsTrue(parent.Children.Count == 0);
            Assert.IsTrue(treeHash.First().Value.Key == parent.Key);
        }
        [Test]
        public void RemoveItem_GroupWithChildren_ItemISRemovedWithAllChildren()
        {
            Dictionary<GroupKey, TreeViewItem> treeHash = new Dictionary<GroupKey, TreeViewItem>();
            List<TreeViewItem> children = new List<TreeViewItem>();
            QuestionnaireTreeView target = new QuestionnaireTreeView(treeHash, children);

            var key = new GroupKey(Guid.NewGuid(), Guid.NewGuid());
            var parentKey = new GroupKey(Guid.NewGuid(), null);
            var parent = new TreeViewItem(parentKey, TreeViewItemType.Group, "parent");
            var child = parent.Add(key, TreeViewItemType.Group, "object for search");

            target.Children.Add(parent);
            treeHash.Add(parentKey, parent);
            treeHash.Add(key, child);

            target.RemoveItem(parentKey);

            Assert.IsTrue(treeHash.Count == 0);
            Assert.IsTrue(children.Count == 0);
        }
    }
}
