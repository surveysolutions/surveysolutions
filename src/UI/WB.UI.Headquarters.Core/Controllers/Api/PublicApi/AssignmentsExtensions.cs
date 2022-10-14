#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi;

public static class AssignmentsExtensions
{
    public static BaseAssignmentValue ToPreloadAnswer(this AssignmentAnswer answer, IQuestionnaire questionnaire, 
        ISerializer serializer)
    {
        return questionnaire.GetAnswerType(answer.QuestionIdentity.Id) switch
        {
            AnswerType.GpsData => ToPreloadGpsAnswer(answer, questionnaire),
            AnswerType.DecimalAndStringArray => ToPreloadTextListAnswer(answer, questionnaire, serializer),
            AnswerType.OptionCodeArray => ToPreloadMultiAnswer(answer, questionnaire, serializer),
            AnswerType.YesNoArray => ToPreloadYesNoAnswer(answer, questionnaire, serializer),
            _ => new PreloadingValue
            {
                VariableOrCodeOrPropertyName = answer.Variable,
                Value = answer.Source.Answer,
                Column = answer.Variable
            }.ToAssignmentAnswer(questionnaire)
        };
    }
        
    private static BaseAssignmentValue ToPreloadGpsAnswer(AssignmentAnswer answer, IQuestionnaire questionnaire)
    {
        var gpsCoordinates = answer.Source.Answer?.Split('$') ?? Array.Empty<string>();

        return new PreloadingCompositeValue
        {
            VariableOrCodeOrPropertyName = answer.Variable,
            Values = new[]
            {
                new PreloadingValue
                {
                    Value = gpsCoordinates.ElementAtOrDefault(0),
                    VariableOrCodeOrPropertyName = nameof(GeoPosition.Latitude).ToLower(),
                    Column = $"{answer.Variable}{ServiceColumns.ColumnDelimiter}{nameof(GeoPosition.Latitude)}"
                },
                new PreloadingValue
                {
                    Value = gpsCoordinates.ElementAtOrDefault(1),
                    VariableOrCodeOrPropertyName = nameof(GeoPosition.Longitude).ToLower(),
                    Column = $"{answer.Variable}{ServiceColumns.ColumnDelimiter}{nameof(GeoPosition.Longitude)}"
                },
            }
        }.ToAssignmentAnswers(questionnaire);
    }
        
    private static BaseAssignmentValue ToPreloadTextListAnswer(AssignmentAnswer answer,
        IQuestionnaire questionnaire, 
        ISerializer serializer) => new PreloadingCompositeValue
    {
        VariableOrCodeOrPropertyName = answer.Variable,
        Values = (serializer.Deserialize<string[]>(answer.Source.Answer) ?? Array.Empty<string>())
            .Select((x, i) => new PreloadingValue
            {
                Value = x,
                VariableOrCodeOrPropertyName = i.ToString(),
                Column = $"{answer.Variable}{ServiceColumns.ColumnDelimiter}{i}"
            }).ToArray()
    }.ToAssignmentAnswers(questionnaire);
        
    private static BaseAssignmentValue ToPreloadMultiAnswer(AssignmentAnswer answer,
        IQuestionnaire questionnaire, 
        ISerializer serializer) => new PreloadingCompositeValue
    {
        VariableOrCodeOrPropertyName = answer.Variable,
        Values = (serializer.Deserialize<string[]>(answer.Source.Answer) ?? Array.Empty<string>())
            .Select((x, i) => new PreloadingValue
            {
                Value = (i + 1).ToString(),
                VariableOrCodeOrPropertyName = x,
                Column = $"{answer.Variable}{ServiceColumns.ColumnDelimiter}{x}"
            }).ToArray()
    }.ToAssignmentAnswers(questionnaire);
        
    private static BaseAssignmentValue ToPreloadYesNoAnswer(AssignmentAnswer answer, IQuestionnaire questionnaire, 
        ISerializer serializer)
    {
        return new PreloadingCompositeValue
        {
            VariableOrCodeOrPropertyName = answer.Variable,
            Values = (serializer.Deserialize<string[]>(answer.Source.Answer) ?? Array.Empty<string>())
                .Select(CheckedYesNoAnswerOption.Parse)
                .Select((x, i) => new PreloadingValue
                {
                    Value = x.Yes ? (i + 1).ToString() : "0",
                    VariableOrCodeOrPropertyName = x.Value.ToString(),
                    Column = $"{answer.Variable}{ServiceColumns.ColumnDelimiter}{x.Value}"
                }).ToArray()
        }.ToAssignmentAnswers(questionnaire);
    }
        
