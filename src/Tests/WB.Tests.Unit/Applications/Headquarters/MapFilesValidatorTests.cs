using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Repositories;
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
                    Files = new List<string>() { "map1.shx", "map1.dbf", }
                },
                new MapFiles()
                {
                    IsShapeFile = true,
                    Name = "map2",
                    Files = new List<string>() { "map2.shp", "map2.dbf", }
                },
                new MapFiles()
                {
                    IsShapeFile = true,
                    Name = "map3",
                    Files = new List<string>() { "map3.shp", "map3.shx", }
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
    public void when_validate_incorrect_maps_file()
    {
        var service = CreateMapFilesValidator();

        var validatorErrors = service.Verify(new AnalyzeResult() { IsValid = false });
        
        Assert.That(validatorErrors.Any(), Is.False);
    }

    private MapFilesValidator CreateMapFilesValidator()
    {
        return new MapFilesValidator();
    }
}