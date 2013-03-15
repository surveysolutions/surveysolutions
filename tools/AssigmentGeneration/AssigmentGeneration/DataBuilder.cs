using System;
using System.Collections.Generic;
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
    public class DataBuilder
    {
        private KeyValuePair<string, string> supervisor;
        private IDictionary<string, string> interviewers;
        private readonly Guid commitid = Guid.NewGuid();

        public DataBuilder(KeyValuePair<string, string> supervisor, IDictionary<string, string> interviewers)
        {
            this.supervisor = supervisor;
            this.interviewers = interviewers;
        }

        public void CreateFile()
        {
            var zipFile = new ZipFile();

            zipFile.CompressionLevel = CompressionLevel.BestSpeed;
            zipFile.ParallelDeflateThreshold = -1;
            var events = BuildUserEventStream();
            var data = new Questionnaire.Core.Web.Export.ZipFileData() { ClientGuid = Guid.NewGuid(), Events = events };
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };

            string result =
                JsonConvert.SerializeObject(data, Formatting.None, settings);

            zipFile.AddEntry("backup.txt", result);

            zipFile.Save("inidata.capi");
        }

        private IList<AggregateRootEvent> BuildUserEventStream()
        {
            var result = new List<AggregateRootEvent>();
            var supervisorKey = Guid.NewGuid();
            result.Add(BuildNewUserEvent(supervisorKey, supervisor.Key, supervisor.Value,
                                                         UserRoles.Supervisor, null));
            foreach (var interviewer in interviewers)
            {
                result.Add(BuildNewUserEvent(Guid.NewGuid(), interviewer.Key, interviewer.Value, UserRoles.Operator,
                                             new UserLight(supervisorKey, supervisor.Key)));
            }
            return result;
        }

        protected AggregateRootEvent BuildNewUserEvent(Guid publicKey, string name, string password, UserRoles role,
                                                       UserLight parent)
        {
            return
                new AggregateRootEvent(new CommittedEvent(commitid, Guid.NewGuid(), publicKey, 1, DateTime.Now,
                                                          new NewUserCreated()
                                                          {
                                                              PublicKey = publicKey,
                                                              Name = name,
                                                              Password = SimpleHash.ComputeHash(password),
                                                              IsLocked = false,
                                                              Roles = new UserRoles[] { role },
                                                              Supervisor = parent
                                                          }, new Version(1,1,1,1)));

        }
    }
}
