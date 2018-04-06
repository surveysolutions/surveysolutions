using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using NpgsqlTypes;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public partial class AssignmentsImportService : IAssignmentsImportService
    {
        private const string AssignmentsToImportTableName = "\"plainstore\".\"assignmenttoimport\"";
        private const string AssignmentsImportProcessTableName = "\"plainstore\".\"assignmentsimportprocess\"";

        private readonly ICsvReader csvReader;
        private readonly IArchiveUtils archiveUtils;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IUserViewFactory userViewFactory;
        private readonly IFileSystemAccessor fileSystem;
        private readonly IPreloadedDataVerifier verifier;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ISessionProvider sessionProvider;
        private readonly AssignmentsImportTask assignemtsImportTask;
        private readonly IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository;
        private readonly IPlainStorageAccessor<AssignmentImportData> importAssignmentsRepository;
        private readonly ISerializer serializer;

        public AssignmentsImportService(ICsvReader csvReader, IArchiveUtils archiveUtils, 
            IQuestionnaireStorage questionnaireStorage, IUserViewFactory userViewFactory, 
            IFileSystemAccessor fileSystem,
            IPreloadedDataVerifier verifier,
            IAuthorizedUser authorizedUser,
            ISessionProvider sessionProvider,
            AssignmentsImportTask assignemtsImportTask,
            IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository,
            IPlainStorageAccessor<AssignmentImportData> importAssignmentsRepository,
            ISerializer serializer)
        {
            this.csvReader = csvReader;
            this.archiveUtils = archiveUtils;
            this.questionnaireStorage = questionnaireStorage;
            this.userViewFactory = userViewFactory;
            this.fileSystem = fileSystem;
            this.verifier = verifier;
            this.authorizedUser = authorizedUser;
            this.sessionProvider = sessionProvider;
            this.assignemtsImportTask = assignemtsImportTask;
            this.importAssignmentsProcessRepository = importAssignmentsProcessRepository;
            this.importAssignmentsRepository = importAssignmentsRepository;
            this.serializer = serializer;
        }

        public IEnumerable<PanelImportVerificationError> VerifySimple(PreloadedFile file, QuestionnaireIdentity questionnaireIdentity)
        {
            if (this.assignemtsImportTask.IsJobRunning())
                throw new PreloadingException(PreloadingVerificationMessages.HasAssignmentsToImport);

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            bool hasErrors = false;

            foreach (var columnError in this.verifier.VerifyColumns(new[] { file.FileInfo }, questionnaire))
            {
                hasErrors = true;
                yield return columnError;
            }

            if (hasErrors) yield break;

            var assignmentRows = new List<AssignmentRow>();

            foreach (var assignmentRow in this.GetAssignmentRows(file, questionnaire))
            {
                foreach (var answerError in this.verifier.VerifyAnswers(assignmentRow, questionnaire))
                    yield return answerError;

                assignmentRows.Add(assignmentRow);
            }

            if (hasErrors) yield break;

            this.Save(file.FileInfo.FileName, assignmentRows.Select(ToAssignmentToImport).ToArray());
            assignemtsImportTask.Run();
        }

        private AssignmentImportData ToAssignmentToImport(AssignmentRow row)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PanelImportVerificationError> VerifyPanel(string originalFileName,
            PreloadedFile[] allImportedFiles,
            QuestionnaireIdentity questionnaireIdentity)
        {
            if (this.assignemtsImportTask.IsJobRunning())
                throw new PreloadingException(PreloadingVerificationMessages.HasAssignmentsToImport);

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            bool hasErrors = false;

            var preloadedFileInfos = allImportedFiles.Select(x => x.FileInfo).ToArray();

            foreach (var fileInfo in preloadedFileInfos)
                foreach (var fileError in this.verifier.VerifyFile(fileInfo, questionnaire))
                {
                    hasErrors = true;
                    yield return fileError;
                }

            if (hasErrors) yield break;

            foreach (var columnError in this.verifier.VerifyColumns(preloadedFileInfos, questionnaire))
            {
                hasErrors = true;
                yield return columnError;
            }

            if (hasErrors) yield break;

            var assignmentRows = new List<AssignmentRow>();

            foreach (var importedFile in allImportedFiles)
                foreach (var assignmentRow in this.GetAssignmentRows(importedFile, questionnaire))
                {
                    foreach (var answerError in this.verifier.VerifyAnswers(assignmentRow, questionnaire))
                    {
                        hasErrors = true;
                        yield return answerError;
                    }

                    assignmentRows.Add(assignmentRow);
                }

            if (hasErrors) yield break;

            foreach (var rosterError in this.verifier.VerifyRosters(assignmentRows, questionnaire))
            {
                hasErrors = true;
                yield return rosterError;
            }

            if (hasErrors) yield break;

            this.Save(originalFileName, assignmentRows.Select(ToAssignmentToImport).ToArray());
            assignemtsImportTask.Run();
        }

        public AssignmentImportData GetAssignmentToImport()
            => this.importAssignmentsRepository.Query(x => x.FirstOrDefault());

        public void RemoveImportedAssignment(AssignmentImportData assignment)
            => this.importAssignmentsRepository.Remove(new[] { assignment });

        public AssignmentsImportStatus GetImportStatus()
        {
            var process = this.importAssignmentsProcessRepository.Query(x => x.FirstOrDefault());
            var assignmentsInQueue = this.importAssignmentsRepository.Query(x => x.Count());

            return new AssignmentsImportStatus
            {
                IsOwnerOfRunningProcess = process?.Responsible == this.authorizedUser.UserName,
                IsInProgress = assignmentsInQueue > 0,
                TotalAssignmentsWithResponsible = process?.AssignedToInterviewersCount + process?.AssignedToSupervisorsCount ?? 0,
                AssignmentsInQueue = assignmentsInQueue,
                FileName = process?.FileName,
                StartedDate = process?.StartedDate,
                ResponsibleName = process?.Responsible,
                QuestionnaireIdentity = process?.QuestionnaireId == null ? null : QuestionnaireIdentity.Parse(process.QuestionnaireId)
            };
        }

        public void RemoveAllAssignmentsToImport()
        => this.sessionProvider.GetSession().Connection.Execute(
                $"DELETE FROM {AssignmentsToImportTableName};" +
                $"DELETE FROM {AssignmentsImportProcessTableName};");

        private void Save(string fileName, IList<AssignmentImportData> assignments)
        {
            this.SaveProcess(fileName, assignments);
            this.SaveAssignments(assignments);
        }

        private void SaveProcess(string fileName, IList<AssignmentImportData> assignments)
        {
            var process = this.importAssignmentsProcessRepository.Query(x => x.FirstOrDefault()) ??
                          new AssignmentsImportProcess();

            process.FileName = fileName;
            process.AssignedToInterviewersCount = assignments.Count(x => x.InterviewerId.HasValue);
            process.AssignedToSupervisorsCount = assignments.Count(x => x.SupervisorId.HasValue);
            process.Responsible = this.authorizedUser.UserName;
            process.StartedDate = DateTime.UtcNow;

            this.importAssignmentsProcessRepository.Store(process, process?.Id);
        }

        private void SaveAssignments(IList<AssignmentImportData> assignments)
        {
            var npgsqlConnection = this.sessionProvider.GetSession().Connection as NpgsqlConnection;

            using (var writer = npgsqlConnection.BeginBinaryImport($"COPY  {AssignmentsToImportTableName} (responsible, quantity, answers) " +
                                                                   "FROM STDIN BINARY;"))
            {
                foreach (var assignmentToImport in assignments)
                {
                    writer.StartRow();
                    writer.Write(assignmentToImport.InterviewerId, NpgsqlDbType.Uuid);
                    writer.Write(assignmentToImport.SupervisorId, NpgsqlDbType.Uuid);
                    writer.Write(assignmentToImport.Quantity, NpgsqlDbType.Integer);
                    writer.Write(this.serializer.Serialize(assignmentToImport.PreloadedData.Answers), NpgsqlDbType.Jsonb);
                }
            }
        }
    }
}
