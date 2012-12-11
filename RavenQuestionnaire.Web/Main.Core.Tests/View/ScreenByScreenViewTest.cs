// -----------------------------------------------------------------------
// <copyright file="ScreenByScreenViewTest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.View.ScreenByScreenNavigation;
using Moq;
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
    public class ScreenByScreenViewTest
    {
        [Test]
        public void AddNewScreen_TemplateISAbsent_ArgumentException()
        {
            Mock<IQuestionnaireTreeView> hierarchy = new Mock<IQuestionnaireTreeView>();
            ScreenByScreenView target = new ScreenByScreenView(new Dictionary<GroupKey, GroupScreenView>(),
                                                               new Dictionary<GroupKey, QuestionScreenView>(),
                                                               hierarchy.Object);
            Assert.Throws<ArgumentException>(() => target.AddNewScreen(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Test]
        public void AddNewScreen_TemplateISGroupWithSubItems_ScreenAddedWithAllSubItems()
        {
            Mock<IQuestionnaireTreeView> hierarchy = new Mock<IQuestionnaireTreeView>();
            var screens = new Dictionary<GroupKey, GroupScreenView>();
            ScreenByScreenView target = new ScreenByScreenView(screens,
                                                               new Dictionary<GroupKey, QuestionScreenView>(),
                                                               hierarchy.Object);
            var publicKey =Guid.NewGuid();
            var template = new TreeViewItem(new GroupKey(publicKey, null), TreeViewItemType.Group, "template");
            var subTemplate1 = template.Add(new GroupKey(Guid.NewGuid(), null), TreeViewItemType.Group, "subTemplate1");
            var subTemplate2 = template.Add(new GroupKey(Guid.NewGuid(), null), TreeViewItemType.Group, "subTemplate2");

        /*    template.Children.Add(subTemplate1);
            template.Children.Add(subTemplate2);*/
            var propagationKey = Guid.NewGuid();
            hierarchy.Setup(x => x.GetItemByKey(new GroupKey(publicKey, null))).Returns(template);
            var groupKey = new GroupKey(publicKey, propagationKey);

      //      var clonedParent = new TreeViewItem(groupKey, TreeViewItemType.Group, "template");
           /* hierarchy.Setup(x => x.AddItem(groupKey, TreeViewItemType.Group, "template", null)).Returns(
                clonedParent);


            hierarchy.Setup(x => x.AddItem(new GroupKey(subTemplate1.Key.PublicKey, propagationKey), TreeViewItemType.Group, "subTemplate1", clonedParent.Key)).Returns(
               new TreeViewItem(hierarchy.Object, new GroupKey(subTemplate1.Key.PublicKey, propagationKey), TreeViewItemType.Group, "subTemplate1", clonedParent.Key));
            hierarchy.Setup(x => x.AddItem(new GroupKey(subTemplate2.Key.PublicKey, propagationKey), TreeViewItemType.Group, "subTemplate2", clonedParent.Key)).Returns(
               new TreeViewItem(hierarchy.Object, new GroupKey(subTemplate2.Key.PublicKey, propagationKey), TreeViewItemType.Group, "subTemplate2", clonedParent.Key));
            */
            screens.Add(new GroupKey(publicKey, null), new GroupScreenView(publicKey, null, "desc", "template", true));
            screens.Add(subTemplate1.Key, new GroupScreenView(subTemplate1.Key.PublicKey, null, "desc1", "template1", true));
            screens.Add(subTemplate2.Key, new GroupScreenView(subTemplate2.Key.PublicKey, null, "desc2", "template2", true));
            target.AddNewScreen(publicKey, propagationKey);
            hierarchy.Verify(
                x => x.AddItem(It.Is<TreeViewItem>(i => i.Key == new GroupKey(publicKey, propagationKey))),
                Times.Once());
            Assert.IsTrue(screens.Count == 6);
          //  Assert.IsTrue(screens.ContainsKey(groupKey));
        }

        [Test]
        public void AddNewScreen_TemplateISQuestion_ScreenAdded()
        {
            Mock<IQuestionnaireTreeView> hierarchy = new Mock<IQuestionnaireTreeView>();
            var questions = new Dictionary<GroupKey, QuestionScreenView>();
            ScreenByScreenView target = new ScreenByScreenView(new Dictionary<GroupKey, GroupScreenView>(),
                                                               questions,
                                                               hierarchy.Object);
            var publicKey = Guid.NewGuid();
            var template = new TreeViewItem( new GroupKey(publicKey, null), TreeViewItemType.Leaf, "template");
            var propagationKey = Guid.NewGuid();
            hierarchy.Setup(x => x.GetItemByKey(new GroupKey(publicKey, null))).Returns(template);
            var groupKey = new GroupKey(publicKey, propagationKey);
           /* hierarchy.Setup(x => x.AddItem(groupKey, TreeViewItemType.Leaf, "template", null)).Returns(
                new TreeViewItem(hierarchy.Object, groupKey, TreeViewItemType.Leaf, "template", null));*/

            questions.Add(new GroupKey(publicKey, null), new QuestionScreenView());
            target.AddNewScreen(publicKey, propagationKey);
         /*   hierarchy.Verify(
                x => x.AddItem(groupKey, TreeViewItemType.Leaf, "template", null),
                Times.Once());*/
            Assert.IsTrue(questions.Count == 2);
            Assert.IsTrue(questions.ContainsKey(groupKey));
        }

        [Test]
        public void RemovePropagatedScreen_EmptyGroup_GroupIsRemoved()
        {
            Mock<IQuestionnaireTreeView> hierarchy = new Mock<IQuestionnaireTreeView>();
            var screens = new Dictionary<GroupKey, GroupScreenView>();
            ScreenByScreenView target = new ScreenByScreenView(screens,
                                                               new Dictionary<GroupKey, QuestionScreenView>(),
                                                               hierarchy.Object);
            var publicKey = Guid.NewGuid();
            var propagationKey = Guid.NewGuid();
            var template = new TreeViewItem(new GroupKey(publicKey, propagationKey), TreeViewItemType.Group, "template");
            screens.Add(template.Key, new GroupScreenView(publicKey, propagationKey, "desc", "template", true));

            hierarchy.Setup(x => x.GetItemByKey(template.Key)).Returns(template);

            target.RemovePropagatedScreen(publicKey, propagationKey);
            hierarchy.Verify(x => x.RemoveItem(template.Key), Times.Once());
            Assert.IsTrue(screens.Count == 0);
        }

        [Test]
        public void RemovePropagatedScreen_GroupWithOneQuestionAndSubGroup_GroupIsRemoved()
        {
            Mock<IQuestionnaireTreeView> hierarchy = new Mock<IQuestionnaireTreeView>();
            var screens = new Dictionary<GroupKey, GroupScreenView>();
            var questions = new Dictionary<GroupKey, QuestionScreenView>();
            ScreenByScreenView target = new ScreenByScreenView(screens,
                                                               questions,
                                                               hierarchy.Object);
            var publicKey = Guid.NewGuid();
            var propagationKey = Guid.NewGuid();
            var template = new TreeViewItem(new GroupKey(publicKey, propagationKey), TreeViewItemType.Group, "template");
            var subGroup = template.Add(new GroupKey(Guid.NewGuid(), Guid.NewGuid()), TreeViewItemType.Group, "sub group");
            var subQuestion = template.Add(new GroupKey(Guid.NewGuid(), Guid.NewGuid()), TreeViewItemType.Leaf, "question");
            template.Children.Add(subGroup);
            template.Children.Add(subQuestion);

            screens.Add(template.Key, new GroupScreenView(publicKey, propagationKey, "desc", "template", true));
            screens.Add(subGroup.Key, new GroupScreenView(subGroup.Key.PublicKey, subGroup.Key.PropagationKey, "desc", "sub group", true));
            questions.Add(subQuestion.Key,new QuestionScreenView());

            hierarchy.Setup(x => x.GetItemByKey(template.Key)).Returns(template);

            target.RemovePropagatedScreen(publicKey, propagationKey);
            hierarchy.Verify(x => x.RemoveItem(template.Key), Times.Once());
            Assert.IsTrue(screens.Count == 0);
            Assert.IsTrue(questions.Count == 0);
        }
    }
}
