using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(Identity))]
    public class IdentityTests
    {
        [Test]
        public void Two_identities_with_different_groupId_should_have_different_HashCode()
        {
            Identity identity1 = Create.Identity(Id.gA);
            Identity identity2 = Create.Identity(Id.gB);

            Assert.That(identity1, Is.Not.EqualTo(identity2));
            var hash1 = identity1.GetHashCode();
            var hash2 = identity2.GetHashCode();
            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void Two_identities_with_different_roster_vectors_have_different_HashCode()
        {
            Identity identity1 = Create.Entity.Identity(Id.gA, Create.Entity.RosterVector(1));
            Identity identity2 = Create.Entity.Identity(Id.gA, Create.Entity.RosterVector(2));

            Assert.That(identity1.Equals(identity2), Is.False);
            Assert.That(identity1.GetHashCode(), Is.Not.EqualTo(identity2.GetHashCode()));
        }

        [Test]
        public void Two_identities_with_should_be_equal()
        {
            Identity identity1 = Create.Entity.Identity(Id.gA, Create.Entity.RosterVector(1));
            Identity identity2 = Create.Entity.Identity(Id.gA, Create.Entity.RosterVector(1));

            Assert.That(identity1.Equals(identity2), Is.True);
            Assert.That(identity1.GetHashCode(), Is.EqualTo(identity2.GetHashCode()));
        }

        [Test]
        public void Identity_with_empty_roster_vector_should_be_parsed()
        {
            string sId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            string sRosterVector = "";
            string identityToParse = $"{sId}{sRosterVector}";

            Identity parsedIdentity = Identity.Parse(identityToParse);
            Assert.That(parsedIdentity.Id , Is.EqualTo(Guid.Parse(sId)));
            Assert.That(parsedIdentity.RosterVector, Is.EqualTo(RosterVector.Empty));
        }

        [Test]
        public void should_parse_single_coordinate_vector()
        {
            string sId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            string sRosterVector = "_1";
            string identityToParse = $"{sId}{sRosterVector}";

            Identity parsedIdentity = Identity.Parse(identityToParse);

            Assert.That(parsedIdentity.Id, Is.EqualTo(Guid.Parse(sId)));
            Assert.That(parsedIdentity.RosterVector, Is.EqualTo(new RosterVector(new[] { 1m })));
        }
        
        [Test]
        public void should_parse_single_coordinate_with_zero_vector()
        {
            var vector = Identity.Create(Guid.NewGuid(), new RosterVector(new[] {0m}));
            
            string identityToParse = vector.ToString();

            Identity parsedIdentity = Identity.Parse(identityToParse);

            Assert.That(parsedIdentity.Id, Is.EqualTo(vector.Id));
            Assert.That(parsedIdentity.RosterVector, Is.EqualTo(new RosterVector(new[] { 0m })));
        }

        [Test]
        public void Identity_with_roster_vector_should_be_parsed()
        {
            string sId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            string sRosterVector = "_1-5-6-1-0";
            string identityToParse = $"{sId}{sRosterVector}";

            Identity parsedIdentity = Identity.Parse(identityToParse);
            Assert.That(parsedIdentity.Id, Is.EqualTo(Guid.Parse(sId)));
            Assert.That(parsedIdentity.RosterVector, Is.EqualTo(new RosterVector(new[] {1m, 5m, 6m, 1m, 0m})));
        }
    }
}
