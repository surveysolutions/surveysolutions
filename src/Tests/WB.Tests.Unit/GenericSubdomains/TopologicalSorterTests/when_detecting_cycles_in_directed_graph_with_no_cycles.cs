using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Tests.Abc;

namespace WB.Tests.Unit.GenericSubdomains.TopologicalSorterTests
{
    internal class when_detecting_cycles_in_directed_graph_with_no_cycles
    {
        Establish context = () =>
        {
            dependencies = new Dictionary<int, int[]>()
            {
                {0, new[] {1}},
                {1, new[] {2}},
                {2, new[] {3}}
            };
            sorter = Create.Service.TopologicalSorter<int>();
        };

        Because of = () =>
            cycles = sorter.DetectCycles(dependencies);

        It should_find_0_cycles = () =>
            cycles.Count.ShouldEqual(0);

        private static ITopologicalSorter<int> sorter;
        private static List<List<int>> cycles;
        private static Dictionary<int, int[]> dependencies;
    }
}
