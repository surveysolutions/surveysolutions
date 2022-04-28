using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Services.Maps;

namespace WB.Tests.Unit.Applications.Headquarters;

[TestOf(typeof(UploadMapsService))]
public class UploadMapsServiceTests
{
    [Test]
    public void when_upload_correct_zip_with_two_maps()
    {
        var stream = Stream.Null;
        var uploadPackageAnalyzer = Mock.Of<IUploadPackageAnalyzer>(u =>
            u.Analyze(stream) == new AnalyzeResult()
            {
                IsValid = true,
                Maps = new List<MapFiles>()
                {
                    new MapFiles() { Name = "map1.tpk" },
                    new MapFiles() { Name = "map2.tpk" },
                }
            });
        var mapStorageService = new Mock<IMapStorageService>();
        mapStorageService
            .Setup(s => s.SaveOrUpdateMapAsync(It.IsAny<MapFiles>(), It.IsAny<string>()))
            .Returns((MapFiles map, string mapsDirectory) => Task.FromResult(new MapBrowseItem() { FileName = map.Name }));
        var service = CreateUploadMapsService(uploadPackageAnalyzer, mapStorageService.Object);

        var upload = service.Upload("filename.zip", stream);

        Assert.That(upload.Result.IsSuccess, Is.True);
        Assert.That(upload.Result.Maps.Count, Is.EqualTo(2));
        Assert.That(upload.Result.Maps[0].FileName, Is.EqualTo("map1.tpk"));
        Assert.That(upload.Result.Maps[1].FileName, Is.EqualTo("map2.tpk"));
    }

    [Test]
    public async Task when_upload_empty_file()
    {
        var stream = Stream.Null; 
        var service = CreateUploadMapsService();

        var upload = await service.Upload("filename.zip", stream);

        Assert.That(upload.IsSuccess, Is.False);
    }


    private IUploadMapsService CreateUploadMapsService(IUploadPackageAnalyzer uploadPackageAnalyzer = null,
        IMapStorageService mapStorageService = null)
    {
        return new UploadMapsService(
            Mock.Of<IFileSystemAccessor>(f => f.GetFileExtension(It.IsAny<string>()) == ".zip"),
            uploadPackageAnalyzer ?? Mock.Of<IUploadPackageAnalyzer>(),
            new NullLogger<UploadMapsService>(),
            Mock.Of<IArchiveUtils>(),
            mapStorageService ?? Mock.Of<IMapStorageService>(),
            Mock.Of<IOptions<FileStorageConfig>>(o => o.Value == new FileStorageConfig()));
    }
}