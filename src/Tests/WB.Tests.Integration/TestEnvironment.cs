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
            var sourceFolder = GetSourceFolder(relativePath);

            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException(relativePath);

            return masks.SelectMany(mask => Directory.EnumerateFiles(sourceFolder, mask, SearchOption.AllDirectories));
        }

        public static string GetSourceFolder(string relativePath)
        {
            return Path.Combine(GetSolutionFolder(), relativePath);
        }

        public static string GetSolutionFolder()
        {
            return Directory.GetParent(typeof (TestEnvironment).Assembly.Location).Parent.Parent.Parent.Parent.FullName;
        }
    }
}