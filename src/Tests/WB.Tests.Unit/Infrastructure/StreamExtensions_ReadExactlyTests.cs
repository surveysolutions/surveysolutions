using System;
using System.IO;
using NUnit.Framework;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.Infrastructure
{
    [TestOf(typeof(StreamExtensions))]
    public class StreamExtensions_ReadExactlyTests
    {
        [TestCase(4, new byte[] { 1, 2, 3, 4 })]
        [TestCase(0, new byte[] {  })]
        [TestCase(11, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        public void ShouldRead_Exactly_withing_limits(int count, byte[] expected)
        {
            var test = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9};

            var ms = new MemoryStream(test);

            var result = ms.ReadExactly(0, count);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
