using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;

namespace WB.Tests.Unit.BoundedContexts.Headquarters
{
    [TestOf(typeof(CsvReader))]
    internal class CsvReaderTests
    {
        [Test]
        public void when_read_records()
        {
            //arrange
            var sb = new StringBuilder();
            sb.AppendLine("head1,head2,head3");
            sb.AppendLine("aaa,1");
            //act
            var records = new CsvReader().ReadRowsWithHeader(new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString())), ",").ToArray();
            //assert
            Assert.That(records, Is.EquivalentTo(new[] {new[] {"head1", "head2", "head3"}, new[] {"aaa", "1", null}}));
        }

        [Test]
        public void should_read_columns_with_quotes_in_it()
        {
            //arrange
            var sb = new StringBuilder();
            sb.AppendLine("\"1");
            //act
            var records = new CsvReader().ReadRowsWithHeader(new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString())), ",").ToArray();
            //assert
            Assert.That(records, Is.EquivalentTo(new[] {new[] {"\"1"}}));
        }
    }
}
