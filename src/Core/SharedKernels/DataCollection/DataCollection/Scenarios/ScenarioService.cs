using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.CommandBus;
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
                Identity questionId = null;
                if (interviewCommand is ScenarioAnswerCommand scenarioAnswerCommand)
                {
                    questionId = new Identity(questionnaire.GetQuestionIdByVariable(scenarioAnswerCommand.Variable).Value, scenarioAnswerCommand.RosterVector);
                }

                switch (interviewCommand)
                {
                    case ScenarioRemoveAnswerCommand removeAnswer:
                        result.Add(new RemoveAnswerCommand(stubInterviewId, stubUserId, questionId));
                        break;
                    case ScenarioAnswerAudioCommand audioAnswer:
                        result.Add(new AnswerAudioQuestionCommand(stubInterviewId, stubUserId, questionId.Id,
                            questionId.RosterVector, audioAnswer.FileName, audioAnswer.Length));
                        break;
                    case ScenarioAnswerDateTimeCommand dateTimeAnswer:

                        result.Add(new AnswerDateTimeQuestionCommand(stubInterviewId, stubUserId, questionId.Id, questionId.RosterVector, dateTimeAnswer.Answer));
                        break;

                    case ScenarioAnswerGeoLocationCommand geoAnswer:
                        result.Add(new AnswerGeoLocationQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            geoAnswer.Latitude,
                            geoAnswer.Longitude,
                            geoAnswer.Accuracy,
                            geoAnswer.Altitude,
                            geoAnswer.Timestamp));
                        break;
                    case ScenarioAnswerGeographyCommand geography:
                        result.Add(new AnswerGeographyQuestionCommand(
                            stubInterviewId, stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            geography.Geometry,
                            geography.MapName,
                            geography.Area,
                            geography.Coordinates,
                            geography.Length,
                            geography.DistanceToEditor,
                            geography.NumberOfPoints));
                        break;
                    case ScenarioAnswerMultipleOptionsLinkedCommand linkedMultipleOption:
                        result.Add(new AnswerMultipleOptionsLinkedQuestionCommand(
                            stubInterviewId, stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            linkedMultipleOption.SelectedRosterVectors
                        ));
                        break;
                    case ScenarioAnswerMultipleOptionsCommand multipleOptions:
                        result.Add(new AnswerMultipleOptionsQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            multipleOptions.SelectedValues));
                        break;
                    case ScenarioAnswerNumericIntegerCommand integerAnswer:
                        result.Add(new AnswerNumericIntegerQuestionCommand(
                            stubInterviewId, stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            integerAnswer.Answer));
                        break;
                    case ScenarioAnswerNumericRealCommand realAnswer:
                        result.Add(new AnswerNumericRealQuestionCommand(
                            stubInterviewId, stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            realAnswer.Answer));
                        break;
                    case ScenarioAnswerPictureCommand picture:
                        result.Add(new AnswerPictureQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            picture.PictureFileName));
                        break;
                    case ScenarioAnswerSingleOptionLinkedCommand singleLinkedOption:
                        result.Add(new AnswerSingleOptionLinkedQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            singleLinkedOption.SelectedRosterVector));
                        break;
                    case ScenarioAnswerSingleOptionCommand singleOption:
                        result.Add(new AnswerSingleOptionQuestionCommand(
                            stubInterviewId,
                            stubInterviewId,
                            questionId.Id,
                            questionId.RosterVector,
                            singleOption.SelectedValue
                        ));
                        break;
                    case ScenarioAnswerTextListCommand textList:
                        result.Add(new AnswerTextListQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            textList.Answers.Select(x => Tuple.Create((decimal)x.Code, x.Text)).ToArray()));
                        break;
                    case ScenarioAnswerTextCommand answerText:
                        result.Add(new AnswerTextQuestionCommand(
                            stubInterviewId,
                            stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            answerText.Answer));
                        break;
                    case ScenarioAnswerYesNoCommand answerYesNo:
                        result.Add(new AnswerYesNoQuestion(
                            stubInterviewId,
                            stubUserId,
                            questionId.Id,
                            questionId.RosterVector,
                            answerYesNo.AnsweredOptions.ToList()));
                        break;
                    case ScenarioSwitchTranslationCommand switchTranslation:
                        result.Add(new SwitchTranslation(stubInterviewId, switchTranslation.TargetLanguage, stubUserId));
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
                        result.Add(new ScenarioRemoveAnswerCommand(questionnaire.GetEntityVariableOrThrow(removeAnswer.QuestionId), removeAnswer.RosterVector));
                        break;
                    case AnswerAudioQuestionCommand audioAnswer:
                        result.Add(new ScenarioAnswerAudioCommand(questionnaire.GetEntityVariableOrThrow(audioAnswer.QuestionId), audioAnswer.RosterVector, audioAnswer.FileName, audioAnswer.Length));
                        break;
                    case AnswerDateTimeQuestionCommand dateTimeAnswer:
                        result.Add(new ScenarioAnswerDateTimeCommand(questionnaire.GetEntityVariableOrThrow(dateTimeAnswer.QuestionId), dateTimeAnswer.RosterVector, dateTimeAnswer.Answer));
                        break;

                    case AnswerGeoLocationQuestionCommand geoAnswer:
                        result.Add(new ScenarioAnswerGeoLocationCommand(questionnaire.GetEntityVariableOrThrow(geoAnswer.QuestionId),
                            geoAnswer.RosterVector,
                            geoAnswer.Timestamp,
                            geoAnswer.Latitude,
                            geoAnswer.Longitude,
                            geoAnswer.Accuracy,
                            geoAnswer.Altitude));
                        break;
                    case AnswerGeographyQuestionCommand geography:
                        result.Add(new ScenarioAnswerGeographyCommand(questionnaire.GetEntityVariableOrThrow(geography.QuestionId),
                            geography.RosterVector,
                            geography.Geometry,
                            geography.MapName,
                            geography.Area,
                            geography.Coordinates,
                            geography.Length,
                            geography.DistanceToEditor,
                            geography.NumberOfPoints));
                        break;
                    case AnswerMultipleOptionsLinkedQuestionCommand linkedMultipleOption:
                        result.Add(new ScenarioAnswerMultipleOptionsLinkedCommand(questionnaire.GetEntityVariableOrThrow(linkedMultipleOption.QuestionId),
                            linkedMultipleOption.RosterVector,
                            linkedMultipleOption.SelectedRosterVectors
                        ));
                        break;
                    case AnswerMultipleOptionsQuestionCommand multipleOptions:
                        result.Add(new ScenarioAnswerMultipleOptionsCommand(questionnaire.GetEntityVariableOrThrow(multipleOptions.QuestionId),
                            multipleOptions.RosterVector,
                            multipleOptions.SelectedValues));
                        break;
                    case AnswerNumericIntegerQuestionCommand integerAnswer:
                        result.Add(new ScenarioAnswerNumericIntegerCommand(questionnaire.GetEntityVariableOrThrow(integerAnswer.QuestionId),
                            integerAnswer.RosterVector,
                            integerAnswer.Answer));
                        break;
                    case AnswerNumericRealQuestionCommand realAnswer:
                        result.Add(new ScenarioAnswerNumericRealCommand(questionnaire.GetEntityVariableOrThrow(realAnswer.QuestionId),
                            realAnswer.RosterVector,
                            realAnswer.Answer));
                        break;
                    case AnswerPictureQuestionCommand picture:
                        result.Add(new ScenarioAnswerPictureCommand(questionnaire.GetEntityVariableOrThrow(picture.QuestionId),
                            picture.RosterVector,
                            picture.PictureFileName));
                        break;
                    case AnswerSingleOptionLinkedQuestionCommand singleLinkedOption:
                        result.Add(new ScenarioAnswerSingleOptionLinkedCommand(questionnaire.GetEntityVariableOrThrow(singleLinkedOption.QuestionId),
                            singleLinkedOption.RosterVector,
                            singleLinkedOption.SelectedRosterVector));
                        break;
                    case AnswerSingleOptionQuestionCommand singleOption: 
                        result.Add(new ScenarioAnswerSingleOptionCommand(questionnaire.GetEntityVariableOrThrow(singleOption.QuestionId),
                            singleOption.RosterVector,
                            singleOption.SelectedValue
                        ));
                        break;
                    case AnswerTextListQuestionCommand textList:
                        result.Add(new ScenarioAnswerTextListCommand(questionnaire.GetEntityVariableOrThrow(textList.QuestionId),
                            textList.RosterVector,
                            textList.Answers.Select(x => new TextListAnswer((int)x.Item1, x.Item2)).ToList()));
                        break;
                    case AnswerTextQuestionCommand answerText:
                        result.Add(new ScenarioAnswerTextCommand(questionnaire.GetEntityVariableOrThrow(answerText.QuestionId),
                            answerText.RosterVector,
                            answerText.Answer));
                        break;
                    case AnswerYesNoQuestion answerYesNo:
                        result.Add(new ScenarioAnswerYesNoCommand(questionnaire.GetEntityVariableOrThrow(answerYesNo.QuestionId),
                            answerYesNo.RosterVector,
                            answerYesNo.AnsweredOptions.ToList()));
                        break;
                    case SwitchTranslation switchTranslation:
                        result.Add(new ScenarioSwitchTranslationCommand(switchTranslation.Language));
                        break;
                }
            }

            return result;
        }
    }
}
