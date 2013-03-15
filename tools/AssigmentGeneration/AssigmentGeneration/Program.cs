using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.Core.Events.User;
using Main.Core.Utility;
using Ncqrs.Eventing;
using Newtonsoft.Json;
using Questionnaire.Core.Web.Export;

namespace AssigmentGeneration
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Enter supervisor login and password separated by space:");
            var supervisorString = Console.ReadLine().Split(' ');
            var supervisor = new KeyValuePair<string, string>(supervisorString[0].Trim(), supervisorString[1].Trim());


            var interviwers = new Dictionary<string, string>();
            while (true)
            {
                Console.WriteLine("Enter interviwer login and password separated by space:");
                var interviewer = Console.ReadLine();
                if (string.IsNullOrEmpty(interviewer))
                    break;
                var intData = interviewer.Split(' ');
                if (intData.Count() < 2)
                    break;
                if (string.IsNullOrEmpty(intData[0]) || string.IsNullOrEmpty(intData[1]))
                    break;
                interviwers.Add(intData[0], intData[1]);
            }
            var builder = new DataBuilder(supervisor, interviwers);
            builder.CreateFile();
            Console.ReadLine();
        }

      
    }
}
