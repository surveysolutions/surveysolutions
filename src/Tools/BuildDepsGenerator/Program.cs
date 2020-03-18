using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BuildDepsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = args[0].TrimEnd('\\') + "\\";

            var queue = new Queue<string>();

            var result = new List<string>();

            queue.Enqueue(args[1]);

            while (queue.Count > 0)
            {
                var proj = queue.Dequeue();

                var xml = XDocument.Load(proj);
                var dir = new FileInfo(proj).Directory;
                Directory.SetCurrentDirectory(dir.FullName);
                var tcLine = "+:" + dir.FullName.Replace(root, "") + "\\**";

                result.Add(tcLine);
                
                 foreach (var project in xml.Descendants())
                {
                    if (project.Name.LocalName == "ProjectReference")
                    {
                        var path = Path.GetFullPath(project.Attribute("Include").Value);
                        queue.Enqueue(path);
                    }
                }
            }

            foreach (var line in result.Distinct().OrderBy(r => r))
            {
                Console.WriteLine(line);
            }
        }
    }
}
