using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FixGuids
{
    class Program
    {
        static void Main(string[] args)
        {
            var csFiles = GetAllFilesInSolution("*.cs");
            var rx = new Regex("Guid\\.Parse\\(\"([\\da-fA-F]{12})([^4])([\\da-fA-F]{19})\"\\)", RegexOptions.Compiled);
            var rx2 = new Regex("Guid\\(\"([\\da-fA-F]{12})([^4])([\\da-fA-F]{19})\"\\)", RegexOptions.Compiled);

            foreach (var file in csFiles)
            {
                var content = File.ReadAllText(file);
                var matches = rx.Matches(content).Concat(rx2.Matches(content));
                long count = 0;

                foreach (Match match in matches)
                {
                    var index = match.Groups[2].Index;
                    var left = content.Substring(0, index);
                    var right = content.Substring(index + 1);

                    content = left + "4" + right;
                    count++;
                }

                if (count > 0)
                {
                    var encoding = GetEncoding(file);
                    File.WriteAllText(file, content, encoding);
                    Console.WriteLine($"Updated {count} Guids in {file.Replace(GetSolutionFolderPath(), string.Empty)}");
                }
            }
        }

        public static Encoding GetEncoding(string filename)
        {
            using var sr = new StreamReader(filename, true);
            return sr.CurrentEncoding;
            //// Read the BOM
            //var bom = new byte[4];
            //using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            //{
            //    file.Read(bom, 0, 4);
            //}

            //// Analyze the BOM
            //if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            //if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            //if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            //if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            //if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            //return Encoding.ASCII;
        }
    
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
                    .Select(m => Path.Combine(slnFolder, m.Value)).ToList();
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


        private static string _solutionFolderPathCache = null;

        public static string GetSolutionFolderPath()
        {
            return _solutionFolderPathCache ?? (_solutionFolderPathCache = GetParentDirectoryContainingDirectories(Directory.GetCurrentDirectory(), "UI", "Tests", "Core"));
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
