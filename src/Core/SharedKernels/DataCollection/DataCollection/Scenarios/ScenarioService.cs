using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    class ScenarioService : IScenarioService
    {
        public List<InterviewCommand> ConvertFromScenario(IQuestionnaire questionnaire, IEnumerable<IScenarioCommand> commands)
        {
            List<InterviewCommand> result = new List<InterviewCommand>();

            var stubInterviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var stubUserId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            foreach (var interviewCommand in commands)
            {
                //var questionId = new Identity(questionnaire.GetQuestionIdByVariable(interviewCommand.Variable).Value, interviewCommand.RosterVector);
                switch (interviewCommand)
                {
                    case RemoveAnswer removeAnswer:
                        result.Add(new RemoveAnswerCommand(stubInterviewId, stubUserId, 
                            new Identity(questionnaire.GetQuestionIdByVariable(removeAnswer.Variable).Value, removeAnswer.RosterVector)));
                        break;
                    case AnswerAudio audioAnswer:
                        result.Add(new AnswerAudioQuestionCommand(stubInterviewId, stubUserId, 
                            questionnaire.GetQuestionIdByVariable(audioAnswer.Variable).Value,
                            audioAnswer.RosterVector, audioAnswer.FileName, audioAnswer.Length));
                        break;
                    case AnswerDateTime dateTimeAnswer:
                        result.Add(new AnswerDateTimeQuestionCommand(stubInterviewId, stubUserId, 
                            questionnaire.GetQuestionIdByVariable(dateTimeAnswer.Variable).Value,
                            dateTimeAnswer.RosterVector, dateTimeAnswer.Answer));
                        break;

                    case AnswerGeoLocation geoAnswer:
                        result.Add(new AnswerGeoLocationQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionnaire.GetQuestionIdByVariable(geoAnswer.Variable).Value,
                            geoAnswer.RosterVector,
                            geoAnswer.Latitude,
                            geoAnswer.Longitude,
                            geoAnswer.Accuracy,
                            geoAnswer.Altitude,
                            geoAnswer.Timestamp));
                        break;
                    case AnswerGeography geography:
                        result.Add(new AnswerGeographyQuestionCommand(
                            stubInterviewId, stubUserId,
                            questionnaire.GetQuestionIdByVariable(geography.Variable).Value,
                            geography.RosterVector,
                            geography.Geometry,
                            geography.MapName,
                            geography.Area,
                            geography.Coordinates,
                            geography.Length,
                            geography.DistanceToEditor,
                            geography.NumberOfPoints,
                            geography.RequestedAccuracy));
                        break;
                    case AnswerMultipleOptionsLinked linkedMultipleOption:
                        result.Add(new AnswerMultipleOptionsLinkedQuestionCommand(
                            stubInterviewId, stubUserId,
                            questionnaire.GetQuestionIdByVariable(linkedMultipleOption.Variable).Value,
                            linkedMultipleOption.RosterVector,
                            linkedMultipleOption.SelectedRosterVectors
                        ));
                        break;
                    case AnswerMultipleOptions multipleOptions:
                        result.Add(new AnswerMultipleOptionsQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionnaire.GetQuestionIdByVariable(multipleOptions.Variable).Value,
                            multipleOptions.RosterVector,
                            multipleOptions.SelectedValues));
                        break;
                    case AnswerInteger integerAnswer:
                        result.Add(new AnswerNumericIntegerQuestionCommand(
                            stubInterviewId, stubUserId,
                            questionnaire.GetQuestionIdByVariable(integerAnswer.Variable).Value,
                            integerAnswer.RosterVector,
                            integerAnswer.Answer));
                        break;
                    case AnswerReal realAnswer:
                        result.Add(new AnswerNumericRealQuestionCommand(
                            stubInterviewId, stubUserId,
                            questionnaire.GetQuestionIdByVariable(realAnswer.Variable).Value,
                            realAnswer.RosterVector,
                            realAnswer.Answer));
                        break;
                    case AnswerPicture picture:
                        result.Add(new AnswerPictureQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionnaire.GetQuestionIdByVariable(picture.Variable).Value,
                            picture.RosterVector,
                            picture.PictureFileName));
                        break;
                    case AnswerSingleOptionLinked singleLinkedOption:
                        result.Add(new AnswerSingleOptionLinkedQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionnaire.GetQuestionIdByVariable(singleLinkedOption.Variable).Value,
                            singleLinkedOption.RosterVector,
                            singleLinkedOption.SelectedRosterVector));
                        break;
                    case AnswerSingleOption singleOption:
                        result.Add(new AnswerSingleOptionQuestionCommand(
                            stubInterviewId,
                            stubInterviewId,
                            questionnaire.GetQuestionIdByVariable(singleOption.Variable).Value,
                            singleOption.RosterVector,
                            singleOption.SelectedValue
                        ));
                        break;
                    case AnswerTextList textList:
                        result.Add(new AnswerTextListQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionnaire.GetQuestionIdByVariable(textList.Variable).Value,
                            textList.RosterVector,
                            textList.Answers.Select(x => Tuple.Create((decimal)x.Code, x.Text)).ToArray()));
                        break;
                    case AnswerText answerText:
                        result.Add(new AnswerTextQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionnaire.GetQuestionIdByVariable(answerText.Variable).Value,
                            answerText.RosterVector,
                            answerText.Answer));
                        break;
                    case AnswerYesNo answerYesNo:
                        result.Add(new AnswerYesNoQuestion(
                            stubInterviewId,
                            stubUserId,
                            questionnaire.GetQuestionIdByVariable(answerYesNo.Variable).Value,
                            answerYesNo.RosterVector,
                            answerYesNo.AnsweredOptions.ToList()));
                        break;
                    case SwitchTranslation switchTranslation:
                        result.Add(new Commands.Interview.SwitchTranslation(stubInterviewId, switchTranslation.TargetLanguage, stubUserId));
                        break;
                }
            }

            return result;
        }

        public List<IScenarioCommand> ConvertFromInterview(IQuestionnaire questionnaire, IEnumerable<InterviewCommand> commands)
        {
            List<IScenarioCommand> result = new List<IScenarioCommand>();

            foreach (var interviewCommand in commands)
            {
                switch (interviewCommand)
                {
                    case RemoveAnswerCommand removeAnswer:
                        result.Add(new RemoveAnswer(questionnaire.GetEntityVariableOrThrow(removeAnswer.QuestionId), removeAnswer.RosterVector));
                        break;
                    case AnswerAudioQuestionCommand audioAnswer:
                        result.Add(new AnswerAudio(questionnaire.GetEntityVariableOrThrow(audioAnswer.QuestionId), audioAnswer.RosterVector, audioAnswer.FileName, audioAnswer.Length));
                        break;
                    case AnswerDateTimeQuestionCommand dateTimeAnswer:
                        result.Add(new AnswerDateTime(questionnaire.GetEntityVariableOrThrow(dateTimeAnswer.QuestionId), dateTimeAnswer.RosterVector, dateTimeAnswer.Answer));
                        break;

                    case AnswerGeoLocationQuestionCommand geoAnswer:
                        result.Add(new AnswerGeoLocation(questionnaire.GetEntityVariableOrThrow(geoAnswer.QuestionId),
                            geoAnswer.RosterVector,
                            geoAnswer.Timestamp,
                            geoAnswer.Latitude,
                            geoAnswer.Longitude,
                            geoAnswer.Accuracy,
                            geoAnswer.Altitude));
                        break;
                    case AnswerGeographyQuestionCommand geography:
                        result.Add(new AnswerGeography(questionnaire.GetEntityVariableOrThrow(geography.QuestionId),
                            geography.RosterVector,
                            geography.Geometry,
                            geography.MapName,
                            geography.Area,
                            geography.Coordinates,
                            geography.Length,
                            geography.DistanceToEditor,
                            geography.NumberOfPoints,
                            geography.RequestedAccuracy));
                        break;
                    case AnswerMultipleOptionsLinkedQuestionCommand linkedMultipleOption:
                        result.Add(new AnswerMultipleOptionsLinked(questionnaire.GetEntityVariableOrThrow(linkedMultipleOption.QuestionId),
                            linkedMultipleOption.RosterVector,
                            linkedMultipleOption.SelectedRosterVectors
                        ));
                        break;
                    case AnswerMultipleOptionsQuestionCommand multipleOptions:
                        result.Add(new AnswerMultipleOptions(questionnaire.GetEntityVariableOrThrow(multipleOptions.QuestionId),
                            multipleOptions.RosterVector,
                            multipleOptions.SelectedValues));
                        break;
                    case AnswerNumericIntegerQuestionCommand integerAnswer:
                        result.Add(new AnswerInteger(questionnaire.GetEntityVariableOrThrow(integerAnswer.QuestionId),
                            integerAnswer.RosterVector,
                            integerAnswer.Answer));
                        break;
                    case AnswerNumericRealQuestionCommand realAnswer:
                        result.Add(new AnswerReal(questionnaire.GetEntityVariableOrThrow(realAnswer.QuestionId),
                            realAnswer.RosterVector,
                            realAnswer.Answer));
                        break;
                    case AnswerPictureQuestionCommand picture:
                        result.Add(new AnswerPicture(questionnaire.GetEntityVariableOrThrow(picture.QuestionId),
                            picture.RosterVector,
                            picture.PictureFileName));
                        break;
                    case AnswerSingleOptionLinkedQuestionCommand singleLinkedOption:
                        result.Add(new AnswerSingleOptionLinked(questionnaire.GetEntityVariableOrThrow(singleLinkedOption.QuestionId),
                            singleLinkedOption.RosterVector,
                            singleLinkedOption.SelectedRosterVector));
                        break;
                    case AnswerSingleOptionQuestionCommand singleOption: 
                        result.Add(new AnswerSingleOption(questionnaire.GetEntityVariableOrThrow(singleOption.QuestionId),
                            singleOption.RosterVector,
                            singleOption.SelectedValue
                        ));
                        break;
                    case AnswerTextListQuestionCommand textList:
                        result.Add(new AnswerTextList(questionnaire.GetEntityVariableOrThrow(textList.QuestionId),
                            textList.RosterVector,
                            textList.Answers.Select(x => new TextListAnswer((int)x.Item1, x.Item2)).ToList()));
                        break;
                    case AnswerTextQuestionCommand answerText:
                        result.Add(new AnswerText(questionnaire.GetEntityVariableOrThrow(answerText.QuestionId),
                            answerText.RosterVector,
                            answerText.Answer));
                        break;
                    case AnswerYesNoQuestion answerYesNo:
                        result.Add(new AnswerYesNo(questionnaire.GetEntityVariableOrThrow(answerYesNo.QuestionId),
                            answerYesNo.RosterVector,
                            answerYesNo.AnsweredOptions.ToList()));
                        break;
                    case Commands.Interview.SwitchTranslation switchTranslation:
                        result.Add(new SwitchTranslation(switchTranslation.Language));
                        break;
                }
            }

            return result;
        }
    }
}
