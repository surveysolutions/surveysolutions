using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Humanizer;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Headquarters.Services.Maps;

public class ValidatorError
{
    public ValidatorError(string message)
    {
        Message = message;
    }

    public string Code { get; set; }
    public string Message { get; set; }
}

public class MapFilesValidator
{
    private const int MapFileSizeLimit = 512 * 1024 * 1024;
    
    private IEnumerable<Func<AnalyzeResult, IEnumerable<ValidatorError>>> ErrorsVerifiers => new[]
    {
        (Func<AnalyzeResult, IEnumerable<ValidatorError>>)CheckFileStructureForShapeFile,
        CheckFileSizeLimitForEachFile,
    };
    
    private static IEnumerable<ValidatorError> CheckFileStructureForShapeFile(AnalyzeResult analyzeResults)
    {
        foreach (var map in analyzeResults.Maps)
        {
            if (map.IsShapeFile)
            {
                var fileNames = map.Files.Select(f => f.Name).ToList();
                if (!fileNames.Contains(map.Name + ".shp"))
                    yield return new ValidatorError(string.Format(Resources.Maps.ShpIsMissingInArchive, map.Name));
                if (!fileNames.Contains(map.Name + ".shx"))
                    yield return new ValidatorError(string.Format(Resources.Maps.ShxIsMissingInArchive, map.Name));
                if (!fileNames.Contains(map.Name + ".dbf"))
                    yield return new ValidatorError(string.Format(Resources.Maps.DbfIsMissingInArchive, map.Name));
            }
        }
    }
    
    private static IEnumerable<ValidatorError> CheckFileSizeLimitForEachFile(AnalyzeResult analyzeResults)
    {
        foreach (var map in analyzeResults.Maps)
        {
            foreach (var mapFile in map.Files)
            {
                if (mapFile.Size > MapFileSizeLimit)
                    yield return new ValidatorError(string.Format(Resources.Maps.MapFileSizeLimit, mapFile.Name, MapFileSizeLimit.Bytes().Humanize()));
            }
        }
    }

    
    public IEnumerable<ValidatorError> Verify(AnalyzeResult analyzeResults)
    {
        var errors = new List<ValidatorError>();
        foreach (var verifier in this.ErrorsVerifiers)
        {
            errors.AddRange(verifier.Invoke(analyzeResults));
        }
        return errors;
    }
}