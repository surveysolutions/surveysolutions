using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier
{
    internal partial class ImportDataVerifier
    {
        private class AssignmentRow
        {
            public AssignmentValue[] Answers { get; set; }
        }

        private abstract class AssignmentValue
        {
            public string FileName { get; set; }
            public int Row { get; set; }
            public string Column { get; set; }
            public string Value { get; set; }
            public string InterviewId { get; set; }
        }

        private class AssignmentUnknownValue : AssignmentValue { }


        private class AssignmentAnswer : AssignmentValue
        {
            public string VariableName { get; set; }
            public AnswerValue Answer { get; set; }
        }
        private class AnswerValue
        {
            public string AsString { get; set; }
            public int? AsInt { get; set; }
            public double? AsDouble { get; set; }
            public DateTime? AsDateTime { get; set; }
        }

        private class AssignmentRosterInstanceCode : AssignmentAnswer
        {
            public int Code { get; set; }
        }

        private class AssignmentAnswers : AssignmentAnswer
        {
            public AssignmentAnswer[] Values { get; set; }
        }

        private class AssignmentResponsible : AssignmentValue
        {
            public UserToVerify Responsible { get; set; }
        }

        private class AssignmentQuantity : AssignmentValue
        {
            public int? Quantity { get; set; }
        }
        
        private IEnumerable<AssignmentRow> ToAssignmentRows(Questionnaire questionnaire, PreloadedFile file)
        {
            var rosterSizeQuestionColumns = questionnaire.Rosters.ContainsKey(file.QuestionnaireOrRosterName)
                ? questionnaire.Rosters[file.QuestionnaireOrRosterName].RosterSizeQuestions
                    .Select(x => string.Format(ServiceColumns.IdSuffixFormat, x)).ToArray()
                : new string[0];

            foreach (var preloadingRow in file.Rows)
            {
                var assignmentAnswers = this.ToAssignmentAnswers(file.FileName, questionnaire, rosterSizeQuestionColumns, preloadingRow);

                yield return new AssignmentRow
                {
                    Answers = assignmentAnswers.ToArray()
                };
            }
        }

        private class Questionnaire
        {
            public IDictionary<string, InterviewQuestion> Questions { get; set; }
            public IDictionary<string, InterviewRoster> Rosters { get; set; }
        }

        private class InterviewQuestion
        {
            public string Variable { get; set; }
            public Guid Id { get; set; }
            public InterviewQuestionType Type { get; set; }
            public bool IsRosterSize { get; set; }
            public bool IsRosterSizeForLongRoster { get; set; }
        }

        private class InterviewRoster
        {
            public string Variable { get; set; }
            public Guid Id { get; set; }
            public string[] RosterSizeQuestions { get; set; }
        }

        private IEnumerable<AssignmentValue> ToAssignmentAnswers(string fileName, Questionnaire questionnaire, string[] rosterInstanceColumns, PreloadingRow row)
        {
            foreach (var answer in row.Cells)
            {
                if (rosterInstanceColumns.Contains(answer.VariableOrCodeOrPropertyName)) continue;

                switch (answer)
                {
                    case PreloadingCompositeValue compositeCell:
                        yield return ToAssignmentAnswers(fileName, row, compositeCell, questionnaire);
                        break;
                    case PreloadingValue regularCell:
                        switch (answer.VariableOrCodeOrPropertyName)
                        {
                            case ServiceColumns.ResponsibleColumnName:
                                yield return this.ToAssignmentResponsible(fileName, row.InterviewId, regularCell);
                                break;
                            case ServiceColumns.AssignmentsCountColumnName:
                                yield return ToAssignmentQuantity(fileName, row.InterviewId, regularCell);
                                break;
                            default:
                                yield return ToAssignmentAnswer(fileName, row, regularCell, questionnaire);
                                break;
                        }

                        break;
                }
            }

        }

        private static AssignmentAnswers ToAssignmentAnswers(string fileName, PreloadingRow row,
            PreloadingCompositeValue preloadingCompositeValue, Questionnaire questionnaire) => new AssignmentAnswers
            {
                InterviewId = row.InterviewId,
                VariableName = preloadingCompositeValue.VariableOrCodeOrPropertyName,
                Values = preloadingCompositeValue.Values.Select(x => ToAssignmentAnswer(fileName, row, x, questionnaire)).ToArray(),
            };

        private static AssignmentAnswer ToAssignmentAnswer(string fileName, PreloadingRow row,
            PreloadingValue answer, Questionnaire questionnaire) => new AssignmentAnswer
            {
                InterviewId = row.InterviewId,
                VariableName = answer.VariableOrCodeOrPropertyName,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
                Value = answer.Value,
                Answer = ToAnswerValue(answer, questionnaire)
            };

        private static AnswerValue ToAnswerValue(PreloadingValue answer, Questionnaire questionnaire)
        {
            throw new NotImplementedException();
        }

        private static AssignmentQuantity ToAssignmentQuantity(string fileName, string interviewId, PreloadingValue answer)
        {
            int.TryParse(answer.Value, out var quantity);

            return new AssignmentQuantity
            {
                InterviewId = interviewId,
                Quantity = quantity,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
                Value = answer.Value
            };
        }

        private AssignmentResponsible ToAssignmentResponsible(string fileName, string interviewId, PreloadingValue answer)
        {
            var responsible = new AssignmentResponsible
            {
                InterviewId = interviewId,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
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

        private Questionnaire ToQuestionnaire(IQuestionnaire questionnaire) => new Questionnaire
        {
            Questions = questionnaire.GetAllQuestions().Select(x => this.ToQuestion(x, questionnaire))
                .ToDictionary(x => x.Variable),
            Rosters = questionnaire.GetAllGroups().Where(questionnaire.IsRosterGroup).Select(x => this.ToRoster(x, questionnaire))
                .ToDictionary(x => x.Variable)
        };

        private InterviewRoster ToRoster(Guid rosterId, IQuestionnaire questionnaire) => new InterviewRoster
        {
            Id = rosterId,
            Variable = questionnaire.GetRosterVariableName(rosterId),
            RosterSizeQuestions = questionnaire.GetRostersFromTopToSpecifiedEntity(rosterId)
                .Select(questionnaire.GetRosterVariableName)
                .Union(new[] { questionnaire.GetRosterVariableName(rosterId) })
                .ToArray()
        };

        private InterviewQuestion ToQuestion(Guid questionId, IQuestionnaire questionnaire)
        {
            var questionType = questionnaire.GetQuestionType(questionId);
            bool isYesNoQuestion = questionnaire.IsQuestionYesNo(questionId);
            bool isDecimalQuestion = !questionnaire.IsQuestionInteger(questionId);
            Guid? cascadingParentQuestionId = questionnaire.GetCascadingQuestionParentId(questionId);
            Guid? sourceForLinkedQuestion = null;

            var isLinkedToQuestion = questionnaire.IsQuestionLinked(questionId);
            var isLinkedToRoster = questionnaire.IsQuestionLinkedToRoster(questionId);
            var isLinkedToListQuestion = questionnaire.IsLinkedToListQuestion(questionId);

            if (isLinkedToQuestion)
                sourceForLinkedQuestion = questionnaire.GetQuestionReferencedByLinkedQuestion(questionId);

            if (isLinkedToRoster)
                sourceForLinkedQuestion = questionnaire.GetRosterReferencedByLinkedQuestion(questionId);

            return new InterviewQuestion
            {
                Id = questionId,
                IsRosterSize = questionnaire.IsRosterSizeQuestion(questionId),
                IsRosterSizeForLongRoster = questionnaire.IsQuestionIsRosterSizeForLongRoster(questionId),
                Variable = questionnaire.GetQuestionVariableName(questionId).ToLower(),
                Type = GetQuestionType(questionType, cascadingParentQuestionId, isYesNoQuestion, isDecimalQuestion,
                    isLinkedToListQuestion, sourceForLinkedQuestion)
            };
        }
        private static InterviewQuestionType GetQuestionType(
            QuestionType questionType,
            Guid? cascadingParentQuestionId,
            bool isYesNo,
            bool isDecimal,
            bool isLinkedToListQuestion,
            Guid? linkedSourceId = null)

        {
            switch (questionType)
            {
                case QuestionType.SingleOption:
                    {
                        return linkedSourceId.HasValue
                            ? (isLinkedToListQuestion
                                ? InterviewQuestionType.SingleLinkedToList
                                : InterviewQuestionType.SingleLinkedOption)
                            : (cascadingParentQuestionId.HasValue
                                ? InterviewQuestionType.Cascading
                                : InterviewQuestionType.SingleFixedOption);
                    }
                case QuestionType.MultyOption:
                    {
                        return isYesNo
                            ? InterviewQuestionType.YesNo
                            : (linkedSourceId.HasValue
                                ? (isLinkedToListQuestion
                                    ? InterviewQuestionType.MultiLinkedToList
                                    : InterviewQuestionType.MultiLinkedOption)
                                : InterviewQuestionType.MultiFixedOption);
                    }
                case QuestionType.DateTime:
                    return InterviewQuestionType.DateTime;
                case QuestionType.GpsCoordinates:
                    return InterviewQuestionType.Gps;
                case QuestionType.Multimedia:
                    return InterviewQuestionType.Multimedia;
                case QuestionType.Numeric:
                    return isDecimal ? InterviewQuestionType.Double : InterviewQuestionType.Integer;
                case QuestionType.QRBarcode:
                    return InterviewQuestionType.QRBarcode;
                case QuestionType.Area:
                    return InterviewQuestionType.Area;
                case QuestionType.Text:
                    return InterviewQuestionType.Text;
                case QuestionType.TextList:
                    return InterviewQuestionType.TextList;
                case QuestionType.Audio:
                    return InterviewQuestionType.Audio;
                default:
                    throw new NotSupportedException($"Not supported question type: {questionType}");
            }
        }

    }
}
