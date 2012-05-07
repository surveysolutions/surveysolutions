using System;
using System.Linq;
using System.Windows.Data;
using System.Globalization;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Entities.SubEntities;


namespace QApp.Helpers.Extensions
{
    public class ChangeObjectMultiConverter:IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ///bad !!! Need refactoring
            if (values.Count() == 0)
                return null;
            var param = values[0] as CompleteQuestionView;
            var answers = new CompleteAnswerView[1]  {
                new CompleteAnswerView(
                    new CompleteAnswer
                        {
                            AnswerText = values[1].ToString(),
                            AnswerValue= values[1].ToString(),
                            AnswerType = AnswerType.Select,
                            Selected = true
                        })
                };
            param.Answers = answers;
            return param;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}