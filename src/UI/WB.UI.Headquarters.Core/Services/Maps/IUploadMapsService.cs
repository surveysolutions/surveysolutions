using System.IO;
using System.Threading.Tasks;
using WB.UI.Headquarters.Implementation.Maps;

namespace WB.UI.Headquarters.Services.Maps;

public interface IUploadMapsService
{
    Task<UploadMapsResult> Upload(string filename, Stream content);
}