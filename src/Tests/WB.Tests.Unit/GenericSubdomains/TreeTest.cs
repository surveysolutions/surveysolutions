using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

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
    }
}
