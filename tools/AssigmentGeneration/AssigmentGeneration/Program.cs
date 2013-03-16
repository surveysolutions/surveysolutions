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
            Console.WriteLine("Enter path to file with users. Firs user have to be supervisor:");
            var usersPath = Console.ReadLine();

            var builder = new DataBuilder(usersPath);
            builder.CreateFile();

            Console.WriteLine("Enter questionnaire template path and assigment values path:");
            var files = Console.ReadLine().Split(' ');
            AssigmentBuilder assigments = new AssigmentBuilder(files[0].Trim(), files[1].Trim(), builder.SupervisorKey,
                                                               builder.SupervisorName);
            assigments.CreateFile();
            Console.ReadLine();
        }

      
    }
}
