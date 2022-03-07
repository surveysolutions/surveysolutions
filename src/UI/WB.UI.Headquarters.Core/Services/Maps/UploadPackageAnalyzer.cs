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
    /*
Mandatory files
    .shp — shape format; the feature geometry itself {content-type: x-gis/x-shapefile}
    .shx — shape index format; a positional index of the feature geometry to allow seeking forwards and backwards quickly {content-type: x-gis/x-shapefile}
    .dbf — attribute format; columnar attributes for each shape, in dBase IV format {content-type: application/octet-stream OR text/plain}
Other files
    .prj — projection description, using a well-known text representation of coordinate reference systems {content-type: text/plain OR application/text}
    .sbn and .sbx — a spatial index of the features {content-type: x-gis/x-shapefile}
    .fbn and .fbx — a spatial index of the features that are read-only {content-type: x-gis/x-shapefile}
    .ain and .aih — an attribute index of the active fields in a table {content-type: x-gis/x-shapefile}
    .ixs — a geocoding index for read-write datasets {content-type: x-gis/x-shapefile}
    .mxs — a geocoding index for read-write datasets (ODB format) {content-type: x-gis/x-shapefile}
    .atx — an attribute index for the .dbf file in the form of shapefile.columnname.atx (ArcGIS 8 and later) {content-type: x-gis/x-shapefile }
    .shp.xml — geospatial metadata in XML format, such as ISO 19115 or other XML schema {content-type: application/fgdc+xml}
    .cpg — used to specify the code page (only for .dbf) for identifying the character encoding to be used {content-type: text/plain OR x-gis/x-shapefile }
    .qix — an alternative quadtree spatial index used by MapServer and GDAL/OGR software {content-type: x-gis/x-shapefile}
     */
    private readonly HashSet<string> shapeMapFileExtensions = new HashSet<string>()
    {
        ".dbf", ".shp", ".shx", 
        ".prj", ".sbn", ".sbx", ".fbn", ".fbx", ".ain", ".aih", ".ixs", ".mxs", ".atx", ".shp.xml", ".cpg", ".qix"
    };

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