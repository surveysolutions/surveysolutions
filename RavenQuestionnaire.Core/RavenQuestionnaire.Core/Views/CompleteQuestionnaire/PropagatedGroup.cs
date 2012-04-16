using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class PropagatedGroup
    {
        public bool AutoPropagate { get; set; }
        public Guid PropogationKey { get; set; }
        public List<CompleteQuestionView> Questions { get; set; }
        public string FirstAnswer
        {
            get
            {
                var answers = Questions.First().Answers.Where(a => a.Selected).ToList();
                string firstAnswer;

                switch (answers.Count)
                {
                    case 0:
                        firstAnswer = "Answer the first question";
                        break;
                    case 1:
                        {
                            var answer = answers[0];
                            if (!string.IsNullOrEmpty(answer.AnswerValue))
                            {
                                firstAnswer = answer.AnswerValue;
                            }
                            else if (!string.IsNullOrEmpty(answer.AnswerText))
                            {
                                firstAnswer = answer.AnswerText;
                            }
                            else
                            {
                                firstAnswer = "Fill parcel name field";
                            }
                        }
                        break;
                    default:
                        firstAnswer = "Multiple answers";
                        break;
                }
                if (string.IsNullOrWhiteSpace(firstAnswer))
                {
                    firstAnswer = "Answer the first question";
                }
                return firstAnswer;
            }
        }
    }
}