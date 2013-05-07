using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace AssigmentGeneration
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Create new users? (Y/N)");
            var answer = Console.ReadLine();
            Guid supervisorKey;
            string supervisorName;
            if (string.Compare(answer,"Y",true)==0)
            {
                Console.WriteLine("Enter path to file with users. Firs user have to be supervisor:");
                var usersPath = Console.ReadLine();

                var builder = new DataBuilder(usersPath);
                builder.CreateFile();
                supervisorKey = builder.SupervisorKey;
                supervisorName = builder.SupervisorName;
            }
            else if(string.Compare(answer,"N", true)==0)
            {
                Console.WriteLine("Enter supervisorKey and login name:");
                var supervisor = Console.ReadLine().Split(' ');
                supervisorKey = Guid.Parse(supervisor[0].Trim());
                supervisorName = supervisor[1].Trim();

            }
            else
            {
                return;
            }

            /*------------------------------------------Assigments----------------------------------------------------*/
            Console.WriteLine("Create assigment users? (Y/N)");
            var answerAssigments = Console.ReadLine();
            if (string.Compare(answerAssigments, "Y", true) == 0)
            {

                Console.WriteLine("Enter questionnaire template path and assigment values path:");
                var files = Console.ReadLine().Split(' ');
                AssigmentBuilder assigments = new AssigmentBuilder(files[0].Trim(), files[1].Trim(), supervisorKey,
                                                                   supervisorName);
                assigments.CreateFile();
            }
            Console.ReadLine();
        }

      
    }
}
