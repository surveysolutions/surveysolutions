using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.Latest
{

    public interface IExpressionBuilder
    {
        IEnumerable<string> BuildScript(QuestionnaireDocument questionnaire);
    }

    public class CsharpExpressionBuilder : IExpressionBuilder
    {
        public IEnumerable<string> BuildScript(QuestionnaireDocument questionnaire)
        {
            yield return $"public {nameof(Identity)} parentId = null;";
            yield return "public class RosterInstance";
            yield return "{";
            yield return $"    private readonly {nameof(InterviewTree)} tree;";
            yield return $"    private readonly {nameof(Identity)} parentId;";
            yield return $"    public RosterInstance(InterviewTree tree, Identity parentId) {{ this.tree = tree; this.parentId = parentId; }}";
            yield return "}";

            foreach (var lookupTable in questionnaire.Lookups)
                yield return this.ToLookup(lookupTable.Name, lookupTable.Columns, lookupTable.Values);

            foreach (var entity in questionnaire.Children.OfType<IGroup>())
                foreach (var script in this.ToGroup(entity, questionnaire))
                    yield return script;
        }

        private IEnumerable<string> ToGroup(IGroup parentGroup, QuestionnaireDocument questionnaire)
        {
            foreach (var entity in parentGroup.Children)
            {
                var variable = entity as IVariable;
                if (variable != null)
                    yield return ToVariable(variable);

                var question = entity as IQuestion;
                if (question != null)
                    yield return ToQuestion(question, questionnaire);

                var group = entity as IGroup;
                if (group == null) yield break;

                foreach (var script in !@group.IsRoster ? this.ToGroup(@group, questionnaire) : this.ToRoster(@group, questionnaire))
                    yield return script;
            }
        }

        private IEnumerable<string> ToRoster(IGroup roster, QuestionnaireDocument questionnaire)
        {
            int numberOfParentRosters = this.GetParentRostersCount(roster);

            var rosterClassName = roster.VariableName.ToTitleCase();
            var rosterClassIntent = CharExtensions.Tabs(numberOfParentRosters);
            var rosterPropertiesIndent = CharExtensions.Tabs(numberOfParentRosters + 1);

            yield return $"{rosterClassIntent}public {rosterClassName}[] {roster.VariableName} => " +
                $"this.tree.GetRosterInstances(\"{roster.VariableName}\").Select(rosterId => new {rosterClassName}(this.tree, rosterId)).ToArray();";

            yield return $"{rosterClassIntent}public class {rosterClassName}: RosterInstance";
            yield return $"{rosterClassIntent}{{";
            foreach (var script in this.ToGroup(roster, questionnaire))
                yield return $"{rosterPropertiesIndent}{script}";
            yield return $"{rosterClassIntent}}}";
        }

        private int GetParentRostersCount(IGroup roster)
        {
            var parent = roster;
            int numberOfParentRosters = 0;
            do
            {
                parent = (IGroup)parent?.GetParent();

                if (parent?.IsRoster ?? false)
                    numberOfParentRosters++;
            } while (parent != null);

            return numberOfParentRosters;
        }

        private string ToVariable(IVariable variable)
            => $"public {ToVariableType(variable.Type)} {variable.Name} = {variable.Expression};";

        private string ToVariableType(VariableType variableType)
        {
            switch (variableType)
            {
                case VariableType.Bool:
                    return "bool?";
                case VariableType.DateTime:
                    return "DateTime?";
                case VariableType.Double:
                    return "double?";
                case VariableType.Long:
                    return "long?";
                default:
                    return "string";
            }
        }

        public string ToLookup(string variableName, string[] columns, double?[][] rows)
        {
            var lookupTableClassName = variableName.ToTitleCase();

            var script = new StringBuilder();
            script.AppendLine($"public class {lookupTableClassName}");
            script.AppendLine("{");
            script.AppendLine("    public long rowcode;");
            foreach (var columnName in columns)
                script.AppendLine($"    public double? {columnName};");
            script.AppendLine("}");

            script.AppendLine($"public {lookupTableClassName}[] {variableName} = new {lookupTableClassName}[]");
            script.AppendLine("{");
            for (int rowCode = 0; rowCode < rows.Length; rowCode++)
            {
                var row = rows[rowCode];

                script.Append($"    new {lookupTableClassName}");
                script.Append(" {");
                script.Append($" rowcode = {rowCode}, ");

                for (int columnIndex = 0; columnIndex < row.Length; columnIndex++)
                    script.Append($"{columns[columnIndex]} = {row[columnIndex]?.ToString() ?? "null"}, ");

                script.Append("},");
                script.AppendLine();
            }
            script.AppendLine("};");

            return script.ToString();
        }

        private string ToQuestion(IQuestion question, QuestionnaireDocument questionnaire)
            => $"public {this.ToQuestionType(question)} {question.StataExportCaption} => this.tree.GetQuestion(\"{question.StataExportCaption}\", this.parentId)?.{this.ToQuestionAnswerType(question, questionnaire)};";

        private string ToQuestionType(IQuestion question)
        {
            switch (question.QuestionType)
            {
                case QuestionType.SingleOption:
                    return question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue
                        ? "decimal[]"
                        : "decimal?";
                case QuestionType.MultyOption:
                    return ((bool) ((IMultyOptionsQuestion) question)?.YesNoView)
                        ? typeof (YesNoAnswers).Name
                        : (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue
                            ? "decimal[][]"
                            : "decimal[]");
                case QuestionType.Numeric:
                    return ((bool) ((INumericQuestion) question)?.IsInteger) ? "long?" : "double?";
                case QuestionType.DateTime:
                    return "DateTime?";
                case QuestionType.GpsCoordinates:
                    return typeof (GeoPosition).Name;
                case QuestionType.TextList:
                    return "Tuple<decimal, string>[]";
                default:
                    return "string";
            }
        }

        private string ToQuestionAnswerType(IQuestion question, QuestionnaireDocument questionnaire)
        {
            var isLinkedToList = question.LinkedToQuestionId.HasValue &&
                questionnaire.Find<IQuestion>(question.LinkedToQuestionId.Value).QuestionType == QuestionType.TextList;

            switch (question.QuestionType)
            {
                case QuestionType.SingleOption:
                    if (isLinkedToList)
                        return $"{nameof(InterviewTreeQuestion.AsSingleLinkedToList)}?.{nameof(InterviewTreeSingleOptionLinkedToListQuestion.GetAnswer)}().{nameof(CategoricalFixedSingleOptionAnswer.SelectedValue)}";
                    else if(question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue)
                        return $"{nameof(InterviewTreeQuestion.AsSingleLinkedOption)}?.{nameof(InterviewTreeSingleLinkedToRosterQuestion.GetAnswer)}().{nameof(CategoricalLinkedSingleOptionAnswer.SelectedValue)}";
                    else
                        return $"{nameof(InterviewTreeQuestion.AsSingleFixedOption)}?.{nameof(InterviewTreeSingleOptionQuestion.GetAnswer)}().{nameof(CategoricalFixedSingleOptionAnswer.SelectedValue)}";
                case QuestionType.MultyOption:
                    if ((bool) ((IMultyOptionsQuestion) question)?.YesNoView)
                        return $"{nameof(InterviewTreeQuestion.AsYesNo)}?.{nameof(InterviewTreeYesNoQuestion.GetAnswer)}().{nameof(YesNoAnswer.ToYesNoAnswersOnly)}()";
                    else if(isLinkedToList)
                        return $"{nameof(InterviewTreeQuestion.AsMultiLinkedToList)}?.{nameof(InterviewTreeMultiOptionLinkedToListQuestion.GetAnswer)}().{nameof(CategoricalFixedMultiOptionAnswer.ToDecimals)}()";
                    else if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue)
                        return $"{nameof(InterviewTreeQuestion.AsMultiLinkedOption)}?.{nameof(InterviewTreeMultiLinkedToRosterQuestion.GetAnswer)}().{nameof(CategoricalLinkedMultiOptionAnswer.ToDecimalArrayArray)}()";
                    else
                        return $"{nameof(InterviewTreeQuestion.AsMultiFixedOption)}?.{nameof(InterviewTreeMultiOptionQuestion.GetAnswer)}().{nameof(CategoricalFixedMultiOptionAnswer.ToDecimals)}()";
                case QuestionType.Numeric:
                    return ((bool) ((INumericQuestion) question)?.IsInteger)
                        ? $"{nameof(InterviewTreeQuestion.AsInteger)}?.{nameof(InterviewTreeIntegerQuestion.GetAnswer)}().{nameof(NumericIntegerAnswer.Value)}"
                        : $"{nameof(InterviewTreeQuestion.AsDouble)}?.{nameof(InterviewTreeDoubleQuestion.GetAnswer)}().{nameof(NumericRealAnswer.Value)}";
                case QuestionType.DateTime:
                    return $"{nameof(InterviewTreeQuestion.AsDateTime)}?.{nameof(InterviewTreeDateTimeQuestion.GetAnswer)}().{nameof(DateTimeAnswer.Value)}";
                case QuestionType.GpsCoordinates:
                    return $"{nameof(InterviewTreeQuestion.AsGps)}?.{nameof(InterviewTreeGpsQuestion.GetAnswer)}().{nameof(GpsAnswer.Value)}";
                case QuestionType.TextList:
                    return $"{nameof(InterviewTreeQuestion.AsTextList)}?.{nameof(InterviewTreeTextListQuestion.GetAnswer)}().{nameof(TextListAnswer.ToTupleArray)}()";
                case QuestionType.Text:
                    return $"{nameof(InterviewTreeQuestion.AsText)}?.{nameof(InterviewTreeTextQuestion.GetAnswer)}().{nameof(TextAnswer.Value)}";
                case QuestionType.QRBarcode:
                    return $"{nameof(InterviewTreeQuestion.AsQRBarcode)}?.{nameof(InterviewTreeQRBarcodeQuestion.GetAnswer)}().{nameof(QRBarcodeAnswer.DecodedText)}";
                case QuestionType.Multimedia:
                    return $"{nameof(InterviewTreeQuestion.AsMultimedia)}?.{nameof(InterviewTreeMultimediaQuestion.GetAnswer)}().{nameof(MultimediaAnswer.FileName)}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}