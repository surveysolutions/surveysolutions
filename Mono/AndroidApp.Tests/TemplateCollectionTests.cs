using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using NUnit.Framework;

namespace AndroidApp.Core.Model.Tests
{
    [TestFixture]
    public class TemplateCollectionTests
    {
        [Test]
        public void Add_ValidDate_ItemIsAddedToCollection()
        {
            var target = new TemplateCollection();
            var key = Guid.NewGuid();
            var item = new QuestionnairePropagatedScreenViewModel(Guid.NewGuid(), "", "", true,
                                                                  new ItemPublicKey(Guid.NewGuid(), null),
                                                                  Enumerable.Empty<IQuestionnaireItemViewModel>(),
                                                                  Enumerable.Empty<ItemPublicKey>(), 0, 0, null, null);
            target.Add(key, item);
            Assert.AreEqual(item, target[key]);
        }
        [Test]
        public void AssignScope_FewITemsInSope_ScopeIsAssigned()
        {
            var target = new TemplateCollection();
            var scopeKey = Guid.NewGuid();
            var scopedItems = new Guid[] {Guid.NewGuid()};
            target.AssignScope(scopeKey, scopedItems);
            IEnumerable<Guid> resultedScope = target.GetItemsInScope(scopeKey);
            Assert.AreEqual(scopedItems.Count(), resultedScope.Count());
            foreach (var item in resultedScope)
            {
                Assert.IsTrue(scopedItems.Contains(item));
            }
        }
        [Test]
        public void GetItemsFromScope_ScopeIsAbsent_EmptyListIsReturned()
        {
            var target = new TemplateCollection();
            IEnumerable<Guid> resultedScope = target.GetItemsFromScope(Guid.NewGuid());
            Assert.AreEqual(resultedScope.Count(),0);
        }

        [Test]
        public void GetItemsFromScope_OneItemIsPassed_AllOtherItemsFromScopeAreReturnedIncludingPassedItem()
        {
            var target = new TemplateCollection();
            var scopeKey = Guid.NewGuid();
            var scopedItems = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            target.AssignScope(scopeKey, scopedItems);
            IEnumerable<Guid> resultedScope = target.GetItemsFromScope(scopedItems[1]);
            Assert.AreEqual(scopedItems.Count(), resultedScope.Count());
            int i = 0;
            foreach (var item in resultedScope)
            {
                Assert.AreEqual(item, scopedItems[i]);
                i++;

            }
        }
    }
}