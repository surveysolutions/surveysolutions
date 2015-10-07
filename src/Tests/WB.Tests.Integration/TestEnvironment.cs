using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WB.Tests.Integration
{
    internal static class TestEnvironment
    {
        public static IEnumerable<string> GetAllFilesFromSourceFolder(string relativePath, params string[] masks)
        {
            var sourceFolder = GetSourcePath(relativePath);

            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException(relativePath);

            return masks.SelectMany(mask => Directory.EnumerateFiles(sourceFolder, mask, SearchOption.AllDirectories));
        }

        public static string GetSourcePath(string relativePath)
        {
            return Path.Combine(GetSolutionFolderPath(), relativePath);
        }

        public static string GetSolutionFolderPath()
        {
            return Directory.GetParent(typeof (TestEnvironment).Assembly.Location).Parent.Parent.Parent.Parent.FullName;
        }
    }
}