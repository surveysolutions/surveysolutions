using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ClassificationsTests
{

    //TODO: Core migration - fix 
    //[TestFixture]
    //public class ClassificationStorageIntegrationTests : IntegrationTest
    //{
    //    [Test]
    //    public async Task When_getting_classifications_for_user()
    //    {
    //        RunActionInScope(locator =>
    //        {
    //            var accessor = locator.GetInstance<IPlainStorageAccessor<ClassificationEntity>>();

    //            var entities = new[]
    //            {
    //                Entity.Classification.Group(id: Id.g1, title: "A"),
    //                Entity.Classification.Group(id: Id.g2, title: "B"),
    //                Entity.Classification.Classification(id: Id.g5, title: "E", userId: Id.gA, parent: Id.g1),
    //                Entity.Classification.Classification(id: Id.g3, title: "C", parent: Id.g1),
    //                Entity.Classification.Classification(id: Id.g4, title: "D", userId: Id.gB, parent: Id.g1),
    //                Entity.Classification.Classification(id: Id.g6, title: "F", parent: Id.g2),
    //                Entity.Classification.Category()
    //            };

    //            accessor.Store(entities.Select(x => new Tuple<ClassificationEntity, object>(x, x.Id)));
    //        });

    //        List<Classification> classifications = null;

    //        await RunActionInScopeAsync(async locator =>
    //        {
    //            var classificationStorage = locator.GetInstance<IClassificationsStorage>();
    //            classifications = (await classificationStorage.GetClassifications(Id.g1, userId: Id.gA)).ToList();
    //        });

    //        Assert.That(classifications.Count, Is.EqualTo(2));
    //        CollectionAssert.IsOrdered(classifications.Select(x => x.Title));
    //        CollectionAssert.DoesNotContain(classifications.Select(x => x.Id), Id.g4);
    //        CollectionAssert.AllItemsAreInstancesOfType(classifications, typeof(Classification));
    //    }
    //}
}
