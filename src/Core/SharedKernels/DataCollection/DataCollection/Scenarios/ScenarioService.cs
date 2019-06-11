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
        public List<ICommand> ConvertFromScenario(IEnumerable<IScenarioCommand> commands)
        {
            throw new NotImplementedException();
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
