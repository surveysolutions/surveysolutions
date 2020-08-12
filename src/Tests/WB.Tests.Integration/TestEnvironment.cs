using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace WB.Tests.Integration
{
    internal static class TestEnvironment
    {
        private static List<string> projectsInSolution = null;

        public static IEnumerable<string> GetAllProjectsInSolution()
        {
            if (projectsInSolution == null)
            {
                var slnFolder = GetSolutionFolderPath();
                var sln = Path.Combine(slnFolder, "WB.sln");
                var content = File.ReadAllLines(sln);
                projectsInSolution = content.Where(c => c.Contains(".csproj", StringComparison.OrdinalIgnoreCase))
                    .Select(l => Regex.Match(l, @"[^""]*\.csproj"))
                    .Where(m => m.Success)
                    .Select(m => Path.Combine(slnFolder, m.Value.Replace('\\', Path.DirectorySeparatorChar))).ToList();
            }

            return projectsInSolution;
        }

        public static IEnumerable<string> GetAllFilesInSolution(string mask)
        {
            var projects = GetAllProjectsInSolution();
            return projects
                .SelectMany(p =>
                {
                    var dir = new FileInfo(p).Directory.FullName;
                    return Directory.EnumerateFiles(dir, mask, SearchOption.AllDirectories);
                }).ToList();
        }

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

        private static string _solutionFolderPathCache = null;

        public static string GetSolutionFolderPath()
        {
            return _solutionFolderPathCache ?? (_solutionFolderPathCache = GetParentDirectoryContainingDirectories(TestContext.CurrentContext.TestDirectory, "UI", "Tests", "Core"));
        }

        private static string GetParentDirectoryContainingDirectories(string fodler, params string[] dirMarkers)
        {
            var dirInfo = new DirectoryInfo(fodler);

            while (dirInfo.Root != dirInfo)
            {
                var folders = dirInfo.EnumerateDirectories().Select(ed => ed.Name).ToArray();

                if (dirMarkers.All(dm => folders.Contains(dm)))
                {
                    return dirInfo.FullName;
                }

                dirInfo = dirInfo.Parent;
            }

            return null;
        }
    }
}
