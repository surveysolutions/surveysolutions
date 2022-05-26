using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services.Maps;

namespace WB.Tests.Unit.Applications.Headquarters;

[TestOf(typeof(MapFilesValidator))]
public class MapFilesValidatorTests
{
    [Test]
    public void when_validate_shapefile_without_required_files()
    {
        var analyzeResults = new AnalyzeResult()
        {
            IsValid = false,
            Maps = new List<MapFiles>()
            {
                new MapFiles()
                {
                    IsShapeFile = true,
                    Name = "map1",
                    Files = new List<MapFile>() { MapFile("map1.shx"), MapFile("map1.dbf"), }
                },
                new MapFiles()
                {
                    IsShapeFile = true,
                    Name = "map2",
                    Files = new List<MapFile>() { MapFile("map2.shp"), MapFile("map2.dbf"), }
                },
                new MapFiles()
                {
                    IsShapeFile = true,
                    Name = "map3",
                    Files = new List<MapFile>() { MapFile("map3.shp"), MapFile("map3.shx"), }
                },
            }
        };
        var service = CreateMapFilesValidator();

        var validatorErrors = service.Verify(analyzeResults).ToList();
        
        Assert.That(validatorErrors.Count, Is.EqualTo(3));
        Assert.That(validatorErrors[0].Message, Is.EqualTo(string.Format(Maps.ShpIsMissingInArchive, "map1")));
        Assert.That(validatorErrors[1].Message, Is.EqualTo(string.Format(Maps.ShxIsMissingInArchive, "map2")));
        Assert.That(validatorErrors[2].Message, Is.EqualTo(string.Format(Maps.DbfIsMissingInArchive, "map3")));
    }

    [Test]
    public void when_validate_file_size_limit()
    {
        var analyzeResults = new AnalyzeResult()
        {
            IsValid = false,
            Maps = new List<MapFiles>()
            {
                new MapFiles()
                {
                    IsShapeFile = true,
                    Name = "map1",
                    Files = new List<MapFile>() { MapFile("map1.shp"), MapFile("map1.shx"), MapFile("map1.dbf", 512 * 1024 * 1024 + 1), }
                },
                new MapFiles()
                {
                    IsShapeFile = false,
                    Name = "map2",
                    Files = new List<MapFile>() { MapFile("map2.tif", 512 * 1024 * 1024 + 1), }
                },
            }
        };
        var service = CreateMapFilesValidator();

        var validatorErrors = service.Verify(analyzeResults).ToList();
        
        Assert.That(validatorErrors.Count, Is.EqualTo(2));
        Assert.That(validatorErrors[0].Message, Is.EqualTo(string.Format(Maps.MapFileSizeLimit, "map1.dbf", "512 MB")));
        Assert.That(validatorErrors[1].Message, Is.EqualTo(string.Format(Maps.MapFileSizeLimit, "map2.tif", "512 MB")));
    }

    [Test]
    public void when_validate_incorrect_maps_file()
    {
        var service = CreateMapFilesValidator();

        var validatorErrors = service.Verify(new AnalyzeResult() { IsValid = false });
        
        Assert.That(validatorErrors.Any(), Is.False);
    }
    
    [Test]
    public void when_validate_incorrect_map_name()
    {
        var analyzeResults = new AnalyzeResult()
        {
            IsValid = false,
            Maps = new List<MapFiles>()
            {
                new MapFiles()
                {
                    IsShapeFile = false,
                    Name = "map1",
                    Files = new List<MapFile>() { MapFile("map\\1.shp") }
                },
            }
        };
        
        var service = CreateMapFilesValidator();

        var validatorErrors = service.Verify(analyzeResults).ToList();
        
        Assert.That(validatorErrors.Any(), Is.True);
        Assert.That(validatorErrors.Count(), Is.EqualTo(1));
    }

    private MapFile MapFile(string name, int size = 5000) => new MapFile() { Name = name, Size = size };

    private MapFilesValidator CreateMapFilesValidator()
    {
        return new MapFilesValidator(new FileSystemIOAccessor());
    }
}