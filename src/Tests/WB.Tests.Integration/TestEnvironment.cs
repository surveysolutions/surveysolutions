using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace WB.Tests.Integration
{
    internal static class TestEnvironment
    {
        private static readonly HashSet<string> IgnoredDirectoryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bin",
            "obj",
            "packages",
            "node_modules",
            ".git",
            ".svn",
            ".vs"
        };

        private static List<string> projectsInSolution = null;

        // Caches the full recursive file listing (per mask) so that repeated calls
        // (e.g. once per culture in a [TestCase]-driven test) don't re-walk the whole
        // solution's directory tree every time. Additionally, since callers in this
        // codebase always request "*.resx"-style masks, we cache a single superset
        // listing of "*.resx" files and filter it in-memory for any narrower mask,
        // avoiding multiple full directory-tree scans altogether.
        private static List<string> allResxFilesInSolutionCache = null;

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
            // All current callers pass a "*.resx"-based mask (e.g. "*.resx", "*.ru.resx",
            // "*.??.resx", "*.??-??.resx"). Rather than re-scanning every project directory
            // on disk for every call/culture, scan once for "*.resx" and reuse that listing,
            // filtering in-memory using the same wildcard semantics as Directory.EnumerateFiles.
            if (allResxFilesInSolutionCache == null)
            {
                var projects = GetAllProjectsInSolution();
                var projectDirectories = projects
                    .Select(p => new FileInfo(p).Directory.FullName)
                    .Distinct();

                allResxFilesInSolutionCache = projectDirectories
                    .SelectMany(dir => EnumerateFilesRecursively(dir, "*.resx"))
                    .Distinct()
                    .ToList();
            }

            if (string.Equals(mask, "*.resx", StringComparison.OrdinalIgnoreCase))
                return allResxFilesInSolutionCache;

            return allResxFilesInSolutionCache
                .Where(f => FileSystemName.MatchesSimpleExpression(mask, Path.GetFileName(f)))
                .ToList();
        }

        public static IEnumerable<string> GetAllFilesFromSourceFolder(string relativePath, params string[] masks)
        {
            var sourceFolder = GetSourcePath(relativePath);

            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException(relativePath);

            return masks.SelectMany(mask => EnumerateFilesRecursively(sourceFolder, mask));
        }

        private static IEnumerable<string> EnumerateFilesRecursively(string rootDirectory, string mask)
        {
            if (string.IsNullOrEmpty(rootDirectory) || !Directory.Exists(rootDirectory))
                return Enumerable.Empty<string>();

            var stack = new Stack<string>();
            stack.Push(rootDirectory);

            var files = new List<string>();
            while (stack.Count > 0)
            {
                var directory = stack.Pop();

                foreach (var childDirectory in Directory.EnumerateDirectories(directory))
                {
                    var childDirectoryName = Path.GetFileName(childDirectory);
                    if (IgnoredDirectoryNames.Contains(childDirectoryName))
                        continue;

                    stack.Push(childDirectory);
                }

                files.AddRange(Directory.EnumerateFiles(directory, mask, SearchOption.TopDirectoryOnly));
            }

            return files;
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
