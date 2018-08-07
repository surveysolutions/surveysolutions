using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Ncqrs.Eventing;
using Utils.CustomInfrastructure;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace Utils.Commands
{
    public class CoreDebugger
    {
        private readonly ISerializer serializer;
        private readonly IQuestionnaireAssemblyAccessor assemblyAccessor;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ICommandService commandService;

        public CoreDebugger(
            ISerializer serializer,
            IQuestionnaireAssemblyAccessor assemblyAccessor,
            IQuestionnaireStorage questionnaireStorage,
            ICommandService commandService)
        {
            this.serializer = serializer;
            this.assemblyAccessor = assemblyAccessor;
            this.questionnaireStorage = questionnaireStorage;
            this.commandService = commandService;
        }

        public int Run(string folder)
        {
            int result = 1;

            var directories = Directory.EnumerateDirectories(folder).ToList();
            if (!directories.Any())
                result *= RunForQuestionnire(folder);

            foreach (var directory in directories)
            {
                result *= Run(directory);
            }

            return result;
        }

        public int RunForQuestionnire(string folder)
        {
            var files = Directory.EnumerateFiles(folder).ToList();

            if (!files.Any())
            {
                Console.WriteLine("Empty folder. Check -f param");
                return 0;
            }

            var assemblyDllFileName = files.Single(x => Path.GetFileName(x).StartsWith("assembly-"));

            var questionnairePrefix = "questionnaire-";
            var questionnaireJsonFileName = files.Single(x => Path.GetFileName(x).StartsWith(questionnairePrefix));
            var questionnaireIdentity = QuestionnaireIdentity.Parse(Path.GetFileNameWithoutExtension(questionnaireJsonFileName).Substring(questionnairePrefix.Length));
            var questionnaireDocument = serializer.Deserialize<QuestionnaireDocument>(File.ReadAllText(questionnaireJsonFileName));

            var isExistsMacrosesInDocument = Utils.IsExistsMacrosesInDocument(questionnaireDocument);
            if (isExistsMacrosesInDocument)
            {
                if (!Utils.IsSupportedDecompile(assemblyDllFileName))
                {
                    Console.WriteLine($"Analyze folder {folder}. Dll doesn't supported decompile operation. Skiped.");
                    return 1;
                }

                Utils.InlineMacrosesInDocument(questionnaireDocument, assemblyDllFileName);
            }

            questionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
            if (questionnaireDocument == null)
            {
                Console.WriteLine("Can't deserialize questionnaire");
                return 0;
            }

            Console.WriteLine($"Analyze folder {folder}. Is exists macroses: {isExistsMacrosesInDocument}");

            assemblyAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, File.ReadAllBytes(assemblyDllFileName));

            foreach (var file in files)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                if (Guid.TryParse(fileNameWithoutExtension, out Guid interviewId))
                {
                    var events = serializer.Deserialize<List<CommittedEvent>>(File.ReadAllText(file));
                    try
                    {
                        CreateInterviewAndApplyEvents(interviewId, events);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            return 1;
        }

        private void CreateInterviewAndApplyEvents(Guid interviewId, List<CommittedEvent> committedEvents)
        {
//            if (interviewId != Guid.Parse("07153208-526f-4688-9173-2d7d691328f1"))
//                return;

            //Console.WriteLine($"Process interviewId {interviewId}");

            var userId = Guid.Parse("22222222222222222222222222222222");
            var createCommand = EventsToCommandConverter.GetCreateInterviewCommand(committedEvents, interviewId, userId);
            commandService.Execute(createCommand);

            var indexOfFirstSupervisorAssignedEvent = committedEvents.FindIndex(0, x => x.Payload is SupervisorAssigned);

            for (int i = indexOfFirstSupervisorAssignedEvent; i < committedEvents.Count; i++)
            {
                var committedEvent = committedEvents[i];

                var commands = EventsToCommandConverter.ConvertEventToCommands(interviewId, committedEvent)?.ToList();

                if (commands == null)
                    continue;

                try
                {
                    foreach (var command in commands)
                    {
                        if (command is RemoveAnswerCommand)
                        {
                            try
                            {
                                commandService.Execute(command);
                            }
                            catch (InterviewException exception)
                            {
                                if (!(exception.Message.Contains(
                                          "is disabled and question's answer cannot be changed") ||
                                      exception.Message.Contains("No questions found for roster vector")))
                                {
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            commandService.Execute(command);
                        }
                    }
                }
                catch (InterviewException exception)
                {
                    var message = exception.ExceptionType ==
                                  InterviewDomainExceptionType.ExpessionCalculationError
                        ? $"Calculation error! IN: {interviewId}. Event: {committedEvent.EventSequence} / {committedEvents.Count}"
                        : $"General error! IN: {interviewId}. Event: {committedEvent.EventSequence} / {committedEvents.Count}";

                    Console.WriteLine(message);

                    break;
                }
            }
        }
    }
}
