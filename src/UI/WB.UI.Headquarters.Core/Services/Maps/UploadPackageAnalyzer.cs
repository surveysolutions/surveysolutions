using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.Infrastructure.FileSystem;

namespace WB.UI.Headquarters.Services.Maps;

public class AnalyzeResult
{
    public bool IsValid { get; set; }
    public List<MapFiles> Maps { get; set; } = new List<MapFiles>();
}



public class UploadPackageAnalyzer : IUploadPackageAnalyzer
{
    private readonly HashSet<string> permittedMapFileExtensions = new HashSet<string>() { ".tpk", ".mmpk", ".tif" };
    private readonly HashSet<string> shapeMapFileExtensions = new HashSet<string>() { ".dbf", ".shp", ".shx", ".cpg", ".prj" };

    private readonly IFileSystemAccessor fileSystemAccessor;
    private readonly IArchiveUtils archiveUtils;

    public UploadPackageAnalyzer(IFileSystemAccessor fileSystemAccessor,
        IArchiveUtils archiveUtils)
    {
        this.fileSystemAccessor = fileSystemAccessor;
        this.archiveUtils = archiveUtils;
    }

    public AnalyzeResult Analyze(Stream file)
    {
        var result = new AnalyzeResult();
        
        var filesInArchive = archiveUtils.GetArchivedFileNamesAndSize(file);

        foreach (var fileInArchive in filesInArchive)
        {
            var fileName = fileInArchive.Key;
            var mapExtension = fileSystemAccessor.GetFileExtension(fileName);
            var mapName = fileSystemAccessor.GetFileNameWithoutExtension(fileName);

            if (permittedMapFileExtensions.Contains(mapExtension))
            {
                result.Maps.Add(new MapFiles() { Name = fileName, Files = new List<string>() { fileName }});
            }
            else if (shapeMapFileExtensions.Contains(mapExtension))
            {
                var mapFiles = result.Maps.FirstOrDefault(m => m.Name == mapName);
                if (mapFiles == null)
                    result.Maps.Add(new MapFiles() { Name = mapName, IsShapeFile = true, Files = new List<string>() { fileName }});
                else
                    mapFiles.Files.Add(fileName);
            }
        }

        result.IsValid = true;
        return result;
    }
}