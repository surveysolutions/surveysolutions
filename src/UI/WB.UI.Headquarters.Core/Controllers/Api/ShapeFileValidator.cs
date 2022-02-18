using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers.Api;

public class ValidatorError
{
    public ValidatorError(string message)
    {
        Message = message;
    }

    public string Code { get; set; }
    public string Message { get; set; }
}

public class ShapeFileValidator
{
    private readonly string[] shapeMapFileExtensions = { ".dbf", ".shp", ".shx", ".cpg", ".prj" };

    private IEnumerable<Func<Dictionary<string, long>, IEnumerable<ValidatorError>>> ErrorsVerifiers => new[]
    {
        (Func<Dictionary<string, long>, IEnumerable<ValidatorError>>)CheckFileStructureForShapeFile,
    };
    
    private static IEnumerable<ValidatorError> CheckFileStructureForShapeFile(Dictionary<string, long> fileNamesAndSize)
    {
        var fileNames = fileNamesAndSize.Keys.ToList();
        HashSet<string> mapNames = new HashSet<string>();
        fileNames
            .Where(name => shapeMapFileExtensions.Contains(Path.GetExtension(name)))
            .ForEach(name => mapNames.Add(Path.GetFileNameWithoutExtension(name)));

        foreach (var mapName in mapNames)
        {
            if (!fileNames.Contains(mapName + ".shp"))
                yield return new ValidatorError(string.Format(Maps.ShpIsMissingInArchive, mapName));
            if (!fileNames.Contains(mapName + ".shx"))
                yield return new ValidatorError(string.Format(Maps.ShxIsMissingInArchive, mapName));
            if (!fileNames.Contains(mapName + ".dbf"))
                yield return new ValidatorError(string.Format(Maps.DbfIsMissingInArchive, mapName));
        }
    }

    
    public IEnumerable<ValidatorError> Verify(Dictionary<string, long> fileNamesAndSize)
    {
        var errors = new List<ValidatorError>();
        foreach (var verifier in this.ErrorsVerifiers)
        {
            errors.AddRange(verifier.Invoke(fileNamesAndSize));
        }
        return errors;
    }
}