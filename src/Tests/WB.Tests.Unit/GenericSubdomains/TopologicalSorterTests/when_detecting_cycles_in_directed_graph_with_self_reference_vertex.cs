using System.Collections.Generic;
using FluentAssertions;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Tests.Abc;

namespace WB.Tests.Unit.GenericSubdomains.TopologicalSorterTests
{
    // https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
    internal class when_detecting_cycles_in_directed_graph_with_self_reference_vertex
    {
        [NUnit.Framework.OneTimeSetUp] public void context ()
        {
            dependencies = new Dictionary<int, int[]>()
            {
                {0, new[] {1}},
                {1, new[] {2}},
                {2, new[] {0}},
                {3, new[] {1, 2, 5}},
                {4, new[] {2, 6}},
                {5, new[] {3, 4}},
                {6, new[] {4}},
                {7, new[] {5, 6, 7}}
            };
            sorter = Create.Service.TopologicalSorter<int>();
            BecauseOf();
        }

        public void BecauseOf() =>
            cycles = sorter.DetectCycles(dependencies);

        [NUnit.Framework.Test] public void should_find_4_cycles () =>
            cycles.Count.Should().Be(4);

        [NUnit.Framework.Test] public void should_find_cycle_with_0_1_2 () =>
            cycles[0].Should().BeEquivalentTo(new []{0, 1, 2 });

        [NUnit.Framework.Test] public void should_find_cycle_with_4_6 () =>
            cycles[1].Should().BeEquivalentTo(new []{4, 6});

        [NUnit.Framework.Test] public void should_find_cycle_with_3_5 () =>
            cycles[2].Should().BeEquivalentTo(new[]{3, 5});

        [NUnit.Framework.Test] public void should_find_cycle_with_7 () =>
            cycles[3].Should().BeEquivalentTo(new [] { 7 });

        private static ITopologicalSorter<int> sorter;
        private static List<List<int>> cycles;
        private static Dictionary<int, int[]> dependencies;
    }
}
