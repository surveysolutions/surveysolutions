using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private readonly HashSet<string> shapeMapFileExtensions = new HashSet<string>() { ".dbf", ".shp", ".shx", ".cpg", ".prj" };

    private IEnumerable<Func<AnalyzeResult, IEnumerable<ValidatorError>>> ErrorsVerifiers => new[]
    {
        (Func<AnalyzeResult, IEnumerable<ValidatorError>>)CheckFileStructureForShapeFile,
    };
    
    private static IEnumerable<ValidatorError> CheckFileStructureForShapeFile(AnalyzeResult analyzeResults)
    {
        foreach (var map in analyzeResults.Maps)
        {
            if (map.IsShapeFile)
            {
                if (!map.Files.Contains(map.Name + ".shp"))
                    yield return new ValidatorError(string.Format(Resources.Maps.ShpIsMissingInArchive, map.Name));
                if (!map.Files.Contains(map.Name + ".shx"))
                    yield return new ValidatorError(string.Format(Resources.Maps.ShxIsMissingInArchive, map.Name));
                if (!map.Files.Contains(map.Name + ".dbf"))
                    yield return new ValidatorError(string.Format(Resources.Maps.DbfIsMissingInArchive, map.Name));
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