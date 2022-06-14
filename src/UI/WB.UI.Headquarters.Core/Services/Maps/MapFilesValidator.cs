using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Humanizer;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;

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

public class MapFilesValidator : IMapFilesValidator
{
    private readonly IFileSystemAccessor fileSystemAccessor;
    private const int MapFileSizeLimit = 512 * 1024 * 1024;

    public MapFilesValidator(IFileSystemAccessor fileSystemAccessor)
    {
        this.fileSystemAccessor = fileSystemAccessor;
    }

    private IEnumerable<Func<AnalyzeResult, IEnumerable<ValidatorError>>> ErrorsVerifiers => new[]
    {
        (Func<AnalyzeResult, IEnumerable<ValidatorError>>)CheckFileStructureForShapeFile,
        CheckFileSizeLimitForEachFile,
        CheckFileNamesOnInvalidChars,
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
    
    private IEnumerable<ValidatorError> CheckFileNamesOnInvalidChars(AnalyzeResult analyzeResults)
    {
        //var invalidPathChars = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).ToHashSet();
        foreach (var map in analyzeResults.Maps)
        {
            foreach (var mapFile in map.Files)
            {
                var validFileName = fileSystemAccessor.MakeValidFileName(mapFile.Name);
                //var hasInvalidChar = mapFile.Name.Any(c => invalidPathChars.Contains(c));
                var hasInvalidChar = string.CompareOrdinal(validFileName, mapFile.Name) != 0;
                if (hasInvalidChar)
                    yield return new ValidatorError(string.Format(Resources.Maps.MapFileNameHasInvalidChars, mapFile.Name));
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