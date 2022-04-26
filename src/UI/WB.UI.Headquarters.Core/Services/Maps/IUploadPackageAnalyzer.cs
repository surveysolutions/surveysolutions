using System.IO;
using Microsoft.AspNetCore.Http;

namespace WB.UI.Headquarters.Services.Maps;

public interface IUploadPackageAnalyzer
{
    AnalyzeResult Analyze(Stream file);
}