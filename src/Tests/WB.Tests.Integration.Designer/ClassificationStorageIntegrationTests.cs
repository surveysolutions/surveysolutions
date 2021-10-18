using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Classifications;

namespace WB.Tests.Integration.Designer
{
    [TestFixture]
    [TestOf(typeof(ClassificationsStorage))]
    public class ClassificationStorageIntegrationTests : IntegrationTest
    {
        [Test]
        public async Task When_getting_classifications_for_user()
        {
            // arrange
            var g1 = Guid.Parse("11111111111111111111111111111111");
            var g2 = Guid.Parse("22222222222222222222222222222222");
            var g3 = Guid.Parse("33333333333333333333333333333333");
            var g4 = Guid.Parse("44444444444444444444444444444444");
            var g5 = Guid.Parse("55555555555555555555555555555555");
            var g6 = Guid.Parse("66666666666666666666666666666666");
            var gA = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var gB = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            await RunActionInScopeAsync(locator =>
            {
                var accessor = locator.GetInstance<IClassificationsStorage>();

                var entities = new[]
                {
                    Create.Classification.Group(id: g1, title: "A"),
                    Create.Classification.Group(id: g2, title: "B"),
                    Create.Classification.Classification(id: g5, title: "E", userId: gA, parent: g1),
                    Create.Classification.Classification(id: g3, title: "C", parent: g1),
                    Create.Classification.Classification(id: g4, title: "D", userId: gB, parent: g1),
                    Create.Classification.Classification(id: g6, title: "F", parent: g2),
                    Create.Classification.Category()
                };

                accessor.Store(entities);
                return Task.CompletedTask;
            });

            List<Classification> classifications = null;

            await RunActionInScopeAsync(async locator =>
            {
                var classificationStorage = locator.GetInstance<IClassificationsStorage>();
                classifications = (await classificationStorage.GetClassifications(g1, userId: gA)).ToList();
            });

            Assert.That(classifications.Count, Is.EqualTo(2));
            CollectionAssert.IsOrdered(classifications.Select(x => x.Title));
            CollectionAssert.DoesNotContain(classifications.Select(x => x.Id), g4);
            CollectionAssert.AllItemsAreInstancesOfType(classifications, typeof(Classification));
        }
    }
}
