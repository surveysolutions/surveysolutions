using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class PropagatedGroup
    {

        public PropagatedGroup(Guid key, string text, bool isAutoPropagate, Guid propagationKey, List<CompleteQuestionView> questions)
        {
            this.PublicKey = key;
            this.Title = text;
            this.AutoPropagate = isAutoPropagate;
            this.PropogationKey = propagationKey;
            this.Questions = questions;
            Navigation = new ScreenNavigation();
        }
        
        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }
        public ScreenNavigation Navigation { get; set; }

        public string FeaturedTitle { get; set; }
        public bool AutoPropagate { get; private set; }
        public Guid PropogationKey { get; private set; }
        public List<CompleteQuestionView> Questions { get; private set; }

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
                            else if (!string.IsNullOrEmpty(answer.Title))
                            {
                                firstAnswer = answer.Title;
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