    public static AssignmentAnswer ToAssignmentAnswer(this AssignmentIdentifyingDataItem item, IQuestionnaire questionnaire)
    {
        if (!string.IsNullOrEmpty(item.Identity) && Identity.TryParse(item.Identity, out Identity identity))
        {
            if (questionnaire.HasQuestion(identity.Id))
            {
                var answer = new AssignmentAnswer(item, identity)
                {
                    Variable = questionnaire.GetQuestionVariableName(identity.Id),
                    QuestionType = questionnaire.GetQuestionType(identity.Id)
                };

                return answer;
            }
        }
        else if (!string.IsNullOrEmpty(item.Variable))
        {
            var questionId = questionnaire.GetQuestionIdByVariable(item.Variable);
            if (questionId.HasValue)
            {
                var answer = new AssignmentAnswer(item, Identity.Create(questionId.Value, RosterVector.Empty))
                {
                    Variable = item.Variable
                };
                answer.QuestionType = questionnaire.GetQuestionType(answer.QuestionIdentity.Id);
                    
                return answer;
            }
        }

        return new AssignmentAnswer(item, Identity.Create(Guid.Empty, RosterVector.Empty))
        {
            IsUnknownQuestion = true
        };
    }
        
    public static IEnumerable<PreloadingAssignmentRow> ToAssignmentRows(this CreateAssignmentApiRequest assignmentInfo,
        List<AssignmentAnswer> answers, IQuestionnaire questionnaire, IUserViewFactory userViewFactory, ISerializer serializer)
    {
        var tempAssignmentId = Guid.NewGuid().ToString("N");

        var identifyingAnswers = answers
            .Where(x => x.QuestionIdentity.RosterVector == RosterVector.Empty)
            .ToList();

        var rosterAnswers = answers.Except(identifyingAnswers).ToList();

        var assignmentRow = new PreloadingAssignmentRow
        {
            FileName = questionnaire.Title,
            QuestionnaireOrRosterName = questionnaire.VariableName,
            InterviewIdValue = new PreloadingValue {Value = tempAssignmentId}.ToAssignmentInterviewId(),
            Email = new PreloadingValue {Value = assignmentInfo.Email, Column = nameof(assignmentInfo.Email)}
                .ToAssignmentEmail(),
            Password = new PreloadingValue
                {Value = assignmentInfo.Password, Column = nameof(assignmentInfo.Password)}.ToAssignmentPassword(),
            Quantity = new AssignmentQuantity
                {Quantity = assignmentInfo.Quantity, Column = nameof(assignmentInfo.Quantity)},
            WebMode = new AssignmentWebMode
                {WebMode = assignmentInfo.WebMode, Column = nameof(assignmentInfo.WebMode)},
            RecordAudio = new AssignmentRecordAudio
            {
                DoesNeedRecord = assignmentInfo.IsAudioRecordingEnabled,
                Column = nameof(assignmentInfo.IsAudioRecordingEnabled)
            },
            Responsible = new PreloadingValue
                    {Value = assignmentInfo.Responsible, Column = nameof(assignmentInfo.Responsible)}
                .ToAssignmentResponsible(
                    userViewFactory, new Dictionary<string, UserToVerify>()),
            Answers = identifyingAnswers.Select(x => x.ToPreloadAnswer(questionnaire, serializer)).ToArray(),
            Comments = new PreloadingValue
                {Value = assignmentInfo.Comments, Column = nameof(assignmentInfo.Comments)}.ToAssignmentComments()
        };

        var rosterRows = rosterAnswers
            .Select(x => new
            {
                answer = x,
                codes = questionnaire.GetRostersFromTopToSpecifiedQuestion(x.QuestionIdentity.Id)
                    .Select(questionnaire.GetRosterVariableName)
                    .Select((y, i) => new AssignmentRosterInstanceCode
                    {
                        Column = y,
                        VariableName = string.Format(ServiceColumns.IdSuffixFormat, y),
                        Code = x.QuestionIdentity.RosterVector.ElementAtOrDefault(i)
                    }).ToArray(),
                roster = questionnaire.GetRosterVariableName(questionnaire
                    .GetRostersFromTopToSpecifiedQuestion(x.QuestionIdentity.Id).Last())
            })
            .GroupBy(x => new{x.roster, x.answer.QuestionIdentity.RosterVector})
            .Where(x => x.Any())
            .Select(x => new PreloadingAssignmentRow
            {
                RosterInstanceCodes = x.First().codes,
                FileName = x.First().roster,
                QuestionnaireOrRosterName = x.First().roster,
                InterviewIdValue = new PreloadingValue {Value = tempAssignmentId}.ToAssignmentInterviewId(),
                Answers = x.Select(y => y.answer.ToPreloadAnswer(questionnaire, serializer)).ToArray()
            });

        return rosterRows.Union(new[] {assignmentRow});
    }
}
