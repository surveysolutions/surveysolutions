using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events;
using Main.Core.Events.User;
using Main.Core.Utility;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public class SampleImportService : ISampleImportService
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> templateRepository;

        public SampleImportService(IReadSideRepositoryWriter<QuestionnaireDocument> templateRepository)
        {
            this.templateRepository = templateRepository;
        }

        public IEnumerable<CompleteQuestionnaireDocument> GetSampleList(Guid templateId, TextReader textReader)
        {
            var template = templateRepository.GetById(templateId);
            string[] header=null;
            using (var reader = new CsvReader(textReader))
            {

                while (reader.Read())
                {
                    if (header == null)
                    {
                        header = reader.FieldHeaders;
                        continue;
                    }
                    yield return BuiltInterview(Guid.NewGuid(), reader.CurrentRecord,header, template);
                }
            }
        }

        private CompleteQuestionnaireDocument BuiltInterview(Guid publicKey, string[] values, string[] header, QuestionnaireDocument template)
        {
            var ar = new CompleteQuestionnaireAR(publicKey, template, null);

            for (int i = 0; i < header.Length; i++)
            {
                var question =
                    template.FirstOrDefault<IQuestion>(q => q.StataExportCaption == header[i]);
                if (question == null)
                    continue;

                var singleOption = question as SingleQuestion;
                if (singleOption != null)
                {
                    var answer = singleOption.Answers.FirstOrDefault(a => a.AnswerValue == values[i]);
                    ar.SetAnswer(question.PublicKey, null, null, new List<Guid> {answer.PublicKey}, DateTime.Now);
                }
                else
                {
                    ar.SetAnswer(question.PublicKey, null, values[i], null, DateTime.Now);
                }
            }
            return ar.CreateSnapshot();

        }

      /*  public void ParseSource(Guid templateId, TextReader textReader)
        {
            var tempFile = new TempFileImportData() {PublicKey = Guid.NewGuid(), TemplateId = templateId};
            var template = templateRepository.GetById(templateId);
            var valueList = new List<string[]>();
            using (var reader = new CsvReader(textReader))
            {

                while (reader.Read())
                {
                    if (tempFile.Header == null)
                    {
                        tempFile.Header = reader.FieldHeaders;
                        continue;
                    }
                    valueList.Add(reader.CurrentRecord);
        //            yield return BuiltInterview(Guid.NewGuid(), reader.CurrentRecord, header, template);
                }
            }
            tempFile.Values = valueList.ToArray();
            tempFileRepository.Store(tempFile,tempFile.TemplateId);
        }*/
    }
}
