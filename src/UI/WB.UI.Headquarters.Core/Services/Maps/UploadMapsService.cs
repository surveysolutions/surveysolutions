using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.Services.Maps;

class UploadMapsService : IUploadMapsService
{
    private readonly IFileSystemAccessor fileSystemAccessor;
    private readonly IUploadPackageAnalyzer uploadPackageAnalyzer;
    private readonly ILogger<UploadMapsService> logger;
    private readonly IArchiveUtils archiveUtils;
    private readonly IMapStorageService mapStorageService;
    private readonly IMapFilesValidator mapFilesValidator;

    private const string TempFolderName = "TempMapsData";

    private readonly string path;

    public UploadMapsService(IFileSystemAccessor fileSystemAccessor,
        IUploadPackageAnalyzer uploadPackageAnalyzer,
        ILogger<UploadMapsService> logger,
        IArchiveUtils archiveUtils,
        IMapStorageService mapStorageService,
        IOptions<FileStorageConfig> fileStorageConfig,
        IMapFilesValidator mapFilesValidator
        )
    {
        this.fileSystemAccessor = fileSystemAccessor;
        this.uploadPackageAnalyzer = uploadPackageAnalyzer;
        this.logger = logger;
        this.archiveUtils = archiveUtils;
        this.mapStorageService = mapStorageService;
        this.mapFilesValidator = mapFilesValidator;

        this.path = fileSystemAccessor.CombinePath(fileStorageConfig.Value.TempData, TempFolderName);
        if (!fileSystemAccessor.IsDirectoryExists(this.path))
            fileSystemAccessor.CreateDirectory(this.path);
    }

    public async Task<UploadMapsResult> Upload(string filename, Stream content)
    {
        var result = new UploadMapsResult();
        
        if (".zip" != this.fileSystemAccessor.GetFileExtension(filename).ToLower())
        {
            result.Errors.Add(Resources.Maps.MapLoadingNotZipError);
            return result;
        }

        AnalyzeResult analyzeResult = null;
        try
        {
            analyzeResult = uploadPackageAnalyzer.Analyze(content);
            if (!analyzeResult.IsValid || analyzeResult.Maps.Count == 0)
            {
                result.Errors.Add(Resources.Maps.MapLoadingNoMapsInArchive);
                return result;
            }
            
            var validatorErrors = mapFilesValidator.Verify(analyzeResult).ToList();
            if (validatorErrors.Any())
            {
                validatorErrors.ForEach(error => result.Errors.Add(error.Message));
                return result;
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Invalid map archive", ex);

            result.Errors.Add(Resources.Maps.MapsLoadingError);
            return result;
        }

        var invalidMaps = new List<Tuple<string, Exception>>();
        string mapsInTempDirectory = null;
        
        try
        {
            mapsInTempDirectory = ExtractMapsToTempDirectory(content);

            foreach (var map in analyzeResult.Maps)
            {
                try
                {
                    result.Maps.Add(await mapStorageService.SaveOrUpdateMapAsync(map, mapsInTempDirectory));
                }
                catch (Exception e)
                {
                    logger.LogError($"Error on maps import map {map.Name}", e);
                    invalidMaps.Add(new Tuple<string, Exception>(map.Name, e));
                }
            }

            if (invalidMaps.Count > 0)
                result.Errors.AddRange(invalidMaps.Select(x => String.Format(Resources.Maps.MapLoadingInvalidFile, x.Item1, x.Item2.Message)).ToList());
            else
                result.IsSuccess = true;
        }
        catch (Exception e)
        {
            logger.LogError("Error on maps import", e);
            result.Errors.Add(Resources.Maps.MapsLoadingError);
            return result;
        }
        finally
        {
            if (mapsInTempDirectory != null && fileSystemAccessor.IsDirectoryExists(mapsInTempDirectory))
                fileSystemAccessor.DeleteDirectory(mapsInTempDirectory);
        }

        return result;
    }
    
    private string ExtractMapsToTempDirectory(Stream content)
    {
        var tempFileStore = Guid.NewGuid().FormatGuid();
        var pathToSave = this.fileSystemAccessor.CombinePath(this.path, tempFileStore);
        if (!this.fileSystemAccessor.IsDirectoryExists(pathToSave))
            this.fileSystemAccessor.CreateDirectory(pathToSave);

        archiveUtils.Unzip(content, pathToSave);
        return pathToSave;
    }
}