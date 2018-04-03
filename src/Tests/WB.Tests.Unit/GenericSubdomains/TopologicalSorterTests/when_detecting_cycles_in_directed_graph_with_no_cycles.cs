using System.Collections.Generic;
using FluentAssertions;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Tests.Abc;

namespace WB.Tests.Unit.GenericSubdomains.TopologicalSorterTests
{
    internal class when_detecting_cycles_in_directed_graph_with_no_cycles
    {
        [NUnit.Framework.OneTimeSetUp] public void context ()
        {
            dependencies = new Dictionary<int, int[]>()
            {
                {0, new[] {1}},
                {1, new[] {2}},
                {2, new[] {3}}
            };
            sorter = Create.Service.TopologicalSorter<int>();
            BecauseOf();
        }

        public void BecauseOf() =>
            cycles = sorter.DetectCycles(dependencies);

        [NUnit.Framework.Test] public void should_find_0_cycles () =>
            cycles.Count.Should().Be(0);

        private static ITopologicalSorter<int> sorter;
        private static List<List<int>> cycles;
        private static Dictionary<int, int[]> dependencies;
    }
}
