using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Utils;

namespace WB.Tests.Unit.GenericSubdomains.Utils
{
    internal class when_sorting_by_property_from_parent_class_with_OrderBy_from_QueryableExtensions
    {
        public class ParentClass
        {
            public ParentClass(int a)
            {
                this.A = a;
            }

            public int A { get; set; }
        }

        public class ChildClass : ParentClass
        {
            public ChildClass(int a)
                : base(a) {}
        }

        [NUnit.Framework.OneTimeSetUp] public void context () {
           unsortedList = new List<ChildClass> { new ChildClass(3), new ChildClass(2), new ChildClass(1) }.AsQueryable();
            BecauseOf();
        }

        public void BecauseOf() =>
            sortedList = unsortedList.OrderBy("A");

        [NUnit.Framework.Test] public void should_have_first_element_equals_1 () =>
            sortedList.ToList()[0].A.Should().Be(1);

        [NUnit.Framework.Test] public void should_have_second_element_equals_2 () =>
            sortedList.ToList()[1].A.Should().Be(2);

        [NUnit.Framework.Test] public void should_have_third_element_equals_3 () =>
            sortedList.ToList()[2].A.Should().Be(3);

        static IQueryable<ChildClass> unsortedList;
        static IOrderedQueryable<ChildClass> sortedList;
    }
}
