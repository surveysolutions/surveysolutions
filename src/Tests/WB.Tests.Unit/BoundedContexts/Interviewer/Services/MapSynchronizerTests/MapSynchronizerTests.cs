using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.MapSynchronizerTests
{
    public class MapSynchronizerTests
    {
        [Test]
        public async Task should_always_delete_map_file()
        {
            var synchronizationService = new Mock<IOnlineSynchronizationService>();
            synchronizationService.Setup(x => x.GetMapList(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MapView>()
                {
                    new MapView(){MapName = "test"},
                    new MapView(){MapName = "test3"}
                });

            var mapService = new Mock<IMapService>();
            mapService.Setup(x => x.GetAvailableMaps(false)).Returns(new List<MapDescription>()
            {
                new MapDescription(MapType.LocalFile, "test"){MapFileName = "test"},
                new MapDescription(MapType.LocalFile, "test1"){MapFileName = "test1"}
            });
            mapService.Setup(x => x.GetAvailableShapefiles()).Returns(new List<ShapefileDescription>()
            {
                new ShapefileDescription(){ ShapefileFileName = "test2"},
                new ShapefileDescription(){ ShapefileFileName = "test3"}
            });

            mapService.Setup(x => x.DoesMapExist(It.IsAny<string>())).Returns(true);

            

            var service = Create.Service.MapSyncProvider(
                synchronizationService : synchronizationService.Object,
                mapService : mapService.Object);
            
            await service.Synchronize(
                new Progress<SyncProgressInfo>(),
                new CancellationToken(),  
                new SynchronizationStatistics());
            
            // assert
            mapService.Verify(x => x.RemoveMap("test"), Times.Never);
            mapService.Verify(x => x.RemoveMap("test1"), Times.Once);
            mapService.Verify(x => x.RemoveMap("test2"), Times.Once);
            mapService.Verify(x => x.RemoveMap("test3"), Times.Never);
            
            //Assert.That(token, Is.EqualTo("offline sync token"));
        }

        [Test]
        public async Task should_report_download_progress_only_when_bucket_changes()
        {
            var synchronizationService = new Mock<IOnlineSynchronizationService>();
            synchronizationService.Setup(x => x.GetMapList(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MapView> { new MapView { MapName = "big-map.tpk" } });

            var sourceBytes = new byte[20 * 1024];
            synchronizationService.Setup(x => x.GetMapContentStream("big-map.tpk", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestStreamResult
                {
                    // Large declared size keeps integer percentage at 0 for the whole stream.
                    ContentLength = 1024 * 1024,
                    Stream = new ChunkedReadStream(sourceBytes, 1024)
                });

            var tempStream = new MemoryStream();
            var mapService = new Mock<IMapService>();
            mapService.Setup(x => x.GetAvailableMaps(false)).Returns(new List<MapDescription>());
            mapService.Setup(x => x.GetAvailableShapefiles()).Returns(new List<ShapefileDescription>());
            mapService.Setup(x => x.DoesMapExist("big-map.tpk")).Returns(false);
            mapService.Setup(x => x.GetTempMapSaveStream("big-map.tpk")).Returns(tempStream);

            var service = Create.Service.MapSyncProvider(
                synchronizationService: synchronizationService.Object,
                mapService: mapService.Object);

            var progress = new ImmediateProgress();

            await service.Synchronize(progress, CancellationToken.None, new SynchronizationStatistics());

            var downloadReports = progress.Items.Count(p => p.Status == SynchronizationStatus.Download);
            Assert.That(downloadReports, Is.EqualTo(3));
            mapService.Verify(x => x.MoveTempMapToPermanent("big-map.tpk"), Times.Once);
        }

        [Test]
        public async Task should_report_download_progress_for_unknown_content_length_without_spam()
        {
            var synchronizationService = new Mock<IOnlineSynchronizationService>();
            synchronizationService.Setup(x => x.GetMapList(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MapView> { new MapView { MapName = "chunked-map.tpk" } });

            var sourceBytes = new byte[20 * 1024];
            synchronizationService.Setup(x => x.GetMapContentStream("chunked-map.tpk", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestStreamResult
                {
                    ContentLength = null,
                    Stream = new ChunkedReadStream(sourceBytes, 1024)
                });

            var tempStream = new MemoryStream();
            var mapService = new Mock<IMapService>();
            mapService.Setup(x => x.GetAvailableMaps(false)).Returns(new List<MapDescription>());
            mapService.Setup(x => x.GetAvailableShapefiles()).Returns(new List<ShapefileDescription>());
            mapService.Setup(x => x.DoesMapExist("chunked-map.tpk")).Returns(false);
            mapService.Setup(x => x.GetTempMapSaveStream("chunked-map.tpk")).Returns(tempStream);

            var service = Create.Service.MapSyncProvider(
                synchronizationService: synchronizationService.Object,
                mapService: mapService.Object);

            var progress = new ImmediateProgress();

            await service.Synchronize(progress, CancellationToken.None, new SynchronizationStatistics());

            var downloadEvents = progress.Items.Where(p => p.Status == SynchronizationStatus.Download).ToList();
            Assert.That(downloadEvents.Count, Is.EqualTo(1));
            Assert.That(downloadEvents[0].Description, Is.Null.Or.Empty);
            Assert.That(downloadEvents[0].Title, Does.Not.Contain("0%"));
            mapService.Verify(x => x.MoveTempMapToPermanent("chunked-map.tpk"), Times.Once);
        }

        private sealed class ImmediateProgress : IProgress<SyncProgressInfo>
        {
            public List<SyncProgressInfo> Items { get; } = new List<SyncProgressInfo>();

            public void Report(SyncProgressInfo value)
            {
                this.Items.Add(value);
            }
        }

        private sealed class ChunkedReadStream : Stream
        {
            private readonly Stream inner;
            private readonly int maxChunkSize;

            public ChunkedReadStream(byte[] data, int maxChunkSize)
            {
                this.inner = new MemoryStream(data);
                this.maxChunkSize = maxChunkSize;
            }

            public override bool CanRead => this.inner.CanRead;
            public override bool CanSeek => this.inner.CanSeek;
            public override bool CanWrite => false;
            public override long Length => this.inner.Length;
            public override long Position
            {
                get => this.inner.Position;
                set => this.inner.Position = value;
            }

            public override int Read(byte[] buffer, int offset, int count)
                => this.inner.Read(buffer, offset, Math.Min(count, this.maxChunkSize));

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => this.inner.ReadAsync(buffer, offset, Math.Min(count, this.maxChunkSize), cancellationToken);

            public override long Seek(long offset, SeekOrigin origin) => this.inner.Seek(offset, origin);
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override void Flush() => this.inner.Flush();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    this.inner.Dispose();

                base.Dispose(disposing);
            }
        }
    }
}
