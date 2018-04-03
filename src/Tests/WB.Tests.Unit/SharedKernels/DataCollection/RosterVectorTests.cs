using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(RosterVector))]
    public class RosterVectorTests
    {
        [Test]
        public void ConvertToArray_should_convert_collections_to_roster_vector_array_from_readonly_collection()
        {
            //arrange
            var list = new List<RosterVector>()
            {
                Create.Entity.RosterVector(2),
                Create.Entity.RosterVector(5)
            };
            var readonlyCollection = new ReadOnlyCollection<RosterVector>(list);

            //act
            var rosterVectors = RosterVector.ConvertToArray(readonlyCollection);

            //assert
            Assert.That(rosterVectors.Length, Is.EqualTo(2));
            Assert.That(rosterVectors, Is.EqualTo(new[] { Create.Entity.RosterVector(2), Create.Entity.RosterVector(5) }));
        }

        [Test]
        public void Take_when_provided_length_larger_than_own_length_Should_return_self()
        {
            var rosterVector = Create.RosterVector(1,2);
            var shrinkedVector = rosterVector.Take(5);

            Assert.That(shrinkedVector, Is.EqualTo(rosterVector));
        }

    }
}