using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class AssignmentsImportService : IAssignmentsImportService
    {
        private readonly IPreloadedDataVerifier verifier;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUnitOfWork sessionProvider;
        private readonly IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository;
        private readonly IPlainStorageAccessor<AssignmentToImport> importAssignmentsRepository;
        private readonly IAssignmentsImportFileConverter assignmentsImportFileConverter;
        private readonly IAssignmentFactory assignmentFactory;
        private readonly IInvitationService invitationService;
        private readonly IAssignmentPasswordGenerator passwordGenerator;

        public AssignmentsImportService(IPreloadedDataVerifier verifier,
            IAuthorizedUser authorizedUser,
            IUnitOfWork sessionProvider,
            IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository,
            IPlainStorageAccessor<AssignmentToImport> importAssignmentsRepository,
            IAssignmentsImportFileConverter assignmentsImportFileConverter,
            IAssignmentFactory assignmentFactory,
            IInvitationService invitationService, 
            IAssignmentPasswordGenerator passwordGenerator)
        {
            this.verifier = verifier;
            this.authorizedUser = authorizedUser;
            this.sessionProvider = sessionProvider;
            this.importAssignmentsProcessRepository = importAssignmentsProcessRepository;
            this.importAssignmentsRepository = importAssignmentsRepository;
            this.assignmentsImportFileConverter = assignmentsImportFileConverter;
            this.assignmentFactory = assignmentFactory;
            this.invitationService = invitationService;
            this.passwordGenerator = passwordGenerator;
        }

        public IEnumerable<PanelImportVerificationError> VerifySimpleAndSaveIfNoErrors(PreloadedFile file, Guid defaultResponsibleId, IQuestionnaire questionnaire)
        {
            bool hasErrors = false;
            var assignmentRows = new List<PreloadingAssignmentRow>();
            var assignmentsRows = this.assignmentsImportFileConverter.GetAssignmentRows(file, questionnaire);

            foreach (var assignmentRow in assignmentsRows)
            {
                foreach (var answerError in this.verifier.VerifyRowValues(assignmentRow, questionnaire))
                {
                    hasErrors = true;
                    yield return answerError;
                }

                assignmentRows.Add(assignmentRow);
            }

            foreach (var passwordError in this.verifier.VerifyWebPasswords(assignmentRows, questionnaire))
            {
                hasErrors = true;
                yield return passwordError;
            }

            if (hasErrors) yield break;

            var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version);

            var assignmentToImports = ConcatRosters(assignmentRows, questionnaire);

            if (assignmentToImports.Count == 0)
                yield return new PanelImportVerificationError(@"PL0024", PreloadingVerificationMessages.PL0024_DataWasNotFound);
            else
                this.Save(file.FileInfo.FileName, questionnaireIdentity, defaultResponsibleId, assignmentToImports);
        }

        public IEnumerable<PanelImportVerificationError> VerifyPanelAndSaveIfNoErrors(string originalFileName,
            PreloadedFile[] allImportedFiles, Guid defaultResponsibleId, PreloadedFile protectedVariablesFile, IQuestionnaire questionnaire)
        {
            bool hasErrors = false;

            var assignmentRows = new List<PreloadingAssignmentRow>();

            foreach (var importedFile in allImportedFiles)
            {
                foreach (var assignmentRow in this.assignmentsImportFileConverter.GetAssignmentRows(importedFile, questionnaire))
                {
                    foreach (var answerError in this.verifier.VerifyRowValues(assignmentRow, questionnaire))
                    {
                        hasErrors = true;
                        yield return answerError;
                    }

                    assignmentRows.Add(assignmentRow);
                }
            }

            foreach (var passwordError in this.verifier.VerifyWebPasswords(assignmentRows, questionnaire))
            {
                hasErrors = true;
                yield return passwordError;
            }

            if (hasErrors) yield break;

            foreach (var rosterError in this.verifier.VerifyRosters(assignmentRows, questionnaire))
            {
                hasErrors = true;
                yield return rosterError;
            }

            if (hasErrors) yield break;

            var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version);

            var protectedVariables = protectedVariablesFile?
                .Rows?
                .Where(x => x.Cells.Length > 0)
                .Select(x => ((PreloadingValue) x.Cells[0]).Value)?
                .ToList();

            var answersByAssignments = this.ConcatRosters(assignmentRows, questionnaire, protectedVariables);

            var assignmentsToImport = FixRosterSizeAnswers(answersByAssignments, questionnaire).ToList();

            if (assignmentsToImport.Count == 0)
                yield return new PanelImportVerificationError(@"PL0024", PreloadingVerificationMessages.PL0024_DataWasNotFound);
            else
                this.Save(originalFileName, questionnaireIdentity, defaultResponsibleId, assignmentsToImport);
        }

        public AssignmentToImport GetAssignmentById(int assignmentId)
            => this.importAssignmentsRepository.Query(x => x.FirstOrDefault(_ => _.Id == assignmentId));

        public int[] GetAllAssignmentIdsToVerify()
            => this.importAssignmentsRepository.Query(x => x.Where(_ => !_.Verified).Select(_ => _.Id).ToArray());

        public int[] GetAllAssignmentIdsToImport()
            => this.importAssignmentsRepository.Query(x => x.Where(_ => _.Verified && _.Error == null).Select(_ => _.Id).ToArray());

        public AssignmentsImportStatus GetImportStatus()
        {
            var process = this.importAssignmentsProcessRepository.Query(x => x.FirstOrDefault());
            if (process == null) return null;

            var status = new AssignmentsImportStatus
            {
                IsOwnerOfRunningProcess = process.Responsible == this.authorizedUser.UserName,
                TotalCount = process.TotalCount,
                FileName = process.FileName,
                StartedDate = process.StartedDate,
                ResponsibleName = process.Responsible,
                QuestionnaireIdentity = QuestionnaireIdentity.Parse(process.QuestionnaireId),
                ProcessStatus = process.Status,
                AssignedTo = process.AssignedTo
            };

            var statistics = this.importAssignmentsRepository.Query(x =>
                x.Select(_ =>
                        new
                        {
                            Total = 1,
                            Verified = _.Verified ? 1 : 0,
                            VerifiedWithoutError = _.Verified && _.Error == null ? 1 : 0,
                            HasError = _.Error != null ? 1 : 0,
                            AssignedToInterviewer = _.Interviewer != null ? 1 : 0,
                            AssignedToSupervisor = _.Interviewer == null && _.Supervisor != null ? 1 : 0
                        })
                    .GroupBy(_ => _.Total)
                    .Where(_=>_.Count() > 0)
                    .Select(_ => new
                    {
                        Total = _.Sum(y => y.Total),
                        WithErrors = _.Sum(y => y.HasError),
                        Verified = _.Sum(y => y.Verified),
                        AssignedToInterviewers = _.Sum(y => y.AssignedToInterviewer),
                        AssignedToSupervisors = _.Sum(y => y.AssignedToSupervisor),
                    })
                    .FirstOrDefault());

            if (statistics != null)
            {
                status.WithErrorsCount = statistics.WithErrors;
                status.VerifiedCount = statistics.Verified;
                status.AssignedToInterviewersCount = statistics.AssignedToInterviewers;
                status.AssignedToSupervisorsCount = statistics.AssignedToSupervisors;
                status.InQueueCount = statistics.Total;
                status.ProcessedCount = process.TotalCount - statistics.Total;
            }

            return status;

        }

        public void RemoveAllAssignmentsToImport()
        {
            this.sessionProvider.Session.Query<AssignmentToImport>().Delete();
            this.sessionProvider.Session.Query<AssignmentsImportProcess>().Delete();
        }

        public IEnumerable<string> GetImportAssignmentsErrors()
            => this.importAssignmentsRepository.Query(x => x.Where(_ => _.Error != null).Select(_ => _.Error));

        public int ImportAssignment(int assignmentId, Guid defaultAssignedTo, IQuestionnaire questionnaire, Guid responsibleId)
        {
            var assignmentToImport = this.GetAssignmentById(assignmentId);
            return this.ImportAssignment(assignmentToImport, questionnaire, responsibleId, defaultAssignedTo);
        }
        
        public int ImportAssignment(AssignmentToImport assignment, IQuestionnaire questionnaire, Guid responsibleId)
        {
            assignment = this.GetAssignmentWithoutEmptyAnswersAndFillPasswords(assignment);
            
            return this.ImportAssignment(assignment, questionnaire, responsibleId, responsibleId);
        }
        
        private int ImportAssignment(AssignmentToImport assignmentToImport, IQuestionnaire questionnaire, Guid responsibleId, Guid defaultAssignedTo)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version);

            var assignedTo = assignmentToImport.Interviewer ?? assignmentToImport.Supervisor ??
                assignmentToImport.Headquarters ?? defaultAssignedTo;
            
            var assignment = this.assignmentFactory.CreateAssignment(
                responsibleId,
                questionnaireIdentity,
                assignedTo,
                assignmentToImport.Quantity,
                assignmentToImport.Email,
                assignmentToImport.Password,
                assignmentToImport.WebMode,
                assignmentToImport.IsAudioRecordingEnabled,
                assignmentToImport.Answers,
                assignmentToImport.ProtectedVariables,
                assignmentToImport.Comments);

            this.invitationService.CreateInvitationForWebInterview(assignment);

            return assignment.Id;
        }

        public void SetVerifiedToAssignment(int assignmentId, string errorMessage = null)
            => this.sessionProvider.Session.Query<AssignmentToImport>()
                .Where(c => c.Id == assignmentId)
                .UpdateBuilder()
                .Set(c => c.Verified, c => true)
                .Set(c => c.Error, c => errorMessage)
                .Update();

        public void RemoveAssignmentToImport(int assignmentId)
            => this.importAssignmentsRepository.Remove(assignmentId);

        public void SetImportProcessStatus(AssignmentsImportProcessStatus status)
            => this.sessionProvider.Session.Query<AssignmentsImportProcess>()
                .UpdateBuilder()
                .Set(c => c.Status, c => status)
                .Update();

        private void Save(string fileName, QuestionnaireIdentity questionnaireIdentity, Guid defaultResponsible, IList<AssignmentToImport> assignments)
        {
            this.RemoveAllAssignmentsToImport();

            this.SaveProcess(fileName, questionnaireIdentity, defaultResponsible, assignments);
            this.SaveAssignments(assignments);
        }

        private void SaveProcess(string fileName, QuestionnaireIdentity questionnaireIdentity,
            Guid defaultResponsible, IList<AssignmentToImport> assignments)
        {
            var process = this.importAssignmentsProcessRepository.Query(x => x.FirstOrDefault()) ??
                          new AssignmentsImportProcess();

            process.FileName = fileName;
            process.TotalCount = assignments.Count;
            process.Responsible = this.authorizedUser.UserName;
            process.StartedDate = DateTime.UtcNow;
            process.QuestionnaireId = questionnaireIdentity.ToString();
            process.Status = AssignmentsImportProcessStatus.Verification;
            process.AssignedTo = defaultResponsible;

            this.importAssignmentsProcessRepository.Store(process, process.Id);
        }

        private void SaveAssignments(IList<AssignmentToImport> assignments)
        {
            Dictionary<string, bool> usedPasswords =
                assignments.Where(x => x.Password != AssignmentConstants.PasswordSpecialValue)
                    .ToDictionary(import => import.Password, y => false);
            
            List<string> currentBatch = new List<string>();

            var itemsToStore = assignments.Select(x =>
                new Tuple<AssignmentToImport, object>(GetAssignmentWithoutEmptyAnswersAndFillPasswords(x, GetPasswordOrGenerate(usedPasswords, currentBatch, x.Password)), x.Id));
            
            this.importAssignmentsRepository.Store(itemsToStore);
        }

        private string GetPasswordOrGenerate(Dictionary<string, bool> usedPasswords, List<string> currentBatch, string argPassword)
        {
            if (argPassword != AssignmentConstants.PasswordSpecialValue)
            {
                return argPassword;
            }
            
            while (currentBatch.Count == 0)
            {
                currentBatch.AddRange(passwordGenerator.GeneratePasswordBatch(usedPasswords));
            }
            
            var currentPass = currentBatch[0];
            
            usedPasswords.Add(currentPass, false);
            currentBatch.RemoveAt(0);

            return currentPass;
        }

        private AssignmentToImport GetAssignmentWithoutEmptyAnswersAndFillPasswords(
            AssignmentToImport assignmentToImport, string newPassword)
        {
            assignmentToImport.Answers = assignmentToImport.Answers.Where(x => x.Answer != null).ToList();
            assignmentToImport.Password = newPassword;
            return assignmentToImport;
        }

        private AssignmentToImport GetAssignmentWithoutEmptyAnswersAndFillPasswords(AssignmentToImport assignmentToImport)
        {
            return GetAssignmentWithoutEmptyAnswersAndFillPasswords(assignmentToImport, 
                passwordGenerator.GetPassword(assignmentToImport.Password));
        }

        private List<AssignmentToImport> ConcatRosters(List<PreloadingAssignmentRow> assignmentRows,
            IQuestionnaire questionnaire, List<string> protectedVariables = null)
            => assignmentRows
                .GroupBy(assignmentRow => assignmentRow.InterviewIdValue?.Value ??
                                          /*for single/anvanced preloading with main file only without interview ids*/
                                          Guid.NewGuid().ToString())
                .Select(x => ConvertToAssignmentToImport(x.ToList(), questionnaire, protectedVariables))
                .ToList();

        public AssignmentToImport ConvertToAssignmentToImport(List<PreloadingAssignmentRow> assignment,
            IQuestionnaire questionnaire, List<string> protectedQuestions)
        {
            var quantity = assignment.Select(_ => _.Quantity).FirstOrDefault(_ => _ != null)?.Quantity;
            var responsible = assignment.Select(_ => _.Responsible).FirstOrDefault(_ => _ != null)?.Responsible;
            var answers = assignment.SelectMany(_ => _.Answers.OfType<IAssignmentAnswer>().Select(y =>
                ToInterviewAnswer(y, ToRosterVector(_.RosterInstanceCodes), questionnaire)));

            var email = assignment.Select(_ => _.Email).FirstOrDefault(_ => _ != null)?.Value;
            var password = assignment.Select(_ => _.Password).FirstOrDefault(_ => _ != null)?.Value;
            var webMode = assignment.Select(_ => _.WebMode).FirstOrDefault(_ => _ != null)?.WebMode;
            var isAudioRecordingEnabled = assignment.Select(_ => _.RecordAudio).FirstOrDefault(_ => _ != null)?.DoesNeedRecord;
            var comments = assignment.Select(_ => _.Comments).FirstOrDefault(_ => _ != null)?.Value;

            return new AssignmentToImport
            {
                Quantity = quantity.HasValue ? (quantity > -1 ? quantity : null) : 1,
                Answers = answers.ToList(),
                Interviewer = responsible?.InterviewerId,
                Supervisor = responsible?.SupervisorId,
                Headquarters = responsible?.HeadquartersId,
                Verified = false,
                ProtectedVariables = protectedQuestions,
                Email = email,
                Password = password,
                WebMode = webMode ?? false,
                IsAudioRecordingEnabled = isAudioRecordingEnabled,
                Comments = comments
            };
        }

        private static RosterVector ToRosterVector(AssignmentRosterInstanceCode[] rosterInstanceCodes)
            => new RosterVector(rosterInstanceCodes.Select(x => x.Code.Value).ToArray());

        private static IEnumerable<AssignmentToImport> FixRosterSizeAnswers(IEnumerable<AssignmentToImport> assignments,
            IQuestionnaire questionnaire)
        {
            var allRosterSizeQuestions = questionnaire.GetAllRosterSizeQuestions();

            var questionsInsideRosters = allRosterSizeQuestions
                .Select(x => (rosterSize: x, rosters: questionnaire.GetRosterGroupsByRosterSizeQuestion(x)))
                .Select(x => (rosterSize: x.rosterSize,
                    rosterQuestions: x.rosters.SelectMany(questionnaire.GetAllUnderlyingQuestions).ToArray()))
                .ToArray();

            foreach (var assignment in assignments)
            {
                if (allRosterSizeQuestions.Count > 0 && assignment.Answers.Any(x => x.Identity.RosterVector.Length > 0))
                {
                    BuildRosterSizeAnswersByRosterQuestionAnswers(assignment, allRosterSizeQuestions,
                        questionsInsideRosters, questionnaire);
                }

                yield return assignment;
            }
        }

        private static void BuildRosterSizeAnswersByRosterQuestionAnswers(AssignmentToImport assignment, IReadOnlyCollection<Guid> rosterSizeQuestions,
            (Guid rosterSize, Guid[] rosterQuestions)[] questionsInsideRosters, IQuestionnaire questionnaire)
        {
            // answers by text list roster size question from roster file
            var listRosterTitles = assignment.Answers
                .Where(x => rosterSizeQuestions.Contains(x.Identity.Id) && x.Answer is TextAnswer)
                .ToArray();

            // roster size answers from parent files
            var sourceRosterSizeAnswers = assignment.Answers
                .Where(x => rosterSizeQuestions.Contains(x.Identity.Id) && !(x.Answer is TextAnswer))
                .ToArray();

            // answers in rosters triggered by concrete rosters size question
            var rosterAnswersByRosterSizeQuestion = questionsInsideRosters.ToDictionary(x => x.rosterSize,
                x => new
                {
                    answers = assignment.Answers.Where(y => x.rosterQuestions.Contains(y.Identity.Id)).ToArray(),
                    rosterSizeType = questionnaire.GetQuestionType(x.rosterSize),
                    rosterSizeLevel = questionnaire.GetRosterLevelForQuestion(x.rosterSize),
                    levelsOfRostersByRosterSize = questionnaire.GetRosterGroupsByRosterSizeQuestion(x.rosterSize).Select(questionnaire.GetRosterLevelForGroup).ToArray()
                }).Where(x => x.Value.answers.Length > 0);

            var calculatedRosterSizeAnswers = new List<InterviewAnswer>();

            foreach (var rosterAnswers in rosterAnswersByRosterSizeQuestion.OrderBy(x => x.Value.rosterSizeLevel))
            {
                var answersByRosterLevels = rosterAnswers.Value.rosterSizeLevel == 0
                    ? rosterAnswers.Value.answers.GroupBy(x => RosterVector.Empty)
                    : rosterAnswers.Value.answers
                        .GroupBy(x => x.Identity.RosterVector.Take(rosterAnswers.Value.rosterSizeLevel)).ToArray();

                foreach (var answersByRosterLevel in answersByRosterLevels)
                {
                    var rosterSizeLevel = rosterAnswers.Value.rosterSizeLevel;
                    var rosterSizeType = rosterAnswers.Value.rosterSizeType;
                    var rosterSizeQuestionId = rosterAnswers.Key;
                    var rosterSizeQuestionRosterVector = answersByRosterLevel.Key;

                    var answersGroupedByRosterInstanceId = answersByRosterLevel
                        .GroupBy(x => x.Identity.RosterVector.ElementAt(rosterSizeLevel))
                        .OrderBy(x => x.Key);

                    // user defined roster instance ids
                    var rosterSizeAnsweredOptions = answersGroupedByRosterInstanceId.Select(x => x.Key).ToArray();

                    // difference between roster instance ids of user and what we can import to interview tree
                    var oldToNewRosterInstanceIds = rosterSizeAnsweredOptions
                        .Select((x, i) => (newId: i, oldId: x))
                        .ToDictionary(x => x.oldId, x => x.newId);

                    var rosterSizeAnswer = GetRosterSizeAnswerByRosterAnswers(questionnaire, rosterSizeType,
                        rosterSizeQuestionRosterVector, listRosterTitles, rosterSizeQuestionId,
                        rosterSizeAnsweredOptions);
                    
                    if (rosterSizeType == QuestionType.Numeric)
                        FixRosterVectors(answersGroupedByRosterInstanceId, oldToNewRosterInstanceIds, rosterAnswers.Value.levelsOfRostersByRosterSize);

                    calculatedRosterSizeAnswers.Add(rosterSizeAnswer);
                }
            }

            // remove roster size answer from parent file if we have calulated by roster files roster size answer
            foreach (var sourceRosterSizeAnswer in sourceRosterSizeAnswers.Where(x => calculatedRosterSizeAnswers.Any(y => y.Identity == x.Identity)))
                assignment.Answers.Remove(sourceRosterSizeAnswer);

            // remove text list roster size answers from roster files
            foreach (var listRosterTitle in listRosterTitles)
                assignment.Answers.Remove(listRosterTitle);

            assignment.Answers.AddRange(calculatedRosterSizeAnswers);
        }

        private static InterviewAnswer GetRosterSizeAnswerByRosterAnswers(IQuestionnaire questionnaire,
            QuestionType rosterSizeType, RosterVector rosterSizeRosterVector, InterviewAnswer[] listRosterTitles,
            Guid rosterSizeQuestionId, int[] rosterSizeAnsweredOptions)
        {
            var rosterSizeAnswer = new InterviewAnswer
            {
                Identity = Identity.Create(rosterSizeQuestionId, rosterSizeRosterVector)
            };

            switch (rosterSizeType)
            {
                case QuestionType.MultyOption:
                    rosterSizeAnswer.Answer = ToRosterSizeCategoricalAnswer(questionnaire, rosterSizeAnswer, rosterSizeAnsweredOptions);
                    break;
                case QuestionType.Numeric:
                    rosterSizeAnswer.Answer = NumericIntegerAnswer.FromInt(rosterSizeAnsweredOptions.Length);
                    break;
                case QuestionType.TextList:
                    rosterSizeAnswer.Answer = ToRosterSizeListAnswer(rosterSizeAnsweredOptions, listRosterTitles, rosterSizeAnswer);
                    break;
            }

            return rosterSizeAnswer;
        }

        private static void FixRosterVectors(IOrderedEnumerable<IGrouping<int, InterviewAnswer>> answersGroupedByRosterInstanceId,
            Dictionary<int, int> oldToNewRosterInstanceIds, int[] levelsOfRostersByRosterSize)
        {
            foreach (var answersByRosterInstanceId in answersGroupedByRosterInstanceId)
                foreach (var interviewAnswer in answersByRosterInstanceId)
                    foreach (var rosterLevel in levelsOfRostersByRosterSize.Distinct())
                    {
                        if (rosterLevel > interviewAnswer.Identity.RosterVector.Length) continue;

                        var rosterVectorIndex = rosterLevel - 1;
                        var numericRosterInstanceCode = interviewAnswer.Identity.RosterVector[rosterVectorIndex];

                        // this means that roster instance id in roster file as we expected in our system
                        if (oldToNewRosterInstanceIds[numericRosterInstanceCode] == numericRosterInstanceCode) continue;

                        var newRosterVector = interviewAnswer.Identity.RosterVector.Replace(
                            rosterVectorIndex, oldToNewRosterInstanceIds[numericRosterInstanceCode]);

                        interviewAnswer.Identity = Identity.Create(interviewAnswer.Identity.Id, newRosterVector);
                    }
        }

        private static TextListAnswer ToRosterSizeListAnswer(int[] rosterInstanceCodes,
            InterviewAnswer[] listRosterTitles, InterviewAnswer rosterSizeAnswer)
        {
            var rosterSizeQuestionId = rosterSizeAnswer.Identity.Id;
            var rosterVector = rosterSizeAnswer.Identity.RosterVector;

            var rosterSizeListItems = rosterInstanceCodes
                .Select(rosterInstanceCode => ToRosterSizeListItem(rosterSizeQuestionId, rosterVector,
                    rosterInstanceCode, listRosterTitles))
                .ToArray();

            return TextListAnswer.FromTupleArray(rosterSizeListItems);
        }

        private static Tuple<int, string> ToRosterSizeListItem(Guid rosterSizeQuestionId, RosterVector rosterVertor,
            int rosterInstanceCode, InterviewAnswer[] listRosterTitles)
        {
            var rosterInstanceId = Identity.Create(rosterSizeQuestionId,
                rosterVertor.ExtendWithOneCoordinate(rosterInstanceCode));

            var rosterInstanceTitle = (TextAnswer) listRosterTitles.FirstOrDefault(x => x.Identity == rosterInstanceId)?.Answer;

            return new Tuple<int, string>(rosterInstanceCode, rosterInstanceTitle?.Value ?? "");
        }

        private static AbstractAnswer ToRosterSizeCategoricalAnswer(IQuestionnaire questionnaire, InterviewAnswer rosterSizeAnswer, int[] rosterSizeAnsweredOptions) 
            => questionnaire.IsQuestionYesNo(rosterSizeAnswer.Identity.Id)
            ? YesNoAnswer.FromAnsweredYesNoOptions(rosterSizeAnsweredOptions.Select(x => new AnsweredYesNoOption(x, true)).ToArray())
            : (AbstractAnswer) CategoricalFixedMultiOptionAnswer.FromIntArray(rosterSizeAnsweredOptions);

        private InterviewAnswer ToInterviewAnswer(IAssignmentAnswer value, RosterVector rosterVector, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(value.VariableName);
            if (!questionId.HasValue) return null;

            var questionType = questionnaire.GetQuestionType(questionId.Value);
            var isRosterSizeQuestion = questionnaire.IsRosterSizeQuestion(questionId.Value);
            var isLinkedToQuestion = questionnaire.IsQuestionLinked(questionId.Value);
            var isLinkedToRoster = questionnaire.IsQuestionLinkedToRoster(questionId.Value);

            var interviewAnswer = new InterviewAnswer { Identity = Identity.Create(questionId.Value, rosterVector) };

            switch (value)
            {
                case AssignmentAnswer answer when string.IsNullOrWhiteSpace(answer.Value):
                case AssignmentAnswers answers when answers.Values?.All(x => string.IsNullOrWhiteSpace(x.Value)) ?? true:
                    break;
                // magic for text list question only
                case AssignmentTextAnswer textListRosterTitle when isRosterSizeQuestion:
                    interviewAnswer.Answer = TextAnswer.FromString(textListRosterTitle.Value);
                    break;
                case AssignmentTextAnswer textAnswer when questionType == QuestionType.Text:
                    interviewAnswer.Answer = TextAnswer.FromString(textAnswer.Value);
                    break;
                case AssignmentTextAnswer qrBarcodeAnswer when questionType == QuestionType.QRBarcode:
                    interviewAnswer.Answer = QRBarcodeAnswer.FromString(qrBarcodeAnswer.Value);
                    break;
                case AssignmentCategoricalSingleAnswer categoricalSingleAnswer when !isLinkedToQuestion && !isLinkedToRoster && categoricalSingleAnswer.OptionCode.HasValue:
                    interviewAnswer.Answer = CategoricalFixedSingleOptionAnswer.FromInt(categoricalSingleAnswer.OptionCode.Value);
                    break;
                case AssignmentDateTimeAnswer dateTimeAnswer when dateTimeAnswer.Answer.HasValue:
                    interviewAnswer.Answer = DateTimeAnswer.FromDateTime(dateTimeAnswer.Answer.Value);
                    break;
                case AssignmentIntegerAnswer integerAnswer when integerAnswer.Answer.HasValue:
                    interviewAnswer.Answer = NumericIntegerAnswer.FromInt(integerAnswer.Answer.Value);
                    break;
                case AssignmentDoubleAnswer doubleAnswer when doubleAnswer.Answer.HasValue:
                    interviewAnswer.Answer = NumericRealAnswer.FromDouble(doubleAnswer.Answer.Value);
                    break;
                case AssignmentGpsAnswer gpsAnswer:
                    interviewAnswer.Answer = gpsAnswer.ToInterviewAnswer();
                    break;
                case AssignmentMultiAnswer textListAnswer when questionType == QuestionType.TextList:
                    interviewAnswer.Answer = textListAnswer.ToInterviewTextListAnswer();
                    break;
                case AssignmentMultiAnswer yesNoAnswer when questionnaire.IsQuestionYesNo(questionId.Value):
                    interviewAnswer.Answer = yesNoAnswer.ToInterviewYesNoAnswer();
                    break;
                case AssignmentMultiAnswer categoricalMultiAnswer when questionType == QuestionType.MultyOption && !isLinkedToQuestion && !isLinkedToRoster:
                    interviewAnswer.Answer = categoricalMultiAnswer.ToInterviewCategoricalMultiAnswer();
                    break;
            }

            return interviewAnswer;
        }
    }
}
