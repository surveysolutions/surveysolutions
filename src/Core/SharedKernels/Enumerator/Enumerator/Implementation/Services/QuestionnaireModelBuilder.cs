using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class QuestionnaireModelBuilder : IQuestionnaireModelBuilder
    {
        const int RosterUpperBoundDefaultValue = 40;

        public QuestionnaireModel BuildQuestionnaireModel(QuestionnaireDocument questionnaireDocument)
        {
            questionnaireDocument.ConnectChildrenWithParent();

            var questionnaireModel = new QuestionnaireModel();

            var groups = questionnaireDocument.GetAllGroups().ToList();
            var questions = questionnaireDocument.GetAllQuestions().ToList();
            
            var questionIdToRosterLevelDepth = new Dictionary<Guid, int>();
            questionnaireDocument.Children.TreeToEnumerable(x => x.Children)
                .ForEach(x => PerformCalculationsBasedOnTreeStructure(x, questionIdToRosterLevelDepth));

            //questionnaireModel.GroupsHierarchy = questionnaireDocument.Children.Cast<Group>().Select(x => this.BuildGroupsHierarchy(x, 0)).ToList();

            //questionnaireModel.Questions = questions.ToDictionary(x => x.PublicKey, x => CreateQuestionModel(x, questionnaireDocument, questionIdToRosterLevelDepth));

            return questionnaireModel;
        }

        private GroupsHierarchyModel BuildGroupsHierarchy(Group currentGroup, int layerIndex)
        {
            var childrenHierarchy = currentGroup.Children.OfType<Group>()
                .Select(x => this.BuildGroupsHierarchy(x, layerIndex + 1))
                .ToList();

            var resultModel = new GroupsHierarchyModel
            {
                Id = currentGroup.PublicKey,
                Title = currentGroup.Title,
                IsRoster = currentGroup.IsRoster,
                ZeroBasedDepth = layerIndex,
                Children = childrenHierarchy
            };
            return resultModel;
        }

        private static void PerformCalculationsBasedOnTreeStructure(IComposite item, Dictionary<Guid, int> questionIdToRosterLevelDeep)
        {
            var parentAsGroup = item.GetParent() as Group;

            var countOfRostersToTop = 0;

            while (parentAsGroup != null)
            {
                countOfRostersToTop += parentAsGroup.IsRoster ? 1 : 0;

                parentAsGroup = parentAsGroup.GetParent() as Group;
            }

            if (item is IQuestion)
            {
                questionIdToRosterLevelDeep.Add(item.PublicKey, countOfRostersToTop);
            }
        }

        private static BaseQuestionModel CreateQuestionModel(IQuestion question, QuestionnaireDocument questionnaireDocument, Dictionary<Guid, int> questionIdToRosterLevelDeep)
        {
            BaseQuestionModel questionModel;

            var isRosterSizeQuestion = questionnaireDocument.Find<Group>(g => g.IsRoster && g.RosterSizeQuestionId == question.PublicKey).Any();

            switch (question.QuestionType)
            {
                case QuestionType.SingleOption:
                    var singleQuestion = question as SingleQuestion;
                    if (singleQuestion.LinkedToQuestionId.HasValue)
                    {
                        questionModel = new LinkedSingleOptionQuestionModel
                        {
                            LinkedToQuestionId = singleQuestion.LinkedToQuestionId.Value
                        };
                    }else if (singleQuestion.LinkedToRosterId.HasValue)
                    {
                        questionModel = new LinkedToRosterSingleOptionQuestionModel
                        {
                            LinkedToRosterId = singleQuestion.LinkedToRosterId.Value
                        };
                    }
                    else
                    {
                        var isFilteredCombobox = singleQuestion.IsFilteredCombobox.GetValueOrDefault();
                        if (isFilteredCombobox)
                        {
                            questionModel = new FilteredSingleOptionQuestionModel
                            {
                                Options = singleQuestion.Answers.Select(ToOptionModel).ToList(),
                            };
                        }
                        else
                        {
                            if (singleQuestion.CascadeFromQuestionId.HasValue)
                            {
                                questionModel = new CascadingSingleOptionQuestionModel
                                {
                                    CascadeFromQuestionId = singleQuestion.CascadeFromQuestionId.Value,
                                    Options = singleQuestion.Answers.Select(ToCascadingOptionModel).ToList(),
                                    RosterLevelDepthOfParentQuestion =
                                        questionIdToRosterLevelDeep[singleQuestion.CascadeFromQuestionId.Value],
                                };
                            }
                            else
                            {
                                questionModel = new SingleOptionQuestionModel
                                {
                                    Options = singleQuestion.Answers.Select(ToOptionModel).ToList(),
                                };
                            }
                        }
                    }
                    break;
                case QuestionType.MultyOption:
                    var multiQuestion = (MultyOptionsQuestion)question;
                    if (multiQuestion.YesNoView)
                    {
                        questionModel = new YesNoQuestionModel()
                        {
                            AreAnswersOrdered = multiQuestion.AreAnswersOrdered,
                            MaxAllowedAnswers = multiQuestion.MaxAllowedAnswers,
                            Options = question.Answers.Select(ToOptionModel).ToList(),
                            IsRosterSizeQuestion = isRosterSizeQuestion
                        };
                    }
                    else if (multiQuestion.LinkedToQuestionId.HasValue)
                    {
                        questionModel = new LinkedMultiOptionQuestionModel
                        {
                            LinkedToQuestionId = multiQuestion.LinkedToQuestionId.Value,
                            MaxAllowedAnswers = multiQuestion.MaxAllowedAnswers,
                            AreAnswersOrdered = multiQuestion.AreAnswersOrdered
                        };
                    }
                    else if(multiQuestion.LinkedToRosterId.HasValue)
                    {
                        questionModel = new LinkedToRosterMultiOptionQuestionModel
                        {
                            LinkedToRosterId = multiQuestion.LinkedToRosterId.Value,
                            MaxAllowedAnswers = multiQuestion.MaxAllowedAnswers,
                            AreAnswersOrdered = multiQuestion.AreAnswersOrdered
                        };
                    }
                    else
                    {
                        questionModel = new MultiOptionQuestionModel
                        {
                            AreAnswersOrdered = multiQuestion.AreAnswersOrdered,
                            MaxAllowedAnswers = multiQuestion.MaxAllowedAnswers,
                            Options = question.Answers.Select(ToOptionModel).ToList(),
                            IsRosterSizeQuestion = isRosterSizeQuestion
                        };
                    }
                    break;
                case QuestionType.Numeric:
                    var numericQuestion = question as INumericQuestion;
                    if (numericQuestion.IsInteger)
                    {
                        questionModel = new IntegerNumericQuestionModel
                        {
                            IsRosterSizeQuestion = isRosterSizeQuestion
                        };
                    }
                    else
                    {
                        questionModel = new RealNumericQuestionModel { CountOfDecimalPlaces = numericQuestion.CountOfDecimalPlaces };
                    }
                    break;
                case QuestionType.DateTime:
                    questionModel = new DateTimeQuestionModel();
                    break;
                case QuestionType.GpsCoordinates:
                    questionModel = new GpsCoordinatesQuestionModel();
                    break;
                case QuestionType.Text:
                    questionModel = new TextQuestionModel { Mask = (question as TextQuestion).Mask };
                    break;
                case QuestionType.TextList:
                    var listQuestion = question as TextListQuestion;

                    var maxAnswerCount = isRosterSizeQuestion
                           ? listQuestion.MaxAnswerCount.HasValue ? listQuestion.MaxAnswerCount : RosterUpperBoundDefaultValue
                           : listQuestion.MaxAnswerCount;

                    questionModel = new TextListQuestionModel
                    {
                        IsRosterSizeQuestion = isRosterSizeQuestion,
                        MaxAnswerCount = maxAnswerCount
                    };
                    break;
                case QuestionType.QRBarcode:
                    questionModel = new QRBarcodeQuestionModel();
                    break;
                case QuestionType.Multimedia:
                    questionModel = new MultimediaQuestionModel();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            questionModel.Id = question.PublicKey;
            questionModel.Title = question.QuestionText;
            questionModel.IsPrefilled = question.Featured;
            questionModel.ValidationMessage = question.ValidationMessage;
            questionModel.Instructions = question.Instructions;
            questionModel.Variable = question.StataExportCaption;

            return questionModel;
        }

        private static OptionModel ToOptionModel(Answer answer)
        {
            return new OptionModel
            {
                Value = decimal.Parse(answer.AnswerValue, CultureInfo.InvariantCulture),
                Title = answer.AnswerText,
            };
        }

        private static CascadingOptionModel ToCascadingOptionModel(Answer answer)
        {
            return new CascadingOptionModel
            {
                Value = decimal.Parse(answer.AnswerValue, CultureInfo.InvariantCulture),
                Title = answer.AnswerText,
                ParentValue = decimal.Parse(answer.ParentValue, CultureInfo.InvariantCulture),
            };
        }
    }
}