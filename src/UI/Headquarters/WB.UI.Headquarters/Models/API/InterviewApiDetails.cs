using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.UI.Headquarters.Models.API.Interview;

namespace WB.UI.Headquarters.Models.API
{
    public class InterviewApiDetails
    {
        public InterviewApiDetails(InterviewDetailsView interview)
        {
            this.Questions = new List<QuestionApiItem>();
            this.Rosters = new List<RosterApiItem>();

            var rosters = new Dictionary<string, RosterApiItem>();

            if (interview != null)
            {
                foreach (var interviewGroupView in interview.Groups)
                {
                    if (interviewGroupView.RosterVector.Length == 0)
                        AddQuestionsToRoster(this.Questions, interviewGroupView.Questions);
                    else
                    {
                        var key = CreateLeveKeyFromPropagationVector(interviewGroupView.RosterVector);
                        RosterApiItem item;

                        if (rosters.ContainsKey(key))
                            item = rosters[key];
                        else
                        {
                            item = new RosterApiItem()
                            {
                                Id = interviewGroupView.Id,
                                RosterVector = interviewGroupView.RosterVector,
                                Item = interviewGroupView.RosterVector.Last()
                            };

                            rosters.Add(key, item);
                        }

                        AddQuestionsToRoster(item.Questions, interviewGroupView.Questions);
                    }
                }

                foreach (var rosterApiItem in rosters.Values)
                {
                    if (rosterApiItem.RosterVector.Length > 1)
                    {
                        var key = CreateLeveKeyFromPropagationVector(rosterApiItem.RosterVector.Take(rosterApiItem.RosterVector.Length - 1).ToArray());

                        if (!rosters.ContainsKey(key))
                            throw new Exception("Error in structure");

                        rosters[key].Rosters.Add(rosterApiItem);
                    }
                }

                this.Rosters = rosters.Values.Where(i => i.RosterVector.Length == 1).ToList();
            }
        }

        private static string CreateLeveKeyFromPropagationVector(decimal[] vector)
        {
            return string.Join(",", vector.Select(v => v.ToString("0.############################", CultureInfo.InvariantCulture)));
        }

        private static void AddQuestionsToRoster(List<QuestionApiItem> questions, IEnumerable<InterviewQuestionView> questionsToAdd)
        {
            questions.AddRange(questionsToAdd.Select(question => new QuestionApiItem(question.Variable, question.Answer ?? string.Empty)));
        }

        [DataMember]
        public List<QuestionApiItem> Questions { set; get; }

        [DataMember]
        public List<RosterApiItem> Rosters { set; get; }
    }
}