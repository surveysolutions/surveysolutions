using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace Utils.Commands
{
    public class DebugInformationDumper
    {
        private readonly IPlainTransactionManagerProvider plainTransactionManager;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireRepository;
        private readonly IEventStore eventStore;
        private readonly ISerializer serializer;
        private readonly IExpressionsPlayOrderProvider expressionsPlayOrderProvider;
        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;
        private readonly ILogger logger;


        public DebugInformationDumper(
            ISerializer serializer, 
            IPlainTransactionManagerProvider plainTransactionManager, 
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireRepository,
            IEventStore eventStore,
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider, 
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor,
            ILogger logger)
        {
            this.serializer = serializer;
            this.plainTransactionManager = plainTransactionManager;
            this.questionnaireRepository = questionnaireRepository;
            this.eventStore = eventStore;
            this.expressionsPlayOrderProvider = expressionsPlayOrderProvider;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.logger = logger;
        }

        public int Run(string serverName)
        {
            logger.Info($"Dumping for for db {serverName}");

            var fileName = $"{serverName}.results.txt";

            if (!File.Exists(fileName))
            {
                //File format exaple. 
                //============================================
                //=Questionnaire: 3c049ea6-8524-4aa8-8ca3-9f8ba4ab351f$10
                //=               LPDP_Tajikistan_IFAD_v6
                //=Interviews with calculation error: 
                //3cdf0898-d31e-492e-b747-00e1c0edb80e
                //9a6d3115-de81-449a-aa8d-499f0507a0ed
                //c1fc1c69-dbe6-467a-9bc9-a1842fb24384
                //============================================
                Console.WriteLine($"File {fileName} is missing. Nothing to dump");
                return 0;
            }

            if (!Directory.Exists(serverName))
            {
                Directory.CreateDirectory(serverName);
            }

            var lines = File.ReadAllLines(fileName);

            var currentFolder = "";
            foreach (var line in lines)
            {
                var questionnaireMarker = "=Questionnaire: ";
                if (line.StartsWith(questionnaireMarker))
                {
                    var questionnaireIdentity = QuestionnaireIdentity.Parse(line.Substring(questionnaireMarker.Length).Trim());
                    currentFolder = Path.Combine(serverName, $"{questionnaireIdentity.QuestionnaireId.FormatGuid()}_{questionnaireIdentity.Version}");
                    if (!Directory.Exists(currentFolder))
                    {
                        Directory.CreateDirectory(currentFolder);
                    }
                    DumpQuestionnaireAsJsonFile(currentFolder, questionnaireIdentity);
                    DumpQuestionnaireAssembly(currentFolder, questionnaireIdentity);
                }

                if (Guid.TryParse(line, out Guid interviewId))
                {
                    DumpSerializedEventStream(currentFolder, interviewId);
                }
            }

            return 1;
        }

        private void DumpSerializedEventStream(string folder, Guid interviewId)
        {
            var eventFileName = Path.Combine(folder, $"{interviewId.FormatGuid()}.json");
            List<CommittedEvent> committedEvents = this.eventStore.Read(interviewId, 0).ToList();
            var serializedEvents = serializer.Serialize(committedEvents);

            if (File.Exists(eventFileName))
                File.Delete(eventFileName);

            File.WriteAllText(eventFileName, serializedEvents);
        }

        private void DumpQuestionnaireAssembly(string folder, QuestionnaireIdentity questionnaireIdentity)
        {
            var assemblyFileName = Path.Combine(folder, $"assembly-{questionnaireIdentity}.dll");
            var assemblyAsBytes = this.plainTransactionManager.GetPlainTransactionManager()
                .ExecuteInQueryTransaction(() => questionnaireAssemblyFileAccessor.GetAssemblyAsByteArray(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

            if (File.Exists(assemblyFileName))
                File.Delete(assemblyFileName);

            File.WriteAllBytes(assemblyFileName, assemblyAsBytes);
        }

        private void DumpQuestionnaireAsJsonFile(string folder, QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireFileName = Path.Combine(folder, $"questionnaire-{questionnaireIdentity}.json");
            QuestionnaireDocument questionnaireDocument = this.plainTransactionManager.GetPlainTransactionManager()
                .ExecuteInQueryTransaction(() => questionnaireRepository.GetById(questionnaireIdentity.ToString()));

            var readOnlyQuestionnaireDocument = questionnaireDocument.AsReadOnly();
            questionnaireDocument.ExpressionsPlayOrder = this.expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            questionnaireDocument.DependencyGraph = this.expressionsPlayOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            questionnaireDocument.ValidationDependencyGraph = this.expressionsPlayOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument)
                .ToDictionary(x => x.Key, x => x.Value.ToArray());

            var serializedQuestionnaire = serializer.Serialize(questionnaireDocument);

            if(File.Exists(questionnaireFileName))
                File.Delete(questionnaireFileName);

            File.WriteAllText(questionnaireFileName, serializedQuestionnaire);
        }
    }
}
