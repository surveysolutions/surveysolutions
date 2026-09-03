using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestFixture]
    [TestOf(typeof(InterviewBinaryDataDescriptor))]
    public class InterviewBinaryDataDescriptorTests
    {
        [Test]
        public async Task when_stream_accessor_is_not_provided_should_create_stream_from_data_accessor()
        {
            var descriptor = new InterviewBinaryDataDescriptor(
                Id.gA,
                "recording.m4a",
                "audio/mp4",
                () => Task.FromResult(new byte[] { 1, 2, 3 }),
                null);

            await using var stream = await descriptor.GetStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            Assert.That(memoryStream.ToArray(), Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public async Task when_data_accessor_is_not_provided_should_load_data_from_stream_accessor()
        {
            var descriptor = new InterviewBinaryDataDescriptor(
                Id.gA,
                "recording.m4a",
                "audio/mp4",
                null,
                null,
                () => Task.FromResult<Stream>(new MemoryStream(new byte[] { 4, 5, 6 }, writable: false)));

            var data = await descriptor.GetData();

            Assert.That(data, Is.EqualTo(new byte[] { 4, 5, 6 }));
        }
    }
}
