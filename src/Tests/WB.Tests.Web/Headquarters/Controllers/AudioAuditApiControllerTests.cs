using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.UI.Headquarters.Controllers.Api;
using WB.UI.Headquarters.Services;

namespace WB.Tests.Web.Headquarters.Controllers;

[TestFixture]
[TestOf(typeof(AudioAuditApiController))]
public class AudioAuditApiControllerTests
{
    [Test]
    public async Task GetAudioAuditSegment_should_enable_ranges_for_non_seekable_storage_stream()
    {
        var interviewId = Guid.NewGuid();
        const string segmentId = "segment";
        var content = new byte[] { 1, 2, 3, 4 };
        var descriptor = new InterviewBinaryDataDescriptor(
            interviewId,
            "recording.m4a",
            "audio/mp4",
            () => Task.FromResult(content),
            null,
            () => Task.FromResult<Stream>(new NonSeekableReadStream(content)));
        var accessService = new Mock<IAudioAuditAccessService>();
        accessService
            .Setup(service => service.ResolveSegmentAsync(interviewId, segmentId))
            .ReturnsAsync(descriptor);
        var controller = new AudioAuditApiController(accessService.Object);

        var result = await controller.GetAudioAuditSegment(interviewId, segmentId) as FileStreamResult;

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.EnableRangeProcessing, Is.True);
            Assert.That(result.FileStream.CanSeek, Is.True);
        });
    }

    private sealed class NonSeekableReadStream : Stream
    {
        private readonly MemoryStream stream;

        public NonSeekableReadStream(byte[] content)
        {
            stream = new MemoryStream(content, writable: false);
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => stream.ReadAsync(buffer, offset, count, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing) stream.Dispose();
            base.Dispose(disposing);
        }
    }
}