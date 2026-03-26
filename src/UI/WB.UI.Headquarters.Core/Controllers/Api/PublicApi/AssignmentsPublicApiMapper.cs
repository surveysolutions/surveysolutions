#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.UI.Headquarters.API.PublicApi.Models;
using AggregateInterviewAnswer = WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers.InterviewAnswer;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Localizable(false)]
    public static class AssignmentsPublicApiMapper
    {
        public static FullAssignmentDetails ToFullAssignmentDetails(Assignment assignment, IQuestionnaire? questionnaire)
        {
            var result = new FullAssignmentDetails();
            MapToAssignmentDetails(assignment, result, questionnaire);
            result.Answers = assignment.Answers
                .Select(a => ToInterviewAnswer(a, questionnaire))
                .ToList();
            return result;
        }

        public static AssignmentDetails ToAssignmentDetails(Assignment assignment)
        {
            var result = new AssignmentDetails();
            MapToAssignmentDetails(assignment, result, null);
            return result;
        }

        public static List<AssignmentViewItem> ToAssignmentViewItemList(IEnumerable<AssignmentRow> rows) =>
            rows.Select(ToAssignmentViewItem).ToList();

        private static void MapToAssignmentDetails(Assignment assignment, AssignmentDetails result, IQuestionnaire? questionnaire)
        {
            MapToAssignmentViewItem(assignment, result);
            result.IdentifyingData = assignment.IdentifyingData
                .Select(id => ToAssignmentIdentifyingDataItem(id, questionnaire))
                .ToList();
        }

        private static void MapToAssignmentViewItem(Assignment assignment, AssignmentViewItem result)
        {
            result.Id = assignment.Id;
            result.Quantity = assignment.Quantity;
            result.QuestionnaireId = assignment.QuestionnaireId.ToString();
            result.InterviewsCount = assignment.InterviewSummaries.Count;
            result.ResponsibleName = assignment.Responsible.Name;
            result.IsAudioRecordingEnabled = assignment.AudioRecording;
        }

        private static InterviewAnswer ToInterviewAnswer(AggregateInterviewAnswer source, IQuestionnaire? questionnaire)
        {
            var item = new InterviewAnswer
            {
                Answer = source.Answer,
                Identity = source.Identity
            };
            item.Variable = GetVariableName(item.Variable, source.Identity.Id.ToString(), questionnaire);
            return item;
        }

        private static AssignmentIdentifyingDataItem ToAssignmentIdentifyingDataItem(IdentifyingAnswer source, IQuestionnaire? questionnaire)
        {
            var item = new AssignmentIdentifyingDataItem
            {
                Answer = source.Answer,
                Variable = source.VariableName,
                Identity = source.Identity?.ToString()
            };
            item.Variable = GetVariableName(item.Variable, source.Identity?.Id.ToString(), questionnaire);
            return item;
        }

        public static AssignmentIdentifyingDataItem ToAssignmentIdentifyingDataItem(AssignmentIdentifyingQuestionRow source, IQuestionnaire? questionnaire)
        {
            var item = new AssignmentIdentifyingDataItem
            {
                Answer = source.Answer,
                Variable = source.Variable,
                Identity = source.Identity?.ToString()
            };
            item.Variable = GetVariableName(item.Variable, source.Identity?.ToString(), questionnaire);
            return item;
        }

        public static AssignmentViewItem ToAssignmentViewItem(AssignmentRow row) => new AssignmentViewItem
        {
            Id = row.Id,
            QuestionnaireId = row.QuestionnaireId?.ToString(),
            Quantity = row.Quantity,
            InterviewsCount = row.InterviewsCount,
            ResponsibleId = row.ResponsibleId,
            ResponsibleName = row.Responsible,
            CreatedAtUtc = row.CreatedAtUtc,
            UpdatedAtUtc = row.UpdatedAtUtc,
            ReceivedByTabletAtUtc = row.ReceivedByTabletAtUtc,
            Archived = row.Archived,
            IsAudioRecordingEnabled = row.IsAudioRecordingEnabled
        };

        private static string? GetVariableName(string? value, string? identity, IQuestionnaire? questionnaire)
        {
            if (string.IsNullOrWhiteSpace(value) && identity != null && questionnaire != null)
            {
                return questionnaire.GetEntityVariableOrThrow(Guid.Parse(identity));
            }
            return value;
        }
    }
}
