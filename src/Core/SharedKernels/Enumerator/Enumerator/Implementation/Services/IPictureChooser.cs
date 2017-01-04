using System.IO;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public interface IPictureChooser
    {
        Task<Stream> TakePicture();
    }
}