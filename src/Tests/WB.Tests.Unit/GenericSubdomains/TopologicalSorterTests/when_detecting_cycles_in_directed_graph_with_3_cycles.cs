﻿using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.GenericSubdomains.TopologicalSorterTests
{
    /// <see cref="http://algs4.cs.princeton.edu/42digraph/"/>
    internal class when_detecting_cycles_in_directed_graph_with_3_cycles
    {
        Establish context = () =>
        {
            dependencies = new Dictionary<int, int[]>()
            {
                {0, new[] {5, 1}},
                {2, new[] {0, 3}},
                {3, new[] {5, 2}},
                {4, new[] {3, 2}},
                {5, new[] {4}},
                {6, new[] {9, 4, 8, 0}},
                {7, new[] {6, 9}},
                {8, new[] {6}},
                {9, new[] {11, 10}},
                {10, new[] {12}},
                {11, new[] {4, 12}},
                {12, new[] {9}}
            };
            sorter = Create.TopologicalSorter();
        };

        Because of = () =>
            cycles = sorter.DetectCycles(dependencies);

        It should_find_3_cycles = () =>
            cycles.Count.ShouldEqual(3);

        It should_find_cycle_with_0_2_3_4_5 = () =>
            cycles[0].ShouldContainOnly(0,2,3,4,5);

        It should_find_cycle_with_9_10_11_12 = () =>
            cycles[1].ShouldContainOnly(9, 10, 11, 12);

        It should_find_cycle_with_6_8 = () =>
            cycles[2].ShouldContainOnly(6, 8);

        private static ITopologicalSorter<int> sorter;
        private static List<List<int>> cycles;
        private static Dictionary<int, int[]> dependencies;
    }
}