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
            public List<Node> Childrens { get; } = new List<Node>();

            public override string ToString()
            {
                return $"{this.Id} [{string.Join(",", this.Childrens.Select(c => c.Id.ToString()))}]";
            }
        }

        [Test]
        public void TraverseTreeAlgorithmTest()
        {
            var tree = new Node
            {
                Id = 1,
                Childrens =
                {
                    new Node
                    {
                        Id = 11,
                        Childrens =
                        {
                            new Node
                            {
                                Id = 111
                            }
                        }
                    },
                    new Node
                    {
                        Id = 12,
                        Childrens =
                        {
                            new Node
                            {
                                Id = 121
                            },
                            new Node
                            {
                                Id = 122
                            },
                        }
                    }
                }
            };

            var breadthFirst = tree.TreeToEnumerable(n => n.Childrens).Select(n => n.Id).ToArray();
            Assert.That(breadthFirst.SequenceEqual(new long[] { 1, 11, 12, 111, 121, 122 }));

            var newDepthFirstStack = tree.TreeToEnumerableDeepFirst(n => n.Childrens).Select(n => n.Id).ToArray();
            Assert.That(newDepthFirstStack.SequenceEqual(new long[] { 1, 11, 111, 12, 121, 122 }));
        }
    }
}
