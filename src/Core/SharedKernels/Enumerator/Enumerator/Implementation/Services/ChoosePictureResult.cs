using System.IO;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services;

public class ChoosePictureResult
{
    public ChoosePictureResult(string fileName, Stream stream)
    {
        FileName = fileName;
        Stream = stream;
    }

    public string FileName { get; }
    public Stream Stream { get; }
}