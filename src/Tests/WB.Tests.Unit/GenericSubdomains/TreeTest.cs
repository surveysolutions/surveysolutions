using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Tests.Abc;

namespace WB.Tests.Unit.GenericSubdomains
{
    public class TreeTests
    {
        public class Node
        {
            public long Id { get; set; }
            public List<Node> Childrens { get; }

            public override string ToString()
            {
                return $"{this.Id} [{string.Join(",", this.Childrens.Select(c => c.Id.ToString()))}]";
            }

            public Node(long id, params Node[] nodes)
            {
                Id = id;
                Childrens = nodes.ToList();
            }
        }

        [Test]
        public void TraverseTreeAlgorithmTest()
        {
            var tree = new Node(1,
                new Node(11,
                    new Node(111),
                    new Node(112),
                    new Node(113)
                ),
                new Node(12,
                    new Node(121),
                    new Node(122),
                    new Node(123)
                ),
                new Node(13,
                    new Node(131),
                    new Node(132),
                    new Node(133, new Node(1331))
                )
            );

            var breadthFirst = tree.TreeToEnumerable(n => n.Childrens).Select(n => n.Id).ToArray();
            Assert.True(breadthFirst.SequenceEqual(new long[] { 1, 11, 12, 13, 111, 112, 113, 121, 122, 123, 131, 132, 133, 1331 }),
                "Actual: {0}", string.Join(", ", breadthFirst));

            var newDepthFirstStack = tree.TreeToEnumerableDepthFirst(n => n.Childrens).Select(n => n.Id).ToArray();
            Assert.True(newDepthFirstStack.SequenceEqual(new long[] { 1, 11, 111, 112, 113, 12, 121, 122, 123, 13, 131, 132, 133, 1331 }),
                "Actual: {0}", string.Join(", ", newDepthFirstStack));

            var newEnumerableDepthFirstStack = tree.Childrens.TreeToEnumerableDepthFirst(n => n.Childrens).Select(n => n.Id).ToArray();
            Assert.True(newEnumerableDepthFirstStack.SequenceEqual(new long[] { 11, 111, 112, 113, 12, 121, 122, 123, 13, 131, 132, 133, 1331 }),
                "Actual: {0}", string.Join(", ", newEnumerableDepthFirstStack));
        }

        [TestCase(new[] { 1, 1, 1 }, new[] { 1, 1, 2 }, ExpectedResult = -1)]
        [TestCase(new[] { 1 }, new[] { 1, 1, 2 }, ExpectedResult = -1)]
        [TestCase(new[] { 1, 1, 1 }, new[] { 1 }, ExpectedResult = 1)]
        [TestCase(new[] { 1, 1, 1 }, new[] { 1, 1, 1 }, ExpectedResult = 0)]
        [TestCase(new int[] { }, new[] { 1, 1, 2 }, ExpectedResult = -1)]
        public int CanCompareRosterVectorAsCoordinates(int[] left, int[] right)
        {
            return RosterVectorAsCoordinatesComparer.Instance.Compare(left, right);
        }

        [Test]
        public void CanGetProperOrderAddressInTree()
        {
            var section1 = Create.Entity.InterviewTreeSection(Create.Entity.Identity(1));
            var section2 = Create.Entity.InterviewTreeSection(Create.Entity.Identity(2));

            section1.AddChild(Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Id.gA)));
            var roster = Create.Entity.InterviewTreeRoster(Create.Entity.Identity(Id.gB));
            roster.AddChild(Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Id.gB, new[] { 20 })));
            roster.AddChild(Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Id.gB, new[] { 12 })));

            section2.AddChild(roster);

            var interviewTree = Create.Entity.InterviewTree(Guid.NewGuid(), sections: new[] {section1, section2});

            var address1 = interviewTree.GetNodeCoordinatesInEnumeratorOrder(Create.Entity.Identity(Id.gA));
            var address2 = interviewTree.GetNodeCoordinatesInEnumeratorOrder(Create.Entity.Identity(Id.gB, new[] { 12 }));
            var address3 = interviewTree.GetNodeCoordinatesInEnumeratorOrder(Create.Entity.Identity(Id.gB, new[] { 20 }));

            Assert.That(address1.SequenceEqual(new[] { 1, 1 }));
            Assert.That(address2.SequenceEqual(new[] { 2, 1, 2 }));
            Assert.That(address3.SequenceEqual(new[] { 2, 1, 1 }));
        }
    }
}
