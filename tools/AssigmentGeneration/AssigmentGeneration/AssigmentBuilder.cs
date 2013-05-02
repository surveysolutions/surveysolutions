using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Ionic.Zip;
using Ionic.Zlib;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Complete.Question;
using Main.Core.Events;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Restoring.EventStapshoot;
using Newtonsoft.Json;
using SynchronizationMessages.Export;

namespace AssigmentGeneration
{
    public class AssigmentBuilder
    {
        private readonly Guid commitid = Guid.NewGuid();
        private readonly Guid supKey;
        private readonly string supName;
        private readonly  QuestionnaireDocument template;
        private readonly  JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
        private List<string[]> assigmentValues;

        public AssigmentBuilder(string filePath, string valueFilePath, Guid supKey,string supName)
        {
            this.supKey = supKey;
            this.supName = supName;
            template = JsonConvert.DeserializeObject<QuestionnaireDocument>(File.OpenText(filePath).ReadToEnd(),
                                                                            settings);
            
            this.assigmentValues = new List<string[]>();
            using (var reader = new CsvReader(File.OpenText(valueFilePath)))
            {
                
                while (reader.Read())
                {
                    if(assigmentValues.Count==0)
                        assigmentValues.Add(reader.FieldHeaders);
                    assigmentValues.Add(reader.CurrentRecord);
                }
            }

        }

        public void CreateFile()
        {
           
            var zipFile = new ZipFile();

            zipFile.CompressionLevel = CompressionLevel.BestSpeed;
            zipFile.ParallelDeflateThreshold = -1;
            var events = BuildAssigmentEventStream();
            var data = new ZipFileData() { ClientGuid = Guid.NewGuid(), Events = events };

            string result =
                JsonConvert.SerializeObject(data, Formatting.None, settings);

            zipFile.AddEntry("backup.txt", result, Encoding.UTF8);

            zipFile.Save("assigmentdata.capi");
        }

        protected IList<AggregateRootEvent> BuildAssigmentEventStream()
        {
            var result = new List<AggregateRootEvent>();
            result.Add(
                new AggregateRootEvent(new CommittedEvent(commitid, Guid.NewGuid(), template.PublicKey, 1, DateTime.Now,
                                                          new SnapshootLoaded()
                                                              {
                                                                  Template = new Snapshot(template.PublicKey, 1, template)
                                                              },
                                                          new Version(1, 1, 1, 1))));
            foreach (var assigmentValue in assigmentValues.Skip(1))
            {
                result.Add(BuildNewAssigmentEvent(Guid.NewGuid(), assigmentValue));
            }
            return result;
        }

        protected AggregateRootEvent BuildNewAssigmentEvent(Guid publicKey, string[] values)
        {
            return
                new AggregateRootEvent(new CommittedEvent(commitid, Guid.NewGuid(), publicKey, 1, DateTime.Now,
                                                          new SnapshootLoaded()
                                                              {
                                                                  Template = new Snapshot(publicKey, 1, BuiltSnapshoot( publicKey, values))
                                                              }, new Version(1, 1, 1, 1)));

        }

        private CompleteQuestionnaireDocument BuiltSnapshoot(Guid publicKey, string[] values)
        {
            var result = (CompleteQuestionnaireDocument)template;

            result.PublicKey = publicKey;
            result.Status = SurveyStatus.Unassign;
            result.Creator = new UserLight(this.supKey, this.supName);
            for (int i = 0; i < assigmentValues[0].Length; i++)
            {
                var question =
                    result.FirstOrDefault<ICompleteQuestion>(q => q.StataExportCaption == assigmentValues[0][i]);
                var singleOption = question as SingleCompleteQuestion;
                if (singleOption!=null)
                {
                    var answer = singleOption.Answers.FirstOrDefault(a => a.AnswerValue == values[i]);
                    question.SetAnswer(new List<Guid> { answer.PublicKey }, null);
                }
                else
                {
                    question.SetAnswer(null, values[i]);
                }
            }
            return result;
        }
    }
}
