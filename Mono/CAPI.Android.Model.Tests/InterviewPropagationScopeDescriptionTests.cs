using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Androids.Core.Model.Tests
{
    [TestFixture]
    public class InterviewPropagationScopeDescriptionTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void Add_ValidDate_ItemIsAddedToCollection()
        {
            var target = new InterviewPropagationScopeDescription();
            var key = Guid.NewGuid();
            var item = new QuestionnairePropagatedScreenViewModel(Guid.NewGuid(), "", "", true,
                                                                  new InterviewItemId(Guid.NewGuid(), null),
                                                                  Enumerable.Empty<IQuestionnaireItemViewModel>().ToList(),
                                                                  Enumerable.Empty<InterviewItemId>(), 0, 0, null, null);
            target.AddTemplateOfPropagatedScreen(key, item);
            Assert.AreEqual(item, target.GetTemplateOfPropagatedScreen(key));
        }
        [Test]
        public void GetItemsFromScope_ScopeIsAbsent_ExeptionIsThrowed()
        {
            var target = new InterviewPropagationScopeDescription();
            Assert.Throws<ArgumentException>(() => target.GetScreenSiblingsByPropagationLevel(Guid.NewGuid()));
        }

        [Test]
        public void GetItemsFromScope_OneItemIsPassed_AllOtherItemsFromScopeAreReturnedIncludingPassedItem()
        {
            var target = new InterviewPropagationScopeDescription();
            var scopeKey = Guid.NewGuid();
            var scopedItems = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            foreach (var scopedItem in scopedItems)
            {
                AddItemInCollections(target, scopedItem);
            }
            target.CreateScopeOfPropagatedScreens(scopeKey, scopedItems);
            IEnumerable<Guid> resultedScope = target.GetScreenSiblingsByPropagationLevel(scopedItems[1]);
            Assert.AreEqual(scopedItems.Count(), resultedScope.Count());
            int i = 0;
            foreach (var item in resultedScope)
            {
                Assert.AreEqual(item, scopedItems[i]);
                i++;

            }
        }
        [Test]
        public void Add_ValidData_SingleITemScopeIsCreatedWithSameGuidAsItemKey()
        {
            var target = new InterviewPropagationScopeDescription();
            var key = Guid.NewGuid();
            var item = new QuestionnairePropagatedScreenViewModel(Guid.NewGuid(), "", "", true,
                                                                  new InterviewItemId(Guid.NewGuid(), null),
                                                                  Enumerable.Empty<IQuestionnaireItemViewModel>().ToList(),
                                                                  Enumerable.Empty<InterviewItemId>(), 0, 0, null, null);
            target.AddTemplateOfPropagatedScreen(key, item);
            var result = target.GetTemplatesOfPropagatedScreensInScope(key);
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.First(), key);
        }
        [Test]
        public void AssignScope_TwoItemsInDifferentScopes_ScopesAreMeged()
        {
            var target = new InterviewPropagationScopeDescription();
            var key1 = Guid.NewGuid();
            var key2 = Guid.NewGuid();
            AddItemInCollections(target, key1);
            AddItemInCollections(target, key2);
            var newScope = Guid.NewGuid();
            target.CreateScopeOfPropagatedScreens(newScope, new Guid[] {key1, key2});

            var result = target.GetTemplatesOfPropagatedScreensInScope(newScope);
            Assert.AreEqual(result.Count(),2);
            Assert.IsTrue(result.Contains(key1));
            Assert.IsTrue(result.Contains(key2));
        }
        [Test]
        public void AssignScope_SingleItemsScopeExists_SingleITemScopeIsDeleted()
        {
            var target = new InterviewPropagationScopeDescription();
            var key1 = Guid.NewGuid();
            AddItemInCollections(target, key1);
            var newScope = Guid.NewGuid();
            target.CreateScopeOfPropagatedScreens(newScope, new Guid[] { key1});

            Assert.Throws<ArgumentException>(() => target.GetTemplatesOfPropagatedScreensInScope(key1));
        }
        [Test]
        public void Add_ItemAlreadyInScope_SecondScopeWasntCreated()
        {
            var target = new InterviewPropagationScopeDescription();
            var scopeKey = Guid.NewGuid();
            var itemKey = Guid.NewGuid();
            target.CreateScopeOfPropagatedScreens(scopeKey, new Guid[] {itemKey});
            AddItemInCollections(target, itemKey);
            Assert.Throws<ArgumentException>(() => target.GetTemplatesOfPropagatedScreensInScope(itemKey));
        }

        protected void AddItemInCollections(InterviewPropagationScopeDescription target,Guid key)
        {
            var item = new QuestionnairePropagatedScreenViewModel(Guid.NewGuid(), "", "", true,
                                                                 new InterviewItemId(Guid.NewGuid(), null),
                                                                 Enumerable.Empty<IQuestionnaireItemViewModel>().ToList(),
                                                                 Enumerable.Empty<InterviewItemId>(), 0, 0, null, null);
            target.AddTemplateOfPropagatedScreen(key, item);
        }
    }
}