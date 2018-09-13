using System.IO;

namespace WB.Services.Export.Infrastructure
{
    public interface IFileSystemAccessor
    {
        Stream OpenOrCreateFile(string filePath, bool append);
        void WriteAllText(string contentFilePath, string toString);
    }
}
