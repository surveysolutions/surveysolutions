using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(Identity))]
    public class IdentityTests
    {
        [Test]
        public void Two_identities_with_different_groupId_should_have_different_HashCode()
        {
            Identity identity1 = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
            Identity identity2 = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"));

            Assert.That(identity1.Equals(identity2), Is.False);
            Assert.That(identity1.GetHashCode(), Is.Not.EqualTo(identity2.GetHashCode()));
        }

        [Test]
        public void Two_identities_with_different_roster_vectors_have_different_HashCode()
        {
            Identity identity1 = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Create.Entity.RosterVector(1));
            Identity identity2 = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Create.Entity.RosterVector(2));

            Assert.That(identity1.Equals(identity2), Is.False);
            Assert.That(identity1.GetHashCode(), Is.Not.EqualTo(identity2.GetHashCode()));
        }

        [Test]
        public void Two_identities_with_should_be_equal()
        {
            Identity identity1 = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Create.Entity.RosterVector(1));
            Identity identity2 = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Create.Entity.RosterVector(1));

            Assert.That(identity1.Equals(identity2), Is.True);
            Assert.That(identity1.GetHashCode(), Is.EqualTo(identity2.GetHashCode()));
        }
    }
}