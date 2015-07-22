using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.GenericSubdomains.Utils
{
    public class when_sorting_by_property_from_parent_class_with_OrderBy_from_QueryableExtensions
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

        Establish context = () =>
        {
           unsortedList = new List<ChildClass> { new ChildClass(3), new ChildClass(2), new ChildClass(1) }.AsQueryable();
        };

        Because of = () =>
            sortedList = unsortedList.OrderBy("A");

        It should_have_first_element_equals_1 = () =>
            sortedList.ToList()[0].A.ShouldEqual(1);

        It should_have_second_element_equals_2 = () =>
            sortedList.ToList()[1].A.ShouldEqual(2);

        It should_have_third_element_equals_3 = () =>
            sortedList.ToList()[2].A.ShouldEqual(3);

        static IQueryable<ChildClass> unsortedList;
        static IOrderedQueryable<ChildClass> sortedList;
    }
}