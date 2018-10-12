using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    public class InterviewQuestionInvariants
    {
        public class ExceptionKeys
        {
            public static readonly string InterviewId = "Interview ID";
            public static readonly string QuestionId = "Question ID";
            public static readonly string Variable = "Variable";
            public static readonly string ProvidedAnswerValue= "ProvidedAnswer";
            public static readonly string AvailableAnswersList = "AvailableAnswers";
            public static readonly string MaxAnswersCount = "MaxAnswersCount";
            public static readonly string AnswersCount = "AnswersCount";
            public static readonly string DecimalPlacesAllowed = "DecimalPlacesAllowed";
            public static readonly string MaxRosterRowCount = "MaxRosterRowCount";
            public static readonly string AvailableRosterVectors = "AvailableRosterVectors";
            public static readonly string ParentValue = "ParentValue";
            public static readonly string ProtectedAnswer = "ProtectedAnswer";
        }

        private IQuestionOptionsRepository QuestionOptionsRepository => ServiceLocator.Current.GetInstance<IQuestionOptionsRepository>();

        public InterviewQuestionInvariants(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree interviewTree)
        {
            this.QuestionIdentity = questionIdentity;
            this.Questionnaire = questionnaire;
            this.InterviewTree = interviewTree;
        }

        public Identity QuestionIdentity { get; }
        private IQuestionnaire Questionnaire { get; }
        private InterviewTree InterviewTree { get; }

        private Guid QuestionId => this.QuestionIdentity.Id;

        public InterviewQuestionInvariants RequireQuestionExists(QuestionType? questionType = null)
            => this
                .RequireQuestionDeclared(questionType)
                .RequireQuestionInstanceExists();

        public InterviewQuestionInvariants RequireQuestionEnabled()
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (question.IsDisabled())
                throw new InterviewException("Question (or it's parent) is disabled and question's answer cannot be changed.")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, question.Identity.ToString() },
                        {ExceptionKeys.Variable, question.VariableName }
                    }
                };

            return this;
        }


        public void RequireTextPreloadValueAllowed()
            => this
                .RequireQuestionExists(QuestionType.Text);

        public void RequireTextAnswerAllowed()
            => this
                .RequireQuestionExists(QuestionType.Text)
                .RequireQuestionEnabled();

        public void RequireNumericIntegerPreloadValueAllowed(int answer)
            => this
                .RequireQuestionExists(QuestionType.Numeric)
                .RequireNumericIntegerQuestionDeclared()
                .RequireRosterSizeAnswerNotNegative(answer)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(answer);

        public void RequireNumericIntegerAnswerAllowed(int answer, int? protectedAnswer)
            => this
                .RequireQuestionExists(QuestionType.Numeric)
                .RequireNumericIntegerQuestionDeclared()
                .RequireRosterSizeAnswerNotNegative(answer)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(answer)
                .RequireQuestionEnabled()
                .RequireProtectedAnswersNotReduced(answer, protectedAnswer);

        private void RequireProtectedAnswersNotReduced(int answer, int? protectedAnswer)
        {
            if (protectedAnswer.HasValue)
            {
                if (answer < protectedAnswer)
                {
                    throw new InterviewException("Reduce value of protected answer is not allowed", InterviewDomainExceptionType.AnswerNotAccepted)
                    {
                        Data =
                        {
                            {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                            {ExceptionKeys.QuestionId, this.QuestionIdentity.ToString()},
                            {ExceptionKeys.ProvidedAnswerValue, answer },
                            {ExceptionKeys.ProtectedAnswer, protectedAnswer }
                        }
                    };
                }
            }
        }

        public void RequireNumericRealPreloadValueAllowed()
            => this
                .RequireQuestionExists(QuestionType.Numeric)
                .RequireNumericRealQuestionDeclared();

        public void RequireNumericRealAnswerAllowed(double answer)
            => this
                .RequireQuestionExists(QuestionType.Numeric)
                .RequireNumericRealQuestionDeclared()
                .RequireAllowedDecimalPlaces(answer)
                .RequireQuestionEnabled();

        public void RequireDateTimePreloadValueAllowed()
            => this
                .RequireQuestionExists(QuestionType.DateTime);

        public void RequireDateTimeAnswerAllowed()
            => this
                .RequireQuestionExists(QuestionType.DateTime)
                .RequireQuestionEnabled();

        public void RequireFixedSingleOptionPreloadValueAllowed(decimal selectedValue)
            => this
                .RequireQuestionExists(QuestionType.SingleOption)
                .RequireOptionExists(selectedValue);

        public void RequireFixedSingleOptionAnswerAllowed(decimal selectedValue, QuestionnaireIdentity questionnaireIdentity)
            => this
                .RequireQuestionExists(QuestionType.SingleOption)
                .RequireOptionExists(selectedValue)
                .RequireQuestionEnabled()
                .RequireCascadingQuestionAnswerCorrespondsToParentAnswer(selectedValue, questionnaireIdentity, this.Questionnaire.Translation);

        public void RequireLinkedToListSingleOptionAnswerAllowed(int selectedValue)
            => this
                .RequireQuestionExists(QuestionType.SingleOption)
                .RequireQuestionEnabled()
                .RequireLinkedToListOptionIsAvailable(selectedValue);

        public void RequireLinkedToRosterSingleOptionAnswerAllowed(decimal[] selectedLinkedOption)
            => this
                .RequireQuestionExists(QuestionType.SingleOption)
                .RequireQuestionEnabled()
                .RequireLinkedOptionIsAvailable(selectedLinkedOption);

        public void RequireFixedMultipleOptionsPreloadValueAllowed(IReadOnlyCollection<int> selectedValues)
            => this
                .RequireQuestionExists(QuestionType.MultyOption)
                .RequireOptionsExist(selectedValues)
                .RequireNotYesNoMultipleOptionsQuestion()
                .RequireRosterSizeAnswerNotNegative(selectedValues.Count)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(selectedValues.Count)
                .RequireMaxAnswersCountLimit(selectedValues.Count);


        public void RequireFixedMultipleOptionsAnswerAllowed(IReadOnlyCollection<int> selectedValues, IReadOnlyCollection<int> protectedValues)
            => this
                .RequireQuestionExists(QuestionType.MultyOption)
                .RequireOptionsExist(selectedValues)
                .RequireNotYesNoMultipleOptionsQuestion()
                .RequireRosterSizeAnswerNotNegative(selectedValues.Count)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(selectedValues.Count)
                .RequireMaxAnswersCountLimit(selectedValues.Count)
                .RequireQuestionEnabled()
                .RequireProtectedAnswersNotRemoved(selectedValues, protectedValues);

        public void RequireProtectedAnswersNotRemoved(IReadOnlyCollection<int> selectedValues, IReadOnlyCollection<int> protectedValues)
        {
            var missingProtectedAnswers = protectedValues?.Any(p => !selectedValues.Contains(p)) ?? false;
            if (missingProtectedAnswers)
            {
                throw new InterviewException("Removing protected answer is not allowed", InterviewDomainExceptionType.AnswerNotAccepted)
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionIdentity.ToString()},
                        {ExceptionKeys.ProvidedAnswerValue, JoinUsingCommas(selectedValues) },
                        {ExceptionKeys.ProtectedAnswer, JoinUsingCommas(protectedValues)}
                    }
                };
            }
        }

        public void RequireLinkedToListMultipleOptionsAnswerAllowed(IReadOnlyCollection<int> selectedValues)
            => this
                .RequireQuestionExists(QuestionType.MultyOption)
                .RequireLinkedToListOptionsAreAvailable(selectedValues)
                .RequireNotYesNoMultipleOptionsQuestion()
                .RequireRosterSizeAnswerNotNegative(selectedValues.Count)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(selectedValues.Count)
                .RequireMaxAnswersCountLimit(selectedValues.Count)
                .RequireQuestionEnabled();

        public void RequireLinkedToRosterMultipleOptionsAnswerAllowed(RosterVector[] selectedLinkedOptions)
            => this
                .RequireQuestionExists(QuestionType.MultyOption)
                .RequireQuestionEnabled()
                .RequireLinkedOptionsAreAvailable(selectedLinkedOptions)
                .RequireMaxAnswersCountLimit(selectedLinkedOptions.Length);

        public void RequireYesNoPreloadValueAllowed(YesNoAnswer answer)
        {
            int[] selectedValues = answer.CheckedOptions.Select(answeredOption => answeredOption.Value).ToArray();
            var yesAnswersCount = answer.CheckedOptions.Count(answeredOption => answeredOption.Yes);

            this
                .RequireQuestionExists(QuestionType.MultyOption)
                .RequireOptionsExist(selectedValues)
                .RequireRosterSizeAnswerNotNegative(yesAnswersCount)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(yesAnswersCount)
                .RequireMaxAnswersCountLimit(yesAnswersCount);
        }

        public void RequireYesNoAnswerAllowed(YesNoAnswer answer)
        {
            int[] selectedValues = answer.CheckedOptions.Select(answeredOption => answeredOption.Value).ToArray();
            var yesAnswersCount = answer.CheckedOptions.Count(answeredOption => answeredOption.Yes);

            this
                .RequireQuestionExists(QuestionType.MultyOption)
                .RequireOptionsExist(selectedValues)
                .RequireRosterSizeAnswerNotNegative(yesAnswersCount)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(yesAnswersCount)
                .RequireMaxAnswersCountLimit(yesAnswersCount)
                .RequireQuestionEnabled();
        }

        public void RequireTextListPreloadValueAllowed(Tuple<decimal, string>[] answers)
            => this
                .RequireQuestionExists(QuestionType.TextList)
                .RequireRosterSizeAnswerNotNegative(answers.Length)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(answers.Length)
                .RequireMaxAnswersCountLimit(answers.Length);

        public void RequireTextListAnswerAllowed(Tuple<decimal, string>[] answers, IReadOnlyList<TextListAnswerRow> protectedAnswers)
            => this
                .RequireQuestionExists(QuestionType.TextList)
                .RequireRosterSizeAnswerNotNegative(answers.Length)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(answers.Length)
                .RequireMaxAnswersCountLimit(answers.Length)
                .RequireQuestionEnabled()
                .RequireUniqueValues(answers)
                .RequireNotEmptyTexts(answers)
                .RequireMaxAnswersCountLimit(answers)
                .RequireProtectedAnswersNotRemoved(answers, protectedAnswers);

        private void RequireProtectedAnswersNotRemoved(Tuple<decimal, string>[] selectedValues, IReadOnlyList<TextListAnswerRow> protectedAnswers)
        {
            foreach (var protectedAnswer in protectedAnswers)
            {
                if (!selectedValues.Any(x => protectedAnswer.Value == x.Item1 && protectedAnswer.Text == x.Item2))
                {
                    throw new InterviewException("Removing or modification of protected answer is not allowed", InterviewDomainExceptionType.AnswerNotAccepted)
                    {
                        Data =
                        {
                            {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                            {ExceptionKeys.QuestionId, this.QuestionIdentity.ToString()},
                            {ExceptionKeys.ProvidedAnswerValue, JoinUsingCommas(selectedValues.Select(x => x.Item1)) },
                            {ExceptionKeys.ProtectedAnswer, JoinUsingCommas(protectedAnswers.Select(x => x.Value))}
                        }
                    };
                }
            }
        }

        public void RequireGpsCoordinatesPreloadValueAllowed(GeoPosition answer)
            => this
                .RequireQuestionExists(QuestionType.GpsCoordinates)
                .RequireGpsCoordinatesBeInRange(answer);

        public void RequireGpsCoordinatesAnswerAllowed()
            => this
                .RequireQuestionExists(QuestionType.GpsCoordinates)
                .RequireQuestionEnabled();

        public void RequireQRBarcodePreloadValueAllowed()
            => this
                .RequireQuestionExists(QuestionType.QRBarcode);

        public void RequireQRBarcodeAnswerAllowed()
            => this
                .RequireQuestionExists(QuestionType.QRBarcode)
                .RequireQuestionEnabled();

        public void RequireAudioAnswerAllowed()
            => this
                .RequireQuestionExists(QuestionType.Audio)
                .RequireQuestionEnabled();

        public void RequirePictureAnswerAllowed()
            => this
                .RequireQuestionExists(QuestionType.Multimedia)
                .RequireQuestionEnabled();

        public void RequireAreaAnswerAllowed()
            => this
                .RequireQuestionExists(QuestionType.Area)
                .RequireQuestionEnabled();

        private InterviewQuestionInvariants RequireQuestionDeclared()
        {
            if (!this.Questionnaire.HasQuestion(this.QuestionId))
                throw new InterviewException(
                    $"Question is missing")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireQuestionDeclared(QuestionType? questionType)
        {
            this.RequireQuestionDeclared();

            if (questionType.HasValue)
            {
                this.RequireQuestionType(questionType.Value);
            }

            return this;
        }

        private InterviewQuestionInvariants RequireQuestionType(QuestionType expectedQuestionType)
        {
            QuestionType actualQuestionType = this.Questionnaire.GetQuestionType(this.QuestionId);

            if (expectedQuestionType != actualQuestionType)
                throw new AnswerNotAcceptedException(
                    $"Question has type {actualQuestionType:G}. " +
                    $"But following type was expected: {expectedQuestionType:G}")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireNumericRealQuestionDeclared()
        {
            if (this.Questionnaire.IsQuestionInteger(this.QuestionId))
                throw new AnswerNotAcceptedException(
                    $"Question doesn't support answer of type real")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId }
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireNumericIntegerQuestionDeclared()
        {
            if (!this.Questionnaire.IsQuestionInteger(this.QuestionId))
                throw new AnswerNotAcceptedException(
                    $"Question doesn't support answer of type integer")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireNotEmptyTexts(Tuple<decimal, string>[] answers)
        {
            if (answers.Any(x => string.IsNullOrWhiteSpace(x.Item2)))
                throw new InterviewException(
                    $"String values should be not empty or whitespaces for question"){
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireUniqueValues(Tuple<decimal, string>[] answers)
        {
            var decimals = answers.Select(x => x.Item1).Distinct().ToArray();

            if (answers.Length > decimals.Length)
                throw new InterviewException(
                    $"Decimal values should be unique for question")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireOptionExists(decimal value)
        {
            var availableValues = this.Questionnaire.GetOptionForQuestionByOptionValue(this.QuestionId, value);

            if (availableValues == null)
                throw new AnswerNotAcceptedException(
                    $"Provided answer is not in the list part of predefined answers")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId},
                        {ExceptionKeys.ProvidedAnswerValue, value}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireGpsCoordinatesBeInRange(GeoPosition answer)
        {
            if (answer.Latitude < -90 || answer.Latitude > 90)
                throw new AnswerNotAcceptedException(
                    $"Latitude should be in range (-90, 90)")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId},
                        {ExceptionKeys.ProvidedAnswerValue, answer}

                    }
                };

            if (answer.Longitude < -180 || answer.Longitude > 180)
                throw new AnswerNotAcceptedException(
                    $"Longitude should be in range (-180, 180)")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId},
                        {ExceptionKeys.ProvidedAnswerValue, answer}

                    }
                };

            return this;
        }

        

        private InterviewQuestionInvariants RequireOptionsExist(IReadOnlyCollection<int> values)
        {
            IEnumerable<decimal> availableValues = this.Questionnaire.GetMultiSelectAnswerOptionsAsValues(this.QuestionId);

            bool someValueIsNotOneOfAvailable = values.Any(value => !availableValues.Contains(value));
            if (someValueIsNotOneOfAvailable)
                throw new AnswerNotAcceptedException(
                    $"Provided selected values as answer are not listed in the allowed answers list")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId},
                        {ExceptionKeys.ProvidedAnswerValue, JoinUsingCommas(values)},
                        {ExceptionKeys.AvailableAnswersList, JoinUsingCommas(availableValues)}

                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireNotYesNoMultipleOptionsQuestion()
        {
            if (this.Questionnaire.IsQuestionYesNo(this.QuestionIdentity.Id))
                throw new InterviewException(
                    $"Question has Yes/No type, but answer is given for Multiple Options type")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireMaxAnswersCountLimit(Tuple<decimal, string>[] answers)
        {
            int? maxAnswersCountLimit = this.Questionnaire.GetListSizeForListQuestion(this.QuestionIdentity.Id);

            if (maxAnswersCountLimit.HasValue && answers.Length > maxAnswersCountLimit.Value)
                throw new InterviewException(
                    $"Answers exceed MaxAnswerCount limit") {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId},
                        {ExceptionKeys.MaxAnswersCount, maxAnswersCountLimit},
                        {ExceptionKeys.AnswersCount, answers.Length}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireMaxAnswersCountLimit(int answersCount)
        {
            int? maxSelectedOptions = this.Questionnaire.GetMaxSelectedAnswerOptions(this.QuestionId);

            if (maxSelectedOptions.HasValue && maxSelectedOptions > 0 && answersCount > maxSelectedOptions)
                throw new AnswerNotAcceptedException(
                    $"Number of answers is greater than the maximum number of selected answers")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId},
                        {ExceptionKeys.MaxAnswersCount, maxSelectedOptions},
                        {ExceptionKeys.AnswersCount, answersCount}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireAllowedDecimalPlaces(double answer)
        {
            int? countOfDecimalPlacesAllowed = this.Questionnaire.GetCountOfDecimalPlacesAllowedByQuestion(this.QuestionId);
            if (!countOfDecimalPlacesAllowed.HasValue)
                return this;

            var roundedAnswer = Math.Round(answer, countOfDecimalPlacesAllowed.Value);
            if (roundedAnswer != answer)
                throw new AnswerNotAcceptedException(
                    $"Answer is incorrect because has more decimal places than allowed by questionnaire")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId},
                        {ExceptionKeys.DecimalPlacesAllowed, countOfDecimalPlacesAllowed},
                        {ExceptionKeys.ProvidedAnswerValue, answer}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireRosterSizeAnswerNotNegative(int answer)
        {
            if (!this.Questionnaire.IsRosterSizeQuestion(this.QuestionId))
                return this;

            if (answer < 0)
            {
                if (this.Questionnaire.GetOptionForQuestionByOptionValue(this.QuestionId, answer) == null)
                {
                    throw new AnswerNotAcceptedException(
                        $"Answer is incorrect because question is used as size of roster and specified answer is negative")
                    {
                        Data =
                        {
                            {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                            {ExceptionKeys.QuestionId, this.QuestionIdentity.ToString()},
                            {ExceptionKeys.ProvidedAnswerValue, answer}
                        }
                    };
                }
            }

            return this;
        }

        private InterviewQuestionInvariants RequireRosterSizeAnswerRespectsMaxRosterRowCount(int answer)
        {
            int maxRosterRowCount = this.Questionnaire.IsQuestionIsRosterSizeForLongRoster(this.QuestionId)
                ? this.Questionnaire.GetMaxLongRosterRowCount()
                : this.Questionnaire.GetMaxRosterRowCount();

            if (!this.Questionnaire.IsRosterSizeQuestion(this.QuestionId))
                return this;

            if (answer > maxRosterRowCount)
                throw new AnswerNotAcceptedException(
                    $"Answer is incorrect because question is used as size of roster and specified answer is greater than allowed rows count")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionIdentity.ToString()},
                        {ExceptionKeys.MaxRosterRowCount, maxRosterRowCount},
                        {ExceptionKeys.ProvidedAnswerValue, answer}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireQuestionInstanceExists()
        {
            if (this.QuestionIdentity.RosterVector == null)
                throw new InterviewException("Roster information for question is missing. Roster vector cannot be null", InterviewDomainExceptionType.QuestionIsMissing)
                      {
                          Data =
                          {
                              {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                              {ExceptionKeys.QuestionId, this.QuestionIdentity.Id}
                          }
                      };

            var questions = this.InterviewTree.FindQuestions(this.QuestionIdentity.Id);
            var rosterVectors = questions.Select(question => question.Identity.RosterVector).ToList();

            if (!rosterVectors.Contains(this.QuestionIdentity.RosterVector))
                throw new InterviewException("Roster information for question is incorrect. No questions found for roster vector", InterviewDomainExceptionType.QuestionIsMissing)
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionIdentity.ToString()},
                        {ExceptionKeys.AvailableRosterVectors, string.Join(", ", rosterVectors)}
                    }
                };

            return this;
        }

        private InterviewQuestionInvariants RequireLinkedOptionsAreAvailable(RosterVector[] options)
        {
            foreach (var option in options)
            {
                this.RequireLinkedOptionIsAvailable(option);
            }

            return this;
        }

        private void RequireLinkedOptionIsAvailable(RosterVector option)
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (!question.AsLinked.Options.Contains(option))
                throw new InterviewException(
                    $"Answer on linked categorical question cannot be saved. Specified option is absent")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId.ToString()},
                        {ExceptionKeys.AvailableAnswersList, string.Join(", ", question.AsLinked.Options)},
                        {ExceptionKeys.ProvidedAnswerValue, option.ToString()}
                    }
                };

        }

        private InterviewQuestionInvariants RequireLinkedToListOptionsAreAvailable(IEnumerable<int> options)
        {
            foreach (var option in options)
            {
                this.RequireLinkedToListOptionIsAvailable(option);
            }

            return this;
        }


        private InterviewQuestionInvariants RequireLinkedToListOptionIsAvailable(int option)
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (!question.AsLinkedToList.Options.Contains(option))
                throw new InterviewException("Answer on linked to list question cannot be saved. Specified option is absent")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionId},
                        {ExceptionKeys.AvailableAnswersList, string.Join(", ", question.AsLinked.Options)},
                        {ExceptionKeys.ProvidedAnswerValue, option}
                    }
                };
            return this;
        }

        private InterviewQuestionInvariants RequireCascadingQuestionAnswerCorrespondsToParentAnswer(decimal answer, QuestionnaireIdentity questionnaireId, Translation translation)
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (!question.IsCascading)
                return this;

            var answerOption = this.QuestionOptionsRepository.GetOptionForQuestionByOptionValue(questionnaireId,
                this.QuestionIdentity.Id, answer, translation);

            if (!answerOption.ParentValue.HasValue)
                throw new QuestionnaireException(
                    $"Answer option has no parent value"
                )
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionIdentity.ToString()},
                        {ExceptionKeys.ProvidedAnswerValue, answer}
                    }
                };

            int answerParentValue = answerOption.ParentValue.Value;
            var parentQuestion = (question.GetAsInterviewTreeCascadingQuestion()).GetCascadingParentQuestion();

            if (!parentQuestion.IsAnswered())
                return this;

            int actualParentValue = parentQuestion.GetAnswer().SelectedValue;

            if (answerParentValue != actualParentValue)
                throw new AnswerNotAcceptedException(
                    $"Selected value do not correspond to the parent answer selected value")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewTree.InterviewId},
                        {ExceptionKeys.QuestionId, this.QuestionIdentity.ToString()},
                        {ExceptionKeys.ProvidedAnswerValue, answer},
                        {ExceptionKeys.ParentValue, answerParentValue}
                    }
                };

            return this;
        }

        private string FormatQuestionForException()
            => $"'{this.GetQuestionTitleForException()} [{this.GetQuestionVariableNameForException()}]'";

        private string GetQuestionTitleForException()
            => this.Questionnaire.HasQuestion(this.QuestionId)
                ? this.Questionnaire.GetQuestionTitle(this.QuestionId) ?? "<<NO TITLE>>"
                : "<<MISSING>>";

        private string GetQuestionVariableNameForException()
            => this.Questionnaire.HasQuestion(this.QuestionId)
                ? this.Questionnaire.GetQuestionVariableName(this.QuestionId) ?? "<<NO VARIABLE>>"
                : "<<MISSING>>";

        private static string JoinUsingCommas(IEnumerable<decimal> values)
            => JoinUsingCommas(values.Select(value => value.ToString(CultureInfo.InvariantCulture)));

        private static string JoinUsingCommas(IEnumerable<int> values)
            => JoinUsingCommas(values.Select(value => value.ToString(CultureInfo.InvariantCulture)));

        private static string JoinUsingCommas(IEnumerable<string> values) => string.Join(", ", values);
    }
}
