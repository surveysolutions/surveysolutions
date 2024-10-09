using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class AssignmentsImportFileConverter : IAssignmentsImportFileConverter
    {
        private readonly IFileSystemAccessor fileSystem;
        private readonly IUserViewFactory userViewFactory;

        public AssignmentsImportFileConverter(IFileSystemAccessor fileSystem,
            IUserViewFactory userViewFactory)
        {
            this.fileSystem = fileSystem;
            this.userViewFactory = userViewFactory;
        }

        public IEnumerable<PreloadingAssignmentRow> GetAssignmentRows(PreloadedFile file, IQuestionnaire questionnaire)
        {
            for (int i = 0; i < file.Rows.Length; i++)
            {
                var preloadingRow = file.Rows[i];
                
                var preloadingValues = preloadingRow.Cells.OfType<PreloadingValue>().ToArray();

                var preloadingInterviewId = preloadingValues.FirstOrDefault(x => x.VariableOrCodeOrPropertyName == ServiceColumns.InterviewId);
                var preloadingResponsible = preloadingValues.FirstOrDefault(x => x.VariableOrCodeOrPropertyName == ServiceColumns.ResponsibleColumnName);
                var preloadingQuantity = preloadingValues.FirstOrDefault(x => x.VariableOrCodeOrPropertyName == ServiceColumns.AssignmentsCountColumnName);
                var preloadingEmail = preloadingValues.FirstOrDefault(x => x.VariableOrCodeOrPropertyName == ServiceColumns.EmailColumnName);
                var preloadingPassword = preloadingValues.FirstOrDefault(x => x.VariableOrCodeOrPropertyName == ServiceColumns.PasswordColumnName);
                var preloadingWebMode = preloadingValues.FirstOrDefault(x => x.VariableOrCodeOrPropertyName == ServiceColumns.WebModeColumnName);
                var preloadingRecordAudio = preloadingValues.FirstOrDefault(x => x.VariableOrCodeOrPropertyName == ServiceColumns.RecordAudioColumnName);
                var preloadingComments = preloadingValues.FirstOrDefault(x => x.VariableOrCodeOrPropertyName == ServiceColumns.CommentsColumnName);
                var preloadingTargetArea = preloadingValues.FirstOrDefault(x => x.VariableOrCodeOrPropertyName == ServiceColumns.TargetAreaColumnName);

                yield return new PreloadingAssignmentRow
                {
                    Row = preloadingRow.RowIndex,
                    FileName = file.FileInfo.FileName,
                    QuestionnaireOrRosterName = file.FileInfo.QuestionnaireOrRosterName,
                    Answers = this.ToAssignmentAnswers(preloadingRow.Cells, questionnaire).Where(x=>/*not supported question types*/ x != null).ToArray(),
                    RosterInstanceCodes = this.ToAssignmentRosterInstanceCodes(preloadingValues, questionnaire, file.FileInfo.QuestionnaireOrRosterName).ToArray(),
                    Responsible = preloadingResponsible?.ToAssignmentResponsible(this.userViewFactory, this.users),
                    Quantity = preloadingQuantity?.ToAssignmentQuantity(),
                    InterviewIdValue = preloadingInterviewId?.ToAssignmentInterviewId(),
                    Email = preloadingEmail?.ToAssignmentEmail(),
                    Password = preloadingPassword?.ToAssignmentPassword(),
                    WebMode = preloadingWebMode?.ToAssignmentWebMode(),
                    RecordAudio = preloadingRecordAudio?.ToAssignmentRecordAudio(),
                    Comments = preloadingComments?.ToAssignmentComments(),
                    TargetArea = preloadingTargetArea?.ToAssignmentTargetArea(),
                };
            }
        }

        
        private IEnumerable<AssignmentRosterInstanceCode> ToAssignmentRosterInstanceCodes(
            PreloadingValue[] preloadingValues, IQuestionnaire questionnaire, string questionnaireOrRosterName)
        {
            if (IsQuestionnaireFile(questionnaireOrRosterName, questionnaire)) yield break;

            var rosterId = questionnaire.GetRosterIdByVariableName(questionnaireOrRosterName, true);
            if (!rosterId.HasValue) yield break;


            var parentRosterIds = questionnaire.GetRostersFromTopToSpecifiedGroup(rosterId.Value).ToArray();

            for (int i = 0; i < parentRosterIds.Length; i++)
            {
                var newName = string.Format(ServiceColumns.IdSuffixFormat, questionnaire.GetRosterVariableName(parentRosterIds[i]).ToLower());
                var oldName = $"{ServiceColumns.ParentId}{i + 1}".ToLower();

                var code = preloadingValues.FirstOrDefault(x =>
                    new[] {newName, oldName}.Contains(x.Column.ToLower()));

                if (code != null)
                    yield return code.ToAssignmentRosterInstanceCode();
            }
        }
        private bool IsQuestionnaireFile(string questionnaireOrRosterName, IQuestionnaire questionnaire)
            => string.Equals(this.fileSystem.MakeStataCompatibleFileName(questionnaireOrRosterName),
                this.fileSystem.MakeStataCompatibleFileName(questionnaire.Title), StringComparison.InvariantCultureIgnoreCase);

        private IEnumerable<BaseAssignmentValue> ToAssignmentAnswers(PreloadingCell[] cells, IQuestionnaire questionnaire)
        {
            foreach (var cell in cells)
            {
                switch (cell)
                {
                    case PreloadingCompositeValue compositeCell:
                        yield return compositeCell.ToAssignmentAnswers(questionnaire);
                        break;
                    case PreloadingValue regularCell:
                        yield return regularCell.ToAssignmentAnswer(questionnaire);
                        break;
                }
            }
        }
        
        private readonly Dictionary<string, UserToVerify> users = new Dictionary<string, UserToVerify>();
    }
}
