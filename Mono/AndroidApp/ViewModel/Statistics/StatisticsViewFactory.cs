using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Main.Core.Entities.SubEntities;
using Main.Core.View;

namespace AndroidApp.ViewModel.Statistics
{
    public class StatisticsViewFactory : IViewFactory<StatisticsInput, StatisticsViewModel>
    {

        #region Implementation of IViewFactory<StatisticsInput,StatisticsViewModel>

        public StatisticsViewModel Load(StatisticsInput input)
        {
            var answered = new List<StatisticsQuestionViewModel>()
                {
                    new StatisticsQuestionViewModel(new ItemPublicKey(Guid.NewGuid(), null), Guid.NewGuid(),
                                                    "Respondent's age:", "24",
                                                    string.Empty),
                    new StatisticsQuestionViewModel(new ItemPublicKey(Guid.NewGuid(), null), Guid.NewGuid(),
                                                    "Respondent's highest level of education:", "University degree",
                                                    string.Empty)
                };
            var invalid = new List<StatisticsQuestionViewModel>();
            for (int i = 0; i < 100; i++)
            {
                invalid.Add(new StatisticsQuestionViewModel(new ItemPublicKey(Guid.NewGuid(), null), Guid.NewGuid(),
                                                            "How do you go to work?", string.Empty,
                                                            string.Empty));
            }
            var result = new StatisticsViewModel(input.QuestionnaireId, "2012 Research department general survey",
                                                 SurveyStatus.Initial, 20, new List<StatisticsQuestionViewModel>(0),
                                                 answered, invalid);
            return result;
        }

        #endregion
    }
}