using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestFixture]
    [TestOf(typeof(RosterScope))]
    public class RosterScopeTests
    {
        private static readonly Guid A = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid B = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid C = Guid.Parse("33333333-3333-3333-3333-333333333333");

        [Test]
        public void ctor_should_throw_on_null()
        {
            Action act = () => new RosterScope(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Empty_should_have_zero_length()
        {
            RosterScope.Empty.Length.Should().Be(0);
        }

        [Test]
        public void ToString_should_join_coordinates()
        {
            new RosterScope(new[] { A, B }).ToString().Should().Be($"<{A}-{B}>");
        }

        [Test]
        public void Indexer_and_enumeration_should_expose_coordinates()
        {
            var scope = new RosterScope(new[] { A, B });

            scope[1].Should().Be(B);
            scope.ToArray().Should().Equal(A, B);
        }

        [Test]
        public void Equals_should_compare_by_value()
        {
            new RosterScope(new[] { A, B }).Equals(new RosterScope(new[] { A, B })).Should().BeTrue();
            (new RosterScope(new[] { A }) == new RosterScope(new[] { A })).Should().BeTrue();
            (new RosterScope(new[] { A }) != new RosterScope(new[] { B })).Should().BeTrue();
            new RosterScope(new[] { A }).Equals(new RosterScope(new[] { A, B })).Should().BeFalse();
        }

        [Test]
        public void Equals_should_support_guid_array_and_reject_other_types()
        {
            var scope = new RosterScope(new[] { A });

            scope.Equals((object)new[] { A }).Should().BeTrue();
            scope.Equals("text").Should().BeFalse();
            scope.Equals((object)null).Should().BeFalse();
            scope.Equals((RosterScope)null).Should().BeFalse();
        }

        [Test]
        public void GetHashCode_should_be_equal_for_equal_scopes()
        {
            new RosterScope(new[] { A, B }).GetHashCode().Should().Be(new RosterScope(new[] { A, B }).GetHashCode());
        }

        [Test]
        public void Implicit_conversions_should_round_trip()
        {
            RosterScope scope = new[] { A, B };
            Guid[] array = scope;

            array.Should().Equal(A, B);
        }

        [Test]
        public void Extend_should_append_coordinate_without_mutating_original()
        {
            var original = new RosterScope(new[] { A });
            var extended = original.Extend(B);

            extended.ToArray().Should().Equal(A, B);
            original.Length.Should().Be(1);
        }

        [Test]
        public void Shrink_should_return_expected_scope()
        {
            var scope = new RosterScope(new[] { A, B, C });

            scope.Shrink(0).Should().BeSameAs(RosterScope.Empty);
            scope.Shrink(3).Should().BeSameAs(scope);
            scope.Shrink(2).ToArray().Should().Equal(A, B);
        }

        [Test]
        public void Shrink_should_throw_when_target_longer()
        {
            Action act = () => new RosterScope(new[] { A }).Shrink(2);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void IsParentScopeFor_should_detect_prefix_scope()
        {
            var parent = new RosterScope(new[] { A });
            var child = new RosterScope(new[] { A, B });

            parent.IsParentScopeFor(child).Should().BeTrue();
            child.IsParentScopeFor(parent).Should().BeFalse();
            parent.IsParentScopeFor(parent).Should().BeFalse();
            new RosterScope(new[] { C }).IsParentScopeFor(child).Should().BeFalse();
        }

        [Test]
        public void IsChildScopeFor_should_detect_and_respect_max_levels()
        {
            var parent = new RosterScope(new[] { A });
            var child = new RosterScope(new[] { A, B, C });

            child.IsChildScopeFor(parent).Should().BeTrue();
            child.IsChildScopeFor(parent, maxLevelsDifference: 1).Should().BeFalse();
            child.IsChildScopeFor(parent, maxLevelsDifference: 2).Should().BeTrue();
            parent.IsChildScopeFor(child).Should().BeFalse();
        }

        [Test]
        public void IsSame_or_related_scopes()
        {
            var parent = new RosterScope(new[] { A });
            var child = new RosterScope(new[] { A, B });

            parent.IsSameOrParentScopeFor(new RosterScope(new[] { A })).Should().BeTrue();
            parent.IsSameOrParentScopeFor(child).Should().BeTrue();
            child.IsSameOrParentScopeFor(parent).Should().BeFalse();

            child.IsSameOrChildScopeFor(new RosterScope(new[] { A, B })).Should().BeTrue();
            child.IsSameOrChildScopeFor(parent).Should().BeTrue();
            parent.IsSameOrChildScopeFor(child).Should().BeFalse();
        }
    }
}

