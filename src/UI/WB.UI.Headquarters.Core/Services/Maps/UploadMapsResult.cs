using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.UI.Headquarters.Services.Maps;

public class UploadMapsResult
{
    public List<string> Errors { get; set; } = new List<string>();
    public List<MapBrowseItem> Maps { get; set; } = new List<MapBrowseItem>();
    public bool IsSuccess { get; set; }
}