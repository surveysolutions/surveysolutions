using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public partial class AssignmentsImportService
    {
        private IEnumerable<AssignmentRow> GetAssignmentRows(PreloadedFile file, IQuestionnaire questionnaire)
        {
            for (int i = 0; i < file.Rows.Length; i++)
            {
                var preloadingRow = file.Rows[i];
                
                var preloadingValues = preloadingRow.Cells.OfType<PreloadingValue>().ToArray();

                var preloadingInterviewId = preloadingValues.FirstOrDefault(x => x.Column == ServiceColumns.InterviewId);
                var preloadingResponsible = preloadingValues.FirstOrDefault(x => x.Column == ServiceColumns.ResponsibleColumnName);
                var preloadingQuantity = preloadingValues.FirstOrDefault(x => x.Column == ServiceColumns.AssignmentsCountColumnName);

                yield return new AssignmentRow
                {
                    Row = i,
                    FileName = file.FileInfo.FileName,
                    QuestionnaireOrRosterName = file.FileInfo.QuestionnaireOrRosterName,
                    Answers = this.ToAssignmentAnswers(preloadingRow.Cells, questionnaire).Where(x=>/*not supported question types*/ x != null).ToArray(),
                    RosterInstanceCodes = this.ToAssignmentRosterInstanceCodes(preloadingValues, questionnaire, file.FileInfo.QuestionnaireOrRosterName).ToArray(),
                    Responsible = preloadingResponsible == null ? null : ToAssignmentResponsible(preloadingResponsible),
                    Quantity = preloadingQuantity == null ? null : ToAssignmentQuantity(preloadingQuantity),
                    InterviewIdValue = preloadingInterviewId == null ? null : this.ToAssignmentInterviewId(preloadingInterviewId),
                };
            }
        }

        private AssignmentInterviewId ToAssignmentInterviewId(PreloadingValue value)
            => new AssignmentInterviewId
            {
                Column = value.Column,
                Value = value.Value
            };

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
                    yield return ToAssignmentRosterInstanceCode(code);
            }
        }
        private bool IsQuestionnaireFile(string questionnaireOrRosterName, IQuestionnaire questionnaire)
            => string.Equals(this.fileSystem.MakeStataCompatibleFileName(questionnaireOrRosterName),
                this.fileSystem.MakeStataCompatibleFileName(questionnaire.Title), StringComparison.InvariantCultureIgnoreCase);

        private IEnumerable<AssignmentValue> ToAssignmentAnswers(PreloadingCell[] cells, IQuestionnaire questionnaire)
        {
            foreach (var cell in cells)
            {
                switch (cell)
                {
                    case PreloadingCompositeValue compositeCell:
                        yield return ToAssignmentAnswers(compositeCell, questionnaire);
                        break;
                    case PreloadingValue regularCell:
                        yield return ToAssignmentAnswer(regularCell, questionnaire);
                        break;
                }
            }

        }

        private static AssignmentAnswers ToAssignmentAnswers(PreloadingCompositeValue compositeValue, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(compositeValue.VariableOrCodeOrPropertyName);

            if (questionId.HasValue)
            {
                var answerType = questionnaire.GetAnswerType(questionId.Value);
                switch (answerType)
                {
                    case AnswerType.DecimalAndStringArray:
                        return ToAssignmentTextListAnswer(compositeValue);
                    case AnswerType.GpsData:
                        return ToAssignmentGpsAnswer(compositeValue);
                    case AnswerType.OptionCodeArray:
                    case AnswerType.YesNoArray:
                        {
                            compositeValue.Values.ForEach(x => x.VariableOrCodeOrPropertyName = x.VariableOrCodeOrPropertyName.Replace("n", "-"));
                            return ToAssignmentCategoricalMultiAnswer(compositeValue);
                        }
                }
            }

            return null;
        }

        private static AssignmentAnswer ToGpsPropertyAnswer(PreloadingValue answer)
        {
            var doublePropertyNames = new[]
            {
                nameof(GeoPosition.Longitude).ToLower(),
                nameof(GeoPosition.Latitude).ToLower(),
                nameof(GeoPosition.Altitude).ToLower(),
                nameof(GeoPosition.Accuracy).ToLower(),
            };

            if (doublePropertyNames.Contains(answer.VariableOrCodeOrPropertyName))
                return ToAssignmentDoubleAnswer(answer);

            if (answer.VariableOrCodeOrPropertyName == nameof(GeoPosition.Timestamp).ToLower())
                return ToAssignmentDateTimeAnswer(answer);

            throw new NotSupportedException(
                $"Gps property {answer.Value} not supported. " +
                $"Supported properties: {string.Join(", ", GeoPosition.PropertyNames)}");
        }

        private AssignmentRosterInstanceCode ToAssignmentRosterInstanceCode(PreloadingValue answer)
        {
            int? intValue = null;
            if (int.TryParse(answer.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat,
                out var intNumericValue))
                intValue = intNumericValue;

            return new AssignmentRosterInstanceCode
            {
                Code = intValue,
                VariableName = answer.VariableOrCodeOrPropertyName,
                Column = answer.Column,
                Value = answer.Value,
            };
        }

        private static AssignmentAnswer ToAssignmentAnswer(PreloadingValue answer, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableOrCodeOrPropertyName);

            if (questionId.HasValue)
            {
                var answerType = questionnaire.GetAnswerType(questionId.Value);

                switch (answerType)
                {
                    case AnswerType.Integer:
                        return ToAssignmentIntegerAnswer(answer);
                    case AnswerType.OptionCode:
                    case AnswerType.Decimal:
                        return ToAssignmentDoubleAnswer(answer);
                    case AnswerType.DateTime:
                        return ToAssignmentDateTimeAnswer(answer);
                    case AnswerType.String:
                    case AnswerType.DecimalAndStringArray:
                        return ToAssignmentTextAnswer(answer);
                }
            }

            return null;
        }

        private static AssignmentGpsAnswer ToAssignmentGpsAnswer(PreloadingCompositeValue compositeValue)
            => new AssignmentGpsAnswer
            {
                VariableName = compositeValue.VariableOrCodeOrPropertyName,
                Column = compositeValue.VariableOrCodeOrPropertyName,
                Values = compositeValue.Values.Select(ToGpsPropertyAnswer).ToArray()
            };

        private static AssignmentMultiAnswer ToAssignmentTextListAnswer(PreloadingCompositeValue compositeValue)
            => new AssignmentMultiAnswer
            {
                VariableName = compositeValue.VariableOrCodeOrPropertyName,
                Column = compositeValue.VariableOrCodeOrPropertyName,
                Values = compositeValue.Values.Select(ToAssignmentTextAnswer).ToArray()
            };

        private static AssignmentMultiAnswer ToAssignmentCategoricalMultiAnswer(PreloadingCompositeValue compositeValue)
            => new AssignmentMultiAnswer
            {
                VariableName = compositeValue.VariableOrCodeOrPropertyName,
                Column = compositeValue.VariableOrCodeOrPropertyName,
                Values = compositeValue.Values.Select(ToAssignmentDoubleAnswer).ToArray()
            };

        private static AssignmentAnswer ToAssignmentDoubleAnswer(PreloadingValue answer)
        {
            double? doubleValue = null;
            if (double.TryParse(answer.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat,
                out var doubleNumericValue))
                doubleValue = doubleNumericValue;

            return new AssignmentDoubleAnswer
            {
                Answer = doubleValue,
                VariableName = answer.VariableOrCodeOrPropertyName,
                Column = answer.Column,
                Value = answer.Value,
            };
        }

        private static AssignmentAnswer ToAssignmentDateTimeAnswer(PreloadingValue answer)
        {
            DateTime? dataTimeValue = null;
            if (DateTime.TryParse(answer.Value, out var date))
                dataTimeValue = date;

            return new AssignmentDateTimeAnswer
            {
                Answer = dataTimeValue,
                VariableName = answer.VariableOrCodeOrPropertyName,
                Column = answer.Column,
                Value = answer.Value,
            };
        }

        private static AssignmentAnswer ToAssignmentTextAnswer(PreloadingValue answer)
            => new AssignmentTextAnswer
            {
                VariableName = answer.VariableOrCodeOrPropertyName,
                Column = answer.Column,
                Value = answer.Value,
            };

        private static AssignmentAnswer ToAssignmentIntegerAnswer(PreloadingValue answer)
        {
            int? intValue = null;
            if (int.TryParse(answer.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat,
                out var intNumericValue))
                intValue = intNumericValue;

            return new AssignmentIntegerAnswer
            {
                Answer = intValue,
                VariableName = answer.VariableOrCodeOrPropertyName,
                Column = answer.Column,
                Value = answer.Value,
            };
        }

        private static AssignmentQuantity ToAssignmentQuantity(PreloadingValue answer)
        {
            int? quantityValue = null;

            if (int.TryParse(answer.Value, out var quantity))
                quantityValue = quantity;

            return new AssignmentQuantity
            {
                Quantity = quantityValue,
                Column = answer.Column,
                Value = answer.Value
            };
        }

        private readonly Dictionary<string, UserToVerify> users = new Dictionary<string, UserToVerify>();
        private AssignmentResponsible ToAssignmentResponsible(PreloadingValue answer)
        {
            var responsible = new AssignmentResponsible
            {
                Column = answer.Column,
                Value = answer.Value
            };

            var responsibleName = answer.Value;
            if (!string.IsNullOrWhiteSpace(responsibleName))
            {
                if (!users.ContainsKey(responsibleName))
                    users.Add(responsibleName, this.userViewFactory.GetUsersByUserNames(new[] { responsibleName }).FirstOrDefault());

                responsible.Responsible = users[responsibleName];
            }

            return responsible;
        }
    }
}
